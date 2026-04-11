using GeneralUpdate.ClientCore.Hubs;

/*
 * PushSample — 演示 UpgradeHubService (SignalR 推送) 的完整用法
 * PushSample — Demonstrates full usage of UpgradeHubService (SignalR push notifications)
 *
 * 使用前提 / Prerequisites:
 *   1. 服务端已运行并注册了 UpgradeHub (MapHub<UpgradeHub>("/UpgradeHub"))
 *      The server is running and has registered UpgradeHub
 *   2. 客户端 appkey 与服务端约定一致
 *      The client appkey matches the server-side agreement
 */

const string HubUrl = "http://localhost:5000/UpgradeHub";
const string AppKey = "dfeb5833-975e-4afb-88f1-6278ee9aeff6";

Console.WriteLine($"=== UpgradeHubService 推送示例 / Push Sample ===");
Console.WriteLine($"连接地址 / Hub URL: {HubUrl}");
Console.WriteLine($"应用密钥 / App Key: {AppKey}");
Console.WriteLine();

// UpgradeHubService 参数说明:
// url    — SignalR Hub 订阅地址
// token  — ID4 认证令牌（可选），例如 Bearer Token
// appkey — 客户端唯一标识，推荐使用 GUID，用于服务端识别推送目标
var hub = new UpgradeHubService(HubUrl, token: null, appkey: AppKey);

// 1. 订阅服务端推送的版本更新消息
//    Subscribe to version update messages pushed by the server
//    message: 服务端推送的内容，建议使用 JSON 格式的 Packet 对象字符串
hub.AddListenerReceive((message) =>
{
    Console.WriteLine($"[ReceiveMessage] 收到更新推送 / Received update push:");
    Console.WriteLine($"  {message}");
});

// 2. 订阅在线 / 离线状态通知
//    Subscribe to online/offline status notifications
hub.AddListenerOnline((info) =>
{
    Console.WriteLine($"[Online] 在线状态变化 / Online status change: {info}");
});

// 3. 订阅重连成功通知
//    Subscribe to reconnection notifications
hub.AddListenerReconnected((connectionId) =>
{
    Console.WriteLine($"[Reconnected] 已重连 / Reconnected, connectionId={connectionId}");
    return Task.CompletedTask;
});

// 4. 订阅连接关闭通知
//    Subscribe to connection closed notifications
hub.AddListenerClosed((exception) =>
{
    if (exception is not null)
        Console.WriteLine($"[Closed] 连接因异常关闭 / Connection closed due to exception: {exception.Message}");
    else
        Console.WriteLine("[Closed] 连接已正常关闭 / Connection closed normally");
    return Task.CompletedTask;
});

try
{
    // 5. 建立 SignalR 连接，启动推送监听
    //    Establish SignalR connection and start listening for push notifications
    Console.WriteLine("正在连接 Hub... / Connecting to Hub...");
    await hub.StartAsync();
    Console.WriteLine("已成功连接 Hub，等待服务端推送... / Connected. Waiting for server push...");
    Console.WriteLine("按 Enter 键断开连接并退出 / Press Enter to disconnect and exit");
    Console.ReadLine();
}
finally
{
    // 6. 先暂停连接（可在应用进入后台时调用）
    //    First stop the connection gracefully (call when the app enters background)
    Console.WriteLine("正在停止连接... / Stopping connection...");
    await hub.StopAsync();

    // 7. 彻底释放 Hub 实例（停止后不可再复用）
    //    Fully dispose the Hub instance (cannot be reused after disposal)
    Console.WriteLine("正在释放资源... / Disposing resources...");
    await hub.DisposeAsync();
    Console.WriteLine("资源已释放 / Resources disposed.");
}
