using System.Text;
using GeneralUpdate.ClientCore;
using GeneralUpdate.Common.Shared.Object;

var paramsOSS = new GlobalConfigInfoOSS();
paramsOSS.Url = "http://192.168.50.203/versions.json";
paramsOSS.CurrentVersion = "1.0.0.0";
paramsOSS.VersionFileName = "versions.json";
paramsOSS.AppName = "GeneralUpdate.Client.exe";
paramsOSS.Encoding = Encoding.UTF8.WebName;
GeneralClientOSS.Start(paramsOSS);