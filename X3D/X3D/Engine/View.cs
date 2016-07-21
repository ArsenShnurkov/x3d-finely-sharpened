using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace X3D.Engine
{
    public class View
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }


        public int Left
        {
            get
            {
                return this.X;
            }
        }
        public int Right
        {
            get
            {
                return this.X + this.Width;
            }
        }
        public int Top
        {
            get
            {
                return this.Y;
            }
        }
        public int Bottom
        {
            get
            {
                return this.Y + this.Height;
            }
        }


        public static View CreateViewFromWindow(INativeWindow window)
        {
            View view = new View()
            {
                X = window.Bounds.X,
                Y = window.Bounds.Y,
                Width = window.Bounds.Width,
                Height = window.Bounds.Height
            };

            X3DWindow.SetView(view);

            return view;
        }

        //TODO: create left and right steroscopic methods
    }
}
