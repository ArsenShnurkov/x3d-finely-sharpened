//#define DEBUG_SCENE_GRAPH

//TODO: implement containerField

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using OpenTK;
using X3D.Parser;

namespace X3D.Engine
{
    /// <summary>
    ///     http://en.wikipedia.org/wiki/Scene_graph
    ///     http://www.drdobbs.com/jvm/understanding-scene-graphs/184405094
    ///     http://edutechwiki.unige.ch/en/X3DV#The_scene_graph
    /// </summary>
    public class SceneGraph
    {
        #region Public Fields

        public bool Loaded { get; set; }

        /// <summary>
        ///     DEF/USE Semantics
        ///     See: http://www.web3d.org/documents/specifications/19775-1/V3.2/Part01/concepts.html#DEFL_USESemantics
        ///     Also, name scope: http://www.web3d.org/documents/specifications/19775-1/V3.3/Part01/concepts.html#Runtimenamescope
        /// </summary>
        public Dictionary<string, SceneGraphNode> defUseScope = new Dictionary<string, SceneGraphNode>();

        /// <summary>
        ///     See, name scope: http://www.web3d.org/documents/specifications/19775-1/V3.3/Part01/concepts.html#Runtimenamescope
        /// </summary>
        public List<KeyValuePair<string, SceneGraphNode>>
            nameScope = new List<KeyValuePair<string, SceneGraphNode>>(); // slow

        public Dictionary<string, List<SceneGraphNode>> classScope = new Dictionary<string, List<SceneGraphNode>>();

        /// <summary>
        ///     List of event ROUTE nodes in runtime state.
        ///     Managed by Event Graph model.
        /// </summary>
        public List<ROUTE> Routes = new List<ROUTE>();

        public EventGraph EventGraph = new EventGraph();

        #endregion

        #region Private Fields

        private SceneGraphNode _root;
        private int __lastid;

        private class _graph_node
        {
            public XElement node;
            public SceneGraphNode parent;
        }

        #endregion

        #region Constructors

        public SceneGraph(XDocument xml)
        {
            XPathNavigator nav;

            Loaded = false;

            nav = xml.CreateNavigator();

            nav.MoveToRoot();

            // PASS 1
            //build_scene_iterativily(nav);
            build_scene_iterativily_xdoc(xml);

            // PASS 2
            Optimise();

            // PASS 3
            // ROUTE and event propagation
            EventGraph.AssignSceneGraph(this);
            EventGraph.CreateEventGraph();

            Loaded = true;
        }

        /// <summary>
        ///     Makes a SceneGraph from an entry point.
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
        ///     Optimises the current scene graph.
        ///     Current optimisations include:
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
            root = GetRoot();
            work_items = new Stack<SceneGraphNode>();
            work_items.Push(root);

            do
            {
                node = work_items.Pop();
                // TODO: display DEF attribute value and node name
                graph += "".PadLeft(node.Depth, ' ') + node + " " + node.Depth + "/" + node._id + "\n";

                foreach (X3DNode child in node.Children) work_items.Push(child);
            } while (work_items.Count > 0);

            return graph;
        }

        /// <summary>
        ///     Finds a scene graph node by _id using a depth first graph traversal approach.
        /// </summary>
        /// <param name="id">
        ///     The id attribute coresponding to the _id attribute of the node to locate.
        /// </param>
        /// <returns>
        ///     A node within the scene graph bearing the same id field.
        /// </returns>
        public SceneGraphNode QueryDFS(int id)
        {
            SceneGraphNode result;

            result = Query(n => { return n._id == id; }, GraphQueryTraversalType.DepthFirst, false).FirstOrDefault();

            return result;
        }

        /// <summary>
        ///     Finds a scene graph node by _id using a bredth first graph traversal approach.
        /// </summary>
        /// <param name="id">
        ///     The id attribute coresponding to the _id attribute of the node to locate.
        /// </param>
        /// <returns>
        ///     A node within the scene graph bearing the same id field.
        /// </returns>
        public SceneGraphNode QueryBFS(int id)
        {
            SceneGraphNode result;

            result = Query(n => { return n._id == id; }, GraphQueryTraversalType.BredthFirst, false).FirstOrDefault();

            return result;
        }

