//BUG: relationships in XML graph output not same as input nodes

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace X3D.Parser
{
    /// <summary>
    ///     Serialize an X3D scene graph starting from its root node outputting and inlining XML for each child.
    ///     Scene graph nodes dont have to implement IXmlSerializable.
    ///     Infact using IXmlSerializable causes problems with X3D Model.
    /// </summary>
    public class X3DSceneGraphXMLSerializer
    {
        private const string
            ID_PROPERTY = "__________id"; // tempoary id nodes have to make XmlDocument merging on 2nd pass easier

        private static readonly Encoding encoding = Encoding.UTF8;
        private readonly SceneGraphNode root;

        public X3DSceneGraphXMLSerializer(SceneGraphNode root)
        {
            this.root = root;
        }

        /// <summary>
        ///     Serializes an X3D Scene from model back to XML.
        /// </summary>
        /// <returns>
        ///     Returns an XML document in a string.
        /// </returns>
        public string Serialize()
        {
            string xml, chXml;
            XmlDocument document;
            _node item, ch, ro;
            XmlDocument childDoc;
            SceneGraphNode node;
            XmlNode parent, p;
            var work_items = new Stack<_node>();
            dynamic instanceOfDerived;
            XmlTextWriter writer;
            StreamReader reader;
            MemoryStream ms;
            int depth;
            var maxDepth = 0;
            XmlNode curr;
            XmlDocument @new;
            XmlElement elemp;
            XmlElement elem;
            XmlNode n;

            instanceOfDerived = Activator.CreateInstance(root.GetType());
            instanceOfDerived = Clone(instanceOfDerived, root);

            xml = Serialize(instanceOfDerived);
            xml = removeXmlTypeDefinition(xml);

            document = new XmlDocument();
            document.LoadXml(xml);

            ro = new _node
            {
                data = root,
                dom = document,
                outerXml = xml,
                parent = null
            };

            work_items.Push(ro);


            // X3D SERIALIZE

            // have to do in 2 passes to join the XmlDocuments since each child is a separate XmlDocument

            var id = 0;
            while (work_items.Count > 0)
            {
                item = work_items.Pop();
                node = item.data;
                parent = item.dom.FirstChild;


                elem = (XmlElement)item.dom.FirstChild;
                elem.SetAttribute(ID_PROPERTY, id.ToString()); // add temporary id to make merging XmlDocuments easier.

                id++;

                foreach (dynamic child in node.Children)
                {
                    childDoc = new XmlDocument();
                    chXml = removeXmlTypeDefinition(Serialize(child));
                    childDoc.LoadXml(chXml);

                    p = parent;
                    depth = 0;
                    while (p != null)
                    {
                        depth++;
                        p = p.ParentNode;
                    }

                    if (depth > maxDepth) maxDepth = depth;

                    ch = new _node
                    {
                        data = child,
                        dom = childDoc,
                        outerXml = parent.OuterXml,
                        depth = depth,
                        parent = item
                    };

                    item.children.Add(ch);

                    work_items.Push(ch);
                }
            }

            // find the deapest node to get all the outer xml
            work_items.Push(ro);

            curr = null;
            @new = new XmlDocument();
            @new.AppendChild(@new.CreateNode(XmlNodeType.Element, "X3D", ""));

            curr = null; // root

            while (work_items.Count > 0)
            {
                item = work_items.Pop();

                n = @new.ImportNode(item.dom.FirstChild, true);
                elem = (XmlElement)n;
                elemp = item.parent == null ? null : (XmlElement)item.parent.dom.FirstChild;

                if (curr == null)
                {
                    curr = @new.FirstChild;
                }
                else
                {
                    var _id = elemp.GetAttribute(ID_PROPERTY);

                    curr = findNodeDfs(_id, @new);

                    elem.RemoveAttribute(ID_PROPERTY);

                    curr.AppendChild(n);
                }

                foreach (var child in item.children) work_items.Push(child);
            }

            document = @new;

            // Indent the XML
            ms = new MemoryStream();
            writer = new XmlTextWriter(ms, encoding);
            writer.Formatting = Formatting.Indented;
            document.WriteContentTo(writer);
            writer.Flush();
            ms.Flush();
            ms.Position = 0;
            reader = new StreamReader(ms);

            xml = reader.ReadToEnd();

            return xml;
        }

        private static XmlNode findNodeDfs(string id, XmlDocument document)
        {
            XmlNode n = null;
            XmlElement elem;

            var work_items = new Stack<XmlNode>();
            work_items.Push(document.FirstChild);

            while (work_items.Count > 0)
            {
                n = work_items.Pop();

                elem = (XmlElement)n;

                if (elem.GetAttribute(ID_PROPERTY) == id) break;

                foreach (XmlNode child in n.ChildNodes) work_items.Push(child);
            }

            return n;
        }

        private static string removeXmlTypeDefinition(string xml)
        {
            return xml.Replace("﻿<?xml version=\"1.0\" encoding=\"utf-8\"?>", "");
        }

        private static T1 Clone<T1, T2>(T1 obj, T2 otherObject) where T1 : class where T2 : class
        {
            PropertyInfo[] srcProperties;
            PropertyInfo[] destProperties;
            PropertyInfo dest;

            srcProperties = otherObject.GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty);
            destProperties = obj.GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty);

            foreach (var property in srcProperties)
            {
                dest = destProperties.FirstOrDefault(x => x.Name == property.Name);

                if (dest != null && dest.CanWrite) dest.SetValue(obj, property.GetValue(otherObject, null), null);
            }

            return obj;
        }

        private static string Serialize<T>(T node)
        {
            var memStream = new MemoryStream();
            var ns = new XmlSerializerNamespaces();
            ns.Add("", "");
            XmlSerializer serializer;

            using (var w = new XmlTextWriter(memStream, encoding))
            {
                serializer = new XmlSerializer(typeof(T));
                serializer.Serialize(w, node, ns);

                memStream = w.BaseStream as MemoryStream;
            }

            if (memStream != null)
                return encoding.GetString(memStream.ToArray());
            return string.Empty;
        }

        private class _node
        {
            public readonly List<_node> children = new List<_node>();
            public SceneGraphNode data;
            public int depth;
            public XmlDocument dom;
            public string outerXml;
            public _node parent;
        }
    }
}