#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml;

namespace Phoenix.Functionality.LicenseProvider
{
	/// <summary>
	/// Configuration of a single embedded xml license resource.
	/// </summary>
	public class LicenseConfiguration
	{
		#region Delegates / Events
		#endregion

		#region Constants
		#endregion

		#region Fields
		#endregion

		#region Properties

		/// <summary> The <see cref="AssemblyNameMatchMode"/> used for when matching an assembly against <see cref="AssemblyIdentifier"/>. </summary>
		public AssemblyNameMatchMode NameMatchMode { get; }

		/// <summary> The identifier of the assembly this configuration is valid for. </summary>
		public string AssemblyIdentifier { get; }
		
		/// <summary> All licenses descendingly ordered by the version upon which they are valid. </summary>
		public IReadOnlyCollection<LicenseProvider> LicenseProviders { get; }

		#endregion

		#region (De)Constructors

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="assemblyIdentifier"> <see cref="AssemblyIdentifier"/> </param>
		/// <param name="nameMatchMode"> <see cref="NameMatchMode"/> </param>
		/// <param name="licenseProviders"> <see cref="LicenseProviders"/> </param>
		internal LicenseConfiguration
		(
			string assemblyIdentifier,
			AssemblyNameMatchMode nameMatchMode,
			IReadOnlyCollection<LicenseProvider> licenseProviders
		)
		{
			// Save parameters.
			this.AssemblyIdentifier = assemblyIdentifier;
			this.NameMatchMode = nameMatchMode;
			this.LicenseProviders = licenseProviders;

			// Initialize fields.
		}
		
		internal static LicenseConfiguration? CreateFromXml(ResourceXmlDocument? xml)
		{
			if (xml is null) return null;
			var messagePrefix = $"LICENSE PROVIDER: The license xml resource '{xml.ResourceName}'";

			// Check if the xml is valid.
			if (xml.ParseException != null)
			{
				Trace.WriteLine($"{messagePrefix} could not be parsed from '{xml.XmlString}': {xml.ParseException.Message}.");
				return null;
			}

			// Get the assembly name.
			var assemblyIdentifier = XmlHelper.GetSingleNodeAttribute(xml, "//information[1]/@assemblyIdentifier");
			if (String.IsNullOrWhiteSpace(assemblyIdentifier))
			{
				// Backwards compatibility to v1.0:
				assemblyIdentifier = XmlHelper.GetSingleNodeAttribute(xml, "//information[1]/@assemblyName");
				if (String.IsNullOrWhiteSpace(assemblyIdentifier))
				{
					Trace.WriteLine($"{messagePrefix} does not provide a proper assembly identifier. Further parsing has been aborted.");
					return null;
				}
			}

			// Get the matching mode.
			var matchModeString = XmlHelper.GetSingleNodeAttribute(xml, "//information[1]/@matchMode");
			var success = Enum.TryParse(matchModeString, out AssemblyNameMatchMode matchMode);
			if (!success)
			{
				matchMode = AssemblyNameMatchMode.Exact;
				Trace.WriteLine($"{messagePrefix} does not provide a proper matching mode. '{nameof(AssemblyNameMatchMode.Exact)}' will be used.");
			}

			// Get all licenses and the version upon which they are valid.
			var versionedLicenses = new Dictionary<Version, LicenseProvider>();
			var licenseNodes = XmlHelper.GetNodes(xml, "//information[1]/license");
			if (!licenseNodes.Any())
			{
				Trace.WriteLine($"{messagePrefix} does not provide any licenses. Further parsing has ben aborted.");
				return null;
			}

			for (ushort index = 0; index < licenseNodes.Length; index++)
			{
				var licenseNode = licenseNodes[index];

				// Get the version.
				var versionString = XmlHelper.GetSingleNodeAttribute(licenseNode, "version");
				success = Version.TryParse(versionString, out var version);
				if (!success || version is null) version = new Version();
				if (versionedLicenses.ContainsKey(version))
				{
					Trace.WriteLine($"{messagePrefix} has duplicate license entries with identical version '{version}. Only the first one will be used.");
					continue;
				}

				// Get the file name used for saving the license.
				var fileName = XmlHelper.GetSingleNodeAttribute(licenseNode, "fileName");
				if (String.IsNullOrWhiteSpace(fileName))
				{
					fileName = $"{assemblyIdentifier}_v{versionString}.txt";
					Trace.WriteLine($"{messagePrefix} does not provide a proper file name. '{fileName}' will be used.");
				}
				
				versionedLicenses.Add(version, new LicenseProvider(version, fileName!, xml.ContainingAssembly, xml.ResourceName, (ushort) (index + 1)));
			}
			
			// Finally build the license configuration object.
			return new LicenseConfiguration(assemblyIdentifier!, matchMode, versionedLicenses.Values.OrderByDescending(license => license.Version).ToList().AsReadOnly());
		}

		#endregion

		#region Methods
		
		/// <summary> Returns a string representation of the object. </summary>
		public override string ToString() => $"[<{this.GetType().Name}> :: Assembly Identifier: {this.AssemblyIdentifier} | License Count: {this.LicenseProviders.Count}]";

		#endregion
	}
}