        /// <summary>
        ///     A simple graph search using a custom comparision predicate.
        ///     The predicate is used to find nodes based on the evaluation of the predicate at each graph vertex.
        /// </summary>
        /// <param name="compareFunct">
        ///     Node comparision predicate method used to compare nodes in the scene graph.
        /// </param>
        /// <param name="traversalType">
        ///     The traversal method used to perform the search.
        /// </param>
        /// <param name="findAllMatches">
        ///     Enabled by default, returning the set of all matches the predicate satisfies.
        ///     If false, quits the search upon finding the first node that matches the predicate requirements.
        /// </param>
        /// <returns>
        ///     A node within the scene graph that coresponds to the predicate search criteria external to the query
        ///     implementation.
        /// </returns>
        public List<SceneGraphNode> Query
        (
            Predicate<SceneGraphNode> compareFunct,
            GraphQueryTraversalType traversalType = GraphQueryTraversalType.DepthFirst,
            bool findAllMatches = true)
        {
            List<SceneGraphNode> result;
            SceneGraphNode node;
            Stack<SceneGraphNode> work_items_s;
            Queue<SceneGraphNode> work_items_q;
            SceneGraphNode root;
            bool traversing;
            int i;
            SceneGraphNode child;

            result = new List<SceneGraphNode>();
            root = GetRoot();
            traversing = true;

            if (traversalType == GraphQueryTraversalType.DepthFirst)
            {
                // INTERATIVE DEPTH FIRST SEARCH
                work_items_s = new Stack<SceneGraphNode>();
                work_items_s.Push(root);

                while (work_items_s.Count > 0 && traversing)
                {
                    node = work_items_s.Pop();

                    if (compareFunct(node))
                    {
                        result.Add(node);

                        traversing = findAllMatches;
                    }

                    for (i = 0; i < node.Children.Count; i++)
                    {
                        child = node.Children[i];

                        work_items_s.Push(child);
                    }
                }
            }
            else if (traversalType == GraphQueryTraversalType.BredthFirst)
            {
                // ITERATIVE BREDTH FIRST SEARCH
                work_items_q = new Queue<SceneGraphNode>();
                work_items_q.Enqueue(root);

                while (work_items_q.Count > 0 && traversing)
                {
                    node = work_items_q.Dequeue();

                    if (compareFunct(node))
                    {
                        result.Add(node);

                        traversing = findAllMatches;
                    }

                    for (i = 0; i < node.Children.Count; i++)
                    {
                        child = node.Children[i];

                        work_items_q.Enqueue(child);
                    }
                }
            }
            else
            {
                throw new Exception("Unsupported graph traversal type");
            }

            return result;
        }

        #endregion

        #region Public Static Method

        public static List<SceneGraphNode> QueryDFS(SceneGraphNode root, Predicate<SceneGraphNode> compareFunct)
        {
            List<SceneGraphNode> result;
            SceneGraph sg;

            sg = new SceneGraph(root);

            result = sg.Query(compareFunct);

            return result;
        }

        public static List<SceneGraphNode> QueryBFS(SceneGraphNode root, Predicate<SceneGraphNode> compareFunct)
        {
            List<SceneGraphNode> result;
            SceneGraph sg;

            sg = new SceneGraph(root);

            result = sg.Query(compareFunct, GraphQueryTraversalType.BredthFirst);

            return result;
        }

        #endregion

        #region Private Methods

        private int make_id()
        {
            __lastid++;
            return __lastid;
        }

