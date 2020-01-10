#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


using System.Xml;

namespace Phoenix.Functionality.LicenseProvider {
	internal static class XmlHelper
	{
		/// <summary>
		/// Gets all <see cref="XmlNode"/>s at <paramref name="xPath"/>.
		/// </summary>
		/// <param name="xml"> The <see cref="XmlDocument"/> to search for nodes. </param>
		/// <param name="xPath"> The <c>XPath</c> defining the nodes. </param>
		/// <returns></returns>
		internal static XmlNodeList GetNodes(XmlDocument xml, string xPath)
		{
			return xml?.SelectNodes(xPath);
		}

		/// <summary>
		/// Gets the single node value from <paramref name="nodeXPath"/>.
		/// </summary>
		/// <param name="xml"> The <see cref="XmlDocument"/> that contains the node. </param>
		/// <param name="nodeXPath"> The <c>XPath</c> defining the node. </param>
		/// <returns> The value of the node. </returns>
		internal static string GetSingleNodeValue(XmlDocument xml, string nodeXPath)
		{
			var nodes = XmlHelper.GetNodes(xml, nodeXPath);
			if (nodes.Count == 0) return null;
			if (nodes.Count > 1) return null;

			return XmlHelper.GetSingleNodeValue(nodes[0]);
		}

		internal static string GetSingleNodeValue(XmlNode node)
		{
			return node?.InnerText.Trim();
		}

		/// <summary>
		/// Gets the single attribute value from <paramref name="attributeXPath"/>.
		/// </summary>
		/// <param name="xml"> The <see cref="XmlDocument"/> to search. </param>
		/// <param name="attributeXPath"> The <c>XPath</c> defining the attribute. </param>
		/// <returns> The attribute's value. </returns>
		internal static string GetSingleNodeAttribute(XmlDocument xml, string attributeXPath)
		{
			//string attrVal = xml.SelectSingleNode("/MyConfiguration/@SuperNumber").Value;
			string attributeValue = xml.SelectSingleNode(attributeXPath)?.Value;
			return attributeValue;
		}

		/// <summary>
		/// Gets the value of the attribute <paramref name="attributeName"/> from <paramref name="node"/>.
		/// </summary>
		/// <param name="node"> The <see cref="XmlNode"/> to search. </param>
		/// <param name="attributeName"> The name of the attribute. </param>
		/// <returns> The attribute's value. </returns>
		internal static string GetSingleNodeAttribute(XmlNode node, string attributeName)
		{
			if (node?.Attributes?[attributeName] is null) return null;
			return node.Attributes[attributeName].Value;
		}
	}
}