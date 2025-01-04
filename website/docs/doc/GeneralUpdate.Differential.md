### Definition

Namespace: GeneralUpdate.Differential

Assembly: GeneralUpdate.Differential.dll



This component provides capabilities for generating binary differential patch files (algorithm), file increment identification (version differences), deleted file identification (version differences), blacklisting, patch restoration, and more.

```c#
public sealed class DifferentialCore
```

NuGet Installation

```shell
NuGet\Install-Package GeneralUpdate.Differential -Version 9.1.0
```

### Example

The following example defines methods for increment identification, generating binary patches, patch restoration, and setting blacklists. The packaging tool in the GeneralUpdate.Tools project also strongly depends on this component.

```c#
// Increment identification, generating binary patches
public async Task TestDifferentialClean()
{
    // Path to the previous version's client folder
    var path1 = "D:\\packet\\source";
    // Path to the latest version's client folder
    var path2 = "D:\\packet\\target";
    // Path for generating patch files
    var path3 = "D:\\packet\\patchs";
    await DifferentialCore.Instance.Clean(path1, path2, path3);
}

// Patch restoration
public async Task TestDifferentialDirty()
{
    // Path to the current version's client folder
    var path1 = "D:\\packet\\source";
    // Path for generating patch files
    var path2 = "D:\\packet\\patchs";
    await DifferentialCore.Instance.Dirty(path1, path2);
}
```

### Annotations

DifferentialCore provides capabilities for increment identification, generating binary patches, patch restoration, and setting blacklists.

#### Methods

| Name    | Type   | Description                                                  |
| ------- | ------ | ------------------------------------------------------------ |
| Clean() | Method | Increment identification, deleted file identification, and generating binary patch files |
| Dirty() | Method | Patch restoration (applying patches to old client files to achieve updates) |

### 🌼Clean()

**Method**

Generate patch files [cannot include files with the same name but different extensions].

```c#
public async Task Clean(string sourcePath, string targetPath, string patchPath = null);
```

**Parameters**

**sourcePath** Path to the previous version's folder.

**targetPath** Path to the latest version's folder.

**patchPath** Directory to store the discovered incremental update files temporarily.

### 🌼Dirty()

**Method**

Apply patches [cannot include files with the same name but different extensions].

```c#
public async Task Dirty(string appPath, string patchPath);
```

**Parameters**

**appPath** Client application directory.

**patchPath** Path to the patch files.

### Applicable to

| Product        | Versions      |
| -------------- | ------------- |
| .NET           | 5, 6, 7, 8, 9 |
| .NET Framework | 4.6.1         |
| .NET Standard  | 2.0           |
| .NET Core      | 2.0           |