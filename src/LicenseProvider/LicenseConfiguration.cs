#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
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

		/// <summary> The <see cref="AssemblyNameMatchMode"/> used for when matching an assembly against <see cref="AssemblyName"/>. </summary>
		public AssemblyNameMatchMode NameMatchMode { get; }

		/// <summary> The name of the assembly this configuration is valid for. </summary>
		public string AssemblyName { get; }
		
		/// <summary> All licenses descendingly ordered by the version upon which they are valid. </summary>
		public IReadOnlyCollection<LicenseProvider> LicenseProviders { get; }

		#endregion

		#region (De)Constructors

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="assemblyName"> <see cref="AssemblyName"/> </param>
		/// <param name="nameMatchMode"> <see cref="NameMatchMode"/> </param>
		/// <param name="licenseProviders"> <see cref="LicenseProviders"/> </param>
		internal LicenseConfiguration
		(
			string assemblyName,
			AssemblyNameMatchMode nameMatchMode,
			IReadOnlyCollection<LicenseProvider> licenseProviders
		)
		{
			// Save parameters.
			this.AssemblyName = assemblyName;
			this.NameMatchMode = nameMatchMode;
			this.LicenseProviders = licenseProviders;

			// Initialize fields.
		}

		internal static LicenseConfiguration CreateFromXml(ResourceXmlDocument xml)
		{
			var messagePrefix = $"LICENSE PROVIDER: The license xml resource '{xml.ResourceName}'";

			// Check if the xml is valid.
			if (xml.ParseException != null)
			{
				Trace.WriteLine($"{messagePrefix} could not be parsed from '{xml.XmlString}': {xml.ParseException.Message}.");
				return null;
			}

			// Get the assembly name.
			var assemblyName = XmlHelper.GetSingleNodeAttribute(xml, "//information[1]/@assemblyName");
			if (String.IsNullOrWhiteSpace(assemblyName))
			{
				Trace.WriteLine($"{messagePrefix} does not provide a proper assembly name. Further parsing has ben aborted.");
				return null;
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
			var licenseNodes = XmlHelper.GetNodes(xml, "//information[1]/license").Cast<XmlNode>().ToList();
			if (!licenseNodes.Any())
			{
				Trace.WriteLine($"{messagePrefix} does not provide any licenses. Further parsing has ben aborted.");
				return null;
			}

			for (ushort index = 0; index < licenseNodes.Count; index++)
			{
				var licenseNode = licenseNodes[index];

				// Get the version.
				var versionString = XmlHelper.GetSingleNodeAttribute(licenseNode, "version");
				success = Version.TryParse(versionString, out var version);
				if (!success) version = new Version();
				if (versionedLicenses.ContainsKey(version))
				{
					Trace.WriteLine($"{messagePrefix} has duplicate license entries with identical version '{version}. Only the first one will be used.");
					continue;
				}

				// Get the file name used for saving the license.
				var fileName = XmlHelper.GetSingleNodeAttribute(licenseNode, "fileName");
				if (String.IsNullOrWhiteSpace(fileName))
				{
					fileName = $"{assemblyName}_v{versionString}.txt";
					Trace.WriteLine($"{messagePrefix} does not provide a proper file name. '{fileName}' will be used.");
				}

				versionedLicenses.Add(version, new LicenseProvider(version, fileName, xml.ContainingAssembly, xml.ResourceName, (ushort) (index + 1)));
			}
			
			// Finally build the license configuration object.
			return new LicenseConfiguration(assemblyName, matchMode, versionedLicenses.Values.OrderByDescending(license => license.Version).ToList().AsReadOnly());
		}

		#endregion

		#region Methods
		
		/// <summary> Returns a string representation of the object. </summary>
		public override string ToString() => $"[<{this.GetType().Name}> :: Assembly: {this.AssemblyName} | License Count: {this.LicenseProviders.Count}]";

		#endregion
	}
}