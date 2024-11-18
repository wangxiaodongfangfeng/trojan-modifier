using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace trojan_modifier;

public class ConfigModifier(TrojanManager trojanManager, string configPath = "/usr/src/trojan/config.json")
{
    private static readonly object LockObject = new();
    private string ConfigPath { get; } = configPath;
    private string BackupConfigPath { get; } = configPath.Replace("config.json", "config_backup.json");

    public string ReadConfig()
    {
        return File.ReadAllText(ConfigPath);
    }
    /// <summary>
    ///  Save the modified config to the config file
    /// </summary>
    /// <param name="json">json file</param>
    /// <returns></returns>
    private bool SaveConfig(string json)
    {
        try
        {
            lock (LockObject)
            {
                File.Move(ConfigPath, BackupConfigPath, true);
                File.WriteAllText(ConfigPath, json);
            }
        }
        catch (IOException e)
        {
            Console.WriteLine(e);
            return false;
        }

        return true;
    }

    public bool Revert()
    {
        try
        {
            lock (LockObject)
            {
                File.Move(BackupConfigPath, ConfigPath, true);
                RestartService();
            }
        }
        catch (IOException e)
        {
            Console.WriteLine(e);
            return false;
        }

        return true;
    }
    private void RestartService()
    {
        // it will kill the previous one automatically
        trojanManager.StartTrojanAsync();
    }

    /// <summary>
    ///  modify the configuration and restart the service 
    /// </summary>
    /// <param name="ip"></param>
    /// <param name="port"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    public bool Modify(string ip, string port, string password)
    {
        var jsonObject = JObject.Parse(ReadConfig());
        jsonObject["remote_addr"] = ip;
        jsonObject["remote_port"] = port;
        if (jsonObject["password"] is JArray { HasValues: true } passwords)
        {
            passwords[0] = password;
        }
        try
        {
            SaveConfig(jsonObject.ToString(Formatting.Indented));
            RestartService();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
        return true;
    }
}