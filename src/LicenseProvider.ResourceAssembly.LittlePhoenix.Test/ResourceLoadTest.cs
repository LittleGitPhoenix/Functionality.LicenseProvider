using System;
using System.Linq;
using NUnit.Framework;
using Phoenix.Functionality.LicenseProvider;
using Phoenix.Functionality.LicenseProvider.ResourceAssembly.LittlePhoenix;

namespace LicenseProvider.ResourceAssembly.LittlePhoenix.Test
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
				.DoNotExcludeAssemblies()
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