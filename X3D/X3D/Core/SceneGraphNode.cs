using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace X3D
{
    public abstract partial class SceneGraphNode
    {
        public int id = -1;

        public SceneGraphNode Parent = null;
        public List<SceneGraphNode> Parents = new List<SceneGraphNode>();
        public List<SceneGraphNode> Children = new List<SceneGraphNode>();
        public List<SceneGraphNode> Siblings = new List<SceneGraphNode>();
        
        public bool IsLeaf;

        public int Depth { get; set; }

        public virtual void Load() { }

        public virtual void PostDeserialization() { Load(); }
        public virtual void PostDescendantDeserialization() { }

        public virtual void PreRender() { }
        public virtual void Render(FrameEventArgs e) { }
        public virtual void PostRender() { }

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

    }
}