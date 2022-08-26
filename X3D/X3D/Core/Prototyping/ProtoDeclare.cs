using System.Collections.Generic;
using System.Xml.Serialization;

namespace X3D
{
    public partial class ProtoDeclare
    {
        [XmlIgnore] public SceneGraphNode BaseDefinition;

        [XmlIgnore] public List<connect> Connections;

        [XmlIgnore] public List<field> Fields;

        internal void Initilize(SceneGraphNode baseDefinition, List<field> fields, List<connect> connections)
        {
            BaseDefinition = baseDefinition;
            Fields = fields;
            Connections = connections;
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