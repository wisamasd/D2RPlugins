using D2R_Plugins.Helpers;
using System.Diagnostics;
using Newtonsoft.Json;

namespace D2R_Plugins;

partial class Program
{
    private static bool _debugLogging = false;

    #region Config Classes
    public class MemoryConfig
    {
        public string Description { get; set; }
        public string Address { get; set; }
        public List<string> Addresses { get; set; }
        public int Length { get; set; }
        public string Values { get; set; }
    }

    public class LanguageConfig
    {
        public string Locale { get; set; }
        public string LocaleAudio { get; set; }
    }

    public class Config
    {
        public string CommandLineArguments { get; set; }
        public bool DebugLogging { get; set; }
        public bool MonsterStatsDisplay { get; set; }
        public LanguageConfig Language { get; set; }
        public List<MemoryConfig> MemoryConfigs { get; set; }
    }
    #endregion

    static void Main(string[] args)
    {
        string configPath = "config.json";
        var config = LoadConfig(configPath);

        if (config == null)
        {
            ErrorLogHandler("Failed to load configuration.");
            return;
        }
        _debugLogging = config.DebugLogging;

        TrySetupLanguage(config);

        string processName = "../../../d2r.exe";
        string arguments = config.CommandLineArguments;

        Process process = LaunchProcess(processName, arguments);

        TryInjectHud(process, config);
        TryEditMemory(process, config);

        DebugPressAnyKeyToExit();
    }

    static Process LaunchProcess(string processName, string arguments)
    {
        try
        {
            return ProcessHelper.Start(processName, arguments, !_debugLogging, false);
        }
        catch (Exception ex)
        {
            ErrorLogHandler($"Error launching process: {ex.Message}");
            return null;
        }
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
