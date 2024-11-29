namespace ServerSample.DTOs;

public record VerificationResultDTO
{
    /// <summary>
    /// 用于更新结果上报
    /// </summary>
    public int RecordId { get; set; }
    
    /// <summary>
    /// 包名称
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// 补丁包hash值
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
    /// 版本号
    /// </summary>
    public string? Version { get; set; }

    /// <summary>
    /// 应用类型
    /// </summary>
    public int? AppType { get; set; }

    /// <summary>
    /// 平台
    /// </summary>
    public int? Platform { get; set; }

    /// <summary>
    /// 产品id
    /// </summary>
    public string? ProductId { get; set; }

    /// <summary>
    /// 是否强制更新
    /// </summary>
    public bool? IsForcibly { get; set; }
    
    /// <summary>
    /// 文件格式
    /// </summary>
    public string Format { get; set; }

    /// <summary>
    /// 文件大小
    /// </summary>
    public long? Size { get; set; }
    
    public bool? IsFreeze { get; set; }
}