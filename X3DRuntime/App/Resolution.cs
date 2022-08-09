namespace X3D.Runtime
{
    public sealed class Resolution
    {
        public Resolution(int w, int h)
        {
            Width = w;
            Height = h;
        }

        public static Resolution Size800x600 => new Resolution(800, 600);

        public static Resolution Size1024x768 => new Resolution(1024, 768);

        public int Width { get; set; }
        public int Height { get; set; }
    }
}