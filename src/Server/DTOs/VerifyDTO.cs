namespace ServerSample.DTOs;

public class VerifyDTO
{
    /// <summary>
    /// 客户端当前版本号
    /// </summary>
    public string? Version { get; set; }

    /// <summary>
    /// 应用类型: 1=Client(主应用), 2=Upgrade(升级器)
    /// </summary>
    public int? AppType { get; set; }

    /// <summary>
    /// 应用密钥，用于服务端验证
    /// </summary>
    public string? AppKey { get; set; }

    /// <summary>
    /// 系统平台: 1=Windows, 2=Linux, 3=macOS
    /// </summary>
    public int? Platform { get; set; }

    /// <summary>
    /// 所属产品分支 ID
    /// </summary>
    public string? ProductId { get; set; }

    /// <summary>
    /// 升级模式: 1=VersionChain(逐版本链式升级), 2=CrossVersion(跨版本升级)
    /// </summary>
    public int? UpgradeMode { get; set; }
}