using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using OpenTK;
using X3D.Engine;
using X3D.Parser;

namespace X3D
{
    public abstract class SceneGraphNode
    {
        #region Internal Methods

        /// <summary>
        ///     Used to control the visibility listing of nodes in the graph.
        /// </summary>
        internal void CopyToShadowDom()
        {
            //TODO: Note when XML Writer is used later we need to write from SwitchChildren instead of Children

            if (Children != null && Children.Count > 0)
            {
                var tmp = new SceneGraphNode[Children.Count];

                Children.CopyTo(tmp);

                Shadow = tmp.ToList();

                Children.Clear();
            }
        }

        #endregion

        #region Dynamic Validation

        /// <summary>
        ///     Validate relationships in scene at runtime and make adjustments instantly as necessary.
        /// </summary>
        /// <returns>
        ///     True, if the types of children of the current node are allowable under this type of SceneGraphNode
        ///     Otherwise false, if there were any children found that arent valid relationships.
        /// </returns>
        public bool Validate()
        {
            return X3DRuntimeValidator.Validate(this);
        }

        #endregion

        #region Private Fields

        #endregion

        #region Internal Fields

        internal string __id;
        internal int _id = -1;
        internal bool alreadyWarned = false;

        #endregion

        #region Public Properties

        //[XmlIgnore]
        //public X3DMetadataObject metadata { get; set; }

        [XmlAttribute] public string description { get; set; }

        [XmlAttribute] public string documentation { get; set; }

        [XmlAttribute] public string appinfo { get; set; }

        [XmlAttribute(DataType = "ID")] public string DEF { get; set; }

        [XmlAttribute(DataType = "IDREF")] public string USE { get; set; }

        /// <summary>
        ///     The containerField is responsible for defining where the SceneGraphNode belongs.
        ///     For example, if "children" is defined, the node belongs as a child of its parent.
        ///     Formally: If "children" is defined, the node will appear as a child in the Children property of its parent.
        ///     Otherwise, insert the node into the property name "container" specified by containerField.
        ///     Types may be; ["children", "appearance", "geometry", "material", "displacers", "trimmingContour", "shaders",
        ///     "programs",
        ///     "parts", "texture", "textureTransform", "viewport", "layout", "source", "joints", "collider", "contacts",
        ///     "physics",
        ///     "emitter", "renderStyle", "normal", "texCoord"].
        /// </summary>
        [XmlAttributeAttribute(DataType = "NMTOKEN")]
        [DefaultValue("children")]
        public string containerField { get; set; } = "children";

        [XmlAttributeAttribute(DataType = "NMTOKEN")]
        public string name { get; set; }

        [XmlAttributeAttribute(DataType = "NMTOKENS")]
        public string @class { get; set; }

        /// <summary>
        ///     If true, disabled the node from any Render() calls. KeepAlive() is then called instead.
        /// </summary>
        [XmlIgnore]
        public bool Hidden { get; set; } = false;

        [XmlIgnore] public bool IsPrototypeBase = false;

        #endregion

        #region Public Fields

        /// <summary>
        ///     The line number and column number the element was parsed from in the XML document.
        /// </summary>
        [XmlIgnore] public Vector2 XMLDocumentLocation = new Vector2(-1, -1);

        [XmlIgnore]
        public int _ID
        {
            get => _id;
            set => _id = value;
        }

        [XmlAttributeAttribute]
        public string id
        {
            get => __id;
            set => __id = value;
        }

        [XmlIgnore] public bool PassthroughAllowed = true;

        [XmlIgnore] public SceneGraphNode Parent = null;

        [XmlIgnore] public List<SceneGraphNode> Parents = new List<SceneGraphNode>();

        [XmlIgnore] public List<SceneGraphNode> Children = new List<SceneGraphNode>();

        [XmlIgnore]
        public List<SceneGraphNode> ChildrenWithAppliedReferences
        {
            get
            {
                var lst = Children;

                var refs = new List<SceneGraphNode>();

                foreach (var child in Children)
                    if (!string.IsNullOrEmpty(child.USE))
                    {
                        var def = child.Siblings.FirstOrDefault(n =>
                            !string.IsNullOrEmpty(n.DEF) && n.DEF == child.USE);

                        if (def != null)
                            refs.Add(def);
                        else
                            refs.Add(child);
                    }
                    else
                    {
                        refs.Add(child);
                    }

                return refs;
            }
        }

