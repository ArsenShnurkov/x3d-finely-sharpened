using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using X3D.Core;
using X3D.Core.Shading;
using X3D.Parser;

namespace X3D
{
    public partial class IndexedTriangleSet
    {
        internal PackedGeometry _pack;
        internal Coordinate coordinate;
        private TextureCoordinate texCoordinate;
        internal Vector3[] _coords;
        private float[] color;
        private Color colorNode;
        private ColorRGBA colorRGBANode;
        private Vector2[] _texCoords;
        private const int RESTART_INDEX = -1;
        //private Shape parentShape;

        //private BoundingBox _bbox;
        private bool RGBA = false, RGB = false, coloring = false, texturing = false;
        //private int _vbo_interleaved, _vbo_interleaved4;
        //private int NumVerticies, NumVerticies4;

        //private readonly float tessLevelInner = 137; // 3
        //private readonly float tessLevelOuter = 115; // 2

        public override void CollectGeometry(
                                    RenderingContext rc,
                                    out GeometryHandle handle, 
                                    out BoundingBox bbox,
                                    out bool coloring,
                                    out bool texturing)
        {
            handle = GeometryHandle.Zero;
            bbox = BoundingBox.Zero;

            // INTERLEAVE
            _pack = PackedGeometry.Pack(this);

            coloring = _pack.Coloring;
            texturing = _pack.Texturing;
            bbox = _pack.bbox;

            handle = Buffering.BufferShaderGeometry(_pack);
        }

        //public override void PreRenderOnce(RenderingContext rc)
        //{
        //    base.PreRenderOnce(rc);

        //    parentShape = GetParent<Shape>();

        //    GL.UseProgram(parentShape.CurrentShader.ShaderHandle);

        //    int uniformSize = GL.GetUniformLocation(parentShape.CurrentShader.ShaderHandle, "size");
        //    int uniformScale = GL.GetUniformLocation(parentShape.CurrentShader.ShaderHandle, "scale");

        //    var size = new Vector3(1, 1, 1);
        //    var scale = new Vector3(0.05f, 0.05f, 0.05f);

        //    GL.Uniform3(uniformSize, size);
        //    GL.Uniform3(uniformScale, scale);

        //    Console.WriteLine("IndexedFaceSet [loaded]");
        //}

        //public override void Load()
        //{
        //    base.Load();
        //}

        //public override void Render(RenderingContext rc)
        //{
        //    base.Render(rc);
        //}
    }
}
