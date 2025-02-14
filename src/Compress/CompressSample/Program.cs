using System.Diagnostics;
using System.Text;
using GeneralUpdate.Common.Compress;
using GeneralUpdate.Common.Shared.Object;

var source = @"D:\packet\release";
var target = @"D:\packet\update.zip";
//CompressProvider.Compress(Format.ZIP,source,target, false, Encoding.UTF8);
//CompressProvider.Decompress(Format.ZIP,target,source, Encoding.UTF8);
Decompress(source, target);
Console.WriteLine($"Done {File.Exists(target)}");

void Decompress(string source, string target)
{
    try
    {
        //var zipFilePath = Path.Combine(_appPath, $"{version.PacketName}{Format.ZIP}");
        CompressProvider.Decompress(Format.ZIP, target, source, Encoding.UTF8);

        if (!File.Exists(target)) return;
        File.SetAttributes(target, FileAttributes.Normal);
        File.Delete(target);
    }
    catch (Exception e)
    {
        Debug.WriteLine(e.Message);
    }
}