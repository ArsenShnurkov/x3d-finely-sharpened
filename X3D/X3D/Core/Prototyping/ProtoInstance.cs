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
        private bool hasPrototyped = false;

        /// <summary>
        /// The reference to the declaration of the actual implementation of this ProtoInstance.
        /// Referenced by @name attribute.
        /// </summary>
        [XmlIgnore]
        public ProtoDeclare Prototype { get; set; } // NOTE: scene graph sets this field automatically if ProtoDeclare is declared above ProtoInstance so we dont have to search for node

        public override void Load()
        {
            base.Load();

            fieldValues = ItemsByType<fieldValue>();
            fieldValue value;

            // Assign default values to imported ProtoDeclare:
            foreach (field f in Prototype.Fields)
            {
                value = fieldValues.First(fv => fv.name == f.name);

                f.value = value.value;
            }
        }

        public override void Render(RenderingContext rc)
        {
            base.Render(rc);

            // Use the container field to update the parent periodically
            // from what changes within the Prototype

            //this.Parent.setAttribute(this.containerField, Prototype.BaseDefinition);

            // ... Or use access and update the Children like this:

            SceneGraphNode @base;

            
            //bool found = false;
            //int i;
            //SceneGraphNode c;
            //SceneGraphNode updatee;
            SceneGraphNode newBase;

            if (!hasPrototyped)
            {
                // Use direct references to set field attribute of object in parent from field in ProtoInterface
                // --                                                                                          --
                // Put a new instance of BaseDefinition as a child of parent,
                // then overwrite BaseDefinition instance in ProtoBody with same one that is under target parent.

                @base = Prototype.BaseDefinition;
                @base.IsPrototypeBase = true;

                newBase = (SceneGraphNode)Activator.CreateInstance(@base.GetType());

                Parent.Children.Add(newBase);

                Prototype.BaseDefinition = newBase;


                //for (i = 0; i < Parent.Children.Count && !found; i++)
                //{
                //    c = Parent.Children[i];

                //    if (c.IsPrototypeBase && c._id == @base._id)
                //    {
                //        updatee = c;
                //        found = true;
                //    }
                //}

                //if (!found)
                //{




                //@base.Parent = this.Parent;

                //@base.Parents.Add(this.Parent);

                //this.Parent.Children.Add(@base);
                //}
                //else
                //{
                //    this.Parent.Children[i] = @base;
                //}



                hasPrototyped = true;
            }



        }
    }
}
