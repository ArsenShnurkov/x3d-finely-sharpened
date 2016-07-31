using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using X3D.Parser;
using System.Xml.Serialization;

namespace X3D
{
    public partial class ProtoDeclare
    {
        [XmlIgnore]
        public SceneGraphNode BaseDefinition;

        [XmlIgnore]
        public List<field> Fields;

        [XmlIgnore]
        public List<connect> Connections;

        internal void Initilize(SceneGraphNode baseDefinition, List<field> fields, List<connect> connections)
        {
            this.BaseDefinition = baseDefinition;
            this.Fields = fields;
            this.Connections = connections;


        }

        public override void Load()
        {
            base.Load();


        }

        public override void Render(RenderingContext rc)
        {
            base.Render(rc);


        }


    }
}
