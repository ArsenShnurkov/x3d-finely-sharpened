using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Xml;
using System.IO;

using System.Xml.Linq;
using System.Xml.XPath;
using System.Text.RegularExpressions;

namespace X3D.Engine
{
    public class XMLParser
    {
        private static Regex regTrimMultipleTabs = new Regex("\\t+\\t+"); // replaces multiple tabs with a space
        private static Regex regTrimMultipleSpaces = new Regex("\\s+\\s+"); // replaces multiple spaces with a single space
        private static Regex negOnes = new Regex("[-]{1}[1]{1}[\\s]+||[\\w]+"); // selects -1 integers

        public static T Deserialise<T>(XElement element)
        {
            if (element == null)
            {
                return default(T);
            }
            return Deserialise<T>(element.ToXmlNode().OuterXml);
        }

        public static T Deserialise<T>(XPathNavigator nav)
        {
            return Deserialise<T>(nav.OuterXml);
        }

        public static T Deserialise<T>(string xml)
        {
            XmlSerializer ser;
            //XmlReaderSettings xs;
            //XmlReader xr;
            StreamWriter stw;
            MemoryStream stm;

            ser = new XmlSerializer(typeof(T));

            stm = new MemoryStream();
            stw = new StreamWriter(stm);
            stw.Write(xml);
            stw.Flush();
            stm.Position = 0;
            return (T)ser.Deserialize(stm);

            //xs = new XmlReaderSettings();
            //xs.DtdProcessing = DtdProcessing.Ignore;
            //xs.ValidationType = ValidationType.None;
            //xs.IgnoreProcessingInstructions = true;
            //xs.ValidationFlags = System.Xml.Schema.XmlSchemaValidationFlags.None;

            //stm = new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml));
            //xr = XmlReader.Create((Stream)stm, xs);
            //return (T)ser.Deserialize(xr);
        }

        public static SceneGraphNode ParseXMLElement(XPathNavigator element)
        {
            SceneGraphNode node;

            node = null;

            switch (element.Name.ToUpper())
            {
                case "X3D":
                    node = Deserialise<X3D>(element);
                    break;
                case "HEAD":
                    node = Deserialise<head>(element);
                    break;
                case "META":
                    node = Deserialise<meta>(element);
                    break;
                case "SCENE":
                    node = Deserialise<Scene>(element);
                    break;
                case "ROUTE":
                    node = Deserialise<ROUTE>(element);
                    break;
                //case "PROTO":
                    //node=Deserialise<Proto>(element);
                    //break;
                //case "EXTERNPROTO":
                    //node=Deserialise<ExternProto>(element);
                    //break;
                case "IMPORT":
                    node=Deserialise<IMPORT>(element);
                    break;
                case "EXPORT":
                    node=Deserialise<EXPORT>(element);
                    break;
                case "INLINE":
                    node=Deserialise<Inline>(element);
                    break;
                case "TIMESENSOR":
                    node = Deserialise<TimeSensor>(element);
                    break;
                case "INTEGERSEQUENCER":
                    node = Deserialise<IntegerSequencer>(element);
                    break;
                case "TRANSFORM":
                    node = Deserialise<Transform>(element);
                    break;
                case "GROUP":
                    node = Deserialise<Group>(element);
                    break;
                case "STATICGROUP":
                    node = Deserialise<StaticGroup>(element);
                    break;
                case "SWITCH":
                    node = Deserialise<Switch>(element);
                    break;
                case "SHAPE":
                    node = Deserialise<Shape>(element);
                    break;
                case "COORDINATE":
                    node = Deserialise<Coordinate>(element);
                    break;
                case "NORMAL":
                    node=Deserialise<Normal>(element);
                    break;
                case "TEXTURECOORDINATE":
                    node=Deserialise<TextureCoordinate>(element);
                    break;
                case "COLOR":
                    node=Deserialise<Color>(element);
                    break;
                case "COLORRGBA":
                    node=Deserialise<ColorRGBA>(element);
                    break;
                case "ELEVATIONGRID":
                    node = Deserialise<ElevationGrid>(element);
                    break;
                case "INDEXEDFACESET":
                    node = Deserialise<IndexedFaceSet>(element);
                    break;
                case "APPEARANCE":
                    node = Deserialise<Appearance>(element);
                    break;
                case "MATERIAL":
                    node = Deserialise<Material>(element);
                    break;
                case "IMAGETEXTURE":
                    node = Deserialise<ImageTexture>(element);
                    break;
                case "CYLINDER":
                    node=Deserialise<Cylinder>(element);
                    break;
                case "SPHERE":
                    node = Deserialise<Sphere>(element);
                    break;
                case "BOX":
                    node = Deserialise<Box>(element);
                    break;
                case "EXTRUSION":
                    node = Deserialise<Extrusion>(element);
                    break;
                case "COMPOSEDSHADER":
                    node = Deserialise<ComposedShader>(element);
                    break;
                case "SHADERPART":
                    node = Deserialise<ShaderPart>(element);
                    break;
            }
            

            return node;
        }

        public static XmlAttribute getAttributeById(XElement node, string id)
        {
            id = id.ToLower();
            XmlNode xnode;

            xnode = node.ToXmlNode();

            foreach (XmlAttribute a in xnode.Attributes)
            {
                if (a.Name.ToLower() == id)
                {
                    return a;
                }
            }
            return null;
        }

        public static XmlAttribute getAttributeById(XmlNode node, string id)
        {
            id = id.ToLower();
            foreach (XmlAttribute a in node.Attributes)
            {
                if (a.Name.ToLower() == id)
                {
                    return a;
                }
            }
            return null;
        }






        public static string[] trimStringArray(string[] arr)
        {
            string[] arr2 = new string[arr.Length];
            for (int i = 0; i < arr.Length; i++)
            {
                if (!String.IsNullOrEmpty(arr[i].Trim()))
                {
                    arr2[i] = arr[i];
                }
            }
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
    }
    public static class MyExtensions
    {
        public static XElement ToXElement(this XmlNode node)
        {
            XDocument xDoc = new XDocument();
            using (XmlWriter xmlWriter = xDoc.CreateWriter())
                node.WriteTo(xmlWriter);
            return xDoc.Root;
        }

        public static XmlNode ToXmlNode(this XElement element)
        {
            using (XmlReader xmlReader = element.CreateReader())
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(xmlReader);
                return xmlDoc;
            }
        }
    }
}