        /// <summary>
        ///     The children that arent currently visible in the scene
        /// </summary>
        [XmlIgnore] public List<SceneGraphNode> Shadow = new List<SceneGraphNode>();

        [XmlIgnore] public List<SceneGraphNode> Siblings = new List<SceneGraphNode>();

        [XmlIgnore] public bool IsLeaf;

        [XmlIgnore] public bool HasRendered;

        [XmlIgnore] public bool? hasValid;

        [XmlIgnore] public int Depth { get; set; }

        #endregion

        #region Public Methods

        public void Draw(RenderingContext rc)
        {
            if (!HasRendered)
            {
                PreRenderOnce(rc);

                HasRendered = true;
            }

            PreRender();
            Render(rc);
            PostRender(rc);
        }

        public NameValueCollection GetAttributes()
        {
            NameValueCollection attributes;
            Type type;
            PropertyInfo[] properties;
            FieldInfo[] fields;
            string value;
            object v;

            type = GetType();
            attributes = new NameValueCollection();

            properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(prop => !prop.IsDefined(typeof(XmlIgnoreAttribute), false)).ToArray();

            fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance)
                .Where(field => !field.IsDefined(typeof(XmlIgnoreAttribute), false)).ToArray();

            foreach (var pi in properties)
            {
                v = pi.GetValue(this, null);

                if (v != null)
                    value = v.ToString();
                else
                    value = "";


                attributes.Add(pi.Name, value);
            }

            foreach (var fi in fields)
            {
                v = fi.GetValue(this);

                if (v != null)
                    value = v.ToString();
                else
                    value = "";

                attributes.Add(fi.Name, value);
            }

            return attributes;
        }

        public string ErrorStringWithLineNumbers()
        {
            if (XMLDocumentLocation == null) return ToString();

            return string.Format("line ({2},{1})", ToString(), XMLDocumentLocation.X, XMLDocumentLocation.Y);
        }

        #endregion

        #region Public Virtuals

        /// <summary>
        ///     Node deserilized callback
        /// </summary>
        public virtual void Init()
        {
        }

        public virtual void Load()
        {
        }

        public virtual void PostDeserialization()
        {
            Load();
        }

        public virtual void PostDescendantDeserialization()
        {
            Init();
        }

        public virtual void PreRenderOnce(RenderingContext rc)
        {
        }

        public virtual void PreRender()
        {
        }

        public virtual void Render(RenderingContext rc)
        {
        }

        public virtual void KeepAlive(RenderingContext rc)
        {
        }

        public virtual void PostRender(RenderingContext rc)
        {
        }

        #endregion

        #region Node Search Methods

        public SGn GetParent<SGn>() where SGn : SceneGraphNode
        {
            return AscendantByType<SGn>().FirstOrDefault();
        }

        public List<SceneGraphNode> DecendantsByType(Type t)
        {
            // Breadth first search for decendant node by type
            Queue<SceneGraphNode> work_items;
            SceneGraphNode node;
            var lst = new List<SceneGraphNode>();

            work_items = new Queue<SceneGraphNode>();
            work_items.Enqueue(this);

            do
            {
                node = work_items.Dequeue();

                if (t.IsInstanceOfType(node)) lst.Add(node);

                foreach (var child in node.Children) work_items.Enqueue(child);
            } while (work_items.Count > 0);

            return lst;
        }

        public List<Sgn> DecendantsByType<Sgn>() where Sgn : SceneGraphNode
        {
            // Breadth first search for decendant node by type
            Queue<SceneGraphNode> work_items;
            SceneGraphNode node;
            var lst = new List<Sgn>();
            var t = typeof(Sgn);

            work_items = new Queue<SceneGraphNode>();
            work_items.Enqueue(this);

            do
            {
                node = work_items.Dequeue();

                if (t.IsInstanceOfType(node)) lst.Add((Sgn)node);

                foreach (var child in node.Children) work_items.Enqueue(child);
            } while (work_items.Count > 0);

            return lst;
        }

