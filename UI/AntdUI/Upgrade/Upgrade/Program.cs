namespace Upgrade
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            //AntdUI.Localization.SetLanguage("en-US");
            AntdUI.Config.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            var lang = AntdUI.Localization.CurrentLanguage;
            if (lang.StartsWith("en")) AntdUI.Localization.Provider = new Localizer();
            Application.Run(new Main());
        }
    }
}