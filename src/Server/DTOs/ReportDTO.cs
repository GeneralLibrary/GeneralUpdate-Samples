namespace ServerSample.DTOs;

public record ReportDTO
{
    /// <summary>
    /// 记录id
    /// </summary>
    public int RecordId { get; set; }

    /// <summary>
    /// 更新状态
    /// </summary>
    public int Status { get; set; }

    /// <summary>
    /// 1升级 2推送
    /// </summary>
    public int Type { get; set; }
}