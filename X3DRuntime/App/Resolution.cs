using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace x3druntime.ui.opentk
{
    public sealed class Resolution
    {
        public static Resolution Size800x600 { get { return new Resolution(800, 600); } }

        public static Resolution Size1024x768 { get { return new Resolution(1024, 768); } }

        public int Width { get; set; }
        public int Height { get; set; }

        public Resolution(int w, int h)
        {
            this.Width = w;
            this.Height = h;
        }
    }
}
