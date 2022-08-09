using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace X3D
{
    public partial class IS
    {
        /// <summary>
        ///     The Base definition the ProtoDeclare is "derived" from in a sense in the X3D Runtime.
        /// </summary>
        [XmlIgnore] public SceneGraphNode BaseDefinition;

        [XmlIgnore] public List<connect> Connections;

        [XmlIgnore] public List<field> Fields;

        [XmlIgnore] public ProtoDeclare ProtoDeclaration;

        [XmlIgnore] public ProtoInterface ProtoInterface;

        [XmlIgnore] public ProtoBody PrototypeBody;

        public override void Load()
        {
            base.Load();

            BaseDefinition = Parent;
            PrototypeBody = BaseDefinition != null ? (ProtoBody)BaseDefinition.Parent : null;
            ProtoDeclaration = PrototypeBody != null ? (ProtoDeclare)PrototypeBody.Parent : null;
            ProtoInterface = ProtoDeclaration != null
                ? ProtoDeclaration.ItemsByType<ProtoInterface>().FirstOrDefault()
                : null;

            if (ProtoInterface != null)
            {
                Console.WriteLine("Initilizing prototype IS.");

                Fields = ProtoInterface.ItemsByType<field>();

                Connections = ItemsByType<connect>();

                ProtoDeclaration.Initilize(BaseDefinition, Fields, Connections);
            }
            else
            {
                Console.WriteLine("ProtoInterface could not be found within IS {0}", ErrorStringWithLineNumbers());
            }
        }

        public override void Render(RenderingContext rc)
        {
            base.Render(rc);
        }
    }
}