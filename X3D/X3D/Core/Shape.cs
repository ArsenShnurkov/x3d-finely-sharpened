//TODO: buffer all geometric properties
//TODO: implement ADS shading using materials instantiated from Shape Appearance. 
//      Hardcode ADS Shader model if no shader is specified.
//TODO: implement and instantiate Shader model. implement test scene graph with phong shader.
//TODO: dynamically select shader program for each Shape instance.

//TODO: what if these children have USE lookup requirements?

//TODO: dont unpack indicies or transform them if it is not required. we want to save both time and space if at all possible.
// todo implement ifs geometry shader. ensure colors, texcoords,normals,and verticies render. test using primativiēs.
// todo implement creaseAngle: flat and smooth shading
// todo implememt phong shading. doēs x3d specify this?
// todo implememt optimisations; minimal unpacking/transformation of geometry, go direct to webgl datastructs.
// todo implememt node instancing 
// todo implement dynamic buffers, dynamic attributes, node disposal/scene cleanup, 
// todo īmplememt SAI, scene graph debugger, and UI
// todo implememt ccw, solid, concave tesselator, dynamic polygon types/dynamic faceset capability
// todo: webgl currently lacks support for primativeRestartIndex(), but even then if webgl did support this, i dont think that a restart index can be a signed integer. (-1)

using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace X3D
{
    public partial class Shape : X3DShapeNode
    {
        private bool isComposedGeometry;
        private bool hasShaders;
        private List<X3DShaderNode> shaders;

        #region Test
        string vertexShaderSource = @"
#version 330
 
layout (location = 0) in vec3 Position;
 
uniform float scale;
 
void main()
{
	gl_Position = vec4(scale * Position.x,
	                   scale * Position.y,
	                   Position.z, 1.0);
}";
        string fragmentShaderSource = @"
#version 330
 
out vec4 FragColor;
 
void main()
{
	FragColor = vec4(0.5, 0.8, 1.0, 1.0);
}";

        int vbo, shaderProgramHandle, vertexShaderHandle, fragmentShaderHandle;

        int uniformScale;
        float variableScale;

        double time;

        private void CreateVertexBuffer()
        {
            Vector3[] vertices = new Vector3[3];
            vertices[0] = new Vector3(-1f, -1f, 0f);
            vertices[1] = new Vector3(1f, -1f, 0f);
            vertices[2] = new Vector3(0f, 1f, 0f);

            GL.GenBuffers(1, out vbo);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData<Vector3>(BufferTarget.ArrayBuffer,
                                   new IntPtr(vertices.Length * Vector3.SizeInBytes),
                                   vertices, BufferUsageHint.StaticDraw);
        }

        private void CreateShaders()
        {
            shaderProgramHandle = GL.CreateProgram();

            vertexShaderHandle = GL.CreateShader(ShaderType.VertexShader);
            fragmentShaderHandle = GL.CreateShader(ShaderType.FragmentShader);

            GL.ShaderSource(vertexShaderHandle, vertexShaderSource);
            GL.ShaderSource(fragmentShaderHandle, fragmentShaderSource);

            GL.CompileShader(vertexShaderHandle);
            GL.CompileShader(fragmentShaderHandle);
            Console.WriteLine(GL.GetShaderInfoLog(vertexShaderHandle));
            Console.WriteLine(GL.GetShaderInfoLog(fragmentShaderHandle));

            GL.AttachShader(shaderProgramHandle, vertexShaderHandle);
            GL.AttachShader(shaderProgramHandle, fragmentShaderHandle);
            GL.LinkProgram(shaderProgramHandle);
            Console.WriteLine(GL.GetProgramInfoLog(shaderProgramHandle));
            GL.UseProgram(shaderProgramHandle);

            uniformScale = GL.GetUniformLocation(shaderProgramHandle, "scale");
        }

        #endregion

        #region Render Methods

        public override void Load()
        {
            base.Load();

            // load assets
            GL.ClearColor(System.Drawing.Color.Green);
            CreateVertexBuffer();
            CreateShaders();
        }

        public override void PreRender()
        {
            base.PreRender();

            this.geometry = (X3DGeometryNode)this.Children.FirstOrDefault(c => typeof(X3DGeometryNode).IsInstanceOfType(c));
            this.appearance = (X3DAppearanceNode)this.Children.FirstOrDefault(c => typeof(X3DAppearanceNode).IsInstanceOfType(c));

            this.isComposedGeometry = typeof(X3DComposedGeometryNode).IsInstanceOfType(this.geometry);

            shaders = this.DecendantsByType(typeof(X3DShaderNode)).Select(n => (X3DShaderNode)n).ToList();
            hasShaders = shaders.Any();
        }

        public override void Render(FrameEventArgs e)
        {
            base.Render(e);

            GL.Clear(ClearBufferMask.ColorBufferBit);

            time = (time >= Math.PI) ? 0.0 : time + e.Time;

            variableScale = (float)(Math.Sin(time));
            GL.Uniform1(uniformScale, variableScale);

            GL.EnableVertexAttribArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);

            GL.DrawArrays(BeginMode.Triangles, 0, 3);

            GL.DisableVertexAttribArray(0);

            //SwapBuffers();
        }

        public override void PostRender()
        {
            base.PostRender();

        }

        #endregion
    }
}
