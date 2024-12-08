using Microsoft.Win32;

namespace D2R_Plugins.Helpers;

internal class RegHelper
{
    private static RegistryKey GetKey(RegistryHive hive, string keyPath)
    {
        return RegistryKey.OpenBaseKey(hive, RegistryView.Registry64)?.OpenSubKey(keyPath);
    }

    private static RegistryKey SetKey(RegistryHive hive, string keyPath)
    {
        return RegistryKey.OpenBaseKey(hive, RegistryView.Registry64).OpenSubKey(keyPath, true) 
            ?? RegistryKey.OpenBaseKey(hive, RegistryView.Registry64).CreateSubKey(keyPath);
    }

    internal static bool KeyExist(RegistryHive hive, string path, string name)
    {
        return (GetKey(hive, path)?.GetValue(name) is null).Invert();
    }

    internal static void SetValue(RegistryHive hive, string path, string name, object value)
    {
        SetKey(hive, path).SetValue(name, value);
    }

    internal static string GetStringValue(RegistryHive hive, string path, string name)
    {
        return GetKey(hive, path)?.GetValue(name) as string;
    }
}
