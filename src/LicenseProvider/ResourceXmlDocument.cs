#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

using System.Reflection;
using System.Xml;

namespace Phoenix.Functionality.LicenseProvider;

/// <summary>
/// Specialized <see cref="XmlDocument"/> that additionally holds the name of the resource where it was loaded from.
/// </summary>
internal sealed class ResourceXmlDocument : XmlDocument
{
	#region Delegates / Events
	#endregion

	#region Constants
	#endregion

	#region Fields
	#endregion

	#region Properties

	/// <summary> The assembly that contains the embedded xml resource. </summary>
	public Assembly ContainingAssembly { get; }

	/// <summary> The name of the resource from where this document has been created. </summary>
	public string ResourceName { get; }

	/// <summary> This is only available if the xml could not be created from the string content. </summary>
	public XmlException? ParseException { get; }

	/// <summary> This is only available if the xml could not be created from the string content. </summary>
	public string? XmlString { get; }

	#endregion

	#region (De)Constructors

	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="containingAssembly"> <see cref="ContainingAssembly"/> </param>
	/// <param name="resourceName"> <see cref="ResourceName"/> </param>
	/// <param name="xmlString"> The xml content string. </param>
	public ResourceXmlDocument(Assembly containingAssembly, string resourceName, string xmlString)
	{
		this.ResourceName = resourceName;
		this.ContainingAssembly = containingAssembly;

		try
		{
			base.LoadXml(xmlString);
			this.ParseException = null;
			this.XmlString = null;
		}
		catch (XmlException ex)
		{
			this.ParseException = ex;
			this.XmlString = xmlString;
		}
	}

	#endregion

	#region Methods

	/// <summary> Returns a string representation of the object. </summary>
	public override string ToString() => $"[<{this.GetType().Name}> :: Assembly: {this.ContainingAssembly} | Resource: {this.ResourceName} | Valid: {this.ParseException is null}]";

	#endregion
}