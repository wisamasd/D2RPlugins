using D2R_Plugins.CommandLine;

namespace D2R_Plugins;

partial class Program
{
    static int Main(string[] args)
    {
        CommandLineParser commandLineParser = new CommandLineParser();

        ICommandLineParserResult commandLineParserResult = commandLineParser.Parse(args);

        int result = commandLineParserResult.Execute();

        return result;
    } 
}
