# Phoenix.Functionality.LicenseProvider

| .NET Framework | .NET Standard | .NET Core |
| :-: | :-: | :-: |
| :heavy_minus_sign: | :heavy_check_mark: 2.0 | :heavy_check_mark: 2.0 |

This project aims at providing license information for assemblies referenced by applications, services or even other assemblies.
___

# General Information

When writing software, it is almost always necessary to make different license agreements from the work of other companies or single programmers, whose work is used in some fashion from the software being written, available.

Since it is easy to forget this, the **_Phoenix.Functionality.LicenseProvider_** has been created.

# Concept

The **_Phoenix.Functionality.LicenseProvider_** provides licenses "on the fly" while the application is running. This is done by monitoring and matching the list of all currently loaded assemblies against a configurable collection of **_LicenseConfiguration_**'s. If an assembly matches one of those configurations, the appropriate license file will be written to the applications working or to a configurable other directory.

The idea is, that, instead of manually adding and updating license for all of the many projects typical software consists of, just a single [**Resource Assembly**](#Resource-Assemblies) containing those license files should be created and later on maintained.

# Usage

> The **Usage** part is **mandatory**.
>
> It should describe how to use the code within a repository. Use explanations, code snippets and whatever is needed to give another developer a good starting point.
>
> Please update the README, if you notice, that something is unclear or missing. Keeping things up-to-date will help us all in writing better code faster.

## Set-Up

First step would be to add the **_Phoenix.Functionality.LicenseProvider_** **NuGet** package to your main project and then creating a new instance of the **_LicenseResolver_** class. This class just needs to know is which [**Resource Assemblies**](#Resource-Assemblies) to analyse for license information and optionally where to safe the licenses files to. Both things are supplied via the **_LicenseResolverConfiguration_** class.

``` csharp
var licensesDirectory = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), ".licenses")); // This is also the default path and could be omitted.
var resourceAssembly = typeof(SomeTypeFromTheAssembly).Assembly;

var configuration = new LicenseResolverConfiguration(licensesDirectory, resourceAssembly);
var licenseResolver = new LicenseResolver(configuration);
```

If you prefer builder pattern, here you go:

``` csharp
var licenseResolver = LicenseResolver
        .Construct()
        .AddAssemblyFromReference<SomeTypeFromTheAssembly>()
        .WithDefaultOutputDirectory()
        .Build()
        ;
```

Finally just call **_Start_** on the newly created instance and licenses will be provided for on all loaded assemblies.

``` csharp
licenseResolver.Start();
```

If you don't need the license provider to do its work anymore, you can call its **_Stop_** method.

``` csharp
licenseResolver.Stop();
```

In general the **_LicenseResolver_** should be created and started early on in your application and never be stopped other when the application itself terminates.

## Matching Assemblies to

## Resource Assemblies

A **Resource Assembly** in the world of the license provider is just a rather normal assembly that contains license information as embedded resource. This license information consists of **XML** files with the following structure.

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
> The above example is based on a configuration for **Autofac**.

> Provide as many such files as you need for all of the licensed software you are using.

This **XML** contains all information needed for matching loaded assemblies and providing the license files.

- **assemblyName**

This is the name of the assembly for which license information should be provided.

- **matchMode**

This defines how the name of a loaded assembly is matched against the above **assemblyName**. Its values must equal those of the **_AssemblyNameMatchMode_** enumeration. Possible values are:

|||
|-|-|
|**Exact**|The name of the assembly must match the **assemblyName** ignoring case.|
|Contains|The name of the assembly must contain with **assemblyName**.|
|StartsWith|The name of the assembly must begin with **assemblyName**.|
|EndsWith|The name of the assembly must end with **assemblyName**.|

- **license** (separat xml node)

  - **version**

  The version specifies from which version of the loaded assembly this license node is valid. If the license of some referenced assembly changes with a new version of it, simply add another **license** node with this version. This attribute can be omitted, in which case the version would be **0.0.0**.
  
  - **fileName**

  The name of the file where the license will be saved to. If this is omitted, then the **assemblyName** suffixed by **version** with file extension **.txt** will be used instead.
  
  - **node value**

  This node value is the license text itself.

# Authors

* **Felix Leistner** - _Initial release_