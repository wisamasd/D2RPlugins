namespace D2R_Plugins.Models;

public class Config
{
    public string CommandLineArguments { get; set; }
    public bool DebugLogging { get; set; }
    public bool MonsterStatsDisplay { get; set; }
    public LanguageConfig Language { get; set; }
    public List<MemoryConfig> MemoryConfigs { get; set; }
}

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