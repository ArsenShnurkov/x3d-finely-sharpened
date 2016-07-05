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

            rc.Translate(this.Translation);
            rc.Scale(this.Scale);
            rc.Rotate(this.Rotation);

            rc.PushMatricies();
        }

        public override void PostRender(RenderingContext rc)
        {
            base.PostRender(rc);

            rc.PopMatricies();
        }
    }
}
