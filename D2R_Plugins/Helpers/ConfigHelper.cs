using D2R_Plugins.Models;
using Newtonsoft.Json;

namespace D2R_Plugins.Helpers;

internal class ConfigHelper
{
    internal static Config LoadConfig(string path, Action<string> errorLogging = null)
    {
        Config result = null;

		try
		{
            if (File.Exists(path))
            {
                string jsonContent = File.ReadAllText(path);
                return JsonConvert.DeserializeObject<Config>(jsonContent);
            }
            else
            {
                errorLogging?.Invoke($"Error load config: the file \"{path}\" is missing.\nUsed default settings.");
                result = null;
            }

        }
		catch(Exception ex)
		{
            errorLogging?.Invoke($"Error load config: {ex.Message}\nUsed default settings.");
            result = null;
		}

        return result;
    }

    internal static void SaveConfig(string configPath, Config config, Action<string> errorLogging = null)
    {
        try
        {
            File.WriteAllText(configPath, JsonConvert.SerializeObject(config, Formatting.Indented));
        }
        catch (Exception ex)
        {
            errorLogging?.Invoke($"Error saving config: {ex.Message}.");
        }
    }
}