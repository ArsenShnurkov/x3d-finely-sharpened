using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;

namespace X3D
{
    public abstract partial class SceneGraphNode
    {
        internal string __id;
        internal int _id = -1;

        #region Private Fields

        private bool alreadyWarned = false;

        #endregion

        #region Public Fields

        [XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.__id;
            }
            set
            {
                this.__id = value;
            }
        }

        [XmlIgnore]
        public bool debug = true;

        [XmlIgnore]
        public bool PassthroughAllowed = true;

        [XmlIgnore]
        public SceneGraphNode Parent = null;

        [XmlIgnore]
        public List<SceneGraphNode> Parents = new List<SceneGraphNode>();

        [XmlIgnore]
        public List<SceneGraphNode> Children = new List<SceneGraphNode>();

        [XmlIgnore]
        public List<SceneGraphNode> Siblings = new List<SceneGraphNode>();

        [XmlIgnore]
        public bool IsLeaf;

        [XmlIgnore]
        public bool HasRendered = false;

        [XmlIgnore]
        public bool? isValid;

        [XmlIgnore]
        public int Depth { get; set; }

        #endregion

        #region Public Virtuals

        public virtual void Load() { }

        public virtual void PostDeserialization() { Load(); }
        public virtual void PostDescendantDeserialization() { }

        public virtual void PreRenderOnce(RenderingContext rc) { }
        public virtual void PreRender() { }
        public virtual void Render(RenderingContext rc) { }
        public virtual void PostRender(RenderingContext rc) { }

        #endregion

        #region Dynamic Validation

        /// <summary>
        /// Validate relationships in scene at runtime and make adjustments instantly as necessary.
        /// 
        /// </summary>
        /// <returns>
        /// True, if the types of children of the current node are allowable under this type of SceneGraphNode
        /// Otherwise false, if there were any children found that arent valid relationships.
        /// </returns>
        public bool Validate()
        {
            bool passed = true;
            bool warned = false;
            
            this.isValid = null;

            // X3D VALIDATION

            // Test Shader Component
            shaderValidationConstraints(out passed, out warned);

            //TODO: test other X3D node relationships

            this.isValid = passed;

            this.alreadyWarned = warned;

            return passed;
        }

        private void shaderValidationConstraints(out bool passed, out bool warned)
        {
            List<SceneGraphNode> invalid;

            passed = true;
            warned = false;

            if (typeof(X3DProgrammableShaderObject).IsInstanceOfType(this))
            {
                // Only allowed to have ShaderPart, and field children

                if (!this.Children.Any(n => (typeof(field).IsInstanceOfType(n))))
                {
                    warned = true;

                    if (!alreadyWarned && debug) Console.WriteLine("[Warning] {0} doesnt contain any field children", this.ToString());
                }

                if (typeof(ComposedShader).IsInstanceOfType(this))
                {

                    invalid = this.Children.Where(n => !((typeof(ShaderPart).IsInstanceOfType(n) || typeof(field).IsInstanceOfType(n)))).ToList();

                    processInvalidNodes(invalid, this.ToString(), out passed);

                    if (!this.Children.Any(n => (typeof(ShaderPart).IsInstanceOfType(n))))
                    {
                        passed = false;

                        if (debug) Console.WriteLine("ComposedShader must contain ShaderPart children");
                    }
                }
                else if (typeof(PackagedShader).IsInstanceOfType(this))
                {
                    invalid = this.Children;

                    processInvalidNodes(invalid, this.ToString(), out passed, noChildrenAllowed: true);
                }
                else if (typeof(ShaderProgram).IsInstanceOfType(this))
                {
                    invalid = this.Children;

                    processInvalidNodes(invalid, this.ToString(), out passed, noChildrenAllowed: true);
                }
            }
            else if (typeof(ShaderPart).IsInstanceOfType(this))
            {
                invalid = this.Children;

                processInvalidNodes(invalid, this.ToString(), out passed, noChildrenAllowed: true);
            }
            else if (typeof(ProgramShader).IsInstanceOfType(this))
            {
                invalid = this.Children.Where(n => !typeof(ShaderProgram).IsInstanceOfType(n)).ToList();


                processInvalidNodes(invalid, this.ToString(), out passed);
            }
            else if (typeof(X3DAppearanceNode).IsInstanceOfType(this))
            {
                if (!this.Children.Any(n => (typeof(X3DAppearanceChildNode).IsInstanceOfType(n))))
                {
                    warned = true;

                    if (!alreadyWarned && debug) Console.WriteLine("[Warning] {0} doesnt contain any X3DAppearanceChildNode children", this.ToString());
                }

                invalid = this.Children.Where(n => !typeof(X3DAppearanceChildNode).IsInstanceOfType(n)).ToList();

                processInvalidNodes(invalid, this.ToString(), out passed);
            }
        }

        /// <summary>
        /// Process any invalid nodes found, pruning out each from the Scene Graph as each is discovered.
        /// This transformation applies to immediate children only.
        /// A new validation state to the node is determined after the processing.
        /// </summary>
        private void processInvalidNodes(List<SceneGraphNode> invalid, string parentName, out bool passed, bool noChildrenAllowed = false)
        {
            string msg;

            passed = true;

            if (invalid.Any())
            {
                passed = false;

                msg = getNodeNames(invalid);

                if (noChildrenAllowed && debug) Console.WriteLine("{0} should not contain any children", parentName);
                if (debug) Console.WriteLine("{0} should not contain children of type [{1}]", parentName, msg);
                
                // Maybe it would be better to re-insert nodes in places where they are allowed instead of pruning them?
                //TODO: define node insertion rules
                pruneBadRelationships(invalid);
            }
        }

