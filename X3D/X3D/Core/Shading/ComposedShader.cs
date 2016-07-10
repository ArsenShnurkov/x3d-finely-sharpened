using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace X3D
{
    public partial class ComposedShader
    {
        //public int ShaderHandle;

        [XmlIgnore]
        public List<field> Fields { get; set; }
        [XmlIgnore]
        public List<ShaderPart> ShaderParts { get; set; }

        public override void Load()
        {
            Fields = new List<field>();
            ShaderParts = new List<ShaderPart>();
            base.Load();
        }

        public override void PostDescendantDeserialization()
        {
            base.PostDescendantDeserialization();

            GetParent<Shape>().IncludeComposedShader(this);
        }
    }
}
