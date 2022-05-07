using AutoFixture;
using AutoFixture.AutoMoq;
using System.Reflection;
using NUnit.Framework;
using Phoenix.Functionality.LicenseProvider;

namespace LicenseProvider.Test;

public class LicenseConfigurationTest
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
	public void Create_Succeeds()
	{
		// Arrange
		var xmlString = @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes"" ?>
				<information
					assemblyIdentifier=""Something""
					matchMode=""Exact""
				>
					<license version=""0.0.0"" fileName=""Something.txt"">
					</license>
				</information>";
		var xml = new ResourceXmlDocument(Assembly.GetExecutingAssembly(), Guid.NewGuid().ToString(), xmlString);
			
		// Act
		var licenseConfiguration = LicenseConfiguration.CreateFromXml(xml);
			
		// Assert
		Assert.IsNotNull(licenseConfiguration);
		Assert.AreEqual("Something", licenseConfiguration.AssemblyIdentifier);
		Assert.AreEqual(AssemblyNameMatchMode.Exact, licenseConfiguration.NameMatchMode);
		Assert.AreEqual(1, licenseConfiguration.LicenseProviders.Count);
	}

	[Test]
	public void Create_Succeeds_With_Old_AssemblyName()
	{
		// Arrange
		var xmlString = @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes"" ?>
				<information
					assemblyName=""Something""
					matchMode=""Exact""
				>
					<license version=""0.0.0"" fileName=""Something.txt"">
					</license>
				</information>";
		var xml = new ResourceXmlDocument(Assembly.GetExecutingAssembly(), Guid.NewGuid().ToString(), xmlString);

		// Act
		var licenseConfiguration = LicenseConfiguration.CreateFromXml(xml);

		// Assert
		Assert.IsNotNull(licenseConfiguration);
		Assert.AreEqual("Something", licenseConfiguration.AssemblyIdentifier);
		Assert.AreEqual(AssemblyNameMatchMode.Exact, licenseConfiguration.NameMatchMode);
		Assert.AreEqual(1, licenseConfiguration.LicenseProviders.Count);
	}

	[Test]
	[TestCase(AssemblyNameMatchMode.Exact)]
	[TestCase(AssemblyNameMatchMode.StartsWith)]
	[TestCase(AssemblyNameMatchMode.EndsWith)]
	[TestCase(AssemblyNameMatchMode.RegEx)]
	public void Create_Succeeds_Different_Match_Modes(AssemblyNameMatchMode targetMatchMode)
	{
		// Arrange
		var xmlString = $@"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes"" ?>
				<information
					assemblyName=""Something""
					matchMode=""{targetMatchMode}""
				>
					<license version=""0.0.0"" fileName=""Something.txt"">
					</license>
				</information>";
		var xml = new ResourceXmlDocument(Assembly.GetExecutingAssembly(), Guid.NewGuid().ToString(), xmlString);

		// Act
		var licenseConfiguration = LicenseConfiguration.CreateFromXml(xml);

		// Assert
		Assert.That(licenseConfiguration.NameMatchMode, Is.EqualTo(targetMatchMode));
	}

	[Test]
	public void Create_From_Invalid_XML_Fails()
	{
		// Arrange
		var xmlString = @"NOT AN XML FILE";
		var xml = new ResourceXmlDocument(Assembly.GetExecutingAssembly(), Guid.NewGuid().ToString(), xmlString);

		// Act
		var licenseConfiguration = LicenseConfiguration.CreateFromXml(xml);

		// Assert
		Assert.IsNull(licenseConfiguration);
	}
		
	[Test]
	public void Create_Without_Name_Fails()
	{
		// Arrange
		var xmlString = @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes"" ?>
				<information
					assemblyIdentifier=""""
					matchMode=""Exact""
				>
					<license version=""0.0.0"" fileName=""Something.txt"">
					</license>
				</information>";
		var xml = new ResourceXmlDocument(Assembly.GetExecutingAssembly(), Guid.NewGuid().ToString(), xmlString);

		// Act
		var licenseConfiguration = LicenseConfiguration.CreateFromXml(xml);

		// Assert
		Assert.IsNull(licenseConfiguration);
	}

	[Test]
	public void Missing_MatchMode_is_Substituted()
	{
		// Arrange
		var xmlString = @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes"" ?>
				<information
					assemblyIdentifier=""Something""
				>
					<license version=""0.0.0"" fileName=""Something.txt"">
					</license>
				</information>";
		var xml = new ResourceXmlDocument(Assembly.GetExecutingAssembly(), Guid.NewGuid().ToString(), xmlString);

		// Act
		var licenseConfiguration = LicenseConfiguration.CreateFromXml(xml);

		// Assert
		Assert.IsNotNull(licenseConfiguration);
		Assert.AreEqual(AssemblyNameMatchMode.Exact, licenseConfiguration.NameMatchMode);
	}

	[Test]
	public void Wrong_MatchMode_is_Substituted()
	{
		// Arrange
		var xmlString = @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes"" ?>
				<information
					assemblyIdentifier=""Something""
					matchMode=""Wrong""
				>
					<license version=""0.0.0"" fileName=""Something.txt"">
					</license>
				</information>";
		var xml = new ResourceXmlDocument(Assembly.GetExecutingAssembly(), Guid.NewGuid().ToString(), xmlString);

		// Act
		var licenseConfiguration = LicenseConfiguration.CreateFromXml(xml);

		// Assert
		Assert.IsNotNull(licenseConfiguration);
		Assert.AreEqual(AssemblyNameMatchMode.Exact, licenseConfiguration.NameMatchMode);
	}

	[Test]
	public void Create_Without_Licenses_Fails()
	{
		// Arrange
		var xmlString = @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes"" ?>
				<information
					assemblyIdentifier=""Something""
					matchMode=""Exact""
				>
				</information>";
		var xml = new ResourceXmlDocument(Assembly.GetExecutingAssembly(), Guid.NewGuid().ToString(), xmlString);

		// Act
		var licenseConfiguration = LicenseConfiguration.CreateFromXml(xml);

		// Assert
		Assert.IsNull(licenseConfiguration);
	}

	[Test]
	public void Duplicate_Licenses_Are_Ignored()
	{
		// Arrange
		var xmlString = @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes"" ?>
				<information
					assemblyIdentifier=""Something""
					matchMode=""Exact""
				>
					<license version=""0.0.0"" fileName=""Something.txt"">
					</license>
					<license version=""0.0.0"" fileName=""Other.txt"">
					</license>
				</information>";
		var xml = new ResourceXmlDocument(Assembly.GetExecutingAssembly(), Guid.NewGuid().ToString(), xmlString);

		// Act
		var licenseConfiguration = LicenseConfiguration.CreateFromXml(xml);

		// Assert
		Assert.IsNotNull(licenseConfiguration);
		Assert.AreEqual(1, licenseConfiguration.LicenseProviders.Count);
		Assert.AreEqual("Something.txt", licenseConfiguration.LicenseProviders.First().FileName); // Only the first entry should be used.
	}

	[Test]
	public void Missing_FileName_is_Substituted()
	{
		// Arrange
		var xmlString = @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes"" ?>
				<information
					assemblyIdentifier=""Assembly""
					matchMode=""Exact""
				>
					<license version=""1.2.3"">
					</license>
				</information>";
		var xml = new ResourceXmlDocument(Assembly.GetExecutingAssembly(), Guid.NewGuid().ToString(), xmlString);

		// Act
		var licenseConfiguration = LicenseConfiguration.CreateFromXml(xml);

		// Assert
		Assert.IsNotNull(licenseConfiguration);
		Assert.AreEqual(1, licenseConfiguration.LicenseProviders.Count);
		Assert.AreEqual("Assembly_v1.2.3.txt", licenseConfiguration.LicenseProviders.First().FileName); // Only the first entry should be used.
	}

	#endregion
}