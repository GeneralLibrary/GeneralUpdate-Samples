namespace Upgrade
{
    public class Localizer : AntdUI.ILocalization
    {
        public string? GetLocalizedString(string key)
        {
            switch (key)
            {
                case "ID":
                    return "en-US";

                case "Cancel":
                    return "Cancel";
                case "OK":
                    return "OK";
                case "Now":
                    return "Now";
                case "ToDay":
                    return "Today";
                case "NoData":
                    return "No data";

                case "title":
                    return "General Update Client";
                case "note":
                    return "Fixed some known issues";
                case "btn":
                    return "Update now";
                case "loading":
                    return "Checking for updates";
                case "newVersion":
                    return "New Version";
                case "download":
                    return "Download updates";
                case "verify":
                    return "Verify file";
                case "updating":
                    return "Updating";
                case "incompleteFile":
                    return "Incomplete file";
                case "downloadFailed":
                    return "Download update failed";
                case "downloadCompleted":
                    return "Update completed";

                default:
                    System.Diagnostics.Debug.WriteLine(key);
                    return null;
            }
        }
    }
}