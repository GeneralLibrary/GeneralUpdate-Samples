namespace Hub;

/// <summary>
/// 从 appsettings.json 反序列化的全局配置。
/// </summary>
public class AppConfig
{
    public string ServerUrl { get; set; } = "http://localhost:5000";
    public string AppSecretKey { get; set; } = "";
    public string ProductId { get; set; } = "";
    public string HubUrl { get; set; } = "http://localhost:5000/UpgradeHub";
    public string ClientVersion { get; set; } = "1.0.0.0";
    public string UpgradeClientVersion { get; set; } = "1.0.0.0";
    public string MainAppName { get; set; } = "Hub.exe";
    public string UpgradeAppName { get; set; } = "Hub.exe";
}
