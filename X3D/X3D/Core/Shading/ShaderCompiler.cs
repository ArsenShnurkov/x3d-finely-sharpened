using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using X3D.Core.Shading;

namespace X3D.Core
{
    public class ShaderCompiler
    {
        public static ComposedShader BuildDefaultShader()
        {
            var defaultsh = new ComposedShader();

            defaultsh.IsBuiltIn = true; // specifies this is a built in system shader
            defaultsh.language = "GLSL";
            defaultsh.ShaderParts.Add(new ShaderPart()
            {
                ShaderSource = DefaultShader.vertexShaderSource,
                Type = shaderPartTypeValues.VERTEX
            });

            defaultsh.ShaderParts.Add(new ShaderPart()
            {
                ShaderSource = DefaultShader.fragmentShaderSource,
                Type = shaderPartTypeValues.FRAGMENT
            });

            return defaultsh;
        }

        /// <summary>
        /// Relocate this elsewhere
        /// </summary>
        public static ComposedShader ApplyShader(string vertexShaderSource, string fragmentShaderSource,
            string tessControlSource = "", string tessEvalSource = "", string geometryShaderSource = "")
        {
            ComposedShader shader = new ComposedShader();

            shader.IsBuiltIn = true; // specifies this is a built in system shader
            shader.language = "GLSL";

            if (!string.IsNullOrEmpty(vertexShaderSource))
            {
                shader.ShaderParts.Add(new ShaderPart()
                {
                    ShaderSource = vertexShaderSource,
                    Type = shaderPartTypeValues.VERTEX
                });
            }

            if (!string.IsNullOrEmpty(tessControlSource))
            {
                shader.ShaderParts.Add(new ShaderPart()
                {
                    ShaderSource = tessControlSource,
                    Type = shaderPartTypeValues.TESS_CONTROL
                });
            }

            if (!string.IsNullOrEmpty(tessEvalSource))
            {
                shader.ShaderParts.Add(new ShaderPart()
                {
                    ShaderSource = tessEvalSource,
                    Type = shaderPartTypeValues.TESS_EVAL
                });
            }


            if (!string.IsNullOrEmpty(geometryShaderSource))
            {
                shader.ShaderParts.Add(new ShaderPart()
                {
                    ShaderSource = geometryShaderSource,
                    Type = shaderPartTypeValues.GEOMETRY
                });
            }

            if (!string.IsNullOrEmpty(fragmentShaderSource))
            {
                shader.ShaderParts.Add(new ShaderPart()
                {
                    ShaderSource = fragmentShaderSource,
                    Type = shaderPartTypeValues.FRAGMENT
                });
            }


            return shader;

            //int shaderProgramHandle = GL.CreateProgram();

            //int vertexShaderHandle = GL.CreateShader(ShaderType.VertexShader);
            //int fragmentShaderHandle = GL.CreateShader(ShaderType.FragmentShader);
            //int tessControlShaderHandle = -1, tessEvalShaderHandle = -1, geometryShaderHandle = -1;

            //GL.ShaderSource(vertexShaderHandle, vertexShaderSource);
            //GL.ShaderSource(fragmentShaderHandle, fragmentShaderSource);
            //GL.CompileShader(vertexShaderHandle);
            //GL.CompileShader(fragmentShaderHandle);

            //Console.WriteLine(GL.GetShaderInfoLog(vertexShaderHandle).Trim());
            //Console.WriteLine(GL.GetShaderInfoLog(fragmentShaderHandle).Trim());

            //GL.AttachShader(shaderProgramHandle, vertexShaderHandle);


            //if (!string.IsNullOrEmpty(tessControlSource))
            //{
            //    tessControlShaderHandle = GL.CreateShader(ShaderType.TessControlShader);
            //    GL.ShaderSource(tessControlShaderHandle, tessControlSource);
            //    GL.CompileShader(tessControlShaderHandle);
            //    Console.WriteLine(GL.GetShaderInfoLog(tessControlShaderHandle).Trim());

            //    GL.AttachShader(shaderProgramHandle, tessControlShaderHandle);
            //}
            //if (!string.IsNullOrEmpty(tessEvalSource))
            //{
            //    tessEvalShaderHandle = GL.CreateShader(ShaderType.TessEvaluationShader);
            //    GL.ShaderSource(tessEvalShaderHandle, tessEvalSource);
            //    GL.CompileShader(tessEvalShaderHandle);
            //    Console.WriteLine(GL.GetShaderInfoLog(tessEvalShaderHandle).Trim());

            //    GL.AttachShader(shaderProgramHandle, tessEvalShaderHandle);
            //}
            //if (!string.IsNullOrEmpty(geometryShaderSource))
            //{
            //    geometryShaderHandle = GL.CreateShader(ShaderType.GeometryShader);
            //    GL.ShaderSource(geometryShaderHandle, geometryShaderSource);
            //    GL.CompileShader(geometryShaderHandle);
            //    Console.WriteLine(GL.GetShaderInfoLog(geometryShaderHandle).Trim());

            //    GL.AttachShader(shaderProgramHandle, geometryShaderHandle);
            //}

            //GL.AttachShader(shaderProgramHandle, fragmentShaderHandle);

            //GL.LinkProgram(shaderProgramHandle);

            //Console.WriteLine(GL.GetProgramInfoLog(shaderProgramHandle).Trim());

            ////GL.UseProgram(shaderProgramHandle);

            //if(GL.GetError() != ErrorCode.NoError)
            //{
            //    throw new Exception("Error Linking Shader Program");
            //}

            //return shaderProgramHandle;
        }

        public static int ApplyShaderPart(int shaderProgramHandle, ShaderPart part)
        {
            ShaderType type = ShaderType.VertexShader;

            switch (part.Type)
            {
                case shaderPartTypeValues.VERTEX:
                    type = ShaderType.VertexShader;
                    break;
                case shaderPartTypeValues.FRAGMENT:
                    type = ShaderType.FragmentShader;
                    break;
                case shaderPartTypeValues.TESS_CONTROL:
                    type = ShaderType.TessControlShader;
                    break;
                case shaderPartTypeValues.TESS_EVAL:
                    type = ShaderType.TessEvaluationShader;
                    break;
                case shaderPartTypeValues.GEOMETRY:
                    type = ShaderType.GeometryShader;
                    break;
                default:
                    throw new Exception("unknown shader type");
            }

            part.ShaderHandle = GL.CreateShader(type);

            GL.ShaderSource(part.ShaderHandle, part.ShaderSource);
            GL.CompileShader(part.ShaderHandle);

            string err = GL.GetShaderInfoLog(part.ShaderHandle).Trim();
            if (!string.IsNullOrEmpty(err))
                Console.WriteLine(err);

            GL.AttachShader(shaderProgramHandle, part.ShaderHandle);
            Console.WriteLine("ShaderPart {0} [attaching]", part.Type);

            return part.ShaderHandle;
        }
        public static int ApplyShader(string shaderSource, ShaderType type)
        {
            int shaderProgramHandle = GL.CreateProgram();

            int shaderHandle = GL.CreateShader(type);

            GL.ShaderSource(shaderHandle, shaderSource);

            GL.CompileShader(shaderHandle);

            Console.WriteLine(GL.GetShaderInfoLog(shaderHandle));

            GL.AttachShader(shaderProgramHandle, shaderHandle);
            GL.LinkProgram(shaderProgramHandle);

            Console.WriteLine(GL.GetProgramInfoLog(shaderProgramHandle));

            return shaderProgramHandle;
        }
    }
}
