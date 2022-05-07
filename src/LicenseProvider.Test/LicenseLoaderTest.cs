using AutoFixture;
using AutoFixture.AutoMoq;
using LicenseProvider.Test.Resource.Valid;
using NUnit.Framework;
using Phoenix.Functionality.LicenseProvider;

namespace LicenseProvider.Test;

public class LicenseLoaderTest
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
	public void Check_LoadAllLicenseConfigurationsFromAssembly()
	{
		var resourceAssembly = typeof(Reference).Assembly;
		var resources = LicenseLoader.LoadLicenseConfigurationsFromAssemblies(resourceAssembly);
		Assert.IsNotNull(resources);

		var resource = resources.SingleOrDefault();
		Assert.IsNotNull(resource);
		Assert.AreEqual(resourceAssembly.GetName().Name, resource.AssemblyIdentifier);
		Assert.AreEqual(AssemblyNameMatchMode.StartsWith, resource.NameMatchMode);

		var licenseProviders = resource.LicenseProviders;
		Assert.AreEqual(2, licenseProviders.Count);

		var licenseProvider = licenseProviders.First();
		Assert.AreEqual(new Version(2, 0, 0), licenseProvider.Version);
		Assert.AreEqual("v2.0.0.txt", licenseProvider.FileName);
		var license = licenseProvider.GetLicenseText();
		Assert.AreEqual("Some renewed license text...\r\nWith an additional line.", license);

		licenseProvider = licenseProviders.Last();
		Assert.AreEqual(new Version(1, 0, 0), licenseProvider.Version);
		Assert.AreEqual("v1.0.0.txt", licenseProvider.FileName);
		license = licenseProvider.GetLicenseText();
		Assert.AreEqual("Some license text...", license);
	}

	#endregion
}