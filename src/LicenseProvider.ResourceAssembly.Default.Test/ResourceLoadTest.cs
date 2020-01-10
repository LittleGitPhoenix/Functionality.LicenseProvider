using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Phoenix.Functionality.LicenseProvider;
using Phoenix.Functionality.LicenseProvider.ResourceAssembly.Default;

namespace LicenseProvider.ResourceAssembly.Phoenix.Test
{
	[TestClass]
	public class ResourceLoadTest
	{
		[TestMethod]
		public void Loading_All_Resources_Succeeds()
		{
			var target = LicenseLoader.GetAllXmlLicenseResources(typeof(PhoenixLicenses).Assembly).Count();

			var licenseResolver = LicenseResolver
				.Construct()
				.AddCurrentAssembly()
				.AddAssemblyFromReference<PhoenixLicenses>()
				.WithDefaultOutputDirectory()
				.Build()
				;

			Assert.AreEqual(target, licenseResolver.LicenseConfigurations.Count);
		}
	}
}