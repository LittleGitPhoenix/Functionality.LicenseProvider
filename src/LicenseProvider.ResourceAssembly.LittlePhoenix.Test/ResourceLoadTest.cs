using AutoFixture;
using AutoFixture.AutoMoq;
using NUnit.Framework;
using Phoenix.Functionality.LicenseProvider;
using Phoenix.Functionality.LicenseProvider.ResourceAssembly.LittlePhoenix;

namespace LicenseProvider.ResourceAssembly.LittlePhoenix.Test;

public class ResourceLoadTest
{
	#region Setup

#pragma warning disable 8618 // → Always initialized in the 'Setup' method before a test is run.
	private IFixture _fixture;
#pragma warning restore 8618

	[OneTimeSetUp]
	public void BeforeAllTests() { }

	[SetUp]
	public void BeforeEachTest()
	{
		_fixture = new Fixture().Customize(new AutoMoqCustomization());
	}

	[TearDown]
	public void AfterEachTest() { }

	[OneTimeTearDown]
	public void AfterAllTests() { }

	#endregion

	#region Tests

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

	#endregion
}