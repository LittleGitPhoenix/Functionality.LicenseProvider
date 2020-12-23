#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


namespace Phoenix.Functionality.LicenseProvider
{
	/// <summary>
	/// Defines different modes how an assembly name will be matched for a license file.
	/// </summary>
	public enum AssemblyNameMatchMode
	{
		/// <summary> The name of the assembly must match the <see cref="LicenseConfiguration.AssemblyIdentifier"/> ignoring case. </summary>
		Exact,
		/// <summary> The name of the assembly must contain with <see cref="LicenseConfiguration.AssemblyIdentifier"/>. </summary>
		Contains,
		/// <summary> The name of the assembly must begin with <see cref="LicenseConfiguration.AssemblyIdentifier"/>. </summary>
		StartsWith,
		/// <summary> The name of the assembly must end with <see cref="LicenseConfiguration.AssemblyIdentifier"/>. </summary>
		EndsWith,
		/// <summary> The name of the assembly must match the regular expression in <see cref="LicenseConfiguration.AssemblyIdentifier"/>. </summary>
		RegEx
	}
}