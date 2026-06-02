namespace Hub.Samples;

/// <summary>
/// 每个组件示例必须实现的接口。
/// Hub/Program.cs 通过反射自动发现所有实现。
/// </summary>
public interface ISample
{
    /// <summary>菜单序号（1-9）</summary>
    int Index { get; }

    /// <summary>菜单显示名称</summary>
    string Name { get; }

    /// <summary>是否需要启动 Server 子进程</summary>
    bool RequiresServer { get; }

    /// <summary>
    /// 流程开始前需清理的路径。
    /// 支持占位符：{BaseDir}=Hub运行目录，%TEMP%=系统临时目录
    /// </summary>
    string[] CleanPaths { get; }

    /// <summary>执行示例流程</summary>
    Task RunAsync(AppConfig config, CancellationToken ct);
}
