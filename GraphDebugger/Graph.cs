using System.Collections.Generic;

namespace RadialTreeGraph
{
    public class Graph
    {
        //public List<Edge> Edges { get; set; }

        public Graph(List<TNode> roots)
        {
            Roots = roots;
        }

        public List<TNode> Roots { get; set; }

        public Tree CreateTree()
        {
            Tree t;
            TNode root;

            root = new TNode();
            root.Children = Roots;

            t = new Tree(root);

            return t;
        }

        public void AddEdge(Edge edge)
        {
        }

        public void DetachNode(TNode n)
        {
        }

        public void RemoveEdge(Edge edge)
        {
        }
    }
}