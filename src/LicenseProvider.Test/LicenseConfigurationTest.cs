using System;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Phoenix.Functionality.LicenseProvider;

namespace LicenseProvider.Test
{
	[TestClass]
	public class LicenseConfigurationTest
	{
		[TestMethod]
		public void Create_From_Invalid_XML_Fails()
		{
			var xmlString = @"NOT AN XML FILE";
			var xml = new ResourceXmlDocument(Assembly.GetExecutingAssembly(), Guid.NewGuid().ToString(), xmlString);
			var licenseConfiguration = LicenseConfiguration.CreateFromXml(xml);
			Assert.IsNull(licenseConfiguration);
		}
		
		[TestMethod]
		public void Create_Without_Name_Fails()
		{
			var xmlString = @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes"" ?>
				<information
					assemblyName=""""
					matchMode=""Exact""
				>
					<license version=""0.0.0"" fileName=""Something.txt"">
					</license>
				</information>";
			
			var xml = new ResourceXmlDocument(Assembly.GetExecutingAssembly(), Guid.NewGuid().ToString(), xmlString);
			var licenseConfiguration = LicenseConfiguration.CreateFromXml(xml);
			Assert.IsNull(licenseConfiguration);
		}

		[TestMethod]
		public void Missing_MatchMode_is_Substituted()
		{
			var xmlString = @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes"" ?>
				<information
					assemblyName=""Something""
				>
					<license version=""0.0.0"" fileName=""Something.txt"">
					</license>
				</information>";
			
			var xml = new ResourceXmlDocument(Assembly.GetExecutingAssembly(), Guid.NewGuid().ToString(), xmlString);
			var licenseConfiguration = LicenseConfiguration.CreateFromXml(xml);
			Assert.IsNotNull(licenseConfiguration);
			Assert.AreEqual(AssemblyNameMatchMode.Exact, licenseConfiguration.NameMatchMode);
		}

		[TestMethod]
		public void Wrong_MatchMode_is_Substituted()
		{
			var xmlString = @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes"" ?>
				<information
					assemblyName=""Something""
					matchMode=""Wrong""
				>
					<license version=""0.0.0"" fileName=""Something.txt"">
					</license>
				</information>";
			
			var xml = new ResourceXmlDocument(Assembly.GetExecutingAssembly(), Guid.NewGuid().ToString(), xmlString);
			var licenseConfiguration = LicenseConfiguration.CreateFromXml(xml);
			Assert.IsNotNull(licenseConfiguration);
			Assert.AreEqual(AssemblyNameMatchMode.Exact, licenseConfiguration.NameMatchMode);
		}

		[TestMethod]
		public void Create_Without_Licenses_Fails()
		{
			var xmlString = @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes"" ?>
				<information
					assemblyName=""Something""
					matchMode=""Exact""
				>
				</information>";

			var xml = new ResourceXmlDocument(Assembly.GetExecutingAssembly(), Guid.NewGuid().ToString(), xmlString);
			var licenseConfiguration = LicenseConfiguration.CreateFromXml(xml);
			Assert.IsNull(licenseConfiguration);
		}

		[TestMethod]
		public void Duplicate_Licenses_Are_Ignored()
		{
			var xmlString = @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes"" ?>
				<information
					assemblyName=""Something""
					matchMode=""Exact""
				>
					<license version=""0.0.0"" fileName=""Something.txt"">
					</license>
					<license version=""0.0.0"" fileName=""Other.txt"">
					</license>
				</information>";

			var xml = new ResourceXmlDocument(Assembly.GetExecutingAssembly(), Guid.NewGuid().ToString(), xmlString);
			var licenseConfiguration = LicenseConfiguration.CreateFromXml(xml);
			Assert.IsNotNull(licenseConfiguration);
			Assert.AreEqual(1, licenseConfiguration.LicenseProviders.Count);
			Assert.AreEqual("Something.txt", licenseConfiguration.LicenseProviders.First().FileName); // Only the first entry should be used.
		}

		[TestMethod]
		public void Missing_FileName_is_Substituted()
		{
			var xmlString = @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes"" ?>
				<information
					assemblyName=""Assembly""
					matchMode=""Exact""
				>
					<license version=""1.2.3"">
					</license>
				</information>";

			var xml = new ResourceXmlDocument(Assembly.GetExecutingAssembly(), Guid.NewGuid().ToString(), xmlString);
			var licenseConfiguration = LicenseConfiguration.CreateFromXml(xml);
			Assert.IsNotNull(licenseConfiguration);
			Assert.AreEqual(1, licenseConfiguration.LicenseProviders.Count);
			Assert.AreEqual("Assembly_v1.2.3.txt", licenseConfiguration.LicenseProviders.First().FileName); // Only the first entry should be used.
		}
	}
}