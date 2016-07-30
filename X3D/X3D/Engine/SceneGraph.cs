//#define DEBUG_SCENE_GRAPH

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using X3D.Parser;

namespace X3D.Engine
{
    /// <summary>
    /// http://en.wikipedia.org/wiki/Scene_graph
    /// http://www.drdobbs.com/jvm/understanding-scene-graphs/184405094
    /// http://edutechwiki.unige.ch/en/X3DV#The_scene_graph
    /// </summary>
    public class SceneGraph
    {
        #region Public Fields

        public bool Loaded { get; set; }

        /// <summary>
        /// DEF/USE Semantics
        /// See: http://www.web3d.org/documents/specifications/19775-1/V3.2/Part01/concepts.html#DEFL_USESemantics
        /// </summary>
        public Dictionary<string, SceneGraphNode> defUseScope = new Dictionary<string, SceneGraphNode>();

        /// <summary>
        /// List of event ROUTE nodes in runtime state.
        /// Managed by Event Graph model.
        /// </summary>
        public List<ROUTE> Routes = new List<ROUTE>();

        public EventGraph EventGraph = new EventGraph();

        #endregion

        #region Private Fields

        private SceneGraphNode _root;
        private int __lastid = 0;

        #endregion

        #region Constructors

        public SceneGraph(XDocument xml)
        {
            XPathNavigator nav;

            Loaded = false;

            nav = xml.CreateNavigator();

            nav.MoveToRoot();

            // PASS 1
            build_scene_iterativily(nav);

            // PASS 2
            Optimise();

            // PASS 3
            // ROUTE and event propagation
            EventGraph.AssignSceneGraph(this);
            EventGraph.CreateEventGraph();

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

        #endregion

        #region Public Methods

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
            // No optimisations implemented yet

            //node_reuser();
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
                graph += "".PadLeft(node.Depth, ' ') + node.ToString() + " " + node.Depth.ToString() + "/" + node._id.ToString() + "\n";

                foreach (X3DNode child in node.Children)
                {
                    work_items.Push(child);
                }
            }
            while (work_items.Count > 0);

            return graph;
        }

        #endregion

        #region Private Methods

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
            SceneGraphNode def;
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
                    IXmlLineInfo lineInfo;
                    lineInfo = (IXmlLineInfo)nav;

                    child.XMLDocumentLocation = new OpenTK.Vector2(lineInfo.LinePosition, lineInfo.LineNumber);

#if DEBUG_SCENE_GRAPH
                    Console.ForegroundColor=ConsoleColor.White;
                    Console.WriteLine("".PadLeft(current_depth,'»')+nav.Name+" "+child.DEF+" "+current_depth.ToString()+"/"+node_id.ToString());
#endif
                    child._id = make_id();
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

                    #region DEF/USE 

                    if (!string.IsNullOrEmpty(child.DEF))
                    {
                        child.USE = string.Empty;

                        if (defUseScope.ContainsKey(child.DEF))
                        {
                            defUseScope[child.DEF] = child; // override existing
                        }
                        else
                        {
                            defUseScope.Add(child.DEF, child);
                        }

                    }
                    else if (!string.IsNullOrEmpty(child.USE))
                    {
                        child.DEF = string.Empty;

                        var asc = child.Ascendants();

                        if (asc.Any(ascendant => ascendant.DEF == child.USE))
                        {
                            Console.WriteLine("Cyclic reference ignored DEF_USE ", child.USE);

                            continue;
                        }


                        // if DEF is not an ancestor (self referential) then there are no cyclic references, so we are good to go.
                        // insert the DEF node a 2nd time as a child of the USE node parent
                        // resulting in the DEF node having multiple parents. See DEF/USE semantics.

                        def = _root.SearchDFS((SceneGraphNode n) => n.DEF == child.USE);

                        if (def != null && child.Parent != null)
                        {
                            def.Parents.Add(child.Parent);

                            child.Parent.Children.Add(def);


                        }
                    }

                    #endregion

                    #region Event Model

                    if(child.GetType() == typeof(ROUTE))
                    {
                        // Quick way to get all ROUTE nodes
                        Routes.Add((ROUTE)child);
                    }

                    #endregion

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

                if (node._id == id)
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

                if (node._id == id)
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

        #endregion

    }
}