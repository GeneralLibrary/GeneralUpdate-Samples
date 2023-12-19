using GeneralUpdate.Maui.OSS.Domain.Entity;
using GeneralUpdate.Maui.OSS;
using GeneralUpdate.Core.Events.CommonArgs;
using GeneralUpdate.Core.Events.OSSArgs;

namespace MauiSample
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            InitializeComponent();
        }

        private void OnCounterClicked(object sender, EventArgs e)
        {
            Task.Run(async () =>
            {
                var url = "http://192.168.50.203";
                var apk = "com.companyname.generalupdate.ossclient.apk";
                var authority = "com.generalupdate.oss.fileprovider";
                var currentVersion = "1.0.0.0";
                var versionFileName = "version.json";
                GeneralUpdateOSS.AddListenerDownloadProcess(OnOSSDownload);
                GeneralUpdateOSS.AddListenerException(OnException);
                await GeneralUpdateOSS.Start<Strategy>(new ParamsAndroid(url, apk, authority, currentVersion, versionFileName));
            });
        }

        private void OnOSSDownload(object sender, OSSDownloadArgs e)
        {
            Console.WriteLine($"{e.ReadLength},{e.TotalLength}");
        }

        private void OnException(object sender, ExceptionEventArgs exception)
        {
            Console.WriteLine(exception.Exception.Message);
        }
    }

}
