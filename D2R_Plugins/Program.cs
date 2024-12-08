using D2R_Plugins.Helpers;
using D2R_Plugins.Models;
using System.Diagnostics;

namespace D2R_Plugins;

partial class Program
{
    private const string CONFIG_NAME = "config.json";

    private static bool _debugLogging = false;

    static void Main(string[] args)
    {
        var config = TryLoadConfig();

        _debugLogging = config.DebugLogging;

        TrySetupLanguage(config);
        
        Process process = LaunchProcess(config);

        TryInjectHud(process, config);
        TryEditMemory(process, config);

        DebugPressAnyKeyToExit();
    }

    static Process LaunchProcess(Config config, string processName = "d2r.exe")
    {
        try
        {
            return ProcessHelper.Start(processName, config.CommandLineArguments, !_debugLogging, false);
        }
        catch (Exception ex)
        {
            ErrorLogHandler($"Error launching process: {ex.Message}");
            return null;
        }
    }

    static Config TryLoadConfig()
    {
        return ConfigHelper.LoadConfig(CONFIG_NAME, ErrorLogHandler);
    }

    static void TrySetupLanguage(Config config)
    {
        if (!OsiHelper.HasLocaleSetup())
        {
            OsiHelper.SetupDefaultLocales();
        }
        if (config.Language != null)
        {
            OsiHelper.TrySetupLocales(config.Language.Locale, config.Language.LocaleAudio);
        }
    }

    static void TryInjectHud(Process process, Config config)
    {
        if (process == null) return;
        if (!config.MonsterStatsDisplay) return;

        ProcessHelper.InjectDLL(process.Id, "D2RHUD.dll", DebugLogHandler, ErrorLogHandler);
    }

    static void TryEditMemory(Process process, Config config)
    {
        if (process == null) return;
        if (!(config?.MemoryConfigs is List<MemoryConfig> memoryConfigs)) return;

        ProcessHelper.EditMemory(process.Id, memoryConfigs.ToArray(), DebugLogHandler, ErrorLogHandler);
    }

    private static void DebugLogHandler(string message)
    {
        if (!_debugLogging) return;

        Console.WriteLine(message);
    }
    
    private static void ErrorLogHandler(string message)
    {
        Console.WriteLine(message);
    }
    
    private static void DebugPressAnyKeyToExit()
    {
        if (!_debugLogging) return;
        
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
}
