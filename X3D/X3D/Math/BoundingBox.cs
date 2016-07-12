using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace X3D
{
    public class BoundingBox
    {
        public float Width, Height, Depth;

        public BoundingBox()
        {
            this.Width = 0f;
            this.Height = 0f;
            this.Depth = 0f;
        }

        public BoundingBox(float x, float y, float w, float h, float d)
        {
            this.Width = w;
            this.Height = h;
            this.Depth = d;
        }
    }
}
