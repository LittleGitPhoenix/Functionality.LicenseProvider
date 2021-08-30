#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


namespace Phoenix.Functionality.LicenseProvider
{
	/// <summary>
	/// Special <see cref="LicenseConfiguration"/> used for assemblies that will not be handled by the license provider.
	/// </summary>
	public class ExcludedLicenseConfiguration : LicenseConfiguration
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
		/// Constructor
		/// </summary>
		/// <param name="assemblyIdentifier"> <see cref="LicenseConfiguration.AssemblyIdentifier"/> </param>
		/// <param name="nameMatchMode"> <see cref="LicenseConfiguration.NameMatchMode"/> </param>
		public ExcludedLicenseConfiguration(string assemblyIdentifier, AssemblyNameMatchMode nameMatchMode)
			: base(assemblyIdentifier, nameMatchMode, new[] { LicenseProvider.NoLicenseProvider }) { }

		#endregion

		#region Methods

		/// <summary>
		/// Implicitly converts <paramref name="tuple"/> into a new <see cref="ExcludedLicenseConfiguration"/> instance.
		/// </summary>
		public static implicit operator ExcludedLicenseConfiguration((string AssemblyIdentifier, AssemblyNameMatchMode NameMatchMode) tuple)
		{
			var (assemblyIdentifier, nameMatchMode) = tuple;
			return new ExcludedLicenseConfiguration(assemblyIdentifier, nameMatchMode);
		}

		#endregion
	}
}