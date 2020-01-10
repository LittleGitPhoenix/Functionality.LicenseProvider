#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Phoenix.Functionality.LicenseProvider
{
	/// <summary>
	/// <see cref="LicenseResolver"/> builder interface.
	/// </summary>
	public interface IAddAssemblyLicenseResolverConstructor
	{
		/// <summary> Adds the current assembly as source for embedded xml license files. </summary>
		ISetDirectoryLicenseResolverConstructor AddCurrentAssembly();

		/// <summary> Adds the assembly of <typeparamref name="TLicenseResourceReference"/> as source for embedded xml license files. </summary>
		ISetDirectoryLicenseResolverConstructor AddAssemblyFromReference<TLicenseResourceReference>();

		/// <summary> Adds the specified <paramref name="resourceAssembly"/> as source for embedded xml license files. </summary>
		ISetDirectoryLicenseResolverConstructor AddAssembly(Assembly resourceAssembly);
	}

	/// <summary>
	/// <see cref="LicenseResolver"/> builder interface.
	/// </summary>
	public interface ISetDirectoryLicenseResolverConstructor : IAddAssemblyLicenseResolverConstructor
	{
		/// <summary> The default directory for saving the licenses will be used. See <see cref="LicenseResolverConfiguration.LicenseDirectory"/> for more information. </summary>
		ILicenseResolverBuilder WithDefaultOutputDirectory();

		/// <summary> The specified <paramref name="licenseDirectory"/> will be used for saving the licenses files. </summary>
		ILicenseResolverBuilder WithOutputDirectory(DirectoryInfo licenseDirectory);
	}

	/// <summary>
	/// <see cref="LicenseResolver"/> builder interface.
	/// </summary>
	public interface ILicenseResolverBuilder
	{
		/// <summary>
		/// Creates the new <see cref="LicenseResolver"/>.
		/// </summary>
		LicenseResolver Build();
	}
	
	internal sealed class LicenseResolverConstructor
		: IAddAssemblyLicenseResolverConstructor,
			ISetDirectoryLicenseResolverConstructor,
			ILicenseResolverBuilder
	{
		private readonly HashSet<Assembly> _resourceAssemblies;

		private DirectoryInfo _licenseDirectory;

		public LicenseResolverConstructor()
		{
			_resourceAssemblies = new HashSet<Assembly>();
		}

		/// <inheritdoc />
		public ISetDirectoryLicenseResolverConstructor AddCurrentAssembly()
			=> this.AddAssembly(Assembly.GetCallingAssembly());

		/// <inheritdoc />
		public ISetDirectoryLicenseResolverConstructor AddAssemblyFromReference<TLicenseResourceReference>()
			=> this.AddAssembly(typeof(TLicenseResourceReference).Assembly);

		/// <inheritdoc />
		public ISetDirectoryLicenseResolverConstructor AddAssembly(Assembly resourceAssembly)
		{
			_resourceAssemblies.Add(resourceAssembly);
			return this;
		}

		/// <inheritdoc />
		public ILicenseResolverBuilder WithDefaultOutputDirectory()
			=> this.WithOutputDirectory(null);

		/// <inheritdoc />
		public ILicenseResolverBuilder WithOutputDirectory(DirectoryInfo licenseDirectory)
		{
			_licenseDirectory = licenseDirectory;
			return this;
		}
		
		/// <inheritdoc />
		public LicenseResolver Build()
		{
			return new LicenseResolver(new LicenseResolverConfiguration(_licenseDirectory, _resourceAssemblies.ToArray()));
		}
	}
}