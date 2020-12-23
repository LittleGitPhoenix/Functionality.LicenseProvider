#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


using System;
using System.Reflection;

namespace Phoenix.Functionality.LicenseProvider
{
	/// <summary>
	/// Represents a single license, where the license text will by dynamically loaded from embedded resources.
	/// </summary>
	public class LicenseProvider
	{
		#region Delegates / Events
		#endregion

		#region Constants
		#endregion

		#region Fields

		private readonly Assembly _containingAssembly;

		private readonly string _resourceName;

		private readonly ushort _nodePosition;

		#endregion

		#region Properties
		
		/// <summary> The <see cref="System.Version"/> whence the license is valid. </summary>
		public Version Version { get; }

		/// <summary> The name of the file where license will be saved into. </summary>
		public string FileName { get; }

		#endregion

		#region (De)Constructors

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="version"> <see cref="Version"/> </param>
		/// <param name="fileName"> <see cref="FileName"/> </param>
		/// <param name="containingAssembly"> Used to get the license text on demand from the embedded resource. </param>
		/// <param name="resourceName"> Used to get the license text on demand from the embedded resource. </param>
		/// <param name="nodePosition"> Used to get the license text on demand from the embedded resource. ATTENTION: Nodes are counted beginning at 1! </param>
		internal LicenseProvider(Version version, string fileName, Assembly containingAssembly, string resourceName, ushort nodePosition)
		{
			this.Version = version;
			this.FileName = fileName;
			_containingAssembly = containingAssembly;
			_resourceName = resourceName;
			_nodePosition = nodePosition;
		}

		#endregion

		#region Methods

		internal string? GetLicenseText()
		{
			var xml = LicenseLoader.LoadResourceXmlFromAssembly(_containingAssembly, _resourceName);
			var content = XmlHelper.GetSingleNodeValue(xml, $"//information[1]/license[{_nodePosition}]");
			return content;
		}

		/// <summary> Returns a string representation of the object. </summary>
		public override string ToString() => $"[<{this.GetType().Name}> :: Version: {this.Version} | File: {this.FileName} | Resource: {_resourceName}:{_nodePosition}]";

		#endregion
	}
}