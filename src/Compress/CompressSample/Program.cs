using System.Text;
using GeneralUpdate.Common.Compress;
using GeneralUpdate.Common.Shared.Object;

var source = @"D:\packet\release";
var target = @"D:\packet\1.zip";
CompressProvider.Compress(Format.ZIP,source,target, false, Encoding.UTF8);
CompressProvider.Decompress(Format.ZIP,target,source, Encoding.UTF8);
Console.WriteLine($"Done {File.Exists(target)}");