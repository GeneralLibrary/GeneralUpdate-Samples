using System.Diagnostics;

namespace StartManager;

public class Application
{
    public static void StartClient() => Start("ClientSample.exe", GetAppDirectory());
    
    public static void StartServer() => Start("ServerSample.exe", GetAppDirectory());
    
    public static void Reset() => Start("resource.bat", GetParent());
    
    private static string GetAppDirectory() => Path.Combine(GetParent(), "run", "app");

    private static string GetParent()
    {
        var currentDirectory = Directory.GetCurrentDirectory();
        var indexOfSrc = currentDirectory.IndexOf("src", StringComparison.OrdinalIgnoreCase);
        var truncatedPath = currentDirectory.Substring(0, indexOfSrc + "src".Length);
        return truncatedPath;
    }

    private static void Start(string appName, string path)
    {
        if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
        {
            Console.WriteLine($"路径无效: {path}");
            return;
        }

        var runningProcesses = Process.GetProcessesByName(appName);
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
            var appPath = Path.Combine(path, appName);
            var processStartInfo = new ProcessStartInfo
            {
                FileName = appPath,
                UseShellExecute = true,
                CreateNoWindow = false
            };
            Process.Start(processStartInfo);
            Console.WriteLine($"进程已启动, {appPath}.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"启动进程时发生错误: {ex.Message}");
        }
    }
}