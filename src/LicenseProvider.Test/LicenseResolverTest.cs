﻿using System.Reflection;
using AutoFixture;
using AutoFixture.AutoMoq;
using Moq;
using NUnit.Framework;
using Phoenix.Functionality.LicenseProvider;

namespace LicenseProvider.Test;

public class LicenseResolverTest
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
	[TestCase("UPPERCASE", @"uppercase", AssemblyNameMatchMode.Contains, true)]
	[TestCase("My.UPPERCASE.Assembly", @"uppercase", AssemblyNameMatchMode.Contains, true)]
	[TestCase("UPPERCASE.Assembly", @"uppercase", AssemblyNameMatchMode.StartsWith, true)]
	[TestCase("My.UPPERCASE", @"uppercase", AssemblyNameMatchMode.EndsWith, true)]
	[TestCase("Assembly.resources", @"Assembly", AssemblyNameMatchMode.Exact, true)]
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
		var success = LicenseResolver.TryGetLicenseProvider(assemblyName, targetAssemblyVersion, new List<LicenseConfiguration>() { licenseConfiguration }, out _);

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
			.DoNotExcludeAssemblies()
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

	[Test]
	public void Check_Not_Excluded_Assemblies_Are_Processed()
	{
		// Arrange
		var assembly = typeof(System.Object).Assembly;
		var licenseResolverMock = _fixture.Create<Mock<LicenseResolver>>();
		licenseResolverMock
			.Setup(mock => mock.HandleAssemblyLicense(It.IsAny<AssemblyName>()))
			.Returns(false)
			;
		var licenseResolver = licenseResolverMock.Object;

		// Act
		licenseResolver.TryAddNewAssembly(assembly);

		// Assert
		licenseResolverMock.Verify(mock => mock.HandleAssemblyLicense(It.IsAny<AssemblyName>()), Times.Once);
	}

	[Test]
	public void Check_Excluded_Assemblies_Are_Completely_Ignored()
	{
		// Arrange
		var assembly = typeof(System.Object).Assembly;
		var licenseResolverConfiguration = new LicenseResolverConfiguration(licenseDirectory: null, excludedAssemblies: new[] { (ExcludedLicenseConfiguration)("System", AssemblyNameMatchMode.StartsWith) });
		_fixture.Inject(licenseResolverConfiguration);
		var licenseResolverMock = _fixture.Create<Mock<LicenseResolver>>();
		licenseResolverMock
			.Setup(mock => mock.HandleAssemblyLicense(It.IsAny<AssemblyName>()))
			.Returns(false)
			;
		var licenseResolver = licenseResolverMock.Object;

		// Act
		licenseResolver.TryAddNewAssembly(assembly);

		// Assert
		licenseResolverMock.Verify(mock => mock.HandleAssemblyLicense(It.IsAny<AssemblyName>()), Times.Never);
	}

	[Test]
	[TestCase(null, null)]
	[TestCase("", "")]
	[TestCase("Assembly.resources", "Assembly")]
	[TestCase("Assembly.Untouched", "Assembly.Untouched")]
	public void Check_Cleaning_Assembly_Name_Succeeds(string assemblyName, string target)
	{
		// Arrange

		// Act
		var actual = LicenseResolver.CleanAssemblyName(assemblyName);

		// Assert
		Assert.AreEqual(target, actual);
	}

	#region Missing Licenses File

	[Test]
	public void Check_Missing_File_Is_Created()
	{
		// Arrange
		var licenseResolverConfiguration = new LicenseResolverConfiguration(){LogMissingLicensesToFile = true};
		_fixture.Inject(licenseResolverConfiguration);
		var licenseResolverMock = _fixture.Create<Mock<LicenseResolver>>();
		licenseResolverMock
			.Setup(mock => mock.CheckIfAssemblyIsExcluded(It.IsAny<Assembly>()))
			.Returns(false)
			;
		licenseResolverMock
			.Setup(mock => mock.HandleAssemblyLicense(It.IsAny<AssemblyName>()))
			.Returns(false)
			;
		licenseResolverMock
			.Setup(mock => mock.TryCreateMissingLicensesFile())
			.CallBase()
			.Verifiable()
			;
		licenseResolverMock
			.Setup(mock => mock.DoesMissingLicensesFileExist(It.IsAny<FileInfo>()))
			.Returns(false)
			.Verifiable()
			;
		licenseResolverMock
			.Setup(mock => mock.ClearMissingLicensesFile(It.IsAny<FileInfo>()))
			.Callback(() => {})
			.Verifiable()
			;
		licenseResolverMock
			.Setup(mock => mock.CreateMissingLicensesFile(It.IsAny<FileInfo>()))
			.Callback(() => { })
			.Verifiable()
			;

		var licenseResolver = licenseResolverMock.Object;

		// Act
		licenseResolver.TryAddNewAssembly(typeof(Object).Assembly);

		// Assert
		licenseResolverMock.Verify(mock => mock.TryCreateMissingLicensesFile(), Times.Once);
		licenseResolverMock.Verify(mock => mock.CreateMissingLicensesFile(It.IsAny<FileInfo>()), Times.Once);
		licenseResolverMock.Verify(mock => mock.ClearMissingLicensesFile(It.IsAny<FileInfo>()), Times.Never);
	}

	[Test]
	public void Check_Missing_File_Is_Created_Only_Once()
	{
		// Arrange
		var licenseResolverConfiguration = new LicenseResolverConfiguration() { LogMissingLicensesToFile = true };
		_fixture.Inject(licenseResolverConfiguration);
		var licenseResolverMock = _fixture.Create<Mock<LicenseResolver>>();
		licenseResolverMock
			.Setup(mock => mock.CheckIfAssemblyIsExcluded(It.IsAny<Assembly>()))
			.Returns(false)
			;
		licenseResolverMock
			.Setup(mock => mock.HandleAssemblyLicense(It.IsAny<AssemblyName>()))
			.Returns(false)
			;
		licenseResolverMock
			.Setup(mock => mock.TryCreateMissingLicensesFile())
			.CallBase()
			.Verifiable()
			;
		licenseResolverMock
			.Setup(mock => mock.DoesMissingLicensesFileExist(It.IsAny<FileInfo>()))
			.Returns(false)
			.Verifiable()
			;
		licenseResolverMock
			.Setup(mock => mock.ClearMissingLicensesFile(It.IsAny<FileInfo>()))
			.Callback(() => { })
			.Verifiable()
			;
		licenseResolverMock
			.Setup(mock => mock.CreateMissingLicensesFile(It.IsAny<FileInfo>()))
			.Callback(() => { })
			.Verifiable()
			;

		var licenseResolver = licenseResolverMock.Object;

		// Act
		licenseResolver.TryAddNewAssembly(typeof(Object).Assembly);
		licenseResolver.TryAddNewAssembly(this.GetType().Assembly);

		// Assert
		licenseResolverMock.Verify(mock => mock.TryCreateMissingLicensesFile(), Times.Exactly(2));
		licenseResolverMock.Verify(mock => mock.CreateMissingLicensesFile(It.IsAny<FileInfo>()), Times.Once);
		licenseResolverMock.Verify(mock => mock.ClearMissingLicensesFile(It.IsAny<FileInfo>()), Times.Never);
	}

	[Test]
	public void Check_Missing_File_Is_Overridden()
	{
		// Arrange
		var licenseResolverConfiguration = new LicenseResolverConfiguration(){LogMissingLicensesToFile = true};
		_fixture.Inject(licenseResolverConfiguration);
		var licenseResolverMock = _fixture.Create<Mock<LicenseResolver>>();
		licenseResolverMock
			.Setup(mock => mock.CheckIfAssemblyIsExcluded(It.IsAny<Assembly>()))
			.Returns(false)
			;
		licenseResolverMock
			.Setup(mock => mock.HandleAssemblyLicense(It.IsAny<AssemblyName>()))
			.Returns(false)
			;
		licenseResolverMock
			.Setup(mock => mock.TryCreateMissingLicensesFile())
			.CallBase()
			.Verifiable()
			;
		licenseResolverMock
			.Setup(mock => mock.DoesMissingLicensesFileExist(It.IsAny<FileInfo>()))
			.Returns(true)
			.Verifiable()
			;
		licenseResolverMock
			.Setup(mock => mock.ClearMissingLicensesFile(It.IsAny<FileInfo>()))
			.Callback(() => {})
			.Verifiable()
			;
		licenseResolverMock
			.Setup(mock => mock.CreateMissingLicensesFile(It.IsAny<FileInfo>()))
			.Callback(() => { })
			.Verifiable()
			;

		var licenseResolver = licenseResolverMock.Object;

		// Act
		licenseResolver.TryAddNewAssembly(typeof(Object).Assembly);

		// Assert
		licenseResolverMock.Verify(mock => mock.TryCreateMissingLicensesFile(), Times.Once);
		licenseResolverMock.Verify(mock => mock.CreateMissingLicensesFile(It.IsAny<FileInfo>()), Times.Never);
		licenseResolverMock.Verify(mock => mock.ClearMissingLicensesFile(It.IsAny<FileInfo>()), Times.Once);
	}

	[Test]
	public void Check_Missing_File_Is_Not_Created()
	{
		// Arrange
		var licenseResolverConfiguration = new LicenseResolverConfiguration(){LogMissingLicensesToFile = false};
		_fixture.Inject(licenseResolverConfiguration);
		var licenseResolverMock = _fixture.Create<Mock<LicenseResolver>>();
		licenseResolverMock
			.Setup(mock => mock.CheckIfAssemblyIsExcluded(It.IsAny<Assembly>()))
			.Returns(false)
			;
		licenseResolverMock
			.Setup(mock => mock.HandleAssemblyLicense(It.IsAny<AssemblyName>()))
			.Returns(false)
			;
		licenseResolverMock
			.Setup(mock => mock.TryCreateMissingLicensesFile())
			.CallBase()
			.Verifiable()
			;
		licenseResolverMock
			.Setup(mock => mock.DoesMissingLicensesFileExist(It.IsAny<FileInfo>()))
			.Returns(false)
			.Verifiable()
			;
		licenseResolverMock
			.Setup(mock => mock.ClearMissingLicensesFile(It.IsAny<FileInfo>()))
			.Callback(() => {})
			.Verifiable()
			;
		licenseResolverMock
			.Setup(mock => mock.CreateMissingLicensesFile(It.IsAny<FileInfo>()))
			.Callback(() => { })
			.Verifiable()
			;

		var licenseResolver = licenseResolverMock.Object;

		// Act
		licenseResolver.TryAddNewAssembly(typeof(Object).Assembly);

		// Assert
		licenseResolverMock.Verify(mock => mock.TryCreateMissingLicensesFile(), Times.Once);
		licenseResolverMock.Verify(mock => mock.CreateMissingLicensesFile(It.IsAny<FileInfo>()), Times.Never);
		licenseResolverMock.Verify(mock => mock.ClearMissingLicensesFile(It.IsAny<FileInfo>()), Times.Never);
	}

	#endregion

	#endregion
}