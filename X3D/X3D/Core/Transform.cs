using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Xml.Serialization;

namespace X3D
{
    public partial class Transform
    {
        private bool _isHidden;

        public void ApplyTranslation(Vector3 translation)
        {
            this.Translation += translation;

        }

        public void ApplyRotation(Quaternion rotation)
        {
            //this.rotra
        }

        /// <summary>
        /// If true, disabled the node from any Render() calls. KeepAlive() is then called instead.
        /// </summary>
        [XmlIgnore]
        public new bool Hidden
        {
            get
            {
                return _isHidden;
            }
            set
            {
                _isHidden = value;
            }
        }


        public override void Render(RenderingContext rc)
        {
            base.Render(rc);

            rc.PushMatricies();

            // following code below doesnt work because of depth first traversal

            //rc.Translate(this.Translation);
            //rc.Scale(this.Scale, this.ScaleOrientation);
            //rc.Rotate(this.Rotation);

            
        }

        public override void PostRender(RenderingContext rc)
        {
            base.PostRender(rc);

            rc.PopMatricies();
        }
    }
}
