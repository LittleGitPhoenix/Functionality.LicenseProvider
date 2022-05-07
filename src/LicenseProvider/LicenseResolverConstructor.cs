#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

using System.Reflection;

namespace Phoenix.Functionality.LicenseProvider;

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
	ISetExcludedAssembliesLicenseResolverConstructor WithDefaultOutputDirectory();

	/// <summary> The specified <paramref name="licenseDirectory"/> will be used for saving the licenses files. </summary>
	ISetExcludedAssembliesLicenseResolverConstructor WithOutputDirectory(DirectoryInfo licenseDirectory);
}

/// <summary>
/// <see cref="LicenseResolver"/> builder interface.
/// </summary>
public interface ISetExcludedAssembliesLicenseResolverConstructor : ISetDirectoryLicenseResolverConstructor
{
	/// <summary> Assembly for which no license could be resolved will be logged to a separate file. </summary>
	ISetAdditionalExcludedAssembliesLicenseResolverConstructor ExcludeAssembly((string AssemblyIdentifier, AssemblyNameMatchMode NameMatchMode) tuple);

	/// <summary> Assembly for which no license could be resolved won't be logged to a separate file. </summary>
	ISetLogMissingLicenseResolverConstructor DoNotExcludeAssemblies();
}

/// <summary>
/// <see cref="LicenseResolver"/> builder interface.
/// </summary>
public interface ISetLogMissingLicenseResolverConstructor : ISetExcludedAssembliesLicenseResolverConstructor
{
	/// <summary> Assembly for which no license could be resolved will be logged to a separate file. </summary>
	ILicenseResolverBuilder LogMissingLicensesToFile();

	/// <summary> Assembly for which no license could be resolved won't be logged to a separate file. </summary>
	ILicenseResolverBuilder DoNotLogMissingLicensesToFile();
}

/// <summary>
/// <see cref="LicenseResolver"/> builder interface.
/// </summary>
public interface ISetAdditionalExcludedAssembliesLicenseResolverConstructor : ISetLogMissingLicenseResolverConstructor
{
	/// <inheritdoc cref="ISetExcludedAssembliesLicenseResolverConstructor.ExcludeAssembly" />
	new ISetAdditionalExcludedAssembliesLicenseResolverConstructor ExcludeAssembly((string AssemblyIdentifier, AssemblyNameMatchMode NameMatchMode) tuple);
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
		ISetExcludedAssembliesLicenseResolverConstructor,
		ISetLogMissingLicenseResolverConstructor,
		ISetAdditionalExcludedAssembliesLicenseResolverConstructor,
		ILicenseResolverBuilder
{
	private readonly HashSet<Assembly> _resourceAssemblies;

	private DirectoryInfo? _licenseDirectory;

	private readonly IList<ExcludedLicenseConfiguration> _excludedLicenseConfigurations;

	private bool _logMissing;
		
	public LicenseResolverConstructor()
	{
		_resourceAssemblies = new HashSet<Assembly>();
		_excludedLicenseConfigurations = new List<ExcludedLicenseConfiguration>();
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
	public ISetExcludedAssembliesLicenseResolverConstructor WithDefaultOutputDirectory()
	{
		return this;
	}

	/// <inheritdoc />
	public ISetExcludedAssembliesLicenseResolverConstructor WithOutputDirectory(DirectoryInfo licenseDirectory)
	{
		_licenseDirectory = licenseDirectory;
		return this;
	}

	/// <inheritdoc cref="ISetExcludedAssembliesLicenseResolverConstructor.ExcludeAssembly" />
	public ISetAdditionalExcludedAssembliesLicenseResolverConstructor ExcludeAssembly((string AssemblyIdentifier, AssemblyNameMatchMode NameMatchMode) tuple)
	{
		_excludedLicenseConfigurations.Add(tuple);
		return this;
	}

	/// <inheritdoc />
	public ISetLogMissingLicenseResolverConstructor DoNotExcludeAssemblies()
	{
		return this;
	}

	/// <inheritdoc />
	public ILicenseResolverBuilder LogMissingLicensesToFile()
	{
		_logMissing = true;
		return this;
	}

	/// <inheritdoc />
	public ILicenseResolverBuilder DoNotLogMissingLicensesToFile()
	{
		_logMissing = false;
		return this;
	}
		
	/// <inheritdoc />
	public LicenseResolver Build()
	{
		return new LicenseResolver
		(
			new LicenseResolverConfiguration(_licenseDirectory, _excludedLicenseConfigurations.ToArray(), _resourceAssemblies.ToArray())
			{
				LogMissingLicensesToFile = _logMissing
			}
		);
	}
}