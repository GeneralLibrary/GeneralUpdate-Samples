using System.Text;
using GeneralUpdate.ClientCore;
using GeneralUpdate.Common.Shared.Object;

var paramsOSS = new GlobalConfigInfoOSS
{
    Url = "http://localhost:5000/packages/versions.json",
    CurrentVersion = "1.0.0.0",
    VersionFileName = "versions.json",
    AppName = "OSSClientSample.exe",
    Encoding = Encoding.UTF8.WebName
};

await GeneralClientOSS.Start(paramsOSS, "OSSUpgradeSample.exe");