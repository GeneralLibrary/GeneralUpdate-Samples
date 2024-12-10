using GeneralUpdate.Differential;

var source = @"D:\packet\app";
var target = @"D:\packet\release";
var patch = @"D:\packet\patch";

await DifferentialCore.Instance?.Clean(source, target, patch);
await DifferentialCore.Instance?.Dirty(source, patch);