        /// <summary>
        ///     Build X3D Scene Graph using XDocument
        /// </summary>
        private void build_scene_iterativily_xdoc(XDocument doc)
        {
            SceneGraphNode parent;
            SceneGraphNode child;
            SceneGraphNode def;
            Queue<_graph_node> work_items;
            _graph_node xmlElem;
            bool hasChildren;
            int current_depth;
            int i;
            IEnumerable<XElement> children;
            IXmlLineInfo lineInfo;

            current_depth = 0;
            parent = null;
            work_items = new Queue<_graph_node>();
            work_items.Enqueue(new _graph_node { node = doc.Root });

            while (work_items.Count > 0)
            {
                xmlElem = work_items.Dequeue();

                child = XMLParser.ParseXMLElement(xmlElem.node);

                parent = xmlElem.parent;
                hasChildren = xmlElem.node.Elements().Count() > 0;

                #region Process Node

                if (child != null)
                {
                    lineInfo = xmlElem.node;

                    child.XMLDocumentLocation = new Vector2(lineInfo.LinePosition, lineInfo.LineNumber);
                    child._id = make_id();
                    current_depth = parent == null ? 0 : parent.Depth + 1;
                    child.Depth = current_depth;

#if DEBUG_SCENE_GRAPH
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("".PadLeft(current_depth,'»')+xmlElem.node.Name.LocalName +" "+child.DEF+" "+current_depth.ToString()+"/"+ child._id.ToString());
#endif


                    if (parent != null)
                    {
                        child.Parent = parent;
                        child.Parents.Add(parent);

                        //TODO: implement containerField
                        parent.Children.Add(child);

                        // Apply Container Field
                        if (!string.IsNullOrEmpty(child.containerField) && child.containerField != "children")
                        {
                            if (parent.HasAttribute(child.containerField))
                                parent.setAttribute(child.containerField, child);
                            else
                                Console.WriteLine(
                                    "[warning] could not apply containerField reference \"{0}\" to parent \"{1}\"",
                                    child.containerField, parent);
                        }
                    }
                    else
                    {
                        _root = child;
                    }

                    if (hasChildren)
                        child.IsLeaf = false;
                    else
                        child.IsLeaf = true;

                    //child.ContinueDeserialization();
                    // ... DEF_use

                    #region DEF/USE

                    if (!string.IsNullOrEmpty(child.DEF))
                    {
                        child.USE = string.Empty;

                        if (defUseScope.ContainsKey(child.DEF))
                            defUseScope[child.DEF] = child; // override existing
                        else
                            defUseScope.Add(child.DEF, child);
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
                        def = QueryDFS(_root, n => n.DEF == child.USE).FirstOrDefault();

                        if (def != null && child.Parent != null)
                        {
                            def.Parents.Add(child.Parent);

                            child.Parent.Children.Add(def);
                        }
                    }

                    #endregion

                    #region Class

                    if (!string.IsNullOrEmpty(child.@class))
                    {
                        List<SceneGraphNode> lstClasses;

                        if (classScope.ContainsKey(child.@class))
                            lstClasses = classScope[child.@class];
                        else
                            lstClasses = new List<SceneGraphNode>();

                        lstClasses.Add(child);

                        classScope[child.@class] = lstClasses;
                    }

                    #endregion

                    #region Event Model

                    if (child.GetType() == typeof(ROUTE))
                        // Quick way to get all ROUTE nodes
                        Routes.Add((ROUTE)child);

                    #endregion

                    #region Prototyping

                    if (!string.IsNullOrEmpty(child.name))
                    {
                        nameScope.Add(new KeyValuePair<string, SceneGraphNode>(child.name, child));

                        if (child.GetType() == typeof(ProtoInstance))
                        {
                            var protoInstance = (ProtoInstance)child;

                            var _value = nameScope.FirstOrDefault(n => n.Key == child.name); // slow

                            if (_value.Value != null)
                                protoInstance.Prototype = (ProtoDeclare)_value.Value;
                            else
                                Console.WriteLine(
                                    "[Warning] Could not immediatly find ProtoDeclare \"{0}\". Placing ProtoDeclare above ProtoInstance usually fixes this warning.",
                                    child.name);
                        }
                    }

                    if (child.GetType() == typeof(ProtoDeclare))
                    {
                        child.Hidden = true; // Not renderable.
                        child.PassthroughAllowed =
                            false; // not considered part of the Runtime SceneGraph or EventGraph, 
                        // ProtoDeclare is only a SceneGraphStructureStatement.

                        // Only ProtoInstance can access its ProtoDeclare
                        // Events are not passed in to where the prototype is declared,
                        // instead, ProtoInstance creates a new shadow-instance of the ProtoDeclare. 
                        // All the nodes under the proto declare are shadow-copied under the ProtoInstance
                        // becoming part of the Scene Graph again as a new instance but managed explicitly by ProtoInstance.
                    }

                    #endregion

                    child.PostDeserialization();

                    if (child.IsLeaf)
                        // BACKTRACK
                        if (parent != null && parent.Children.Count - 1 == parent.Children.IndexOf(child))
                            parent.PostDescendantDeserialization();
                }

                #endregion

                #region Process Children

                if (hasChildren)
                {
                    parent = child;
                    children = xmlElem.node.Elements();

                    if (parent.IsLeaf == false && parent.Children.Count > 0)
                        foreach (var ch in parent.Children)
                        foreach (var sibling in parent.Children)
                            if (ch != sibling && ch.Siblings.Contains(sibling) == false)
                            {
                                ch.Siblings.Add(sibling);
                                sibling.Siblings.Add(ch);
                            }

                    for (i = 0; i < children.Count(); i++)
                        work_items.Enqueue(new _graph_node
                        {
                            node = children.ElementAt(i),
                            parent = parent
                        });
                }

                #endregion
            }
        }


        /// <summary>
        ///     Build X3D Scene Graph using XPathNavigator
        /// </summary>
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

