//#define DEBUG_SCENE_GRAPH

using System;
using System.Collections.Generic;
using System.Xml.XPath;
using System.Xml.Linq;


namespace X3D.Engine
{
    /// <summary>
    /// http://en.wikipedia.org/wiki/Scene_graph
    /// http://www.drdobbs.com/jvm/understanding-scene-graphs/184405094
    /// http://edutechwiki.unige.ch/en/X3DV#The_scene_graph
    /// </summary>
    public class SceneGraph
    {
        private SceneGraphNode _root;
        public bool Loaded = false;

        public SceneGraph(XDocument xml)
        {
            XPathNavigator nav;

            nav = xml.CreateNavigator();

            nav.MoveToRoot();

            build_scene_iterativily(nav);

            Optimise();

            Loaded = true;
        }

        /// <summary>
        /// Makes a SceneGraph from an entry point.
        /// </summary>
        public SceneGraph(SceneGraphNode root)
        {
            _root = root;

            Loaded = true;
        }

        public SceneGraphNode GetRoot()
        {
            return _root;
        }

        /// <summary>
        /// Optimises the current scene graph.
        /// Current optimisations include:
        ///     1. Node reuse (mapping USE nodes to DEF node)
        /// </summary>
        public void Optimise()
        {
            //node_reuser();
        }

        private int __lastid = 0;
        private int make_id()
        {
            __lastid++;
            return __lastid;
        }

        private void build_scene_iterativily(XPathNavigator nav)
        {
            XPathNavigator startPosition;
            SceneGraphNode parent;
            SceneGraphNode child;
            int current_depth;

            nav.MoveToFirstChild();

            startPosition = nav.Clone();
            current_depth = 0;
            parent = null;

            do
            {
                child = XMLParser.ParseXMLElement(nav);
                
                if (child != null)
                {
#if DEBUG_SCENE_GRAPH
                    Console.ForegroundColor=ConsoleColor.White;
                    Console.WriteLine("".PadLeft(current_depth,'»')+nav.Name+" "+child.DEF+" "+current_depth.ToString()+"/"+node_id.ToString());
#endif
                    child.id = make_id();
                    child.Depth = current_depth;

                    if (parent != null)
                    {
                        child.Parent = parent;
                        child.Parents.Add(parent);
                        parent.Children.Add(child);
                    }
                    else
                    {
                        _root = child;
                    }

                    if (nav.HasChildren)
                    {
                        child.IsLeaf = false;
                    }
                    else
                    {
                        child.IsLeaf = true;
                    }

                    //child.ContinueDeserialization();
                    // ... DEF_use

                    child.PostDeserialization();
                }

                if (nav.HasChildren)
                {
                    parent = child;
                    current_depth++;
                    nav.MoveToFirstChild();
                }
                else
                {
                    if (nav.MoveToNext(XPathNodeType.Element) == false)
                    {
                        do
                        {
                            if (nav.MoveToParent() == false)
                            {
                                break;
                            }
                            if (parent != null)
                            {
                                parent.PostDescendantDeserialization();

                                if (parent.IsLeaf == false && parent.Children.Count > 0)
                                {
                                    foreach (SceneGraphNode ch in parent.Children)
                                    {
                                        foreach (SceneGraphNode sibling in parent.Children)
                                        {
                                            if (ch != sibling && ch.Siblings.Contains(sibling) == false)
                                            {
                                                ch.Siblings.Add(sibling);
                                                sibling.Siblings.Add(ch);
                                            }
                                        }
                                    }
                                }

                                /* Go back to the previous depth to follow the nav: */
                                parent = parent.Parent;
                                current_depth--;
                            }
                        }
                        while (nav.MoveToNext(XPathNodeType.Element) == false);
                    }
                }
            }
            while (nav.NodeType != XPathNodeType.Root && !nav.IsSamePosition(startPosition));
        }


        private SceneGraphNode depth_first_search_iterative(int id)
        {
            Stack<SceneGraphNode> work_items;
            SceneGraphNode node;
            SceneGraphNode root;

            root = this.GetRoot();
            work_items = new Stack<SceneGraphNode>();
            work_items.Push(root);

            do
            {
                node = work_items.Pop();

                if (node.id == id)
                {
                    return node;
                }

                foreach (X3DNode child in node.Children)
                {
                    work_items.Push(child);
                }
            }
            while (work_items.Count > 0);

            return null;
        }
        private SceneGraphNode breadth_first_search_iterative(int id)
        {
            Queue<SceneGraphNode> work_items;
            SceneGraphNode node;
            SceneGraphNode root;

            root = this.GetRoot();
            work_items = new Queue<SceneGraphNode>();
            work_items.Enqueue(root);

            do
            {
                node = work_items.Dequeue();

                if (node.id == id)
                {
                    return node;
                }

                foreach (X3DNode child in node.Children)
                {
                    work_items.Enqueue(child);
                }
            }
            while (work_items.Count > 0);

            return null;
        }

        public override string ToString()
        {
            string graph;
            Stack<SceneGraphNode> work_items;
            SceneGraphNode node;
            SceneGraphNode root;

            graph = "";
            root = this.GetRoot();
            work_items = new Stack<SceneGraphNode>();
            work_items.Push(root);

            do
            {
                node = work_items.Pop();
                // TODO: display DEF attribute value and node name
                graph += "".PadLeft(node.Depth, ' ') + node.ToString() + " " + node.Depth.ToString() + "/" + node.id.ToString() + "\n";

                foreach (X3DNode child in node.Children)
                {
                    work_items.Push(child);
                }
            }
            while (work_items.Count > 0);

            return graph;
        }
    }
}