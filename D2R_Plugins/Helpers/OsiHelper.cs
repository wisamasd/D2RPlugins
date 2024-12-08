using Microsoft.Win32;

namespace D2R_Plugins.Helpers;

internal class OsiHelper
{
    private const string OSI_SETUP_PATH = @"SOFTWARE\Blizzard Entertainment\Battle.net\Launch Options\OSI";
    private const string OSI_LOCALE = @"LOCALE";
    private const string OSI_LOCALE_AUDIO = @"LOCALE_AUDIO";
    private const string OSI_LOCALE_DEFAULT_VALUE = @"enUS";
    private const string OSI_LOCALE_AUDIO_DEFAULT_VALUE = @"enUS";

    private static readonly HashSet<string> Locales = new HashSet<string>()
    {
        "deDE", "enGB", "enUS", "esES", "esMX", "frFR", "itIT", "jaJP", "koKR", "plPL", "ptBR", "ruRU", "znCN", "zhTW",
    };

    internal static bool HasLocaleSetup()
    {
        var locale = RegHelper.KeyExist(RegistryHive.CurrentUser, OSI_SETUP_PATH, OSI_LOCALE)
            && Locales.Contains(RegHelper.GetStringValue(RegistryHive.CurrentUser, OSI_SETUP_PATH, OSI_LOCALE));

        var localeAudio = RegHelper.KeyExist(RegistryHive.CurrentUser, OSI_SETUP_PATH, OSI_LOCALE_AUDIO)
            && Locales.Contains(RegHelper.GetStringValue(RegistryHive.CurrentUser, OSI_SETUP_PATH, OSI_LOCALE_AUDIO));

        return locale && localeAudio;
    }

    internal static void SetupDefaultLocales()
    {
        TrySetupLocales(OSI_LOCALE_DEFAULT_VALUE, OSI_LOCALE_AUDIO_DEFAULT_VALUE);
    }

    internal static void TrySetupLocales(string locale, string localeAudio)
    {
        if (Locales.Contains(locale)) 
        {
            RegHelper.SetValue(RegistryHive.CurrentUser, OSI_SETUP_PATH, OSI_LOCALE, locale);
        };

        if (Locales.Contains(localeAudio))
        {
            RegHelper.SetValue(RegistryHive.CurrentUser, OSI_SETUP_PATH, OSI_LOCALE_AUDIO, localeAudio);
        };
    }
}
