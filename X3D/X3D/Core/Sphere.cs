﻿using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics.OpenGL4;
using X3D.Parser;
using OpenTK.Input;

namespace X3D
{
    public partial class Sphere
    {
        int vbo, NumVerticies;

        public class ShaderUniforms
        {
            public int Projection;
            public int Modelview;
            public int NormalMatrix;
            public int LightPosition;
            public int AmbientMaterial;
            public int DiffuseMaterial;
            public int TessLevelInner;
            public int TessLevelOuter;
        }

        private ShaderUniforms Uniforms = new ShaderUniforms();
        private Shape parentShape;
        private static float TessLevelInner = 3;
        private static float TessLevelOuter = 2;
        

        static string tessControlShader = @"
// original source Philip Rideout http://prideout.net/blog/?p=48
#version 420 core
layout(vertices = 3) out;
in vec3 vPosition[];
out vec3 tcPosition[];
uniform float TessLevelInner;
uniform float TessLevelOuter;

#define ID gl_InvocationID

void main()
{
    tcPosition[ID] = vPosition[ID];
    if (ID == 0) {
        gl_TessLevelInner[0] = TessLevelInner;
        gl_TessLevelOuter[0] = TessLevelOuter;
        gl_TessLevelOuter[1] = TessLevelOuter;
        gl_TessLevelOuter[2] = TessLevelOuter;
    }
}
";

        static string tessEvalShader = @"
// original source Philip Rideout http://prideout.net/blog/?p=48
#version 420 core
layout(triangles, equal_spacing, cw) in;
in vec3 tcPosition[];
out vec3 tePosition;
out vec3 tePatchDistance;
uniform mat4 projection;
uniform mat4 modelview;
uniform vec3 scale;

void main()
{
    vec3 p0 = gl_TessCoord.x * tcPosition[0];
    vec3 p1 = gl_TessCoord.y * tcPosition[1];
    vec3 p2 = gl_TessCoord.z * tcPosition[2];
    tePatchDistance = gl_TessCoord;
    tePosition = scale * normalize(p0 + p1 + p2);
    gl_Position = projection * modelview * vec4(tePosition, 1);
}
";
        //BUG: geometry shader not compatible with current vertex layout
        static string geometryShaderSource = @"
// original source Philip Rideout http://prideout.net/blog/?p=48
#version 420 core
uniform mat4 modelview;
uniform mat3 normalmatrix;
layout(triangles) in;
layout(triangle_strip, max_vertices = 3) out;

in vec3 tePosition[3];
in vec3 tePatchDistance[3];

out vec3 gFacetNormal;
out vec3 gPatchDistance;
out vec3 gTriDistance;
out vec2 gFacetTexCoord;

#define M_PI 3.1415926535897932384626433832795

void main()
{
    vec3 A = tePosition[2] - tePosition[0];
    vec3 B = tePosition[1] - tePosition[0];
    gFacetNormal = normalmatrix  * normalize(cross(A, B));
    gFacetTexCoord = vec2(asin(gFacetNormal.x)/M_PI + 0.5 , asin(gFacetNormal.y) / M_PI + 0.5 );

    gPatchDistance = tePatchDistance[0];
    gTriDistance = vec3(1, 0, 0);
    gl_Position = gl_in[0].gl_Position; 
    EmitVertex();

    gPatchDistance = tePatchDistance[1];
    gTriDistance = vec3(0, 1, 0);
    gl_Position = gl_in[1].gl_Position; 
    EmitVertex();

    gPatchDistance = tePatchDistance[2];
    gTriDistance = vec3(0, 0, 1);
    gl_Position = gl_in[2].gl_Position; 
    EmitVertex();

    EndPrimitive();
}
";

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
                parentShape.IncludeTesselationShaders(tessControlShader, tessEvalShader, geometryShaderSource);
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
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo); // InterleavedArrayFormat.T2fC4fN3fV3f
            GL.DrawArrays(PrimitiveType.Patches, 0, NumVerticies); // Triangles Points  Lines

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
