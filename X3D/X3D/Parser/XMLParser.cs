using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Xml.XPath;

namespace X3D.Parser
{
    public class XMLParser
    {
        private static readonly Regex
            regTrimMultipleTabs = new Regex("\\t+\\t+"); // replaces multiple tabs with a space

        private static readonly Regex
            regTrimMultipleSpaces = new Regex("\\s+\\s+"); // replaces multiple spaces with a single space

        private static readonly Regex negOnes = new Regex("[-]{1}[1]{1}[\\s]+||[\\w]+"); // selects -1 integers

        private static Dictionary<string, Type> _x3dTypeMap;

        public static SceneGraphNode ParseXMLElement(XElement element)
        {
            var x3dCoreAssembly = Assembly.GetExecutingAssembly();

            if (!(_x3dTypeMap != null && _x3dTypeMap.Count > 0))
                _x3dTypeMap = x3dCoreAssembly.GetTypes()
                    .ToDictionary(t => t.FullName, t => t, StringComparer.OrdinalIgnoreCase);

            if (string.IsNullOrEmpty(element.Name.LocalName)) return null;

            Type type;
            var typeName = string.Format("X3D.{0}", element.Name.LocalName);
            if (_x3dTypeMap.TryGetValue(typeName, out type))
            {
                return DeserialiseSGN(element, type);
            }

            Console.WriteLine("Type {0} not found in X3D.Core", typeName);
            return null;
        }

        public static SceneGraphNode ParseXMLElement(XPathNavigator element)
        {
            var x3dCoreAssembly = Assembly.GetExecutingAssembly();

            if (!(_x3dTypeMap != null && _x3dTypeMap.Count > 0))
                _x3dTypeMap = x3dCoreAssembly.GetTypes()
                    .ToDictionary(t => t.FullName, t => t, StringComparer.OrdinalIgnoreCase);

            if (string.IsNullOrEmpty(element.Name)) return null;

            Type type;
            var typeName = string.Format("X3D.{0}", element.Name);
            if (_x3dTypeMap.TryGetValue(typeName, out type))
            {
                return DeserialiseSGN(element, type);
            }

            Console.WriteLine("Type {0} not found in X3D.Core", typeName);
            return null;
        }


        public static XmlAttribute getAttributeById(XElement node, string id)
        {
            id = id.ToLower();
            XmlNode xnode;

            xnode = node.ToXmlNode();

            foreach (XmlAttribute a in xnode.Attributes)
                if (a.Name.ToLower() == id)
                    return a;
            return null;
        }

        public static XmlAttribute getAttributeById(XmlNode node, string id)
        {
            id = id.ToLower();
            foreach (XmlAttribute a in node.Attributes)
                if (a.Name.ToLower() == id)
                    return a;
            return null;
        }


        public static string[] trimStringArray(string[] arr)
        {
            var arr2 = new string[arr.Length];
            for (var i = 0; i < arr.Length; i++)
                if (!string.IsNullOrEmpty(arr[i].Trim()))
                    arr2[i] = arr[i];
            return arr2;
        }

        public static string removeNegOneIntegers(string input)
        {
            return negOnes.Replace(input, "");
        }

        public static string trimMultipleSpaces(string input)
        {
            // replaces multiple spaces with a single space
            return regTrimMultipleSpaces.Replace(input, " ");
        }

        public static string trimMultipleTabs(string input)
        {
            // replaces multiple tabs with a single space
            return regTrimMultipleTabs.Replace(input, " ");
        }

        #region Deserialization Methods

        public static SceneGraphNode DeserialiseSGN(XElement element, Type type)
        {
            if (element == null) return default;
            return DeserialiseSGN(element.ToXmlNode().OuterXml, type);
        }

        public static SceneGraphNode DeserialiseSGN(XPathNavigator nav, Type type)
        {
            return DeserialiseSGN(nav.OuterXml, type);
        }

        public static SceneGraphNode DeserialiseSGN(string xml, Type type)
        {
            XmlSerializer ser;
            //StreamWriter stw;
            MemoryStream stm;
            XmlReaderSettings xs;
            XmlReader xr;

            ser = new XmlSerializer(type);


            //stm = new MemoryStream();
            //stw = new StreamWriter(stm);
            //stw.Write(xml);
            //stw.Flush();
            //stm.Position = 0;
            //return (SceneGraphNode)ser.Deserialize(stm);

            xs = new XmlReaderSettings();
            xs.DtdProcessing = DtdProcessing.Ignore;
            xs.ValidationType = ValidationType.None;
            xs.IgnoreProcessingInstructions = true;
            xs.ValidationFlags = XmlSchemaValidationFlags.None;
            xs.IgnoreWhitespace = true;
            stm = new MemoryStream(Encoding.UTF8.GetBytes(xml));
            xr = XmlReader.Create(stm, xs);


            return (SceneGraphNode)ser.Deserialize(xr);
        }

        #endregion
    }

    public static class MyExtensions
    {
        public static XElement ToXElement(this XmlNode node)
        {
            var xDoc = new XDocument();
            using (var xmlWriter = xDoc.CreateWriter())
            {
                node.WriteTo(xmlWriter);
            }

            return xDoc.Root;
        }

        public static XmlNode ToXmlNode(this XElement element)
        {
            using (var xmlReader = element.CreateReader())
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.Load(xmlReader);
                return xmlDoc;
            }
        }
    }
}