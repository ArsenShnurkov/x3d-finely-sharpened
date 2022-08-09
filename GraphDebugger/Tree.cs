using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using OpenTK;
using X3D;
using X3D.Engine;

public delegate int CompareFunct(TNode a, TNode b);

public class Tree
{
    #region Layout Methods

    /// <summary>
    ///     precondition: Root position specified
    /// </summary>
    public void LayoutRadial2D(Vector2 radius, double edge_length, TraversalType type, bool grow)
    {
        var k = new TraversalListIDS<List<TNode>>(type);

        Root.angle = 0;
        Root.leftBisector = TREE_MAX_ANGLE;
        Root.rightBisector = 0;
        Root.leftTangent = TREE_MAX_ANGLE;
        Root.rightTangent = 0;

        int j;
        float i;
        float right_limit;
        float left_limit;
        float slice_space; // angle space for each slice in sector
        float level_arc = 2.0f; // TWO_PI*.25
        float level_ratio; // derived level ratio
        float arc; // arc length for a level

        /* LAYOUT LEVEL */
        float remaning_angle;
        float prev_angle;
        float prev_gap;
        TNode prev_sibling;
        TNode first_child;
        List<TNode> parents;
        TNode parent;
        List<TNode> work_item;

        parents = new List<TNode>();
        parents.Add(Root);
        k.Add(parents);

        while (k.Count > 0)
        {
            prev_angle = Root.angle;
            prev_sibling = first_child = null;
            parents = new List<TNode>();

            work_item = k.Take();

            for (j = 0; j < work_item.Count; j++)
            {
                parent = work_item[j];

                right_limit = parent.RightLimit();
                left_limit = parent.LeftLimit();

                slice_space = (left_limit - right_limit) / parent.Children.Count;

                if (grow) edge_length += 0.01;

                i = 0.5f;
                foreach (TNode child in parent.Children)
                {
                    child.angle = right_limit + (i * slice_space);

                    child.Point.X = Root.Point.X +
                                    (float)((edge_length * child.Level * radius.X) * Math.Cos(child.angle));
                    child.Point.Y = Root.Point.Y +
                                    (float)((edge_length * child.Level * radius.Y) * Math.Sin(child.angle));
                    child.Point.Z = Root.Point.Z;

                    if (child.HasChildren) // Is it a parent node?
                    {
                        if (first_child == null)
                            first_child = child;

                        prev_gap = child.angle - prev_angle;
                        child.prev_gap = prev_gap;
                        child.rightBisector = child.angle - (prev_gap / 2.0f);

                        if (prev_sibling != null)
                            prev_sibling.leftBisector = child.rightBisector;

                        // setup sector space for potential decendants that are positioned from current node

                        if (child.Level == MaxDepth) level_ratio = 1.0f;
                        else level_ratio = (child.Level / (child.Level + 1.0f));

                        arc = level_arc * (float)Math.Asin(level_ratio);

                        child.leftTangent = child.angle + arc;
                        child.rightTangent = child.angle - arc;

                        prev_angle = child.prev_angle = child.angle;
                        prev_sibling = child;

                        parents.Add(child);
                    }

                    i++;
                }
            }

            if (first_child != null)
            {
                // calculate the remaining angle to define the next level/sector

                remaning_angle = TREE_MAX_ANGLE - prev_sibling.angle;

                first_child.rightBisector = (first_child.angle - remaning_angle) / 2.0f;

                prev_sibling.leftBisector = first_child.rightBisector + TREE_MAX_ANGLE;
            }

            if (parents.Count > 0)
                k.Add(parents);
        }
    }

    #endregion

    #region Public Properties

    public TNode Root { get; set; }

    /// <summary>
    ///     Total number of nodes in tree
    /// </summary>
    public int Count { get; internal set; }

    public int MaxDepth { get; internal set; }

    #endregion

    #region Public Fields

    const float TWO_PI = 2f * (float)Math.PI;

    //const double TREE_MAX_ANGLE = Math.PI / 2; /*
    const float TREE_MAX_ANGLE = TWO_PI; // */
    const float TREE_MAX_HORIZ = 5.0f;

    public delegate bool VisitNodeFunct2(TNode node, TraversalListIDS<TNode> k);

    public delegate void VisitNodeFunct(TNode node);

