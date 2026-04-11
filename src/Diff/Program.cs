using GeneralUpdate.Differential;
using GeneralUpdate.Differential.Matchers;

// Binary differential update sample:
// - Clean: generate a patch from source (old) vs target (new)
// - Dirty: apply the patch to the app directory

var source = @"D:\packet\app";
var target = @"D:\packet\release";
var patch  = @"D:\packet\patch";

// Generate binary differential patch (old → new)
await DifferentialCore.Clean(source, target, patch, new DefaultCleanStrategy(new DefaultCleanMatcher()));

// Apply the patch to the running app directory
await DifferentialCore.Dirty(source, patch, new DefaultDirtyStrategy(new DefaultDirtyMatcher()));