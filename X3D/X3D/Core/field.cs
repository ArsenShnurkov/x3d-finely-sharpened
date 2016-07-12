using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace X3D
{
    public partial class field
    {
        private ComposedShader parentShader;

        public override void Load()
        {
            base.Load();

            parentShader = GetParent<ComposedShader>();

            if(parentShader != null)
            {
                parentShader.Fields.Add(this);
            }
        }
    }
}
