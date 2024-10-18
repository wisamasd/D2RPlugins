using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace MemoryEditor
{
    class Program
    {
        #region Imports
        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);

        [DllImport("kernel32.dll")]
        public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        public static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint dwSize, out int lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        public static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, out uint lpThreadId);

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetModuleHandle(string lpModuleName);
        #endregion

        #region Constants
        const int PROCESS_VM_READ = 0x0010;
        const int PROCESS_VM_WRITE = 0x0020;
        const int PROCESS_VM_OPERATION = 0x0008;
        const int PROCESS_QUERY_INFORMATION = 0x0400;
        const int PROCESS_CREATE_THREAD = 0x0002;
        const int MEM_COMMIT = 0x1000;
        const int PAGE_EXECUTE_READWRITE = 0x40;

        public static bool debugLogging = false;
        #endregion

        #region Config Classes
        public class MemoryConfig
        {
            public string Description { get; set; }
            public string Address { get; set; }
            public List<string> Addresses { get; set; }
            public int Length { get; set; }
            public string Values { get; set; }
        }

        public class Config
        {
            public string CommandLineArguments { get; set; }
            public bool DebugLogging { get; set; }
            public bool MonsterStatsDisplay { get; set; }
            public List<MemoryConfig> MemoryConfigs { get; set; }
        }
        #endregion

        static void Main(string[] args)
        {
            string configPath = "config.json";
            var config = LoadConfig(configPath);

            if (config == null)
            {
                Console.WriteLine("Failed to load configuration.");
                return;
            }

            debugLogging = config.DebugLogging;

            string processName = "../../../d2r.exe";
            string arguments = config.CommandLineArguments;

            Process process = LaunchProcess(processName, arguments);

            if (process != null)
            {
                if (config.MonsterStatsDisplay)
                    InjectDLL(process.Id, "D2RHUD.dll");

                EditMemory(process.Id, config.MemoryConfigs);
            }
            else
                Console.WriteLine("Failed to launch the process.");

            if (debugLogging)
            {
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
        }

        static Process LaunchProcess(string processName, string arguments)
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = processName,
                    Arguments = arguments,
                    UseShellExecute = false,
                    CreateNoWindow = !debugLogging
                };

                Process process = Process.Start(startInfo);
                process.WaitForInputIdle();
                return process;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error launching process: {ex.Message}");
                return null;
            }
        }

        static void EditMemory(int processId, List<MemoryConfig> memoryConfigs)
        {
            int desiredAccess = PROCESS_VM_READ | PROCESS_VM_WRITE | PROCESS_VM_OPERATION | PROCESS_QUERY_INFORMATION;
            IntPtr hProcess = OpenProcess(desiredAccess, false, processId);

            if (hProcess == IntPtr.Zero)
            {
                Console.WriteLine("Failed to open process for memory editing.");
                return;
            }

            Process process = Process.GetProcessById(processId);
            IntPtr baseAddress = process.MainModule.BaseAddress;

            if (debugLogging)
                Console.WriteLine($"Base Address of {process.ProcessName}: 0x{baseAddress.ToString("X")}");

            foreach (var entry in memoryConfigs)
            {
                try
                {
                    if (entry.Addresses != null && entry.Addresses.Count > 0)
                    {
                        foreach (var address in entry.Addresses)
                        {
                            ProcessAddress(baseAddress, hProcess, address, entry.Length, entry.Values);
                        }
                    }
                    else if (!string.IsNullOrEmpty(entry.Address))
                        ProcessAddress(baseAddress, hProcess, entry.Address, entry.Length, entry.Values);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error editing memory: {ex.Message}");
                }
            }

            CloseHandle(hProcess);
        }

        static void ProcessAddress(IntPtr baseAddress, IntPtr hProcess, string address, int length, string values)
        {
            long offset = Convert.ToInt64(address, 16);
            IntPtr effectiveAddress = IntPtr.Add(baseAddress, (int)offset);

            if (debugLogging)
            {
                Console.WriteLine($"Offset from config: 0x{offset:X}");
                Console.WriteLine($"Calculated Effective Address: 0x{effectiveAddress.ToString("X")}");
            }

            byte[] currentBytes = new byte[length];
            int bytesRead = 0;

            if (ReadProcessMemory(hProcess, effectiveAddress, currentBytes, length, ref bytesRead))
            {
                if (debugLogging)
                    Console.WriteLine($"Current bytes at address 0x{effectiveAddress.ToString("X")}: {BitConverter.ToString(currentBytes)}");
            }
            else
                Console.WriteLine($"Failed to read memory at address 0x{effectiveAddress.ToString("X")}");

            byte[] valueBytes;

            if (int.TryParse(values, out int intValue))
                valueBytes = BitConverter.GetBytes(intValue);
            else
                valueBytes = StringToByteArray(values);

            int bytesWritten = 0;

            if (WriteProcessMemory(hProcess, effectiveAddress, valueBytes, valueBytes.Length, ref bytesWritten))
            {
                if (debugLogging)
                    Console.WriteLine($"Written values {values} to address 0x{effectiveAddress.ToString("X")}");
            }
            else
                Console.WriteLine($"Failed to write to address 0x{effectiveAddress.ToString("X")}");
        }

        #region Helper Functions
        static void InjectDLL(int processId, string dllPath)
        {
            IntPtr hProcess = OpenProcess(PROCESS_CREATE_THREAD | PROCESS_VM_OPERATION | PROCESS_VM_WRITE, false, processId);

            if (hProcess == IntPtr.Zero)
            {
                Console.WriteLine("Failed to open process for DLL injection.");
                return;
            }

            IntPtr allocMemAddress = VirtualAllocEx(hProcess, IntPtr.Zero, (uint)((dllPath.Length + 1) * Marshal.SizeOf(typeof(char))), MEM_COMMIT, PAGE_EXECUTE_READWRITE);

            if (allocMemAddress == IntPtr.Zero)
            {
                Console.WriteLine("Failed to allocate memory in the target process.");
                CloseHandle(hProcess);
                return;
            }

            int bytesWritten;
            if (!WriteProcessMemory(hProcess, allocMemAddress, System.Text.Encoding.Default.GetBytes(dllPath), (uint)(dllPath.Length + 1), out bytesWritten))
            {
                Console.WriteLine("Failed to write DLL path to target process.");
                CloseHandle(hProcess);
                return;
            }

            IntPtr hKernel32 = GetModuleHandle("kernel32.dll");
            IntPtr hLoadLibrary = GetProcAddress(hKernel32, "LoadLibraryA");

            if (hLoadLibrary == IntPtr.Zero)
            {
                Console.WriteLine("Failed to get address of LoadLibraryA.");
                CloseHandle(hProcess);
                return;
            }

            IntPtr hThread = CreateRemoteThread(hProcess, IntPtr.Zero, 0, hLoadLibrary, allocMemAddress, 0, out _);

            if (hThread == IntPtr.Zero)
                Console.WriteLine("Failed to create remote thread.");
            else
                Console.WriteLine("DLL injected successfully!");

            CloseHandle(hThread);
            CloseHandle(hProcess);
        }
        

        static Config LoadConfig(string configPath)
        {
            try
            {
                string jsonContent = File.ReadAllText(configPath);
                return JsonConvert.DeserializeObject<Config>(jsonContent);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading config: {ex.Message}");
                return null;
            }
        }

        static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }
        #endregion
    }
}
