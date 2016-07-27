using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace X3D
{
    public partial class Transform
    {
        public override void Render(RenderingContext rc)
        {
            base.Render(rc);

            rc.PushMatricies();

            // following code below doesnt work because of depth first traversal

            //rc.Translate(this.Translation);
            //rc.Scale(this.Scale, this.ScaleOrientation);
            //rc.Rotate(this.Rotation);

            
        }

        public override void PostRender(RenderingContext rc)
        {
            base.PostRender(rc);

            rc.PopMatricies();
        }
    }
}
