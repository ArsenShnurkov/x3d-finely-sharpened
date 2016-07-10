using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics.OpenGL4;
using X3D.Parser;
using OpenTK.Input;
using X3D.Engine.Shading.DefaultUniforms;

namespace X3D
{
    public partial class Sphere
    {
        int vbo, NumVerticies;

        private TessShaderUniforms Uniforms = new TessShaderUniforms();
        private Shape parentShape;
        private float TessLevelInner = 3;
        private float TessLevelOuter = 2;
        

        #region Rendering Methods

        public override void Load()
        {
            base.Load();

            parentShape = GetParent<Shape>();

            Helpers.Interleave(parentShape, out vbo, out NumVerticies, Faces, null, Verts, null, null, 
                restartIndex: -1, genTexCoordPerVertex: true);

            GL.UseProgram(parentShape.shaderProgramHandle);

            if(parentShape!= null)
            {
                // TESSELLATION
                parentShape.IncludeTesselationShaders(Helpers.tessControlShader, Helpers.tessEvalShader, Helpers.geometryShaderSource);
            }

            Uniforms.Modelview = GL.GetUniformLocation(parentShape.shaderProgramHandle, "modelview");
            Uniforms.Projection = GL.GetUniformLocation(parentShape.shaderProgramHandle, "projection");
            Uniforms.NormalMatrix = GL.GetUniformLocation(parentShape.shaderProgramHandle, "normalmatrix");
            Uniforms.LightPosition = GL.GetUniformLocation(parentShape.shaderProgramHandle, "LightPosition");
            Uniforms.AmbientMaterial = GL.GetUniformLocation(parentShape.shaderProgramHandle, "AmbientMaterial");
            Uniforms.DiffuseMaterial = GL.GetUniformLocation(parentShape.shaderProgramHandle, "DiffuseMaterial");
            Uniforms.TessLevelInner = GL.GetUniformLocation(parentShape.shaderProgramHandle, "TessLevelInner");
            Uniforms.TessLevelOuter = GL.GetUniformLocation(parentShape.shaderProgramHandle, "TessLevelOuter");




        }

        public override void Render(RenderingContext rc)
        {
            base.Render(rc);

            GL.UseProgram(parentShape.shaderProgramHandle);
            int uniformSize = GL.GetUniformLocation(parentShape.shaderProgramHandle, "size");
            int uniformScale = GL.GetUniformLocation(parentShape.shaderProgramHandle, "scale");
            var size = new Vector3(1, 1, 1);
            var scale = new Vector3(0.7f, 0.7f, 0.7f);
            GL.Uniform3(uniformSize, size);
            GL.Uniform3(uniformScale, scale);

            Matrix3 NormalMatrix = new Matrix3(rc.matricies.modelview); // NormalMatrix = M4GetUpper3x3(ModelviewMatrix);
            Vector4 lightPosition = new Vector4(0.25f, 0.25f, 1f, 0f);

            GL.UniformMatrix3(Uniforms.NormalMatrix, false, ref NormalMatrix);
            GL.Uniform1(Uniforms.TessLevelInner, TessLevelInner);
            GL.Uniform1(Uniforms.TessLevelOuter, TessLevelOuter);
            GL.Uniform3(Uniforms.LightPosition, 1, ref lightPosition.X);
            GL.Uniform3(Uniforms.AmbientMaterial, Helpers.ToVec3(OpenTK.Graphics.Color4.Aqua) ); // 0.04f, 0.04f, 0.04f
            GL.Uniform3(Uniforms.DiffuseMaterial, 0.0f, 0.75f, 0.75f); 

            GL.PatchParameter(PatchParameterInt.PatchVertices, 3);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.DrawArrays(PrimitiveType.Patches, 0, NumVerticies);

            // Testing of tessellation parameters
            if (rc.Keyboard[Key.Right] ) TessLevelInner++;
            if (rc.Keyboard[Key.Left]) TessLevelInner = TessLevelInner > 1 ? TessLevelInner - 1 : 1;
            if (rc.Keyboard[Key.Up]) TessLevelOuter++;
            if (rc.Keyboard[Key.Down]) TessLevelOuter = TessLevelOuter > 1 ? TessLevelOuter - 1 : 1;
        }

        #endregion

        #region Icosahedron Geometry

        int[] Faces = new int[] 
        {
            2, 1, 0, -1, 
            3, 2, 0, -1,
            4, 3, 0, -1,
            5, 4, 0, -1,
            1, 5, 0, -1,
            11, 6,  7, -1,
            11, 7,  8, -1,
            11, 8,  9, -1,
            11, 9,  10, -1,
            11, 10, 6, -1,
            1, 2, 6, -1,
            2, 3, 7, -1,
            3, 4, 8, -1,
            4, 5, 9, -1,
            5, 1, 10, -1,
            2,  7, 6, -1,
            3,  8, 7, -1,
            4,  9, 8, -1,
            5, 10, 9, -1,
            1, 6, 10, -1,
        };
 
        Vector3[] Verts = new Vector3[] 
        {
             new Vector3(0.000f,  0.000f,  1.000f),
             new Vector3(0.894f,  0.000f,  0.447f),
             new Vector3(0.276f,  0.851f,  0.447f),
            new Vector3(-0.724f,  0.526f,  0.447f),
            new Vector3(-0.724f, -0.526f,  0.447f),
             new Vector3(0.276f, -0.851f,  0.447f),
             new Vector3(0.724f,  0.526f, -0.447f),
            new Vector3(-0.276f,  0.851f, -0.447f),
            new Vector3(-0.894f,  0.000f, -0.447f),
            new Vector3(-0.276f, -0.851f, -0.447f),
             new Vector3(0.724f, -0.526f, -0.447f),
             new Vector3(0.000f,  0.000f, -1.000f)
        };
        #endregion
    }
}
