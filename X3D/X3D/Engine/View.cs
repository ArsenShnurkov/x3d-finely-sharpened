using OpenTK;

namespace X3D.Engine
{
    public class View
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }


        public int Left => X;

        public int Right => X + Width;

        public int Top => Y;

        public int Bottom => Y + Height;


        public static View CreateViewFromWindow(INativeWindow window)
        {
            var view = new View
            {
                X = window.Bounds.X,
                Y = window.Bounds.Y,
                Width = window.Bounds.Width,
                Height = window.Bounds.Height
            };

            WindowFunction.SetView(view);

            return view;
        }

        //TODO: create left and right steroscopic methods
    }
}