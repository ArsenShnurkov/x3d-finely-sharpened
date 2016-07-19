using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace X3D
{
    public partial class Viewpoint
    {
        public const string VIEWPOINT_DEFAULT_DESCRIPTION = "Origin";
        public static Viewpoint CurrentViewpoint = null;

        public override void Load()
        {
            base.Load();
        }

        public override void Render(RenderingContext rc)
        {
            base.Render(rc);

            CurrentViewpoint = this; // for now until ViewpointGroup is implemented

            rc.Translate(this.Position);
            rc.Rotate(this.Orientation, this.CenterOfRotation);
            
            rc.PushMatricies();
        }

        public override void PostRender(RenderingContext rc)
        {
            base.PostRender(rc);

            rc.PopMatricies();
        }
    }
}
