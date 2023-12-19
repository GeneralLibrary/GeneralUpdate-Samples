using GeneralUpdate.ClientCore;
using GeneralUpdate.Core.Domain.Entity;
using System.Threading.Tasks;

namespace ClientSample.ViewModels;

public class MainViewModel : ViewModelBase
{
    public string Greeting => "Welcome to Avalonia!";

    public void Upgrade() 
    {
        Task.Run(async () =>
        {
            var url = "http://192.168.50.203";
            var appName = "GeneralUpdate.Client";
            var version = "1.0.0.0";
            var versionFileName = "version.json";
            ParamsOSS @params = new ParamsOSS(url, appName, version, versionFileName);
            await GeneralClientOSS.Start(@params);
        });
    }
}
