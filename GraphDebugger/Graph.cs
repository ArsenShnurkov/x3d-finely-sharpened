using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RadialTreeGraph
{
    public class Graph
    {
        public List<TNode> Roots { get; set; }
        //public List<Edge> Edges { get; set; }

        public Graph(List<TNode> roots)
        {
            this.Roots = roots;
        }

        public Tree CreateTree()
        {
            Tree t;
            TNode root;

            root = new TNode();
            root.Children = this.Roots;

            t = new Tree(root);

            return t;
        }

        public void AddEdge(Edge edge) {}
        public void DetachNode(TNode n) { }
        public void RemoveEdge(Edge edge) { }
    }

}