# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).
___

## 1.2.0 (2021-08-30)

### Added
- Assemblies can now be excluded from being processed by the license resolver.

### Changed

- Matching assembly names with configured licenses is now always case **in**sensitive.
- The **.missing.txt** file is no longer appended but overridden each time.
___

## 1.1.0 (2020-12-23)

### Added

- Now also targeting **.NET 5.0** and **.NET Framework 4.5**.
- Added new option for assembly matching: `AssemblyNameMatchMode.RegEx`
- Renamed `LicenseConfiguration.AssemblyName` to `LicenseConfiguration.AssemblyIdentifier` so it won't be confused with real assembly names and to better clarify that this is used for matching against actual assembly names.
- Added new option `LogMissingLicensesToFile` to `LicenseResolverConfiguration`  for logging names of assemblies for which no license could be found to a separate file. This option is also available for builder pattern.

### Changed

- The project now fully uses [nullable reference types](https://docs.microsoft.com/en-us/dotnet/csharp/nullable-references).
- Both methods `LicenseResolver.Start` and `LicenseResolver.Stop` now return the instance upon which they were called. This way those methods can be used in fluent creation.
___

## 1.0.1 (2020-01-11)

### Fixed

- Matching assembly version failed because the comparison was in the wrong order.
___

## 1.0.0 (2020-01-03)

- Initial release