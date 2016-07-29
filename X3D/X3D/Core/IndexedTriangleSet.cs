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

        private readonly float tessLevelInner = 137; // 3
        private readonly float tessLevelOuter = 115; // 2

        public override void PreRenderOnce(RenderingContext rc)
        {
            base.PreRenderOnce(rc);

            int? restartIndex = null;

            parentShape = GetParent<Shape>();

            texturing = texCoordinate != null || parentShape.texturingEnabled;



            texCoordinate = (TextureCoordinate)this.ChildrenWithAppliedReferences.FirstOrDefault(n => n.GetType() == typeof(TextureCoordinate));
            coordinate = (Coordinate)this.ChildrenWithAppliedReferences.FirstOrDefault(n => n.GetType() == typeof(Coordinate));
            colorNode = (Color)this.ChildrenWithAppliedReferences.FirstOrDefault(n => n.GetType() == typeof(Color));
            colorRGBANode = (ColorRGBA)this.ChildrenWithAppliedReferences.FirstOrDefault(n => n.GetType() == typeof(ColorRGBA));

            RGBA = colorRGBANode != null;
            RGB = colorNode != null;
            coloring = RGBA || RGB;


            if (RGB && !RGBA)
            {
                color = X3DTypeConverters.Floats(colorNode.color);
            }
            else if (RGBA && !RGB)
            {
                color = X3DTypeConverters.Floats(colorRGBANode.color);
            }

            if (this.texCoordinate != null)
            {
                _texCoords = X3DTypeConverters.MFVec2f(this.texCoordinate.point);
            }

            if (this.coordinate != null)
            {
                _coords = X3DTypeConverters.MFVec3f(this.coordinate.point);

                restartIndex = null;

                this._bbox = MathHelpers.CalcBoundingBox(this, restartIndex);

                Buffering.Interleave(this._bbox,
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
                // Refactor tessellation 

                //GL.UseProgram(parentShape.CurrentShader.ShaderHandle);

                //if (NumVerticies > 0)
                //{
                //    GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo_interleaved);
                //    Buffering.ApplyBufferPointers(parentShape.uniforms);
                //    GL.DrawArrays(PrimitiveType.Triangles, 0, NumVerticies);
                //}

                var size = new Vector3(1, 1, 1);
                var scale = new Vector3(1, 1, 1);

                Shape shape;
                int vbo;
                int verticies_num;

                shape = parentShape;

                // Refactor tessellation 

                if (parentShape.ComposedShaders.Any(s => s.Linked))
                {
                    if (parentShape.CurrentShader != null)
                    {
                        parentShape.CurrentShader.Use();

                        parentShape.CurrentShader.SetFieldValue("size", size);
                        parentShape.CurrentShader.SetFieldValue("scale", scale);

                        if (parentShape.CurrentShader.IsTessellator)
                        {
                            Vector4 lightPosition = new Vector4(0.25f, 0.25f, 1f, 0f);

                            if (parentShape.CurrentShader.IsBuiltIn)
                            {
                                // its a built in system shader so we are using the the fixed variable inbuilt tesselator
                                parentShape.CurrentShader.SetFieldValue("TessLevelInner", this.tessLevelInner);
                                parentShape.CurrentShader.SetFieldValue("TessLevelOuter", this.tessLevelOuter);
                            }
                            else
                            {
                                //parentShape.CurrentShader.SetFieldValue("TessLevelInner", parentShape.TessLevelInner);
                                //parentShape.CurrentShader.SetFieldValue("TessLevelOuter", parentShape.TessLevelOuter);
                            }

                            parentShape.CurrentShader.SetFieldValue("normalmatrix", ref parentShape.NormalMatrix);
                            //GL.UniformMatrix3(parentShape.Uniforms.NormalMatrix, false, ref parentShape.NormalMatrix);
                            GL.Uniform3(parentShape.Uniforms.LightPosition, 1, ref lightPosition.X);
                            GL.Uniform3(parentShape.Uniforms.AmbientMaterial, X3DTypeConverters.ToVec3(OpenTK.Graphics.Color4.Aqua)); // 0.04f, 0.04f, 0.04f
                            GL.Uniform3(parentShape.Uniforms.DiffuseMaterial, 0.0f, 0.75f, 0.75f);


                            vbo = _vbo_interleaved;
                            verticies_num = NumVerticies;

                            if (verticies_num > 0)
                            {
                                GL.UseProgram(shape.CurrentShader.ShaderHandle);

                                shape.CurrentShader.SetFieldValue("size", size);
                                shape.CurrentShader.SetFieldValue("scale", scale);

                                GL.PatchParameter(PatchParameterInt.PatchVertices, 3);
                                GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
                                Buffering.ApplyBufferPointers(shape.CurrentShader);
                                //Buffering.ApplyBufferPointers(shape.uniforms);
                                GL.DrawArrays(PrimitiveType.Patches, 0, verticies_num);
                            }

                        }
                        else
                        {
                            vbo = _vbo_interleaved;
                            verticies_num = NumVerticies;

                            if (verticies_num > 0)
                            {
                                GL.UseProgram(shape.CurrentShader.ShaderHandle);

                                shape.CurrentShader.SetFieldValue("size", size);
                                shape.CurrentShader.SetFieldValue("scale", scale);

                                GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
                                Buffering.ApplyBufferPointers(shape.CurrentShader);
                                //Buffering.ApplyBufferPointers(shape.uniforms);
                                GL.DrawArrays(PrimitiveType.Triangles, 0, verticies_num);
                            }

                        }
                    }
                }
            }
        }
    }
}
