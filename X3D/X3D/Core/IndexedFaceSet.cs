// todo implement creaseAngle

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
        internal Normal normal;
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
        private Shape _shape4;
        private ComposedShader quadShader;

        private BoundingBox _bbox;
        private bool RGBA = false, RGB = false, coloring = false, texturing = false, generateColorMap = false;
        private int _vbo_interleaved, _vbo_interleaved4;
        private int NumVerticies, NumVerticies4;

        private readonly float tessLevelInner = 137; // 3
        private readonly float tessLevelOuter = 115; // 2

        List<Vertex> interleaved3;
        List<Vertex> interleaved4;

        public override void PreRenderOnce(RenderingContext rc)
        {
            base.PreRenderOnce(rc);

            int? restartIndex = null;

            parentShape = GetParent<Shape>();

            



            _shape4 = new Shape();
            _shape4.Load();

            texturing = texCoordinate != null || parentShape.texturingEnabled;

            

            texCoordinate = (TextureCoordinate)this.ChildrenWithAppliedReferences.FirstOrDefault(n => n.GetType() == typeof(TextureCoordinate));
            coordinate = (Coordinate)this.ChildrenWithAppliedReferences.FirstOrDefault(n => n.GetType() == typeof(Coordinate));
            colorNode = (Color)this.ChildrenWithAppliedReferences.FirstOrDefault(n => n.GetType() == typeof(Color));
            colorRGBANode = (ColorRGBA)this.ChildrenWithAppliedReferences.FirstOrDefault(n => n.GetType() == typeof(ColorRGBA));

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

                Buffering.Interleave(this._bbox,
                    out interleaved3,
                    out interleaved4,
                    _indices, _texIndices, _coords,
                    _texCoords, null, _colorIndicies, color, restartIndex, true,
                    this.colorPerVertex, this.coloring, this.texturing);


                // BUFFER GEOMETRY
                if (interleaved3.Count > 0)
                {
                    Buffering.BufferShaderGeometry(interleaved3, out _vbo_interleaved, out NumVerticies);
                }



                if (interleaved4.Count > 0)
                {
                    quadShader = ShaderCompiler.CreateNewInstance(parentShape.CurrentShader, true);

                    Buffering.BufferShaderGeometry(interleaved4, out _vbo_interleaved4, out NumVerticies4);
                }

                
            }

            Console.WriteLine("IndexedFaceSet [loaded]");
        }

        public override void Load()
        {
            base.Load();
        }

        public override void Render(RenderingContext rc)
        {
            base.Render(rc);

            var size = new Vector3(1, 1, 1);
            var scale = new Vector3(0.05f, 0.05f, 0.05f);

            if (this.coordinate != null && !string.IsNullOrEmpty(this.coordIndex))
            {

                Shape shape;
                PrimitiveType type;
                int vbo;
                int verticies_num;

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

                            parentShape.CurrentShader.SetFieldValue("normalmatrix", ref parentShape.NormalMatrix);
                            //GL.UniformMatrix3(parentShape.Uniforms.NormalMatrix, false, ref parentShape.NormalMatrix);
                            GL.Uniform3(parentShape.Uniforms.LightPosition, 1, ref lightPosition.X);
                            GL.Uniform3(parentShape.Uniforms.AmbientMaterial, X3DTypeConverters.ToVec3(OpenTK.Graphics.Color4.Aqua)); // 0.04f, 0.04f, 0.04f
                            GL.Uniform3(parentShape.Uniforms.DiffuseMaterial, 0.0f, 0.75f, 0.75f);




                            shape = parentShape;
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


                            //shape = _shape4;
                            vbo = _vbo_interleaved4;
                            verticies_num = NumVerticies4;

                            if (verticies_num > 0)
                            {
                                GL.UseProgram(shape.CurrentShader.ShaderHandle);

                                shape.CurrentShader.SetFieldValue("size", size);
                                shape.CurrentShader.SetFieldValue("scale", scale);

                                GL.PatchParameter(PatchParameterInt.PatchVertices, 16);
                                GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
                                Buffering.ApplyBufferPointers(shape.CurrentShader);
                                //Buffering.ApplyBufferPointers(shape.uniforms);
                                GL.DrawArrays(PrimitiveType.Patches, 0, verticies_num);
                            }
                        }
                        else
                        {
                            if (NumVerticies > 0)
                            {
                                GL.UseProgram(parentShape.CurrentShader.ShaderHandle);

                                parentShape.CurrentShader.SetFieldValue("size", size);
                                parentShape.CurrentShader.SetFieldValue("scale", scale);

                                GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo_interleaved);
                                Buffering.ApplyBufferPointers(parentShape.CurrentShader);
                                GL.DrawArrays(PrimitiveType.Triangles, 0, NumVerticies);

                                GL.UseProgram(0);
                            }


                            if (NumVerticies4 > 0)
                            {
                                GL.UseProgram(quadShader.ShaderHandle);

                                parentShape.ApplyGeometricTransformations(rc, quadShader, this);

                                quadShader.SetFieldValue("size", size);
                                quadShader.SetFieldValue("scale", scale);

                                GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo_interleaved4);
                                Buffering.ApplyBufferPointers(quadShader);
                                GL.DrawArrays(PrimitiveType.Quads, 0, NumVerticies4);

                                GL.UseProgram(0);
                            }
                        }
                    }
                }
            }
        }
    }
}
