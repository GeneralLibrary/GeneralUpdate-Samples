using System.Diagnostics;
using GeneralUpdate.ClientCore.Hubs;

var hub = new UpgradeHubService("http://localhost:5000/UpgradeHub"
    , null,"dfeb5833-975e-4afb-88f1-6278ee9aeff6");
hub.AddListenerReceive((message) =>
{
    Debug.WriteLine(message);
});
await hub.StartAsync();