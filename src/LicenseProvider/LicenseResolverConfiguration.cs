#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


using System.IO;
using System.Linq;
using System.Reflection;

namespace Phoenix.Functionality.LicenseProvider
{
	/// <summary>
	/// Configuration values for a <see cref="LicenseResolver"/>.
	/// </summary>
	public record LicenseResolverConfiguration
	{
		#region Delegates / Events
		#endregion

		#region Constants
		#endregion

		#region Fields
		#endregion

		#region Properties

		/// <summary> The <see cref="DirectoryInfo"/> where the licenses will be saved to. Default is <c>.licenses</c> in the executing applications directory. </summary>
		private DirectoryInfo LicenseDirectory { get; }

		/// <summary> Assemblies that will be searched for embedded xml license files. </summary>
		public Assembly[] ResourceAssemblies { get; }

		/// <summary> Should the name of assemblies for which no license could be resolved be written to a file. </summary>
		public bool LogMissingLicensesToFile { get; init; }
		
		#endregion

		#region (De)Constructors

		/// <summary>
		/// Constructor
		/// </summary>
		public LicenseResolverConfiguration() : this(null, Assembly.GetCallingAssembly()) { }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="licenseDirectory"> <see cref="LicenseDirectory"/> </param>
		public LicenseResolverConfiguration(DirectoryInfo? licenseDirectory) : this(licenseDirectory, Assembly.GetCallingAssembly()) { }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="licenseDirectory"> <see cref="LicenseDirectory"/> </param>
		/// <param name="resourceAssemblies"> <see cref="ResourceAssemblies"/> </param>
		public LicenseResolverConfiguration(DirectoryInfo? licenseDirectory, params Assembly[] resourceAssemblies)
		{
			this.LicenseDirectory = licenseDirectory ?? new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), ".licenses"));
			this.ResourceAssemblies = resourceAssemblies.Distinct().ToArray();
		}

		#endregion

		#region Methods

		/// <summary>
		/// If <see cref="LicenseDirectory"/> doesn't exists it will be created and afterwards returned.
		/// </summary>
		/// <returns> An existing directory where the licenses will be stored. </returns>
		internal DirectoryInfo GetAndCreateLicenseDirectory()
		{
			if (!this.LicenseDirectory.Exists)
			{
				this.LicenseDirectory.Create();
				this.LicenseDirectory.Refresh();
			}

			return this.LicenseDirectory;
		}
		
		#endregion
	}
}