using System;
using System.Linq;
using NUnit.Framework;
using Phoenix.Functionality.LicenseProvider;
using Phoenix.Functionality.LicenseProvider.ResourceAssembly.Common;

namespace LicenseProvider.ResourceAssembly.Phoenix.Test
{
	public class ResourceLoadTest
	{
		[Test]
		public void Loading_All_Resources_Succeeds()
		{
			// Arrange
			var targetCount = LicenseLoader.GetAllXmlLicenseResources(typeof(Anchor).Assembly).Count();
			var licenseResolver = LicenseResolver
				.Construct()
				.AddCurrentAssembly()
				.AddAssemblyFromReference<Anchor>()
				.WithDefaultOutputDirectory()
				.DoNotLogMissingLicensesToFile()
				.Build()
				;

			// Act
			var licenseCount = licenseResolver
				.LicenseConfigurations
				.Count(configuration => configuration.AssemblyIdentifier.ToLower() != "Phoenix.Functionality.LicenseProvider".ToLower()) //! Remove the already embedded license of the resolver itself.
				;

			// Assert
			Assert.That(licenseCount, Is.AtLeast(1));
			Assert.AreEqual(targetCount, licenseCount);
		}
	}
}