using System.Diagnostics;

namespace trojan_modifier;

public class TrojanManager(string trojanPath, string configPath)
{
    private Process? ProcessInstance { get; set; }

    public async void StartTrojanAsync()
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

    public bool StopTrojanAsync()
    {
        this.ProcessInstance?.Kill();
        return true;
    }
}