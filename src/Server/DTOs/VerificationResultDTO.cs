namespace ServerSample.DTOs;

public record VerificationResultDTO
{
    /// <summary>
    /// 记录 ID，用于更新结果上报
    /// </summary>
    public int RecordId { get; set; }

    /// <summary>
    /// 包名称
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// 补丁包 SHA256 哈希值
    /// </summary>
    public string? Hash { get; set; }

    /// <summary>
    /// 发布日期
    /// </summary>
    public DateTime? ReleaseDate { get; set; }

    /// <summary>
    /// 下载地址
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    /// 签名 URL 过期时间 (UTC)
    /// </summary>
    public DateTime? UrlExpireTimeUtc { get; set; }

    /// <summary>
    /// 版本号
    /// </summary>
    public string? Version { get; set; }

    /// <summary>
    /// 应用类型: 1=Client(主应用), 2=Upgrade(升级器)
    /// </summary>
    public int? AppType { get; set; }

    /// <summary>
    /// 系统平台: 1=Windows, 2=Linux, 3=macOS
    /// </summary>
    public int? Platform { get; set; }

    /// <summary>
    /// 产品 ID
    /// </summary>
    public string? ProductId { get; set; }

    /// <summary>
    /// 是否强制更新
    /// </summary>
    public bool? IsForcibly { get; set; }

    /// <summary>
    /// 文件格式（如 .zip）
    /// </summary>
    public string? Format { get; set; }

    /// <summary>
    /// 文件大小（字节）
    /// </summary>
    public long? Size { get; set; }

    /// <summary>
    /// 是否冻结（冻结的包不参与更新）
    /// </summary>
    public bool? IsFreeze { get; set; }

    /// <summary>
    /// 包类型: 0=Unspecified, 1=Chain(差分), 2=Full(完整包), 3=Driver
    /// </summary>
    public int PackageType { get; set; }
}