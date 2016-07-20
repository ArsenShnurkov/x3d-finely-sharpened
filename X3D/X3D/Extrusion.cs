using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace X3D
{
    /// <summary>
    /// http://www.web3d.org/documents/specifications/19775-1/V3.2/Part01/components/geometry3D.html#Extrusion
    /// </summary>
    public partial class Extrusion
    {

        public override void Load()
        {
            base.Load();

            // Compute the Spine-aligned cross-section plane (SCP)
            // (remembering computation only has to be done once at load)


        }



        public override void Render(RenderingContext rc)
        {
            base.Render(rc);

            // render the computed SCP 

        }
    }
}
