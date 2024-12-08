using D2R_Plugins.Common;
using D2R_Plugins.Helpers;
using D2R_Plugins.Models;
using System.Diagnostics;

namespace D2R_Plugins.CommandLine;

internal class RunExeWithConfigResult : ICommandLineParserResult
{
    private readonly string _exeName;
    private readonly string _configName;

    private bool _debugLogging = false;

    private Process _process;

    public RunExeWithConfigResult(string exeName, string configName)
    {
        _exeName = exeName;
        _configName = configName;
    }

    public int Execute()
    {        
        var  config = ConfigHelper.LoadConfig(_configName, ErrorLogHandler);

        _debugLogging = config.DebugLogging;

        TrySetupLanguage(config);

        int returnCode = 0;

        if(returnCode == 0)
        {
            returnCode = LaunchProcess(config, _exeName);
        }

        if (returnCode == 0)
        {
            returnCode = TryInjectHud(config);
        }

        if (returnCode == 0)
        {
            returnCode = TryEditMemory(config);
        }

        DebugPressAnyKeyToExit();

        return 0;
    }

    private int LaunchProcess(Config config, string processName = Constants.Defaults.EXE_NAME)
    {
        try
        {
            _process = ProcessHelper.Start(processName, config.CommandLineArguments, !_debugLogging, false);
            return 0;
        }
        catch (Exception ex)
        {
            ErrorLogHandler($"Error launching process: {ex.Message}");
            return Constants.Errors.LAUNCH_EXE;
        }
    }

    private void TrySetupLanguage(Config config)
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

    private int TryInjectHud(Config config)
    {
        if (_process == null) return Constants.Errors.LAUNCH_EXE;

        if (!config.MonsterStatsDisplay) return 0;


        if (!File.Exists(Constants.Defaults.DLL_D2RHUD))
        {
            DebugLogHandler($"The {Constants.Defaults.DLL_D2RHUD} is missing");
        }

        ProcessHelper.InjectDLL(_process.Id, Constants.Defaults.DLL_D2RHUD, DebugLogHandler, ErrorLogHandler);

        return 0;
    }

    private int TryEditMemory(Config config)
    {
        if (_process == null) return Constants.Errors.LAUNCH_EXE;

        if (!(config?.MemoryConfigs is List<MemoryConfig> memoryConfigs)) return 0;

        ProcessHelper.EditMemory(_process.Id, memoryConfigs.ToArray(), DebugLogHandler, ErrorLogHandler);
        return 0;
    }

    private void DebugLogHandler(string message)
    {
        if (!_debugLogging) return;

        Console.WriteLine(message);
    }

    private void ErrorLogHandler(string message)
    {
        Console.WriteLine(message);
    }

    private void DebugPressAnyKeyToExit()
    {
        if (!_debugLogging) return;

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
}
