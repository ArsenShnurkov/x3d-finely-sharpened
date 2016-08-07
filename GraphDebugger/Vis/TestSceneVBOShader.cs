using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace GraphDebugger.OpenGL
{
    public class UniformVariableTestProgram : GameWindow
    {
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




        //        string vertexShaderSource = @"
        //vec3 Position;

        //uniform float scale;

        //void main()
        //{
        //	gl_Position = vec4(scale * Position.x,
        //	                   scale * Position.y,
        //	                   Position.z, 1.0);
        //}";


        //        string fragmentShaderSource = @"
        //vec4 FragColor;

        //void main()
        //{
        //	FragColor = vec4(0.5, 0.8, 1.0, 1.0);
        //}";


        int vbo, shaderProgramHandle, vertexShaderHandle, fragmentShaderHandle;

        int uniformScale;
        float variableScale;

        double time;

        void CreateVertexBuffer()
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

        void CreateShaders()
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

        protected override void OnLoad(EventArgs e)
        {
            GL.ClearColor(Color.Green);
            CreateVertexBuffer();
            CreateShaders();
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);

            time = (time >= Math.PI) ? 0.0 : time + e.Time;

            variableScale = (float)(Math.Sin(time));
            GL.Uniform1(uniformScale, variableScale);

            GL.EnableVertexAttribArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);

            GL.DrawArrays(BeginMode.Triangles, 0, 3);

            GL.DisableVertexAttribArray(0);

            SwapBuffers();
        }

        public static void View_Init()
        {
            using (UniformVariableTestProgram p = new UniformVariableTestProgram())
            {
                p.Run(60);
            }
        }
    }
}