using System.Xml.Serialization;
using OpenTK;

namespace X3D
{
    public partial class Transform
    {
        private bool _isHidden;

        /// <summary>
        ///     If true, disabled the node from any Render() calls. KeepAlive() is then called instead.
        /// </summary>
        [XmlIgnore]
        public new bool Hidden
        {
            get => _isHidden;
            set => _isHidden = value;
        }

        public void ApplyTranslation(Vector3 translation)
        {
            Translation += translation;
        }

        public void ApplyRotation(Quaternion rotation)
        {
            //this.rotra
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

        #region Public Static Methods

        public static Transform Create(Vector3 translation, Vector3 scale, Vector4 rotation)
        {
            return new Transform
            {
                Translation = translation,
                Scale = scale,
                Rotation = rotation
            };
        }

        public static Transform CreateTranslation(Vector3 translation)
        {
            return new Transform
            {
                Translation = translation
            };
        }

        public static Transform CreateScale(Vector3 scale)
        {
            return new Transform
            {
                Scale = scale
            };
        }

        public static Transform CreateRotation(Vector4 rotation)
        {
            return new Transform
            {
                Rotation = rotation
            };
        }

        #endregion
    }
}