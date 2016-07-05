using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace X3D
{
    public class RenderingContext
    {
        public FrameEventArgs e;

        public Matrix4 modelview;
        public Matrix4 projection;

        public Camera cam;
    }
}
