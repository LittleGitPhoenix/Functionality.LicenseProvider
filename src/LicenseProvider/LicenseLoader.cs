#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Phoenix.Functionality.LicenseProvider
{
	/// <summary>
	/// Responsible for loading embedded xml license resource from an assembly and building <see cref="LicenseConfiguration"/> instances from them.
	/// </summary>
	internal static class LicenseLoader
	{
		#region Delegates / Events
		#endregion

		#region Constants
		#endregion

		#region Fields
		#endregion

		#region Properties
		#endregion

		#region (De)Constructors
		
		/// <summary>
		/// Static Constructor
		/// </summary>
		static LicenseLoader()  {  }

		#endregion

		#region Methods

		/// <summary>
		/// 
		/// </summary>
		/// <param name="resourceAssemblies"> A collection of <see cref="Assembly"/>s containing embedded xml resource files. </param>
		/// <returns></returns>
		internal static ICollection<LicenseConfiguration> LoadLicenseConfigurationsFromAssemblies(params Assembly[] resourceAssemblies)
		{
			var allConfigurations = resourceAssemblies
				.Select
				(
					resourceAssembly =>
					{
						return LicenseLoader.GetAllXmlLicenseResources(resourceAssembly)
							.Select(resourceName => LicenseLoader.LoadLicenseConfigurationFromAssembly(resourceAssembly, resourceName))
							.Where(configuration => configuration != null)
							.ToArray()
							;
					}
				)
				.ToList()
				;

			// TODO: Instead of merging via simply combining the lists, use an algorithm that filters and sorts duplicate entries by taking the corresponding AssemblyNameMatchMode into consideration.
			// Merge the multitude of configurations into one.
			return allConfigurations.SelectMany(configurations => configurations).ToList();
		}

		/// <summary>
		/// Loads the license configuration from <paramref name="resourceAssembly"/> with the <paramref name="resourceName"/> .
		/// </summary>
		/// <param name="resourceAssembly"> The <see cref="Assembly"/> from where to load the resource. </param>
		/// <param name="resourceName"> The name of the embedded xml license file. </param>
		/// <returns> A new <see cref="LicenseConfiguration"/> on success, otherwise <c>Null</c>. </returns>
		internal static LicenseConfiguration LoadLicenseConfigurationFromAssembly(Assembly resourceAssembly, string resourceName)
		{
			// Create a xml document and then a new license configuration object from it.
			var xml = LicenseLoader.LoadResourceXmlFromAssembly(resourceAssembly, resourceName);
			var configuration = LicenseConfiguration.CreateFromXml(xml);
			return configuration;
		}

		/// <summary>
		/// Loads the xml license resource from <paramref name="resourceAssembly"/> with <paramref name="resourceName"/>.
		/// </summary>
		/// <param name="resourceAssembly"> The <see cref="Assembly"/> from where to load the resource. </param>
		/// <param name="resourceName"> The name of the embedded xml license file. </param>
		/// <returns> A new <see cref="ResourceXmlDocument"/> on success, otherwise <c>Null</c>. </returns>
		internal static ResourceXmlDocument LoadResourceXmlFromAssembly(Assembly resourceAssembly, string resourceName)
		{
			// Get the content of the embedded xml resource.
			var xmlString = LicenseLoader.GetLicenseContent(resourceAssembly, resourceName);
			if (String.IsNullOrWhiteSpace(xmlString)) return null;

			// Create a special xml document from the above string.
			return new ResourceXmlDocument(resourceAssembly, resourceName, xmlString);
		}

		/// <summary>
		/// Gets all xml resource files from the Resource assembly.
		/// </summary>
		/// <returns> An enumerable of all matching resource names.</returns>
		internal static IEnumerable<string> GetAllXmlLicenseResources(Assembly resourceAssembly)
		{
			return resourceAssembly
				.GetManifestResourceNames()
				.Where(resource => resource.StartsWith($"{resourceAssembly.GetName().Name}.Licenses") && resource.EndsWith(".xml"))
				;
		}

		/// <summary>
		/// Loads the content of the embedded license files.
		/// </summary>
		/// <param name="resourceAssembly"> The <see cref="Assembly"/> from where to load the resource. </param>
		/// <param name="resourceName"> The name of the embedded xml license file. </param>
		/// <returns> The content as string. </returns>
		internal static string GetLicenseContent(Assembly resourceAssembly, string resourceName)
		{
			var internalResource = resourceAssembly.GetManifestResourceStream(resourceName);
			if (internalResource is null) return null; // Should not happen as only existing resources are iterated.
			using (internalResource)
			{
				using (var reader = new StreamReader(internalResource, System.Text.Encoding.UTF8))
				{
					return reader.ReadToEnd();
				}
			}
		}

		#endregion
	}
}