            nav.MoveToFirstChild();

            while (nav.NodeType != XPathNodeType.Root && !nav.IsSamePosition(startPosition))
            {
                child = XMLParser.ParseXMLElement(nav);

                if (child != null)
                {
                    IXmlLineInfo lineInfo;
                    lineInfo = (IXmlLineInfo)nav;

                    child.XMLDocumentLocation = new Vector2(lineInfo.LinePosition, lineInfo.LineNumber);
                    child._id = make_id();
                    child.Depth = current_depth;

#if DEBUG_SCENE_GRAPH
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("".PadLeft(current_depth,'»')+nav.Name+" "+child.DEF+" "+current_depth.ToString()+"/"+ child._id.ToString());
#endif


                    if (parent != null)
                    {
                        child.Parent = parent;
                        child.Parents.Add(parent);

                        //TODO: implement containerField
                        parent.Children.Add(child);

                        // Apply Container Field
                        if (!string.IsNullOrEmpty(child.containerField) && child.containerField != "children")
                        {
                            if (parent.HasAttribute(child.containerField))
                                parent.setAttribute(child.containerField, child);
                            else
                                Console.WriteLine(
                                    "[warning] could not apply containerField reference \"{0}\" to parent \"{1}\"",
                                    child.containerField, parent);
                        }
                    }
                    else
                    {
                        _root = child;
                    }

                    if (nav.HasChildren)
                        child.IsLeaf = false;
                    else
                        child.IsLeaf = true;

                    //child.ContinueDeserialization();
                    // ... DEF_use

                    #region DEF/USE

                    if (!string.IsNullOrEmpty(child.DEF))
                    {
                        child.USE = string.Empty;

                        if (defUseScope.ContainsKey(child.DEF))
                            defUseScope[child.DEF] = child; // override existing
                        else
                            defUseScope.Add(child.DEF, child);
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
                        def = QueryDFS(_root, n => n.DEF == child.USE).FirstOrDefault();

                        if (def != null && child.Parent != null)
                        {
                            def.Parents.Add(child.Parent);

                            child.Parent.Children.Add(def);
                        }
                    }

                    #endregion

                    #region Event Model

                    if (child.GetType() == typeof(ROUTE))
                        // Quick way to get all ROUTE nodes
                        Routes.Add((ROUTE)child);

                    #endregion

                    #region Prototyping

                    if (!string.IsNullOrEmpty(child.name))
                    {
                        nameScope.Add(new KeyValuePair<string, SceneGraphNode>(child.name, child));

                        if (child.GetType() == typeof(ProtoInstance))
                        {
                            var protoInstance = (ProtoInstance)child;

                            var _value = nameScope.FirstOrDefault(n => n.Key == child.name); // slow

                            if (_value.Value != null)
                                protoInstance.Prototype = (ProtoDeclare)_value.Value;
                            else
                                Console.WriteLine(
                                    "[Warning] Could not immediatly find ProtoDeclare \"{0}\". Placing ProtoDeclare above ProtoInstance usually fixes this warning.",
                                    child.name);
                        }
                    }

                    if (child.GetType() == typeof(ProtoDeclare))
                    {
                        child.Hidden = true; // Not renderable.
                        child.PassthroughAllowed =
                            false; // not considered part of the Runtime SceneGraph or EventGraph, 
                        // ProtoDeclare is only a SceneGraphStructureStatement.

                        // Only ProtoInstance can access its ProtoDeclare
                        // Events are not passed in to where the prototype is declared,
                        // instead, ProtoInstance creates a new shadow-instance of the ProtoDeclare. 
                        // All the nodes under the proto declare are shadow-copied under the ProtoInstance
                        // becoming part of the Scene Graph again as a new instance but managed explicitly by ProtoInstance.
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
                        do
                        {
                            if (nav.MoveToParent() == false) break;
                            if (parent != null)
                            {
                                parent.PostDescendantDeserialization();

                                if (parent.IsLeaf == false && parent.Children.Count > 0)
                                    foreach (var ch in parent.Children)
                                    foreach (var sibling in parent.Children)
                                        if (ch != sibling && ch.Siblings.Contains(sibling) == false)
                                        {
                                            ch.Siblings.Add(sibling);
                                            sibling.Siblings.Add(ch);
                                        }

                                /* Go back to the previous depth to follow the nav: */
                                parent = parent.Parent;
                                current_depth--;
                            }
                        } while (nav.MoveToNext(XPathNodeType.Element) == false);
                }
            }
        }

        #endregion
    }
}