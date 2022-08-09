using OpenTK;
using X3D.Core.Shading;
using win = System.Drawing;

namespace X3D
{
    public partial class Box
    {
        internal static BoxGeometry _boxGeometry = new BoxGeometry();
        internal PackedGeometry _pack;
        private Shape parentShape;

        public override void CollectGeometry(
            RenderingContext rc,
            out GeometryHandle handle,
            out BoundingBox bbox,
            out bool Coloring,
            out bool Texturing)
        {
            handle = GeometryHandle.Zero;
            bbox = BoundingBox.Zero;
            Texturing = true;
            Coloring = true;

            parentShape = GetParent<Shape>();


            _pack = new PackedGeometry();
            _pack._indices = _boxGeometry.Indices;
            _pack._coords = _boxGeometry.Vertices;
            //_pack._colorIndicies = _boxGeometry.Colors;
            _pack._texCoords = _boxGeometry.Texcoords;
            _pack.restartIndex = -1;

            _pack.Interleave();

            // BUFFER GEOMETRY
            handle = _pack.CreateHandle();
            //handle = Buffering.BufferShaderGeometry(_pack);

            //this._boxGeometry.Load(parentShape);
        }

        #region Geometry

        public sealed class BoxGeometry : ShapeGeometry
        {
            public BoxGeometry()
            {
                Vertices = new[]
                {
                    new Vector3(-1.0f, -1.0f, 1.0f),
                    new Vector3(1.0f, -1.0f, 1.0f),
                    new Vector3(1.0f, 1.0f, 1.0f),
                    new Vector3(-1.0f, 1.0f, 1.0f),
                    new Vector3(-1.0f, -1.0f, -1.0f),
                    new Vector3(1.0f, -1.0f, -1.0f),
                    new Vector3(1.0f, 1.0f, -1.0f),
                    new Vector3(-1.0f, 1.0f, -1.0f)
                };

                Indices = new[]
                {
                    // front face
                    0, 1, 2, -1,
                    2, 3, 0, -1,
                    // top face
                    3, 2, 6, -1,
                    6, 7, 3, -1,
                    // back face
                    7, 6, 5, -1,
                    5, 4, 7, -1,
                    // left face
                    4, 0, 3, -1,
                    3, 7, 4, -1,
                    // bottom face
                    0, 1, 5, -1,
                    5, 4, 0, -1,
                    // right face
                    1, 5, 6, -1,
                    6, 2, 1, -1
                };

                Normals = new[]
                {
                    new Vector3(-1.0f, -1.0f, 1.0f),
                    new Vector3(1.0f, -1.0f, 1.0f),
                    new Vector3(1.0f, 1.0f, 1.0f),
                    new Vector3(-1.0f, 1.0f, 1.0f),
                    new Vector3(-1.0f, -1.0f, -1.0f),
                    new Vector3(1.0f, -1.0f, -1.0f),
                    new Vector3(1.0f, 1.0f, -1.0f),
                    new Vector3(-1.0f, 1.0f, -1.0f)
                };

                Colors = new[]
                {
                    ColorToRgba32(win.Color.DarkRed),
                    ColorToRgba32(win.Color.DarkRed),
                    ColorToRgba32(win.Color.Green),
                    ColorToRgba32(win.Color.Green),
                    ColorToRgba32(win.Color.DarkRed),
                    ColorToRgba32(win.Color.DarkRed),
                    ColorToRgba32(win.Color.Blue),
                    ColorToRgba32(win.Color.Blue)
                };

                Texcoords = new[]
                {
                    new Vector2(0, 1),
                    new Vector2(1, 1),
                    new Vector2(1, 0),
                    new Vector2(0, 0),
                    new Vector2(0, 1),
                    new Vector2(1, 1),
                    new Vector2(1, 0),
                    new Vector2(0, 0)
                };
            }
        }

        #endregion

        #region Render Methods

        //public override void Load()
        //{
        //    base.Load();

        //}

        //public override void PreRender()
        //{
        //    base.PreRender();
        //}

        //public override void Render(RenderingContext rc)
        //{
        //    base.Render(rc);

        //    Vector3 zeroish = new Vector3(0.05f, 0.05f, 0.05f);

        //    GL.UseProgram(parentShape.CurrentShader.ShaderHandle);

        //    int uniformSize = GL.GetUniformLocation(parentShape.CurrentShader.ShaderHandle, "size");
        //    int uniformScale = GL.GetUniformLocation(parentShape.CurrentShader.ShaderHandle, "scale");

        //    GL.Uniform3(uniformSize, this._vec3);
        //    GL.Uniform3(uniformScale, zeroish);

        //    this._boxGeometry.Render(rc);
        //}

        //public override void PostRender(RenderingContext rc)
        //{
        //    base.PostRender(rc);


        //}

        #endregion
    }
}