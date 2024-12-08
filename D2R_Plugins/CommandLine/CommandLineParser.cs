using D2R_Plugins.Common;
using D2R_Plugins.Helpers;

namespace D2R_Plugins.CommandLine;

internal class CommandLineParser
{
    public ICommandLineParserResult Parse(string[] commandLineArguments)
    {
        if (commandLineArguments.Length > 0)
        {
            switch (commandLineArguments[0].ToLowerInvariant())
            {
                case "/?":
                case "/help":
                    return new PrintUsageResult(PrintUsageResult.Usage.COMMAND_LIST);
                case "/config":
                    return ParseConfigurationCommand(commandLineArguments.Skip(1));
                case "/language":
                    return ParseLanguageConfigurationCommand(commandLineArguments.Skip(1));
                default:
                    return ParseExeFirstCommand(commandLineArguments);

            }
        }
        return new RunExeWithConfigResult(Constants.Defaults.EXE_NAME, Constants.Defaults.CONFIG_NAME);
    }

    private ICommandLineParserResult ParseConfigurationCommand(string[] configurationCommandLineArguments)
    {
        if (configurationCommandLineArguments.Length == 1)
        {
            return new RunExeWithConfigResult(Constants.Defaults.EXE_NAME, configurationCommandLineArguments[0]);
        }
        return new PrintUsageResult(PrintUsageResult.Usage.CONFIG, Constants.Errors.PARSE_CONFIG);
    }

    private ICommandLineParserResult ParseExeFirstCommand(string[] exportDefaultConfigCommandLineArguments)
    {
        if (exportDefaultConfigCommandLineArguments.Length == 2 || exportDefaultConfigCommandLineArguments.Length > 3 || exportDefaultConfigCommandLineArguments.Length < 1)
        {
            Console.WriteLine("Error: Invalid number of arguments");
            return new PrintUsageResult(PrintUsageResult.Usage.EXE, Constants.Errors.PARSE_RUN_EXE1);
        }

        string runExePath = exportDefaultConfigCommandLineArguments[0];
        string configName = Constants.Defaults.CONFIG_NAME;

        if (exportDefaultConfigCommandLineArguments.Length == 3)
        {
            string command = exportDefaultConfigCommandLineArguments[1];
            if (command.ToLowerInvariant() == "/config")
            {
                configName = exportDefaultConfigCommandLineArguments[2];
            }
            else
            {
                Console.WriteLine($"Error: command \"{command}\" is not supported.");
                return new PrintUsageResult(PrintUsageResult.Usage.EXE, Constants.Errors.PARSE_RUN_EXE2);
            }
        }

        return new RunExeWithConfigResult(runExePath, configName);
    }

    private ICommandLineParserResult ParseLanguageConfigurationCommand(string[] languageConfigurationCommandLineArguments)
    {
        if (languageConfigurationCommandLineArguments.Length != 1 && languageConfigurationCommandLineArguments.Length != 2)
        {
            Console.WriteLine("Error: Invalid number of arguments");
            return new PrintUsageResult(PrintUsageResult.Usage.SET_LANGUAGE, Constants.Errors.SET_LANG);
        }

        var locale = languageConfigurationCommandLineArguments[0];
        var localeAudio = languageConfigurationCommandLineArguments[0];
        
        if (languageConfigurationCommandLineArguments.Length == 2)
        {
            localeAudio = languageConfigurationCommandLineArguments[1];
        }

        return new SetLanguageConfigurationResult(locale, localeAudio);
    }
}