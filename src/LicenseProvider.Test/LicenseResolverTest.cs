using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LicenseProvider.Test.Resource.Valid;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Phoenix.Functionality.LicenseProvider;

namespace LicenseProvider.Test {
	[TestClass]
	public class LicenseResolverTest
	{
		[TestMethod]
		public void GetLicenseProvider_Succeeds()
		{
			var targetAssemblyName = Guid.NewGuid().ToString();
			var targetAssemblyVersion = new Version(1, 2, 3);
			var targetAssemblyFileName = $"{Guid.NewGuid().ToString()}.txt";

			var licenseProviders = new List<Phoenix.Functionality.LicenseProvider.LicenseProvider>()
			{
				new Phoenix.Functionality.LicenseProvider.LicenseProvider(targetAssemblyVersion, targetAssemblyFileName, Assembly.GetExecutingAssembly(), targetAssemblyName, 1),
			};
			var licenseConfiguration = new LicenseConfiguration(targetAssemblyName, AssemblyNameMatchMode.Exact, licenseProviders);

			var success = LicenseResolver.TryGetLicenseProvider(targetAssemblyName, targetAssemblyVersion, new List<LicenseConfiguration>() { licenseConfiguration }, out var licenseProvider);
			Assert.IsTrue(success);
			Assert.IsNotNull(licenseProvider);
			Assert.AreEqual(targetAssemblyVersion, licenseProvider.Version);
			Assert.AreEqual(targetAssemblyFileName, licenseProvider.FileName);
		}

		[TestMethod]
		public void GetLicenseProvider_Fails_Version_Mismatch()
		{
			var targetAssemblyName = Guid.NewGuid().ToString();
			var targetAssemblyVersion = new Version(2, 0, 0);
			var targetAssemblyFileName = $"{Guid.NewGuid().ToString()}.txt";

			var licenseProviders = new List<Phoenix.Functionality.LicenseProvider.LicenseProvider>()
			{
				new Phoenix.Functionality.LicenseProvider.LicenseProvider(targetAssemblyVersion, targetAssemblyFileName, Assembly.GetExecutingAssembly(), targetAssemblyName, 1),
			};
			var licenseConfiguration = new LicenseConfiguration(targetAssemblyName, AssemblyNameMatchMode.Exact, licenseProviders);

			var success = LicenseResolver.TryGetLicenseProvider(targetAssemblyName, new Version(1, 0, 0), new List<LicenseConfiguration>() { licenseConfiguration }, out var licenseProvider);
			Assert.IsFalse(success);
			Assert.IsNull(licenseProvider);
		}
	}
}