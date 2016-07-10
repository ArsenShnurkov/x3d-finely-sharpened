using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using X3D.Parser;
using X3D.Engine.Shading;

namespace X3D
{
    public partial class IndexedTriangleSet
    {
        internal Coordinate coordinate;
        private TextureCoordinate texCoordinate;
        internal Vector3[] _coords;
        private float[] color;
        private Color colorNode;
        private ColorRGBA colorRGBANode;
        private Vector2[] _texCoords;
        private const int RESTART_INDEX = -1;
        private Shape parentShape;

        private BoundingBox _bbox;
        private bool RGBA = false, RGB = false, coloring = false, texturing = false;
        private int _vbo_interleaved, _vbo_interleaved4;
        private int NumVerticies, NumVerticies4;

        public override void PreRenderOnce(RenderingContext rc)
        {
            base.PreRenderOnce(rc);

            int? restartIndex = null;

            parentShape = GetParent<Shape>();

            texturing = texCoordinate != null || parentShape.texturingEnabled;



            texCoordinate = (TextureCoordinate)this.Items.FirstOrDefault(n => n.GetType() == typeof(TextureCoordinate));
            coordinate = (Coordinate)this.Items.FirstOrDefault(n => n.GetType() == typeof(Coordinate));
            colorNode = (Color)this.Items.FirstOrDefault(n => n.GetType() == typeof(Color));
            colorRGBANode = (ColorRGBA)this.Items.FirstOrDefault(n => n.GetType() == typeof(ColorRGBA));

            RGBA = colorRGBANode != null;
            RGB = colorNode != null;
            coloring = RGBA || RGB;


            if (RGB && !RGBA)
            {
                color = Helpers.Floats(colorNode.color);
            }
            else if (RGBA && !RGB)
            {
                color = Helpers.Floats(colorRGBANode.color);
            }

            if (this.texCoordinate != null)
            {
                _texCoords = Helpers.MFVec2f(this.texCoordinate.point);
            }

            if (this.coordinate != null)
            {
                _coords = Helpers.MFVec3f(this.coordinate.point);

                restartIndex = null;

                this._bbox = MathHelpers.CalcBoundingBox(this, restartIndex);

                Buffering.Interleave(parentShape, this._bbox,
                    out _vbo_interleaved, out NumVerticies,
                    out _vbo_interleaved4, out NumVerticies4, 
                    this._indicies, null, _coords,
                    null, null, null, color, restartIndex, true,
                    true, coloring, this.texturing);
            }

            GL.UseProgram(parentShape.CurrentShader.ShaderHandle);

            int uniformSize = GL.GetUniformLocation(parentShape.CurrentShader.ShaderHandle, "size");
            int uniformScale = GL.GetUniformLocation(parentShape.CurrentShader.ShaderHandle, "scale");

            var size = new Vector3(1, 1, 1);
            var scale = new Vector3(0.05f, 0.05f, 0.05f);

            GL.Uniform3(uniformSize, size);
            GL.Uniform3(uniformScale, scale);

            Console.WriteLine("IndexedFaceSet [loaded]");
        }

        public override void Load()
        {
            base.Load();
        }

        public override void Render(RenderingContext rc)
        {
            base.Render(rc);

            if (this.coordinate != null && this._indicies != null && this._indicies.Length > 0)
            {
                GL.UseProgram(parentShape.CurrentShader.ShaderHandle);

                if (NumVerticies > 0)
                {
                    GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo_interleaved);
                    GL.DrawArrays(PrimitiveType.Triangles, 0, NumVerticies);
                }
            }
        }
    }
}
