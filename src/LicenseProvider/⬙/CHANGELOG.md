# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).
___

## 3.0.0

:calendar: _2023-03-02_

|        .NET        |     .NET Standard      |   .NET Framework   |
| :----------------: | :--------------------: | :----------------: |
| :heavy_minus_sign: | :heavy_check_mark: 2.0 | :heavy_minus_sign: |

### Removed

- Dropped direct support for specific framework implementations.
___

## 2.0.0

:calendar: _2022-05-07_

### Added

- Project now natively supports **.NET 6**.

### Changed

- The _.missing.txt_ file will no longer be deleted upon initialization of a `LicenseResolver` but overridden with empty content, since this only requires write permission to the file.

### Fixed

- The _.missing.txt_ file was always created, even if `LicenseResolverConfiguration.LogMissingLicensesToFile` was disabled.

### Removed

- Support for **.Net Framework** was removed.
___

## 1.5.0

:calendar: _2021-11-27_

### Changed

- The default directory where license files will be saved to, is now the applications base directory. Previously this was the working directory. Normally those two are the same, expect if the working directory has been changed manually.
___

## 1.4.0

:calendar: _2021-11-01_

### Added

- Resource assemblies (e.g. _Assembly.resources.dll_) are now matched, even if the `LicenseConfiguration` is configured with `AssemblyNameMatchMode.Exact`.
___

## 1.3.0

:calendar: _2021-09-06_

### Added

- Assemblies starting with **System.** are now always ignored while resolving.

### Fixed

- When using the builder pattern to create a `LicenseResolver` the method `ExcludeAssembly` was ultimately without function so that assemblies couldn't be excluded this way.
___

## 1.2.0

:calendar: _2021-08-30_

### Added

- Assemblies can now be excluded from being processed by the license resolver.

### Changed

- Matching assembly names with configured licenses is now always case **in**sensitive.
- The **.missing.txt** file is no longer appended but overridden each time.
___

## 1.1.0

:calendar: _2020-12-23_

### Added

- Now also targeting **.NET 5.0** and **.NET Framework 4.5**.
- Added new option for assembly matching: `AssemblyNameMatchMode.RegEx`
- Renamed `LicenseConfiguration.AssemblyName` to `LicenseConfiguration.AssemblyIdentifier` so it won't be confused with real assembly names and to better clarify that this is used for matching against actual assembly names.
- Added new option `LogMissingLicensesToFile` to `LicenseResolverConfiguration`  for logging names of assemblies for which no license could be found to a separate file. This option is also available for builder pattern.

### Changed

- The project now fully uses [nullable reference types](https://docs.microsoft.com/en-us/dotnet/csharp/nullable-references).
- Both methods `LicenseResolver.Start` and `LicenseResolver.Stop` now return the instance upon which they were called. This way those methods can be used in fluent creation.
___

## 1.0.1

:calendar: _2020-01-11_

### Fixed

- Matching assembly version failed because the comparison was in the wrong order.
___

## 1.0.0

:calendar: _2020-01-03_

- Initial release