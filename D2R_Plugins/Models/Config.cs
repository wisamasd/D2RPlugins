namespace D2R_Plugins.Models;

public class Config
{
    public static Config Default
    { 
        get
        {
            return new Config()
            {
                CommandLineArguments = "-mod tcp -txt -enablerespec",
                DebugLogging = false,
                MonsterStatsDisplay = false,
                Language = null, // Default null - not to reset existing settings in registry
                MemoryConfigs = new List<MemoryConfig>()
                {
                    new MemoryConfig()
                    {
                        Description = "Enable TCP/IP Access",
                        Address = "749AC",
                        Length = 1,
                        Values = "EB" // Conditional Jump -> Forced Jump
                    },
                    new MemoryConfig()
                    {
                        Description = "Display Player Names When Trading",
                        Address = "BBFCF9",
                        Length = 1,
                        Values = "00" // 255 -> 0
                    },
                    new MemoryConfig()
                    {
                        Description = "Customize Autosave Interval",
                        Address = "27D691",
                        Length = 4,
                        Values = "300" // Value is in seconds, Retail = 300
                    },
                    new MemoryConfig()
                    {
                        Description = "Customize Max Stash Gold",
                        Addresses = [ "BB5346", "345FE9", "31CC7A", "319C9E", "2B78F4", "2B7810", "2929D6", "28D940", "28D7E8", "27F75F", "18ED7A", "18E7C9", "D6255", "348A4B", "3489A2" ],
                        Length = 4,
                        Values = "25000000" // Retail Max = 2,500,000
                    },
                    new MemoryConfig()
                    {
                        Description = "Customize Max Character Gold",
                        Addresses = [ "10252C", "19AA9B", "27F7A4", "28D375", "28D4A7", "2B7895", "2B79A4", "2D4E2D", "2D4F56", "31CBD3", "346822", "346497", "348832", "34B4BF", "BB4C89", "BB5397", "30AC3D", "30AC7F" ],
                        Length = 4,
                        Values = "100000" // Retail Max = 10,000 per character level
                    },
                    new MemoryConfig()
                    {
                        Description = "Customize Player Difficulty (Inactive)",
                        Addresses = [ "1D637E4", "1E31C9C" ],
                        Length = 4,
                        Values = "1" // Retail Max = 8
                    },
                    new MemoryConfig()
                    {
                        Description = "Display Player Names When Trading",
                        Address = "BBFCF9",
                        Length = 1,
                        Values = "00" // 255 -> 0
                    },
                    new MemoryConfig()
                    {
                        Description = "Customize Autosave Interval",
                        Address = "27D691",
                        Length = 4,
                        Values = "300" // Value is in seconds, Retail = 300
                    },
                    new MemoryConfig()
                    {
                        Description = "Customize Max Stash Gold",
                        Addresses = [ "BB5346", "345FE9", "31CC7A", "319C9E", "2B78F4", "2B7810", "2929D6", "28D940", "28D7E8", "27F75F", "18ED7A", "18E7C9", "D6255", "348A4B", "3489A2" ],
                        Length = 4,
                        Values = "25000000" // Retail Max = 2,500,000
                    },
                    new MemoryConfig()
                    {
                        Description = "Customize Max Character Gold",
                        Addresses = [ "10252C", "19AA9B", "27F7A4", "28D375", "28D4A7", "2B7895", "2B79A4", "2D4E2D", "2D4F56", "31CBD3", "346822", "346497", "348832", "34B4BF", "BB4C89", "BB5397", "30AC3D", "30AC7F" ],
                        Length =  4,
                        Values = "100000" // Retail Max = 10,000 per character level
                    },
                    new MemoryConfig()
                    {
                        Description = "Customize Player Difficulty (Inactive)",
                        Addresses = [ "1D637E4", "1E31C9C" ],
                        Length =  4,
                        Values = "1" // Retail Max = 8
                    }
                }
            };
        } 
    }

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