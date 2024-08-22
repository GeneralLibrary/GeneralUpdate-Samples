using System.Diagnostics;

namespace StartManager;

public class Application
{
    
    public static void StartFileServer()
    {
        var name = "hfs.exe";
    }
    
    public static void StartClient()
    {
        var name = "ClientSample.exe";
    }
    
    public static void StartServer()
    {
        var name = "ServerSample.exe";
    }
    
    public static void StartUpgrade()
    {
        var name = "UpgradeSample.exe";
    }

    public static void Reset()
    {
    }

    private static void Start(string appName, string path)
    {
        Process[] runningProcesses = Process.GetProcessesByName(appName);
        foreach (var process in runningProcesses)
        {
            try
            {
                process.Kill();
                Console.WriteLine($"已关闭进程: {process.ProcessName}");
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"关闭进程时发生错误: {ex.Message}");
            }
        }
        
        try
        {
            // 启动进程
            var appPath = Path.Combine(path, appName);
            Process.Start(appPath);
            Console.WriteLine($"进程已启动, {appPath}.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"启动进程时发生错误: {ex.Message}");
        }
    }
}