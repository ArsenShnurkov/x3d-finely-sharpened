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
        //internal Normal normal;
        //internal Coordinate coordinate;
        //internal TextureCoordinate texCoordinate;
        //internal Color colorNode;
        //internal ColorRGBA colorRGBANode;

        internal PackedGeometry _pack;

        //internal int[] _indices;
        //internal Vector3[] _coords;
        //private int[] _colorIndicies;
        //private float[] color;
        //private int[] _texIndices;
        //private Vector2[] _texCoords;

        //private Shape parentShape;
        //private Shape _shape4;
        //private ComposedShader quadShader = null;

        //private BoundingBox _bbox;
        //private bool RGBA = false, RGB = false, coloring = false, texturing = false, generateColorMap = false;
        //private int _vbo_interleaved, _vbo_interleaved4;
        //private int NumVerticies, NumVerticies4;

        //List<Vertex> interleaved3;
        //List<Vertex> interleaved4;

        public override void CollectGeometry(
                        RenderingContext rc,
                        out GeometryHandle handle,
                        out BoundingBox bbox,
                        out bool coloring,
                        out bool texturing)
        {
            bbox = BoundingBox.Zero;

            // INTERLEAVE
            _pack = PackedGeometry.Pack(this);
            
            coloring = _pack.Coloring;
            texturing = _pack.Texturing;
            bbox = _pack.bbox;
            
            // BUFFER GEOMETRY
            handle = _pack.CreateHandle();
            //handle = Buffering.BufferShaderGeometry(_pack);

            //int? restartIndex = null;

            //texturing = texCoordinate != null;// || parentShape.texturingEnabled;



            //texCoordinate = (TextureCoordinate)this.ChildrenWithAppliedReferences.FirstOrDefault(n => n.GetType() == typeof(TextureCoordinate));
            //coordinate = (Coordinate)this.ChildrenWithAppliedReferences.FirstOrDefault(n => n.GetType() == typeof(Coordinate));
            //colorNode = (Color)this.ChildrenWithAppliedReferences.FirstOrDefault(n => n.GetType() == typeof(Color));
            //colorRGBANode = (ColorRGBA)this.ChildrenWithAppliedReferences.FirstOrDefault(n => n.GetType() == typeof(ColorRGBA));

            //RGBA = colorRGBANode != null;
            //RGB = colorNode != null;
            //coloring = RGBA || RGB;
            //generateColorMap = coloring;

            //this.coloring = coloring;
            //this.texturing = texturing;

            //if (RGB && !RGBA)
            //{
            //    color = X3DTypeConverters.Floats(colorNode.color);
            //}
            //else if (RGBA && !RGB)
            //{
            //    color = X3DTypeConverters.Floats(colorRGBANode.color);
            //}

            //if (this.texCoordinate != null && !string.IsNullOrEmpty(this.texCoordIndex))
            //{
            //    _texIndices = X3DTypeConverters.ParseIndicies(this.texCoordIndex);
            //    _texCoords = X3DTypeConverters.MFVec2f(this.texCoordinate.point);
            //}

            //if (this.coordinate != null && !string.IsNullOrEmpty(this.coordIndex))
            //{
            //    _indices = X3DTypeConverters.ParseIndicies(this.coordIndex);
            //    _coords = X3DTypeConverters.MFVec3f(this.coordinate.point);

            //    if (!string.IsNullOrEmpty(this.colorIndex))
            //    {
            //        _colorIndicies = X3DTypeConverters.ParseIndicies(this.colorIndex);
            //    }

            //    if (this.coordIndex.Contains(RESTART_INDEX.ToString()))
            //    {
            //        restartIndex = RESTART_INDEX;
            //    }

            //    bbox = MathHelpers.CalcBoundingBox(this, restartIndex);

            //    Buffering.Interleave(bbox,
            //        out interleaved3,
            //        out interleaved4,
            //        _indices, _texIndices, _coords,
            //        _texCoords, null, _colorIndicies, color, restartIndex, true,
            //        this.colorPerVertex, this.coloring, this.texturing);


            //    // BUFFER GEOMETRY
            //    if (interleaved3.Count > 0)
            //    {
            //        Buffering.BufferShaderGeometry(interleaved3, out vbo3, out NumVerticies);
            //    }


            //    if (interleaved4.Count > 0)
            //    {
            //        //quadShader = ShaderCompiler.CreateNewInstance(parentShape.CurrentShader, true);

            //        //BUG: materials not rendering for quad shader
            //        //parentShape.ApplyMaterials(quadShader);

            //        Buffering.BufferShaderGeometry(interleaved4, out vbo4, out NumVerticies4);
            //    }
            //}
        }



        //public override void PreRenderOnce(RenderingContext rc)
        //{
        //    base.PreRenderOnce(rc);

        //    int? restartIndex = null;

        //    parentShape = GetParent<Shape>();


        //    _shape4 = new Shape();
        //    _shape4.Load();

        //    texturing = texCoordinate != null || parentShape.texturingEnabled;



        //    texCoordinate = (TextureCoordinate)this.ChildrenWithAppliedReferences.FirstOrDefault(n => n.GetType() == typeof(TextureCoordinate));
        //    coordinate = (Coordinate)this.ChildrenWithAppliedReferences.FirstOrDefault(n => n.GetType() == typeof(Coordinate));
        //    colorNode = (Color)this.ChildrenWithAppliedReferences.FirstOrDefault(n => n.GetType() == typeof(Color));
        //    colorRGBANode = (ColorRGBA)this.ChildrenWithAppliedReferences.FirstOrDefault(n => n.GetType() == typeof(ColorRGBA));

        //    RGBA = colorRGBANode != null;
        //    RGB = colorNode != null;
        //    coloring = RGBA || RGB;
        //    generateColorMap = coloring;


        //    if (RGB && !RGBA)
        //    {
        //        color = X3DTypeConverters.Floats(colorNode.color);
        //    }
        //    else if (RGBA && !RGB)
        //    {
        //        color = X3DTypeConverters.Floats(colorRGBANode.color);
        //    }

        //    if (this.texCoordinate != null && !string.IsNullOrEmpty(this.texCoordIndex))
        //    {
        //        _texIndices = X3DTypeConverters.ParseIndicies(this.texCoordIndex);
        //        _texCoords = X3DTypeConverters.MFVec2f(this.texCoordinate.point);
        //    }

        //    if (this.coordinate != null && !string.IsNullOrEmpty(this.coordIndex))
        //    {
        //        _indices = X3DTypeConverters.ParseIndicies(this.coordIndex);
        //        _coords = X3DTypeConverters.MFVec3f(this.coordinate.point);

        //        if (!string.IsNullOrEmpty(this.colorIndex))
        //        {
        //            _colorIndicies = X3DTypeConverters.ParseIndicies(this.colorIndex);
        //        }

        //        if (this.coordIndex.Contains(RESTART_INDEX.ToString()))
        //        {
        //            restartIndex = RESTART_INDEX;
        //        }

        //        this._bbox = MathHelpers.CalcBoundingBox(this, restartIndex);

        //        Buffering.Interleave(this._bbox,
        //            out interleaved3,
        //            out interleaved4,
        //            _indices, _texIndices, _coords,
        //            _texCoords, null, _colorIndicies, color, restartIndex, true,
        //            this.colorPerVertex, this.coloring, this.texturing);


        //        // BUFFER GEOMETRY
        //        if (interleaved3.Count > 0)
        //        {
        //            Buffering.BufferShaderGeometry(interleaved3, out _vbo_interleaved, out NumVerticies);
        //        }


        //        if (interleaved4.Count > 0)
        //        {
        //            quadShader = ShaderCompiler.CreateNewInstance(parentShape.CurrentShader, true);

        //            //BUG: materials not rendering for quad shader
        //            //parentShape.ApplyMaterials(quadShader);

        //            Buffering.BufferShaderGeometry(interleaved4, out _vbo_interleaved4, out NumVerticies4);
        //        }


        //    }

        //    Console.WriteLine("IndexedFaceSet [loaded]");
        //}

        //public override void Load()
        //{
        //    base.Load();
        //}

        //public override void Render(RenderingContext rc)
        //{
        //    base.Render(rc);

        //    var size = new Vector3(1, 1, 1);
        //    var scale = new Vector3(0.05f, 0.05f, 0.05f);

        //    if (this.coordinate != null && !string.IsNullOrEmpty(this.coordIndex))
        //    {

        //        Shape shape;
        //        //PrimitiveType type;
        //        int vbo;
        //        int verticies_num;

        //        // Refactor tessellation 

        //        if (parentShape.ComposedShaders.Any(s => s.Linked))
        //        {
        //            if (parentShape.CurrentShader != null)
        //            {

        //                parentShape.CurrentShader.Use();

        //                parentShape.CurrentShader.SetFieldValue("bbox_x", _bbox.Width); //TODO: put this in Shape node
        //                parentShape.CurrentShader.SetFieldValue("bbox_y", _bbox.Height);
        //                parentShape.CurrentShader.SetFieldValue("bbox_z", _bbox.Depth);

        //                if (parentShape.depthMask == false)
        //                {
        //                    //REFACTOR!!
        //                    Matrix4 mat4 = Matrix4.Identity;
        //                    //Quaternion qRotFix = QuaternionExtensions.EulerToQuat(rc.cam.calibOrient.X, rc.cam.calibOrient.Y, rc.cam.calibOrient.Z);
        //                    //mat4 *= Matrix4.CreateTranslation(rc.cam.calibTrans) * Matrix4.CreateFromQuaternion(qRotFix);
        //                    Quaternion qRotFix = QuaternionExtensions.EulerToQuat(0.15f, 3.479997f, 0f);
        //                    mat4 *= Matrix4.CreateTranslation(new Vector3(0f, 0f, -0.29f)) * Matrix4.CreateFromQuaternion(qRotFix);
        //                    // test weapon/gun rendering fixed in front of player
        //                    //TODO: port this to X3D

        //                    parentShape.CurrentShader.SetFieldValue("modelview", ref mat4);
        //                    if (quadShader != null) quadShader.SetFieldValue("modelview", ref mat4);
        //                    //GL.DepthMask(false);
        //                }

        //                parentShape.CurrentShader.SetFieldValue("size", size);
        //                parentShape.CurrentShader.SetFieldValue("scale", scale);

        //                if (parentShape.CurrentShader.IsTessellator)
        //                {
        //                    Vector4 lightPosition = new Vector4(0.25f, 0.25f, 1f, 0f);

        //                    parentShape.CurrentShader.SetFieldValue("normalmatrix", ref parentShape.NormalMatrix);
        //                    //GL.UniformMatrix3(parentShape.Uniforms.NormalMatrix, false, ref parentShape.NormalMatrix);
        //                    GL.Uniform3(parentShape.Uniforms.LightPosition, 1, ref lightPosition.X);
        //                    GL.Uniform3(parentShape.Uniforms.AmbientMaterial, X3DTypeConverters.ToVec3(OpenTK.Graphics.Color4.Aqua)); // 0.04f, 0.04f, 0.04f
        //                    GL.Uniform3(parentShape.Uniforms.DiffuseMaterial, 0.0f, 0.75f, 0.75f);

        //                    shape = parentShape;
        //                    vbo = _vbo_interleaved;
        //                    verticies_num = NumVerticies;

        //                    if (verticies_num > 0)
        //                    {
        //                        GL.UseProgram(shape.CurrentShader.ShaderHandle);

        //                        shape.CurrentShader.SetFieldValue("size", size);
        //                        shape.CurrentShader.SetFieldValue("scale", scale);

        //                        GL.PatchParameter(PatchParameterInt.PatchVertices, 3);
        //                        GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
        //                        Buffering.ApplyBufferPointers(shape.CurrentShader);
        //                        //Buffering.ApplyBufferPointers(shape.uniforms);
        //                        GL.DrawArrays(PrimitiveType.Patches, 0, verticies_num);
        //                    }


        //                    //shape = _shape4;
        //                    vbo = _vbo_interleaved4;
        //                    verticies_num = NumVerticies4;

        //                    if (verticies_num > 0)
        //                    {
        //                        GL.UseProgram(shape.CurrentShader.ShaderHandle);

        //                        shape.CurrentShader.SetFieldValue("size", size);
        //                        shape.CurrentShader.SetFieldValue("scale", scale);

        //                        GL.PatchParameter(PatchParameterInt.PatchVertices, 16);
        //                        GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
        //                        Buffering.ApplyBufferPointers(shape.CurrentShader);
        //                        //Buffering.ApplyBufferPointers(shape.uniforms);
        //                        GL.DrawArrays(PrimitiveType.Patches, 0, verticies_num);
        //                    }
        //                }
        //                else
        //                {
        //                    if (NumVerticies > 0)
        //                    {
        //                        GL.UseProgram(parentShape.CurrentShader.ShaderHandle);

        //                        parentShape.CurrentShader.SetFieldValue("size", size);
        //                        parentShape.CurrentShader.SetFieldValue("scale", scale);

        //                        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo_interleaved);
        //                        Buffering.ApplyBufferPointers(parentShape.CurrentShader);
        //                        GL.DrawArrays(PrimitiveType.Triangles, 0, NumVerticies);

        //                        GL.UseProgram(0);
        //                    }


        //                    if (NumVerticies4 > 0)
        //                    {
        //                        GL.UseProgram(quadShader.ShaderHandle);

        //                        if (parentShape.depthMask)
        //                        {
        //                            parentShape.ApplyGeometricTransformations(rc, quadShader, this);
        //                        }
        //                        else
        //                        {
        //                            //REFACTOR!!
        //                            Matrix4 mat4 = Matrix4.Identity;
        //                            //Quaternion qRotFix = QuaternionExtensions.EulerToQuat(rc.cam.calibOrient.X, rc.cam.calibOrient.Y, rc.cam.calibOrient.Z);
        //                            //mat4 *= Matrix4.CreateTranslation(rc.cam.calibTrans) * Matrix4.CreateFromQuaternion(qRotFix);
        //                            Quaternion qRotFix = QuaternionExtensions.EulerToQuat(0.15f, 3.479997f, 0f);
        //                            mat4 *= Matrix4.CreateTranslation(new Vector3(0f, 0f, -0.29f)) * Matrix4.CreateFromQuaternion(qRotFix);

        //                            // test weapon/gun rendering fixed in front of player
        //                            //TODO: port this to X3D

        //                            quadShader.SetFieldValue("modelview", ref mat4);

        //                            var shader = quadShader;

        //                            parentShape.RefreshDefaultUniforms(shader);

        //                            shader.SetFieldValue("bbox_x", _bbox.Width);
        //                            shader.SetFieldValue("bbox_y", _bbox.Height);
        //                            shader.SetFieldValue("bbox_z", _bbox.Depth);
        //                            shader.SetFieldValue("projection", ref rc.matricies.projection);
        //                            shader.SetFieldValue("camscale", rc.cam.Scale.X); //GL.Uniform1(uniformCameraScale, rc.cam.Scale.X);
        //                            shader.SetFieldValue("X3DScale", rc.matricies.Scale); //GL.Uniform3(uniformX3DScale, rc.matricies.Scale);
        //                            shader.SetFieldValue("coloringEnabled", 0); //GL.Uniform1(uniforms.a_coloringEnabled, 0);
        //                            shader.SetFieldValue("texturingEnabled", parentShape.texturingEnabled ? 1 : 0); //GL.Uniform1(uniforms.a_texturingEnabled, this.texturingEnabled ? 1 : 0);

        //                        }
        //                        quadShader.SetFieldValue("size", size);
        //                        quadShader.SetFieldValue("scale", scale);

        //                        parentShape.ApplyMaterials(quadShader);

        //                        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo_interleaved4);
        //                        Buffering.ApplyBufferPointers(quadShader);
        //                        GL.DrawArrays(PrimitiveType.Quads, 0, NumVerticies4);

        //                        GL.UseProgram(0);
        //                    }
        //                }

        //                if (parentShape.depthMask == false)
        //                {
        //                    //GL.DepthMask(true);
        //                }
        //            }
        //        }
        //    }
        //}

    }
}
