using D2R_Plugins.Helpers;

namespace D2R_Plugins.CommandLine;

internal class SetLanguageConfigurationResult : ICommandLineParserResult
{
    private readonly string _locale;
    private readonly string _localeAudio;

    public SetLanguageConfigurationResult(string locale, string localeAudio)
    {
        _locale = locale;
        _localeAudio = localeAudio;
    }

    public int Execute()
    {
        OsiHelper.TrySetupLocales(_locale, _localeAudio);
        return 0;
    }
}
