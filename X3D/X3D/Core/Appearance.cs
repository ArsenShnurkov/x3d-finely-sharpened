using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace X3D
{
    public partial class Appearance
    {

        #region Private Fields

        private Shape parentShape;

        #endregion

        #region Rendering Methods

        public override void Load()
        {
            base.Load();

            parentShape = GetParent<Shape>();
        }

        public override void PreRenderOnce(RenderingContext rc)
        {
            base.PreRenderOnce(rc);

        }

        public override void Render(RenderingContext rc)
        {
            base.Render(rc);

            
        }

        #endregion
    }
}
