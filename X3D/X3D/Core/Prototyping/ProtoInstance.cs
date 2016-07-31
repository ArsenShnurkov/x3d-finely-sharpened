using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using X3D.Parser;
using System.Xml.Serialization;

namespace X3D
{
    public partial class ProtoInstance
    {
        private List<fieldValue> fieldValues;

        /// <summary>
        /// The reference to the declaration of the actual implementation of this ProtoInstance.
        /// Referenced by @name attribute.
        /// </summary>
        [XmlIgnore]
        public ProtoDeclare Prototype { get; set; } //TODO: scene graph sets this field automatically so we dont have to search for node

        public override void Load()
        {
            base.Load();

            fieldValues = ItemsByType<fieldValue>();


        }

        public override void Render(RenderingContext rc)
        {
            base.Render(rc);

            
        }
    }
}
