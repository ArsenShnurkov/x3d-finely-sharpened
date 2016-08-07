// Only ProtoInstance can access its ProtoDeclare
// Events are not passed in to where the prototype is declared,
// instead, ProtoInstance creates a new shadow-instance of the ProtoDeclare. 
// All the nodes under the proto declare are shadow-copied under the ProtoInstance
// becoming part of the Scene Graph again as a new instance but managed explicitly by ProtoInstance.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using X3D.Parser;
using System.Xml.Serialization;

namespace X3D
{
    public class _ProtoDeclareInstance
    {
        public ProtoDeclare declare;

        internal _ProtoDeclareInstance()
        {

        }
    }

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

        /// <summary>
        /// Shadow copy of the ProtoDeclare.
        /// Events should be routed to and from this underlying instance leaving the original ProtoDeclare untouched.
        /// Leaving the ProtoDeclare untouched 
        /// </summary>
        internal _ProtoDeclareInstance underlyingInstance;

        private _ProtoDeclareInstance CreateInstance()
        {
            _ProtoDeclareInstance ins;

            ins = new _ProtoDeclareInstance();

            ins.declare = Prototype;


            return ins;
        }

        public override void Load()
        {
            base.Load();

            underlyingInstance = CreateInstance();



            //this.underlyingInstance = 
        }

        public override void PreRenderOnce(RenderingContext rc)
        {
            base.PreRenderOnce(rc);

            fieldValues = ItemsByType<fieldValue>();
            fieldValue value;

            // Assign default values to imported ProtoDeclare:
            foreach (field f in Prototype.Fields)
            {
                value = fieldValues.FirstOrDefault(fv => fv.name == f.name);

                if (value != null)
                {
                    f.value = value.value;
                }

            }
        }

        public override void Render(RenderingContext rc)
        {
            base.Render(rc);

            // Use the container field to update the parent periodically
            // from what changes within the Prototype

            //this.Parent.setAttribute(this.containerField, Prototype.BaseDefinition);

            // ... Or use access and update the Children like this:

            //SceneGraphNode @base;

            
            //bool found = false;
            //int i;
            //SceneGraphNode c;
            //SceneGraphNode updatee;
            //SceneGraphNode newBase;

            if (!hasPrototyped)
            {
                // Use direct references to set field attribute of object in parent from field in ProtoInterface
                // --                                                                                          --
                // Put a new instance of BaseDefinition as a child of parent,
                // then overwrite BaseDefinition instance in ProtoBody with same one that is under target parent.



                //@base = Prototype.BaseDefinition;
                //@base.IsPrototypeBase = true;

                //newBase = (SceneGraphNode)Activator.CreateInstance(@base.GetType());

                //Parent.Children.Add(newBase);

                //Prototype.BaseDefinition = newBase;


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
