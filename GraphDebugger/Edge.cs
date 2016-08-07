using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RadialTreeGraph
{
    public class Edge
    {
        public TNode From { get; set; }
        public TNode To { get; set; }
        public double Length { get; set; }

        public Edge(TNode from, TNode to, double length)
        {
            this.From = from;
            this.To = to;
            this.Length = length;
        }
    }
}