        public List<Sgn> AscendantByType<Sgn>() where Sgn : SceneGraphNode
        {
            SceneGraphNode parent;
            var lst = new List<Sgn>();
            var t = typeof(Sgn);

            parent = Parent;

            while (parent != null)
            {
                if (t.IsInstanceOfType(parent)) lst.Add((Sgn)parent);

                parent = parent.Parent;
            }

            return lst;
        }

        public List<SceneGraphNode> Decendants()
        {
            // Breadth first search for decendant node by type
            Queue<SceneGraphNode> work_items;
            SceneGraphNode node;
            var lst = new List<SceneGraphNode>();

            work_items = new Queue<SceneGraphNode>();
            work_items.Enqueue(this);

            do
            {
                node = work_items.Dequeue();

                lst.Add(node);

                foreach (var child in node.Children) work_items.Enqueue(child);
            } while (work_items.Count > 0);

            return lst;
        }

        public List<SceneGraphNode> Ascendants()
        {
            SceneGraphNode ascendant;
            var lst = new List<SceneGraphNode>();

            ascendant = Parent;

            while (ascendant != null)
            {
                lst.Add(ascendant);

                ascendant = ascendant.Parent;
            }

            return lst;
        }

        public List<Sgn> ItemsByType<Sgn>() where Sgn : SceneGraphNode
        {
            // Look for children in Items list (if available) that match specified type
            var lst = new List<Sgn>();
            var t = typeof(Sgn);
            PropertyInfo prop;

            prop = GetType().GetProperty("Items", BindingFlags.Public | BindingFlags.Instance);

            if (prop != null)
            {
                var value = (List<object>)prop.GetValue(this, null);

                foreach (var o in value)
                    if (o.GetType() == t)
                        lst.Add((Sgn)o);
            }

            return lst;
        }

        #endregion

        #region Scene API Methods

        /// <summary>
        ///     Callable through ECMAScript V8 Javascript engine
        /// </summary>
        public SceneGraphNode getElementById(string id)
        {
            //TODO: cache nodes and IDs

            return SceneGraph.QueryBFS(this, n => { return n.id == id; }).FirstOrDefault();
        }

        /// <summary>
        ///     Callable through ECMAScript V8 Javascript engine
        /// </summary>
        public void setAttribute(string name, object value)
        {
            Type type;
            PropertyInfo propertyInfo;
            FieldInfo fieldInfo;

            type = GetType();

            try
            {
                propertyInfo = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);

                if (propertyInfo != null)
                {
                    var @new = !propertyInfo.PropertyType.IsInstanceOfType(value)
                        ? Convert.ChangeType(value, propertyInfo.PropertyType)
                        : value;

                    propertyInfo.SetValue(this, @new, null);

                    return;
                }
            }
            catch
            {
            }

            try
            {
                fieldInfo = type.GetField(name, BindingFlags.Public | BindingFlags.Instance);

                if (fieldInfo != null) fieldInfo.SetValue(this, Convert.ChangeType(value, fieldInfo.FieldType));
            }
            catch
            {
            }
        }

        /// <summary>
        ///     Callable through ECMAScript V8 Javascript engine
        /// </summary>
        public object getAttribute(string name)
        {
            Type type;
            PropertyInfo propertyInfo;
            FieldInfo fieldInfo;
            object value = null;

            type = GetType();

            try
            {
                propertyInfo = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);

                if (propertyInfo != null)
                {
                    value = propertyInfo.GetValue(this, null);

                    return value;
                }
            }
            catch
            {
            }

            try
            {
                fieldInfo = type.GetField(name, BindingFlags.Public | BindingFlags.Instance);

                if (fieldInfo != null)
                {
                    value = fieldInfo.GetValue(this);

                    return value;
                }
            }
            catch
            {
            }

            return null;
        }

        public bool HasAttribute(string name)
        {
            Type type;
            PropertyInfo propertyInfo;

            type = GetType();

            try
            {
                propertyInfo = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);

                if (propertyInfo != null) return true;
            }
            catch
            {
                return false;
            }

            return false;
        }

        #endregion
    }
}