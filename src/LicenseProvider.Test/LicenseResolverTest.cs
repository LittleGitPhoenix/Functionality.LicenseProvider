using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using Phoenix.Functionality.LicenseProvider;

namespace LicenseProvider.Test
{
	public class LicenseResolverTest
	{
		[Test]
		public void GetLicenseProvider_Succeeds()
		{
			// Arrange
			var targetAssemblyName = Guid.NewGuid().ToString();
			var targetAssemblyVersion = new Version(1, 2, 3);
			var targetAssemblyFileName = $"{Guid.NewGuid().ToString()}.txt";

			var licenseProviders = new []
			{
				new Phoenix.Functionality.LicenseProvider.LicenseProvider(targetAssemblyVersion, targetAssemblyFileName, Assembly.GetExecutingAssembly(), targetAssemblyName, 1),
			};
			var licenseConfiguration = new LicenseConfiguration(targetAssemblyName, AssemblyNameMatchMode.Exact, licenseProviders);

			// Act
			var success = LicenseResolver.TryGetLicenseProvider(targetAssemblyName, targetAssemblyVersion, new List<LicenseConfiguration>() { licenseConfiguration }, out var licenseProvider);
			
			// Assert
			Assert.IsTrue(success);
			Assert.IsNotNull(licenseProvider);
			Assert.AreEqual(targetAssemblyVersion, licenseProvider.Version);
			Assert.AreEqual(targetAssemblyFileName, licenseProvider.FileName);
		}

		[Test]
		[TestCase("ExactAssembly", @"ExactAssembly", AssemblyNameMatchMode.Exact, true)]
		[TestCase("ExactAssembly.Base", @"ExactAssembly", AssemblyNameMatchMode.Exact, false)]
		[TestCase("Start.Assembly", @"Start.Assembly", AssemblyNameMatchMode.StartsWith, true)]
		[TestCase("Start.Assembly.Base", @"Start.Assembly", AssemblyNameMatchMode.StartsWith, true)]
		[TestCase("My.Start.ExactAssembly.Base", @"Start.Assembly", AssemblyNameMatchMode.StartsWith, false)]
		[TestCase("Assembly01.Base", @".Base", AssemblyNameMatchMode.EndsWith, true)]
		[TestCase("Assembly02.Base", @".Base", AssemblyNameMatchMode.EndsWith, true)]
		[TestCase("Assembly02.Base.Other", @".Base", AssemblyNameMatchMode.EndsWith, false)]
		[TestCase("Assembly_With_#04", @"with.*\d", AssemblyNameMatchMode.RegEx, true)]
		[TestCase("Assembly_Without_Number", @"with.*\d", AssemblyNameMatchMode.RegEx, false)]
		public void GetLicenseProvider_Succeeds(string assemblyName, string assemblyIdentifier, AssemblyNameMatchMode matchMode, bool result)
		{
			// Arrange
			var targetAssemblyVersion = new Version(1, 2, 3);
			var targetAssemblyFileName = $"{Guid.NewGuid().ToString()}.txt";

			var licenseProviders = new[]
			{
				new Phoenix.Functionality.LicenseProvider.LicenseProvider(targetAssemblyVersion, targetAssemblyFileName, Assembly.GetExecutingAssembly(), assemblyName, 1),
			};
			var licenseConfiguration = new LicenseConfiguration(assemblyIdentifier, matchMode, licenseProviders);

			// Act
			var success = LicenseResolver.TryGetLicenseProvider(assemblyName, targetAssemblyVersion, new List<LicenseConfiguration>() { licenseConfiguration }, out var licenseProvider);

			// Assert
			Assert.That(success, Is.EqualTo(result));
		}

		[Test]
		public void GetLicenseProvider_Fails_Version_Mismatch()
		{
			// Arrange
			var targetAssemblyName = Guid.NewGuid().ToString();
			var targetAssemblyVersion = new Version(2, 0, 0);
			var targetAssemblyFileName = $"{Guid.NewGuid().ToString()}.txt";

			var licenseProviders = new []
			{
				new Phoenix.Functionality.LicenseProvider.LicenseProvider(targetAssemblyVersion, targetAssemblyFileName, Assembly.GetExecutingAssembly(), targetAssemblyName, 1),
			};
			var licenseConfiguration = new LicenseConfiguration(targetAssemblyName, AssemblyNameMatchMode.Exact, licenseProviders);

			// Act
			var success = LicenseResolver.TryGetLicenseProvider(targetAssemblyName, new Version(1, 0, 0), new List<LicenseConfiguration>() { licenseConfiguration }, out var licenseProvider);

			// Assert
			Assert.IsFalse(success);
			Assert.IsNull(licenseProvider);
		}

		[Test]
		public void Check_LicenseResolver_Construction_Succeeds()
		{
			// Act
			var resolver = LicenseResolver
				.Construct()
				.AddCurrentAssembly()
				.WithDefaultOutputDirectory()
				.DoNotLogMissingLicensesToFile()
				.Build()
				;

			// Assert
			Assert.IsNotNull(resolver);
		}

		[Test]
		public void Check_LicenseResolver_Includes_Its_Own_License()
		{
			// Arrange
			var assemblyInformation = Assembly.GetAssembly(typeof(LicenseResolver)).GetName();
			var licenseResolver = new LicenseResolver(new LicenseResolverConfiguration());

			// Act + Assert
			var success = licenseResolver.TryGetLicenseProvider(assemblyInformation, out var licenseProvider);
			Assert.True(success);
			Assert.IsNotNull(licenseProvider);

			// Act + Assert
			var licenseText = licenseProvider.GetLicenseText();
			Assert.IsNotNull(licenseText);
		}
	}
}