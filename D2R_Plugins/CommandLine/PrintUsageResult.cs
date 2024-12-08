using System.Reflection;

namespace D2R_Plugins.CommandLine;

internal class PrintUsageResult : ICommandLineParserResult
{
    internal enum Usage
    {
        COMMAND_LIST,
        CONFIG,
        SET_LANGUAGE,
        EXE
    }

    private readonly Usage _usage;
    private readonly int _returnCode;
    private readonly string _executableName;

    private bool _needEmptyLine;

    public PrintUsageResult(Usage usage, int returnCode = 0)
    {
        _usage = usage;
        _returnCode = returnCode;
        _executableName = Assembly.GetExecutingAssembly().GetName().Name;
        _needEmptyLine = false;
    }

    public int Execute()
    {
        switch (_usage)
        {
            case Usage.COMMAND_LIST:
                PrintUage("[filename] [/config] [/language]");
                PrintCommandsHeader();
                PrintKeyValuePairs(new Dictionary<string, string> {
                    { "[filename]", "The absolute or relative path to d2r.exe. By default, the exe is used in the same directory." },
                    { "/config", "Set the configuration file to be used." },
                    { "/language", "Sets the language for the game. Text and audio." },
                });
                break;
            case Usage.SET_LANGUAGE:
                PrintUage("/language <lang code>");
                PrintUage("/language <lang code> [lang code]");
                PrintOptionsHeader();
                PrintKeyValuePairs(new Dictionary<string, string> {
                    { "<lang code>", "Required parameter. If it is the only one, set lang for text and audio, otherwise, only for text." },
                    { "[lang code]", "Not required parameter. Set lang for audio." },
                    { "values:", "deDE, enUS, esES, esMX, frFR, itIT, jaJP, koKR, plPL, ptBR, ruRU, znCN, zhTW." },
                });
                break;
            case Usage.CONFIG:
                PrintUage("[filename] /config <path to config>");
                PrintOptionsHeader();
                PrintKeyValuePairs(new Dictionary<string, string> {
                    { "[filename]", "The absolute or relative path to d2r.exe. By default, the exe is used in the same directory." },
                    { "/config", "Set the configuration file to be used." },
                });
                break;
            case Usage.EXE:
                PrintUage("[filename]");
                PrintUage("[filename] /config <path to config>");
                break;
        }

        return _returnCode;
    }
    
    private void PrintUage(string commandFormat)
    {
        PrintEmptyLineIfNeed();
        Console.WriteLine($"Usage: {_executableName} {commandFormat}");
        _needEmptyLine = true;
    }

    private void PrintCommandsHeader()
    {
        PrintEmptyLineIfNeed();
        Console.WriteLine("Available commands:");
        _needEmptyLine = false;
    }

    private void PrintOptionsHeader()
    {
        PrintEmptyLineIfNeed();
        Console.WriteLine("Available options:");
        _needEmptyLine = false;
    }

    private void PrintEmptyLineIfNeed()
    {
        if (!_needEmptyLine) return;

        Console.WriteLine();
    }

    private void PrintKeyValuePairs(Dictionary<string, string> keyValuePairs)
    {
        int maxKeyNameLenght = 0;

        foreach (var k in keyValuePairs.Keys)
        {
            if (k.Length <= maxKeyNameLenght) continue;

            maxKeyNameLenght = k.Length;
        }

        foreach (var k in keyValuePairs.Keys)
        {
            var countName = maxKeyNameLenght - k.Length + 2;

            Console.WriteLine($"       {k}{new string(' ', countName)}{keyValuePairs[k]}");
        }
    }
}