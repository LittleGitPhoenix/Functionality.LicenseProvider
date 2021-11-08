#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace Phoenix.Functionality.LicenseProvider
{
	/// <summary>
	/// Resolver of license information for loaded <see cref="Assembly"/>s.
	/// </summary>
#if DEBUG
	public class LicenseResolver
#else
	public sealed class LicenseResolver
#endif
	{
		#region Delegates / Events

		#endregion

		#region Constants

		#endregion

		#region Fields

		/// <summary> Collection of beginnings that will be removed from each assembly name before getting license information. </summary>
		private static readonly IReadOnlyCollection<string> SuperfluousBeginnings;
		
		/// <summary> Collection of endings that will be removed from each assembly name before getting license information. </summary>
		private static readonly IReadOnlyCollection<string> SuperfluousEndings;

		/// <summary> The <see cref="LicenseResolverConfiguration"/> of this instance. </summary>
		private readonly LicenseResolverConfiguration _configuration;

		/// <summary> Collection of all already loaded assemblies. </summary>
		private readonly HashSet<AssemblyName> _loadedAssemblies;

		#endregion

		#region Properties

		/// <summary> Collection of all available <see cref="LicenseConfiguration"/>s. </summary>
		public IReadOnlyCollection<LicenseConfiguration> LicenseConfigurations { get; }

		#endregion

		#region (De)Constructors

		static LicenseResolver()
		{
			SuperfluousBeginnings = new string[] { };
			SuperfluousEndings = new[] { ".resources" };
		}

#if !DEBUG
		/// <summary>
		/// Constructor
		/// </summary>
		public LicenseResolver() : this(new LicenseResolverConfiguration()) { }
#endif

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

			//
			this.TryCreateMissingLicensesFile();

			// Always add this assembly as source for licenses.
			var resourceAssemblies = configuration.ResourceAssemblies.Concat(new[] { Assembly.GetExecutingAssembly() }).ToArray();
			this.LicenseConfigurations = LicenseLoader.LoadLicenseConfigurationsFromAssemblies(resourceAssemblies);
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
		/// <returns> The current instance. </returns>
		public LicenseResolver Start()
		{
			this.UpdatedLoadedAssemblies();
			AppDomain.CurrentDomain.AssemblyLoad += this.AssemblyLoaded;
			return this;
		}

		/// <summary>
		/// Stops monitoring <see cref="Assembly"/> loading.
		/// </summary>
		/// <returns> The current instance. </returns>
		public LicenseResolver Stop()
		{
			AppDomain.CurrentDomain.AssemblyLoad -= this.AssemblyLoaded;
			return this;
		}

		private void UpdatedLoadedAssemblies()
		{
			AppDomain.CurrentDomain
				.GetAssemblies()
				.ToList()
				.ForEach(this.TryAddNewAssembly)
				;
		}

		private void AssemblyLoaded(object? sender, AssemblyLoadEventArgs args)
		{
			var loadedAssembly = args.LoadedAssembly;
			this.TryAddNewAssembly(loadedAssembly);
		}

#if DEBUG
		internal void TryAddNewAssembly(Assembly newAssembly)
#else
		private void TryAddNewAssembly(Assembly newAssembly)
#endif
		{
			var assemblyInformation = newAssembly.GetName();

			if (!_loadedAssemblies.Contains(assemblyInformation))
			{
				_loadedAssemblies.Add(assemblyInformation);

				// Overstep excluded assemblies.
				var isExcluded = newAssembly.IsDynamic || LicenseResolver.TryGetLicenseProvider(assemblyInformation, _configuration.ExcludedAssemblies, out _);
				if (isExcluded)
				{
					return;
				}

				var success = this.HandleAssemblyLicense(assemblyInformation);
				if (!success)
				{
					System.Diagnostics.Trace.WriteLine($"LICENSE PROVIDER: Could not find license for '{assemblyInformation.Name}' in Version {assemblyInformation.Version}.");
					if (_configuration.LogMissingLicensesToFile)
					{
						var licenseDirectory = _configuration.GetAndCreateLicenseDirectory();
						File.AppendAllText(Path.Combine(licenseDirectory.FullName, ".missing.txt"), newAssembly.FullName + "\n");
					}
				}
			}
		}

#if DEBUG
		public virtual bool HandleAssemblyLicense(AssemblyName assemblyInformation)
#else
		private bool HandleAssemblyLicense(AssemblyName assemblyInformation)
#endif
		{
			// Find a matching license provider.
			if (!this.TryGetLicenseProvider(assemblyInformation, out var licenseProvider)) return false;

			// Save the license into a file.
			var licenseDirectory = _configuration.GetAndCreateLicenseDirectory();
			var licenseText = licenseProvider!.GetLicenseText();
			File.WriteAllText(Path.Combine(licenseDirectory.FullName, licenseProvider.FileName), licenseText, Encoding.UTF8);
			return true;
		}

		/// <summary>
		/// Tries to get the matching <see cref="LicenseProvider"/> for the passed <paramref name="assemblyInformation"/>.
		/// </summary>
		/// <param name="assemblyInformation"> Information about the assembly for which to get license information. </param>
		/// <param name="licenseProvider"> The <see cref="LicenseProvider"/> that will be filled. </param>
		/// <returns> <c>True</c> on success, otherwise <c>False</c>. </returns>
#if NETFRAMEWORK || NETSTANDARD || NETCOREAPP1_0 || NETCOREAPP1_1 || NETCOREAPP2_0 || NETCOREAPP2_1 || NETCOREAPP2_2 || NETCOREAPP3_0 || NETCOREAPP3_1
		internal bool TryGetLicenseProvider(AssemblyName assemblyInformation, out LicenseProvider? licenseProvider)
#else
		internal bool TryGetLicenseProvider(AssemblyName assemblyInformation, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out LicenseProvider? licenseProvider)
#endif
			=> LicenseResolver.TryGetLicenseProvider(assemblyInformation, this.LicenseConfigurations, out licenseProvider);

#if NETFRAMEWORK || NETSTANDARD || NETCOREAPP1_0 || NETCOREAPP1_1 || NETCOREAPP2_0 || NETCOREAPP2_1 || NETCOREAPP2_2 || NETCOREAPP3_0 || NETCOREAPP3_1
		internal static bool TryGetLicenseProvider(AssemblyName assemblyInformation, IReadOnlyCollection<LicenseConfiguration> licenseConfigurations, out LicenseProvider? licenseProvider)
#else
		internal static bool TryGetLicenseProvider(AssemblyName assemblyInformation, IReadOnlyCollection<LicenseConfiguration> licenseConfigurations, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out LicenseProvider? licenseProvider)
#endif
			=> LicenseResolver.TryGetLicenseProvider(assemblyInformation.Name, assemblyInformation.Version, licenseConfigurations, out licenseProvider);

		/// <summary>
		/// Tries to get the matching <see cref="LicenseProvider"/> from <paramref name="licenseConfigurations"/> for an assembly.
		/// </summary>
		/// <param name="assemblyName"> The name of the assembly to match. </param>
		/// <param name="assemblyVersion"> The <see cref="Version"/> of the assembly to match. </param>
		/// <param name="licenseConfigurations"> A collection of <see cref="LicenseConfiguration"/>s used for matching. </param>
		/// <param name="licenseProvider"> The <see cref="LicenseProvider"/> that will be filled. </param>
		/// <returns> <c>True</c> on success, otherwise <c>False</c>. </returns>
#if NETFRAMEWORK || NETSTANDARD || NETCOREAPP1_0 || NETCOREAPP1_1 || NETCOREAPP2_0 || NETCOREAPP2_1 || NETCOREAPP2_2 || NETCOREAPP3_0 || NETCOREAPP3_1
		internal static bool TryGetLicenseProvider(string? assemblyName, Version? assemblyVersion, IReadOnlyCollection<LicenseConfiguration> licenseConfigurations, out LicenseProvider? licenseProvider)
#else
		internal static bool TryGetLicenseProvider(string? assemblyName, Version? assemblyVersion, IReadOnlyCollection<LicenseConfiguration> licenseConfigurations, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out LicenseProvider? licenseProvider)
#endif
		{
			assemblyName = LicenseResolver.CleanAssemblyName(assemblyName);

			if (String.IsNullOrWhiteSpace(assemblyName) || assemblyVersion is null)
			{
				licenseProvider = null;
				return false;
			}
			
			licenseProvider = licenseConfigurations
				.FirstOrDefault
				(
					configuration =>
					{
						switch (configuration.NameMatchMode)
						{
							case AssemblyNameMatchMode.Contains:
#if NETFRAMEWORK || NETSTANDARD || NETCOREAPP1_0 || NETCOREAPP1_1 || NETCOREAPP2_0 || NETCOREAPP2_1 || NETCOREAPP2_2 || NETCOREAPP3_0 || NETCOREAPP3_1
								return assemblyName!.IndexOf(configuration.AssemblyIdentifier, StringComparison.OrdinalIgnoreCase) >= 0;
#else
								return assemblyName!.Contains(configuration.AssemblyIdentifier, StringComparison.OrdinalIgnoreCase);
#endif
							case AssemblyNameMatchMode.StartsWith:
								return assemblyName!.StartsWith(configuration.AssemblyIdentifier, StringComparison.OrdinalIgnoreCase);
							case AssemblyNameMatchMode.EndsWith:
								return assemblyName!.EndsWith(configuration.AssemblyIdentifier, StringComparison.OrdinalIgnoreCase);
							case AssemblyNameMatchMode.RegEx:
								return new Regex(configuration.AssemblyIdentifier, RegexOptions.IgnoreCase).IsMatch(assemblyName!);
							case AssemblyNameMatchMode.Exact:
							default:
								return String.Equals(configuration.AssemblyIdentifier, assemblyName, StringComparison.OrdinalIgnoreCase);
						}
					}
				)
				?.LicenseProviders
				.OrderByDescending(provider => provider.Version)
				.FirstOrDefault(provider => assemblyVersion >= provider.Version)
				;

			return licenseProvider != null;
		}

		private void TryCreateMissingLicensesFile()
		{
			var licenseDirectory = _configuration.GetAndCreateLicenseDirectory();
			var missingLicensesFile = new FileInfo(Path.Combine(licenseDirectory.FullName, ".missing.txt"));
			if (missingLicensesFile.Exists) missingLicensesFile.Delete();
			missingLicensesFile.Create().Dispose();
			missingLicensesFile.Refresh();
		}

		internal static string? CleanAssemblyName(string? assemblyName)
		{
			if (assemblyName is null) return assemblyName;
			if (String.IsNullOrWhiteSpace(assemblyName)) return assemblyName;

			foreach (var superfluousBeginning in LicenseResolver.SuperfluousBeginnings)
			{
				var startsWidth = assemblyName.StartsWith(superfluousBeginning, StringComparison.OrdinalIgnoreCase);
				if (startsWidth) assemblyName = assemblyName.Remove(0, superfluousBeginning.Length);
			}

			foreach (var superfluousEnding in LicenseResolver.SuperfluousEndings)
			{
				var index = assemblyName.LastIndexOf(superfluousEnding, StringComparison.OrdinalIgnoreCase);
				if (index != -1) assemblyName = assemblyName.Remove(index);
			}
			return assemblyName;
		}

		#endregion
	}
}