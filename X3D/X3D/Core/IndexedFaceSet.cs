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
    public partial class IndexedFaceSet
    {
        internal Coordinate coordinate;
        private TextureCoordinate texCoordinate;
        internal int[] _indices;
        internal Vector3[] _coords;
        private int[] _colorIndicies;
        private float[] color;
        private Color colorNode;
        private ColorRGBA colorRGBANode;
        private int[] _texIndices;
        private Vector2[] _texCoords;
        private const int RESTART_INDEX = -1;
        private Shape parentShape;

        private BoundingBox _bbox;
        private bool RGBA = false, RGB = false, coloring = false, texturing = false, generateColorMap = false;
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
            generateColorMap = coloring;


            if (RGB && !RGBA)
            {
                color = X3DTypeConverters.Floats(colorNode.color);
            }
            else if (RGBA && !RGB)
            {
                color = X3DTypeConverters.Floats(colorRGBANode.color);
            }

            if (this.texCoordinate != null && !string.IsNullOrEmpty(this.texCoordIndex))
            {
                _texIndices = X3DTypeConverters.ParseIndicies(this.texCoordIndex);
                _texCoords = X3DTypeConverters.MFVec2f(this.texCoordinate.point);
            }

            if (this.coordinate != null && !string.IsNullOrEmpty(this.coordIndex))
            {
                _indices = X3DTypeConverters.ParseIndicies(this.coordIndex);
                _coords = X3DTypeConverters.MFVec3f(this.coordinate.point);

                if (!string.IsNullOrEmpty(this.colorIndex))
                {
                    _colorIndicies = X3DTypeConverters.ParseIndicies(this.colorIndex);
                }

                if (this.coordIndex.Contains(RESTART_INDEX.ToString()))
                {
                    restartIndex = RESTART_INDEX;
                }

                this._bbox = MathHelpers.CalcBoundingBox(this, restartIndex);

                Buffering.Interleave(parentShape, this._bbox, 
                    out _vbo_interleaved, out NumVerticies,
                    out _vbo_interleaved4, out NumVerticies4,
                    _indices, _texIndices, _coords,
                    _texCoords, null, _colorIndicies, color, restartIndex, true,
                    this.colorPerVertex, this.coloring, this.texturing);
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

            if(this.coordinate != null && !string.IsNullOrEmpty(this.coordIndex))
            {
                GL.UseProgram(parentShape.CurrentShader.ShaderHandle);

                if (NumVerticies > 0)
                {
                    GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo_interleaved);
                    Buffering.ApplyBufferPointers(parentShape.uniforms);
                    GL.DrawArrays(PrimitiveType.Triangles, 0, NumVerticies);
                }

                if(NumVerticies4 > 0)
                {
                    GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo_interleaved4);
                    Buffering.ApplyBufferPointers(parentShape.uniforms);
                    GL.DrawArrays(PrimitiveType.Quads, 0, NumVerticies4);
                }
            }
        }
    }
}
