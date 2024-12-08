using System.Diagnostics;
using System.Runtime.InteropServices;

namespace D2R_Plugins.Helpers;

internal class ProcessHelper
{
    private const int PROCESS_VM_READ = 0x0010;
    private const int PROCESS_VM_WRITE = 0x0020;
    private const int PROCESS_VM_OPERATION = 0x0008;
    private const int PROCESS_QUERY_INFORMATION = 0x0400;
    private const int PROCESS_CREATE_THREAD = 0x0002;
    private const int MEM_COMMIT = 0x1000;
    private const int PAGE_EXECUTE_READWRITE = 0x40;

    [DllImport("kernel32.dll")]
    private static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

    [DllImport("kernel32.dll")]
    private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);

    [DllImport("kernel32.dll")]
    private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesWritten);

    [DllImport("kernel32.dll")]
    private static extern bool CloseHandle(IntPtr hObject);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint dwSize, out int lpNumberOfBytesWritten);

    [DllImport("kernel32.dll")]
    private static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, out uint lpThreadId);

    [DllImport("kernel32.dll")]
    private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

    [DllImport("kernel32.dll")]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

    internal static Process Start(string processName, string args = null, bool createNoWindow = true, bool userShellExecute = false)
    {
        ProcessStartInfo startInfo = new ProcessStartInfo()
        {
            FileName = processName,
            Arguments = args,
            UseShellExecute = userShellExecute,
            CreateNoWindow = createNoWindow
        };

        Process process = Process.Start(startInfo);
        process.WaitForInputIdle();
        return process;
    }

    private static void ProcessAddress(IntPtr baseAddress, IntPtr hProcess, string address, int length, string values, Action<string> debugLogging, Action<string> errorLogging)
    {
        long offset = Convert.ToInt64(address, 16);
        IntPtr effectiveAddress = IntPtr.Add(baseAddress, (int)offset);

        debugLogging?.Invoke($"Offset from config: 0x{offset:X}\nCalculated Effective Address: 0x{effectiveAddress.ToString("X")}");

        byte[] currentBytes = new byte[length];
        int bytesRead = 0;

        if (ReadProcessMemory(hProcess, effectiveAddress, currentBytes, length, ref bytesRead))
        {
            debugLogging?.Invoke($"Current bytes at address 0x{effectiveAddress.ToString("X")}: {BitConverter.ToString(currentBytes)}");
        }
        else
        { 
            errorLogging?.Invoke($"Failed to read memory at address 0x{effectiveAddress.ToString("X")}"); 
        }

        byte[] valueBytes;

        if (int.TryParse(values, out int intValue))
        { 
            valueBytes = BitConverter.GetBytes(intValue); 
        }
        else
        { 
            valueBytes = ToByteArray(values); 
        }

        int bytesWritten = 0;

        if (WriteProcessMemory(hProcess, effectiveAddress, valueBytes, valueBytes.Length, ref bytesWritten))
        {
            debugLogging?.Invoke($"Written values {values} to address 0x{effectiveAddress.ToString("X")}");
        }
        else
        { 
            errorLogging?.Invoke($"Failed to write to address 0x{effectiveAddress.ToString("X")}"); 
        }
    }

    internal static void EditMemory(int processId, Program.MemoryConfig[] memoryConfigs, Action<string> debugLogging = null, Action<string> errorLogging = null)
    {
        if (memoryConfigs.Length == 0) return;

        int desiredAccess = PROCESS_VM_READ | PROCESS_VM_WRITE | PROCESS_VM_OPERATION | PROCESS_QUERY_INFORMATION;
        IntPtr hProcess = OpenProcess(desiredAccess, false, processId);

        if (hProcess == IntPtr.Zero)
        {
            errorLogging?.Invoke("Failed to open process for memory editing.");
            return;
        }

        Process process = Process.GetProcessById(processId);
        IntPtr baseAddress = process.MainModule.BaseAddress;

        debugLogging?.Invoke($"Base Address of {process.ProcessName}: 0x{baseAddress.ToString("X")}");

        foreach (var entry in memoryConfigs)
        {
            try
            {
                if (entry.Addresses != null && entry.Addresses.Count > 0)
                {
                    foreach (var address in entry.Addresses)
                    {
                        ProcessAddress(baseAddress, hProcess, address, entry.Length, entry.Values, debugLogging, errorLogging);
                    }
                }
                else if (!string.IsNullOrEmpty(entry.Address))
                    ProcessAddress(baseAddress, hProcess, entry.Address, entry.Length, entry.Values, debugLogging, errorLogging);
            }
            catch (Exception ex)
            {
                errorLogging?.Invoke($"Error editing memory: {ex.Message}");
            }
        }

        CloseHandle(hProcess);
    }

    internal static void InjectDLL(int processId, string dllPath, Action<string> debugLogging = null, Action<string> errorLogging = null)
    {
        IntPtr hProcess = OpenProcess(PROCESS_CREATE_THREAD | PROCESS_VM_OPERATION | PROCESS_VM_WRITE, false, processId);

        if (hProcess == IntPtr.Zero)
        {
            errorLogging?.Invoke("Failed to open process for DLL injection.");
            return;
        }
        IntPtr allocMemAddress = VirtualAllocEx(hProcess, IntPtr.Zero, (uint)((dllPath.Length + 1) * Marshal.SizeOf(typeof(char))), MEM_COMMIT, PAGE_EXECUTE_READWRITE);

        if (allocMemAddress == IntPtr.Zero)
        {
            errorLogging?.Invoke("Failed to allocate memory in the target process.");
            CloseHandle(hProcess);
            return;
        }

        int bytesWritten;
        if (!WriteProcessMemory(hProcess, allocMemAddress, System.Text.Encoding.Default.GetBytes(dllPath), (uint)(dllPath.Length + 1), out bytesWritten))
        {
            errorLogging?.Invoke("Failed to write DLL path to target process.");
            CloseHandle(hProcess);
            return;
        }

        IntPtr hKernel32 = GetModuleHandle("kernel32.dll");
        IntPtr hLoadLibrary = GetProcAddress(hKernel32, "LoadLibraryA");

        if (hLoadLibrary == IntPtr.Zero)
        {
            errorLogging?.Invoke("Failed to get address of LoadLibraryA.");
            CloseHandle(hProcess);
            return;
        }

        IntPtr hThread = CreateRemoteThread(hProcess, IntPtr.Zero, 0, hLoadLibrary, allocMemAddress, 0, out _);

        if (hThread == IntPtr.Zero)
            errorLogging?.Invoke("Failed to create remote thread.");
        else
            errorLogging?.Invoke("DLL injected successfully!");

        CloseHandle(hThread);
        CloseHandle(hProcess);
    }

    private static byte[] ToByteArray(string hex)
    {
        if (hex == null)
            return null;

        if (string.IsNullOrEmpty(hex))
            return new byte[0];

        var numberChars = hex.Length;
        var bytes = new byte[numberChars / 2];
        for (var i = 0; i < numberChars; i += 2)
            bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);

        return bytes;
    }
}
