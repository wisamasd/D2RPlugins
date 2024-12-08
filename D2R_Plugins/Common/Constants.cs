namespace D2R_Plugins.Common;

internal class Constants
{
    internal class Errors
    {
        public const int PARSE_CONFIG = 1;
        public const int PARSE_RUN_EXE1 = 2;
        public const int PARSE_RUN_EXE2 = 3;
        public const int LAUNCH_EXE = 4;
        public const int SET_LANG = 4;
    }

    internal class Defaults
    {
        public const string CONFIG_NAME = "config.json";
        public const string EXE_NAME = "d2r.exe";
        public const string DLL_D2RHUD = "D2RHUD.dll";
    }

    public string[] LangCodes
    {
        get
        {
            return new string[] {  };
        }
    }
}
