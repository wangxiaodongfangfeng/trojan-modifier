using System.Diagnostics;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace trojan_modifier;

public class ConfigModifier(string configPath = "/usr/src/trojan/config.json")
{
    private static readonly object LockObject = new object();
    private string ConfigPath { get; set; } = configPath;
    private string BackupConfigPath { get; set; } = configPath.Replace("config.json", "config_backup.json");

    public string ReadConfig()
    {
        return File.ReadAllText(this.ConfigPath);
    }

    /// <summary>
    ///  Save the modified config to the config file
    /// </summary>
    /// <param name="json">json file</param>
    /// <returns></returns>
    private bool SaveConfig(string json
)
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
            throw;
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
                this.RestartService();
            }
        }
        catch (IOException e)
        {
            Console.WriteLine(e);
            throw;
        }

        return true;
    }

    private void RestartService()
    {
        // The name of the service you want to restart
        var serviceName = "trojan"; // Replace with the name of your service

        // Create the process to run the systemctl command
        var startInfo = new ProcessStartInfo()
        {
            FileName = "/bin/bash", // Use bash shell to run systemctl
            Arguments = $"-c \"sudo systemctl restart {serviceName}\"", // Run systemctl restart command
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        // Start the process to execute the command
        using var process = Process.Start(startInfo);
        // Read and print the output (stdout)
        var output = process?.StandardOutput.ReadToEnd();
        Console.WriteLine("Output:");
        Console.WriteLine(output);

        // Read and print any errors (stderr)
        var error = process?.StandardError.ReadToEnd();
        if (!string.IsNullOrEmpty(error))
        {
            Console.WriteLine("Error:");
            Console.WriteLine(error);
        }

        // Wait for the process to finish
        process?.WaitForExit();
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
        var json = this.ReadConfig();
        var jsonObject = JObject.Parse(json);

        jsonObject["remote_addr"] = ip;
        jsonObject["remote_port"] = port;

        if (jsonObject["password"] is JArray { HasValues: true } passwords)
        {
            passwords[0] = password;
        }

        json = jsonObject.ToString(Formatting.Indented);
        this.SaveConfig(json);
        this.RestartService();
        return true;
    }
}