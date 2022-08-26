namespace RadialTreeGraph
{
    public class Edge
    {
        public Edge(TNode from, TNode to, double length)
        {
            From = from;
            To = to;
            Length = length;
        }

        public TNode From { get; set; }
        public TNode To { get; set; }
        public double Length { get; set; }
    }
}