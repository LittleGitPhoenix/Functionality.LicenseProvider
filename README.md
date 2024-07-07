# Phoenix.Functionality.LicenseProvider

|        .NET        |     .NET Standard      |   .NET Framework   |
| :----------------: | :--------------------: | :----------------: |
| :heavy_minus_sign: | :heavy_check_mark: 2.0 | :heavy_minus_sign: |

This project aims at providing license information for assemblies referenced by applications, services or even other assemblies.
___

# Table of content

[toc]

___

# General Information

When writing software, it is almost always necessary to make different license agreements from the work of other companies or single programmers, whose work is used in some fashion from the software being written, available.

Since it is easy to forget this, the ***Phoenix.Functionality.LicenseProvider*** has been created.

___

# Concept

The ***Phoenix.Functionality.LicenseProvider*** provides licenses "on the fly" while the application is running. This is done by monitoring and matching the list of all currently loaded assemblies against a configurable collection of `LicenseConfiguration`s. If an assembly matches one of those configurations, the appropriate license file will be written to the applications working or to a configurable other directory.

The idea is, that, instead of manually adding and updating licenses for all of the many projects software typical consists of, just a single [**Resource Assembly**](#Resource-Assemblies) containing those license files should be created and maintained.

___

# Usage

## Set-Up

First step would be to add the ***Phoenix.Functionality.LicenseProvider*** **NuGet** package to your main project and then creating a new instance of the `LicenseResolver` class. This class just needs to know which [resource assemblies](#Resource-Assemblies) to analyze for [license information](#License-Information) and optionally where to safe the license files to. Both things are supplied via the [`LicenseResolverConfiguration`](#LicenseResolverConfiguration) class.

``` csharp
// Below defined path is also the default path and could be omitted.
var licensesDirectory = new DirectoryInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".licenses"));
var resourceAssembly = typeof(SomeTypeFromTheAssembly).Assembly;

var configuration = new LicenseResolverConfiguration(licenseDirectory, resourceAssembly)
{
	LogMissingLicensesToFile = _logMissing
};
var licenseResolver = new LicenseResolver(configuration);
```
Finally just call `Start` on the newly created instance and licenses will be provided for all loaded assemblies.

``` csharp
licenseResolver.Start();
```

If you don't need the license provider to do its work anymore, you can call its **_Stop_** method.

``` csharp
licenseResolver.Stop();
```

If you prefer builder pattern, here you go:

``` csharp
var licenseResolver = LicenseResolver
	.Construct()
	.AddCurrentAssembly()
	.AddAssemblyFromReference<SomeTypeFromTheAssembly>()
	.WithDefaultOutputDirectory()
	.DoNotExcludeAssemblies()
	.DoNotLogMissingLicensesToFile()
	.Build()
	.Start()
	;
```

In general the `LicenseResolver` should be created and started early on in your application and never be stopped except when the application itself terminates.

## LicenseResolverConfiguration

The `LicenseResolverConfiguration` contains configuration information to be used by a `LicenseResolver`. This are the options:

### `LicenseDirectory`

The directory where the licenses will be saved to. Default is *.licenses* in the executing applications directory.

### `ResourceAssemblies`

A collection of [assemblies](#Resource-Assemblies) that will be searched for embedded xml license files.

### `ExcludedAssemblies`

  A collection of `ExcludedLicenseConfiguration` containing information about assemblies that will be ignored by the `LicenseResolver`.

> [!Tip]
> All excluded assemblies will be completely overstepped when resolving licenses.

> [!Note]
> By default all dynamic assemblies and some common assemblies listed below are ignored.

| Identifier | Match mode |
| :- | :- |
| system | `AssemblyNameMatchMode.StartsWith` |
| microsoft.net | `AssemblyNameMatchMode.Contains` |

### `LogMissingLicensesToFile`

Should the name of assemblies for which no license could be resolved be written to the file _.missing.txt_ in the same directory where license are saved.

## Resource Assemblies

A **Resource Assembly** in the world of the license provider is just a rather normal assembly that contains [license information](#License-Information) as embedded resource. The easiest way to provide such an assembly to the `LicenseResolver` is by adding a single accessible type into the resource assembly.

```csharp
/// <summary>
/// Just a class used to obtain a reference to its assembly.
/// </summary>
public sealed class Anchor { }
```

The whole assembly can now be added via this class:

```csharp
var licenseResolver = LicenseResolver
	.Construct()
	.AddCurrentAssembly()
	.AddAssemblyFromReference<Anchor>() // Here is the 'Anchor'.
	.WithDefaultOutputDirectory()
	.DoNotExcludeAssemblies()
	.DoNotLogMissingLicensesToFile()
	.Build()
	;
```
> [!Important]
> Resource assemblies shouldn't reference the `Phoenix.Functionality.LicenseProvider`.

## License Information

License information are simple **XML** files that must have the following structure.

``` xml
<?xml version="1.0" encoding="UTF-8" standalone="yes" ?>
<information
  assemblyName="Autofac"
  matchMode="Exact"
>
  <license version="0.0.0" fileName="Autofac.txt">
The MIT License (MIT)

Copyright (c) 2014 Autofac Project

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
  </license>
</information>
```
> [!Note]
> The above example is a license configuration for **Autofac**.

> [!Note]
> Provide as many such files as are needed for all of the licensed software in play.

This **XML** contains all information needed for matching loaded assemblies and providing the license files.

- **assemblyIdentifier**

This is the identifier of the assembly for which license information should be provided.

- **matchMode**

This defines how the name of a loaded assembly is matched against the above **assemblyIdentifier**. Its values must equal those of the `AssemblyNameMatchMode` enumeration. Possible values are:

|Value|Description|
|-|-|
|Exact|The name of the assembly must match the **assemblyIdentifier** ignoring case. This is the default.|
|Contains|The name of the assembly must contain with **assemblyIdentifier**.|
|StartsWith|The name of the assembly must begin with **assemblyIdentifier**.|
|EndsWith|The name of the assembly must end with **assemblyIdentifier**.|
|RegEx|The name of the assembly must match the **assemblyIdentifier** that should be a regular expression.|

- **license** (separat xml node)

  - **version**

  The version specifies from which version of the loaded assembly this license node is valid. If the license of some referenced assembly changes, simply add another **license** node with the corresponding version. This attribute can be omitted, in which case the version would be **0.0.0**.
  
  - **fileName**

  The name of the file where the license will be saved to. If this is omitted, then the **assemblyIdentifer** suffixed by **version** with file extension **.txt** will be used instead.
  
  - **node value**

  This node value is the license text itself.

___

# Authors

* **Felix Leistner**: _v1.x_ - _v3.x_