using GeneralUpdate.Bowl;
using GeneralUpdate.Bowl.Strategys;

/*
 * Bowl is used to monitor your client program. If it detects that the client process has crashed, it will help you export the relevant information to the "fail" directory.
 *
 * ref: https://www.bilibili.com/video/BV1c8iyYZE7P?spm_id_from=333.788.videopod.episodes&vd_source=5b84edd8fee7635c1c999c7962c04602&p=7
 */
var installPath = AppDomain.CurrentDomain.BaseDirectory;
var lastVersion = "1.0.0.3";
var processInfo = new MonitorParameter
{
    ProcessNameOrId = "ThrowApp.exe",
    DumpFileName = $"{lastVersion}_fail.dmp",
    FailFileName = $"{lastVersion}_fail.json",
    TargetPath = installPath,
    FailDirectory = Path.Combine(installPath, "fail", lastVersion),
    BackupDirectory = Path.Combine(installPath, lastVersion),
    WorkModel = "Normal"
};
Bowl.Launch(processInfo);