using System.Diagnostics;

namespace trojan_modifier;

public class TrojanManager(string trojanPath, string configPath)
{
    private Process? ProcessInstance { get; set; }

    public async void StartTrojanAsync()
    {
        try
        {
            this.ProcessInstance?.Kill();
            var startInfo = new ProcessStartInfo(trojanPath)
            {
                Arguments = $"-c {configPath}",
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = false
            };
            using var process = Process.Start(startInfo);
            this.ProcessInstance = process;
            if (process != null) process.OutputDataReceived += (sender, e) => { Console.WriteLine(e.Data); };
            await process?.WaitForExitAsync()!;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Start trojan with failed information {e}");
        }
    }
    /// <summary>
    /// Stop Trojan When I 
    /// </summary>
    /// <returns></returns>
    public bool StopTrojan()
    {
        try
        {
            ProcessInstance?.Kill();
        }
        catch (Exception e)
        {
            Console.WriteLine($"Failed to stop trojan service {e}");
            return false;
        }
        return true;
    }
}