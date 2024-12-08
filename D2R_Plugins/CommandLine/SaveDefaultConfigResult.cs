using D2R_Plugins.Helpers;
using D2R_Plugins.Models;

namespace D2R_Plugins.CommandLine;

internal class SaveDefaultConfigResult : ICommandLineParserResult
{
    private readonly string _fileName;

    public SaveDefaultConfigResult(string fileName)
    {
        _fileName = fileName;
    }

    public int Execute()
    {
        ConfigHelper.SaveConfig(_fileName, Config.Default, ErrorLogHandler);

        return 0;
    }

    private void ErrorLogHandler(string message)
    {
        Console.WriteLine(message);
    }
}