    public delegate void delayFunct();

    #endregion

    #region Public Static Fields

    private static void nop()
    {
    }

    public static delayFunct Delay = nop;

    #endregion

    #region Constructors

    public Tree() : this(root: null)
    {
    }

    public Tree(TNode root)
    {
        this.Root = root;

        if (root == null)
        {
            this.Count = 0;
            this.MaxDepth = 0;
        }
        else
        {
            this.Count = 1;
            this.MaxDepth = 1;
        }
    }

    #endregion

    #region Public Static Methods

    private class _node
    {
        public SceneGraphNode sgn;
        public TNode tnode;

        public _node(TNode tn, SceneGraphNode sgn)
        {
            this.tnode = tn;
            this.sgn = sgn;
        }
    }

    public static Tree CreateTreeFromSceneGraph(SceneGraph graph)
    {
        Tree t;
        Stack<_node> work_items;
        _node n;
        int i;
        _node child;
        SceneGraphNode childsgn;
        TNode childtn;
        SceneGraphNode rootsgn;
        TNode roottn;
        _node root;
        int numNodes;

        t = new Tree();
        rootsgn = graph.GetRoot();
        roottn = new TNode();
        roottn.Data = rootsgn;
        root = new _node(roottn, rootsgn);
        work_items = new Stack<_node>();
        work_items.Push(root);
        numNodes = 0;

        while (work_items.Count > 0)
        {
            n = work_items.Pop();

            n.tnode.Level = n.sgn.Depth;

            numNodes++;

            for (i = 0; i < n.sgn.Children.Count; i++)
            {
                childsgn = n.sgn.Children[i];

                childtn = new TNode()
                {
                    Data = childsgn
                };

                childtn.Parent = n.tnode;
                childtn.Level = n.sgn.Depth;

                child = new _node(childtn, childsgn);

                n.tnode.Children.Add(childtn);

                work_items.Push(child);
            }
        }

        t.Root = root.tnode;
        t.Count = numNodes;

        return t;
    }

    public static Tree CreateBTreeFromList<T>(IEnumerable<T> list) where T : IComparable
    {
        Console.WriteLine("Treeify ...");
        DateTime before = DateTime.Now;

        Tree t = new Tree();

        foreach (T item in list)
        {
            t.InsertBTNode<T>(item, (TNode a, TNode b) =>
            {
                T aa = (T)a.Data;
                T bb = (T)b.Data;

                return aa.CompareTo(bb);
            });
        }

        Console.WriteLine("took " + DateTime.Now.Subtract(before).Seconds.ToString() + " seconds ");

        return t;
    }

    #endregion

    #region Public Methods

    public List<TNode> ToList()
    {
        Stack<TNode> work_items;
        TNode n;
        int i;
        TNode child;
        List<TNode> lst;

        lst = new List<TNode>();
        work_items = new Stack<TNode>();
        work_items.Push(this.Root);

        while (work_items.Count > 0)
        {
            n = work_items.Pop();

            lst.Add(n);

            for (i = 0; i < n.Children.Count; i++)
            {
                child = n.Children[i];

                work_items.Push(child);
            }
        }

        return lst;
    }

    public void UpdateAttributes()
    {
        this.MaxDepth = Root.Level;

        var k = new TraversalListIDS<TNode>(TraversalType.DepthFirst);
        TNode c;
        bool finished = false;

        k.Add(this.Root);

        while (k.Count > 0 && finished == false)
        {
            c = k.Take();

            foreach (TNode child in c.Children)
            {
                child.Parent = c;
                child.Level = c.Level + 1;
                if (child.Level > this.MaxDepth) this.MaxDepth = child.Level;

                k.Add(child);
            }
        }
    }

    /// <summary>
    ///     Connects current tree to parent
    /// </summary>
    public void AssignParent(TNode p)
    {
        //TODO: important pickup angles from parent

        //double prev_gap;
        //double arc;
        //double level_ratio;
        //double level_arc = 2.0;

        Root.Parent = p;
        p.Children.Add(Root);
        Root.Level = p.Level + 1;

        //// right bisector limit
        //prev_gap = c.angle - prev_angle;
        //c.rightBisector = c.angle - (prev_gap / 2.0);


        //// setup sector space for potential decendants that are positioned from current node
        //level_ratio = Level / (Level + 1.0);

        //arc = level_arc * Math.Asin(level_ratio);

        //c.leftTangent = c.angle + arc;
        //c.rightTangent = c.angle - arc;

        //prev_angle = c.angle;
    }

