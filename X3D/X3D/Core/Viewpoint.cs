using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace X3D
{
    public partial class Viewpoint
    {
        public override void Load()
        {
            base.Load();
        }

        public override void Render(RenderingContext rc)
        {
            base.Render(rc);

            //rc.Translate(this.Position);
            //rc.Rotate(this.Orientation, this.CenterOfRotation);
            
            //rc.PushMatricies();
        }

        public override void PostRender(RenderingContext rc)
        {
            base.PostRender(rc);

            //rc.PopMatricies();
        }
    }
}
