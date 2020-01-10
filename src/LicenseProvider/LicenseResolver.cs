#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Phoenix.Functionality.LicenseProvider
{
	/// <summary>
	/// Resolver of license information for loaded <see cref="Assembly"/>s.
	/// </summary>
	public sealed class LicenseResolver
	{
		#region Delegates / Events
		#endregion

		#region Constants
		#endregion

		#region Fields

		/// <summary> The <see cref="LicenseResolverConfiguration"/> of this instance. </summary>
		private readonly LicenseResolverConfiguration _configuration;
		
		/// <summary> Collection of all already loaded assemblies. </summary>
		private readonly HashSet<AssemblyName> _loadedAssemblies;

		#endregion

		#region Properties

		/// <summary> Collection of all available <see cref="LicenseConfiguration"/>s. </summary>
		public ICollection<LicenseConfiguration> LicenseConfigurations { get; }

		#endregion

		#region (De)Constructors

		/// <summary>
		/// Constructor
		/// </summary>
		public LicenseResolver() : this(new LicenseResolverConfiguration()) { }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="configuration"> The configuration. </param>
		public LicenseResolver(LicenseResolverConfiguration configuration)
		{
			// Save parameters.
			_configuration = configuration;
			
			// Initialize fields.
			_loadedAssemblies = new HashSet<AssemblyName>();
			this.LicenseConfigurations = LicenseLoader.LoadLicenseConfigurationsFromAssemblies(configuration.ResourceAssemblies);
		}

		/// <summary>
		/// Starts constructing a new <see cref="LicenseResolver"/>.
		/// </summary>
		public static IAddAssemblyLicenseResolverConstructor Construct()
		{
			return new LicenseResolverConstructor();
		}

		#endregion

		#region Methods

		/// <summary>
		/// Starts monitoring <see cref="Assembly"/> loading and tries to provide license information for each.
		/// </summary>
		public void Start()
		{
			this.UpdatedLoadedAssemblies();
			AppDomain.CurrentDomain.AssemblyLoad += this.AssemblyLoaded;
		}

		/// <summary>
		/// Stops monitoring <see cref="Assembly"/> loading.
		/// </summary>
		public void Stop()
		{
			AppDomain.CurrentDomain.AssemblyLoad -= this.AssemblyLoaded;
		}

		private void UpdatedLoadedAssemblies()
		{
			AppDomain.CurrentDomain
				.GetAssemblies()
				.ToList()
				.ForEach(this.TryAddNewAssembly)
				;
		}

		private void AssemblyLoaded(object sender, AssemblyLoadEventArgs args)
		{
			var loadedAssembly = args.LoadedAssembly;
			this.TryAddNewAssembly(loadedAssembly);
		}

		private void TryAddNewAssembly(Assembly newAssembly)
		{
			var assemblyInformation = newAssembly.GetName();

			if (!_loadedAssemblies.Contains(assemblyInformation))
			{
				_loadedAssemblies.Add(assemblyInformation);

				var success = this.HandleAssemblyLicense(assemblyInformation);
				if (!success)
				{
					// Log missing license only for certain assemblies.
					if
					(
						!newAssembly.IsDynamic // No dynamic assemblies.
						&& !newAssembly.Location.ToLower().Contains("microsoft.net") // No .Net assemblies.
					)
					{
						System.Diagnostics.Trace.WriteLine($"LICENSE PROVIDER: Could not find license for '{assemblyInformation.Name}' in Version {assemblyInformation.Version}.");
					}
				}
			}
		}

		private bool HandleAssemblyLicense(AssemblyName assemblyInformation)
		{
			// Find a matching license provider.
			if (!this.TryGetLicenseProvider(assemblyInformation, out var licenseProvider)) return false;
			
			// Save the license into a file.
			if (!_configuration.LicenseDirectory.Exists) _configuration.LicenseDirectory.Create();
			var licenseText = licenseProvider.GetLicenseText();
			File.WriteAllText(Path.Combine(_configuration.LicenseDirectory.FullName, licenseProvider.FileName), licenseText, Encoding.UTF8);
			return true;
		}


		/// <summary>
		/// Tries to get the matching <see cref="LicenseProvider"/> for the passed <paramref name="assemblyInformation"/>.
		/// </summary>
		/// <param name="assemblyInformation"> Information about the assembly for which to get license information. </param>
		/// <param name="licenseProvider"> The <see cref="LicenseProvider"/> that will be filled. </param>
		/// <returns> <c>True</c> on success, otherwise <c>False</c>. </returns>
		private bool TryGetLicenseProvider(AssemblyName assemblyInformation, out LicenseProvider licenseProvider)
		{
			licenseProvider = this.LicenseConfigurations
				.FirstOrDefault
				(
					configuration =>
					{
						switch (configuration.NameMatchMode)
						{
							case AssemblyNameMatchMode.Contains:
								return assemblyInformation.Name.Contains(configuration.AssemblyName);
							case AssemblyNameMatchMode.StartsWith:
								return assemblyInformation.Name.StartsWith(configuration.AssemblyName);
							case AssemblyNameMatchMode.EndsWith:
								return assemblyInformation.Name.EndsWith(configuration.AssemblyName);
							case AssemblyNameMatchMode.Exact:
							default:
								return String.Equals(configuration.AssemblyName, assemblyInformation.Name, StringComparison.OrdinalIgnoreCase);
						}
					}
				)
				?.LicenseProviders
				.OrderByDescending(provider => provider.Version)
				.FirstOrDefault(provider => provider.Version >= assemblyInformation.Version)
				;

			return licenseProvider != null;
		}
		
		#endregion
	}
}