    /// <summary>
    ///     Binary tree insertion. Inserts the node into a position in the tree.
    ///     Ensures a balanced tree.
    /// </summary>
    /// <param name="n">
    ///     The node to insert in the tree.
    /// </param>
    public void InsertBTNode<T>(T o, CompareFunct nodeCompare)
    {
        var k = new Stack<TNode>();
        TNode c;
        TNode n;

        n = new TNode();
        n.Data = o;

        if (n.Level > this.MaxDepth) this.MaxDepth = n.Level;

        if (Root == null)
        {
            Root = n;
        }
        else
        {
            k.Push(Root);

            while (k.Count > 0)
            {
                c = k.Pop();

                int cmpr = nodeCompare(n, c);

                if (cmpr < 0)
                {
                    if (c.Left == null)
                    {
                        //INSERT
                        c.Left = n;
                        n.Parent = c;
                        n.Level = c.Level + 1;
                        n.ChildIndex = c.Children.Count;

                        this.Count++;
                    }
                    else
                    {
                        k.Push(c.Left);
                    }
                }
                else if (cmpr > 0)
                {
                    if (c.Right == null)
                    {
                        //INSERT
                        c.Right = n;
                        n.Parent = c;
                        n.Level = c.Level + 1;
                        n.ChildIndex = c.Children.Count;

                        this.Count++;
                    }
                    else
                    {
                        k.Push(c.Right);
                    }
                }
            }
        }
    }

    #endregion

    #region Traversal Methods

    public List<TNode> FindNearest(Point p, int radius)
    {
        var l = new List<TNode>();

        TraversePreorder((TNode c) =>
        {
            if (c.Point.X > p.X - radius && c.Point.X < p.X + radius
                                         && c.Point.Y > p.Y - radius && c.Point.Y < p.Y + radius)
            {
                l.Add(c);
            }
        }, TraversalType.DepthFirst);

        return l;
    }

    public TNode FindByPoint(Point p, int tolerance)
    {
        return FindByPoint(new PointF(p.X, p.Y), tolerance);
    }

    public TNode FindByPoint(PointF p, double tolerance)
    {
        TNode n = null;

        TraversePreorder((TNode c) =>
        {
            if (c.Point.X > p.X - tolerance && c.Point.X < p.X + tolerance
                                            && c.Point.Y > p.Y - tolerance && c.Point.Y < p.Y + tolerance)
            {
                n = c;
            }
        }, TraversalType.DepthFirst);

        return n;
    }

    public void Traverse(VisitNodeFunct2 visitNode, TraversalType type)
    {
        var ids = new TraversalListIDS<TNode>(type);
        ids.Add(Root);

        while (ids.Count > 0 && visitNode(ids.Take(), ids) == true) ;
    }

    public void TraversePreorder(VisitNodeFunct visitNode, TraversalType type, bool delayVisit = false)
    {
        var visited = new Hashtable();

        Traverse((TNode c, TraversalListIDS<TNode> k) =>
        {
            if (type == TraversalType.DepthFirst || !visited.ContainsKey(c))
            {
                if (delayVisit) Delay();

                visitNode(c);

                if (type == TraversalType.BreadthFirst)
                    visited.Add(c, (c.Level) * (c.ChildIndex + 1));

                foreach (TNode child in c.Children)
                {
                    k.Add(child);
                }
            }

            return true;
        }, type);
    }

    public void TraversePreorder(VisitNodeFunct2 visitNode, TraversalType type, bool delayVisit = false)
    {
        var visited = new Hashtable();
        bool traverse = true;

        Traverse((TNode c, TraversalListIDS<TNode> k) =>
        {
            if (type == TraversalType.DepthFirst || !visited.ContainsKey(c))
            {
                if (delayVisit) Delay();

                traverse = visitNode(c, k);

                if (type == TraversalType.BreadthFirst)
                    visited.Add(c, (c.Level) * (c.ChildIndex + 1));

                foreach (TNode child in c.Children)
                {
                    k.Add(child);
                }
            }

            return traverse;
        }, type);
    }

    #endregion
}