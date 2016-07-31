using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace X3D
{
    /// <summary>
    /// Used by Script, ComposedShader and ProtoInterface 
    /// to provide an API for the event graph and for scripting.
    /// </summary>
    public partial class field
    {
        private ComposedShader parentShader;
        private ProtoInterface parentProtoInterface;

        public field()
        {
            this.accessType = "inputOutput";
        }

        public override void Load()
        {
            base.Load();

            parentShader = GetParent<ComposedShader>();
            parentProtoInterface = GetParent<ProtoInterface>();

            if (parentShader != null)
            {
                parentShader.Fields.Add(this);
            }
        }
    }
}
