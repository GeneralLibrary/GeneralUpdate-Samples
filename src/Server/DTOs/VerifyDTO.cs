namespace ServerSample.DTOs;

public class VerifyDTO
{
    /// <summary>
    /// 版本号
    /// </summary>
    public string? Version { get; set; }

    /// <summary>
    /// 应用类型
    /// </summary>
    public int? AppType { get; set; }

    /// <summary>
    /// 应用密钥
    /// </summary>
    public string? AppKey { get; set; }

    /// <summary>
    /// 系统平台
    /// </summary>
    public int? Platform { get; set; }

    /// <summary>
    /// 所属产品分支id
    /// </summary>
    public string? ProductId { get; set; }
}