        /// <summary>
        /// Removes nodes classed as invalid from the Scene Graph
        /// </summary>
        private void pruneBadRelationships(List<SceneGraphNode> invalidNodes)
        {
            for(int i=0; i < invalidNodes.Count(); i++)
            {
                SceneGraphNode invalidNode = invalidNodes[i];

                //if(invalidNode.isValid.HasValue && invalidNode.isValid.Value == false)
                {
                    this.Children.Remove(invalidNode);
                }
            }

            if (debug) Console.WriteLine("pruned bad relationships");
        }

        /// <summary>
        /// Removes a node from the Scene Graph that is classed as invalid 
        /// </summary>
        private void pruneBadRelationship(SceneGraphNode invalidNode)
        {
            //if (invalidNode.isValid.HasValue && invalidNode.isValid.Value == false)
            {
                this.Children.Remove(invalidNode);
            }

            if (debug) Console.WriteLine("pruned bad relationship");
        }

        /// <returns>
        /// List of node names in the set of input nodes
        /// </returns>
        private string getNodeNames(IEnumerable<SceneGraphNode> nodes)
        {
            string msg = string.Empty;
            foreach (SceneGraphNode n in nodes)
            {
                msg += n.ToString() + " ";
            }
            msg = msg.TrimEnd().Replace(" ", ", ");

            return msg;
        }

        #endregion

        #region Node Search Methods

        public SGn GetParent<SGn>() where SGn : SceneGraphNode
        {
            return (SGn)AscendantByType<SGn>().FirstOrDefault();
        }

        public SceneGraphNode SearchBFS(string id)
        {
            // Breadth first search for decendant node by type
            Queue<SceneGraphNode> work_items;
            SceneGraphNode node;
            SceneGraphNode result = null;

            work_items = new Queue<SceneGraphNode>();
            work_items.Enqueue(this);

            do
            {
                node = work_items.Dequeue();

                if (node.id == id)
                {
                    result = node;

                    break;
                }

                foreach (SceneGraphNode child in node.Children)
                {
                    work_items.Enqueue(child);
                }
            }
            while (work_items.Count > 0);

            return result;
        }

        public List<SceneGraphNode> DecendantsByType(Type t)
        {
            // Breadth first search for decendant node by type
            Queue<SceneGraphNode> work_items;
            SceneGraphNode node;
            List<SceneGraphNode> lst = new List<SceneGraphNode>();

            work_items = new Queue<SceneGraphNode>();
            work_items.Enqueue(this);

            do
            {
                node = work_items.Dequeue();
                
                if (t.IsInstanceOfType(node))
                {
                    lst.Add(node);
                }

                foreach (SceneGraphNode child in node.Children)
                {
                    work_items.Enqueue(child);
                }
            }
            while (work_items.Count > 0);

            return lst;
        }

        public List<SceneGraphNode> AscendantByType<T>()
        {
            SceneGraphNode parent;
            List<SceneGraphNode> lst = new List<SceneGraphNode>();
            Type t = typeof(T);

            parent = this.Parent;
            
            while (parent != null)
            {
                if (t.IsInstanceOfType(parent))
                {
                    lst.Add(parent);
                }

                parent = parent.Parent;
            }

            return lst;
        }

        #endregion

        #region Scene API Methods

        /// <summary>
        /// Callable through ECMAScript V8 Javascript engine
        /// </summary>
        public SceneGraphNode getElementById(string id)
        {
            //TODO: cache nodes and IDs
            return SearchBFS(id);
        }

        /// <summary>
        /// Callable through ECMAScript V8 Javascript engine
        /// </summary>
        public void setAttribute(string name, object value)
        {

            Type type;
            PropertyInfo propertyInfo;
            FieldInfo fieldInfo;

            type = this.GetType();

            try
            {
                propertyInfo = type.GetProperty(name);

                if(propertyInfo != null)
                {
                    propertyInfo.SetValue(this, Convert.ChangeType(value, propertyInfo.PropertyType), null);

                    return;
                }
            }
            catch (Exception ex) { }

            try
            {
                fieldInfo = type.GetField(name, BindingFlags.Public | BindingFlags.Instance);

                if(fieldInfo!= null)
                {
                    fieldInfo.SetValue(this, Convert.ChangeType(value, fieldInfo.FieldType));

                    return;
                }
            }
            catch (Exception ex) { }

        }

        /// <summary>
        /// Callable through ECMAScript V8 Javascript engine
        /// </summary>
        public object getAttribute(string name)
        {
            Type type;
            PropertyInfo propertyInfo;
            FieldInfo fieldInfo;
            object value = null;

            type = this.GetType();

            try
            {
                propertyInfo = type.GetProperty(name);

                if (propertyInfo != null)
                {
                    value = propertyInfo.GetValue(this, null);

                    return value;
                }
            }
            catch (Exception ex) { }

            try
            {
                fieldInfo = type.GetField(name, BindingFlags.Public | BindingFlags.Instance);

                if (fieldInfo != null)
                {
                    value = fieldInfo.GetValue(this);

                    return value;
                }
            }
            catch (Exception ex) { }

            return null;
        }

        #endregion
    }

}