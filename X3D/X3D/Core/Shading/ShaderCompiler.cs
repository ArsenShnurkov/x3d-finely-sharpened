using System;
using System.Collections.Generic;
using System.IO;
using OpenTK.Graphics.OpenGL4;
using X3D.Core.Shading;

namespace X3D.Core
{
    public class ShaderCompiler
    {
        public static string GetShaderSource(string fileAbsolutePath, string basePath)
        {
            var @base = Path.GetFullPath(basePath);

            return File.ReadAllText(@base + fileAbsolutePath);
        }

        /// <summary>
        ///     Create a copy of a Shader program using same source but linked in another new program instance.
        /// </summary>
        /// <param name="copyTarget">
        ///     The shader to derive a new instance from.
        /// </param>
        /// <param name="link">
        ///     If the shader copy is to be linked
        /// </param>
        /// <returns></returns>
        public static ComposedShader CreateNewInstance(ComposedShader copyTarget, bool link = true)
        {
            ComposedShader derived;
            ShaderPart partClone;

            derived = new ComposedShader();
            derived.language = copyTarget.language;
            derived.IsBuiltIn = copyTarget.IsBuiltIn;

            foreach (var part in copyTarget.ShaderParts)
            {
                partClone = new ShaderPart();
                partClone.ShaderSource = part.ShaderSource;
                partClone.Type = part.Type;

                derived.ShaderParts.Add(partClone);
            }

            if (link) derived.Link();

            return derived;
        }

        public static ComposedShader BuildDefaultShader(List<ShaderPart> additionalShaderParts = null)
        {
            var defaultsh = new ComposedShader();

            defaultsh.IsBuiltIn = true; // specifies this is a built in system shader
            defaultsh.language = "GLSL";

            if (additionalShaderParts != null)
                // FORWARD DECLARATION of additional shader parts
                foreach (var part in additionalShaderParts)
                    defaultsh.ShaderParts.Add(part);

            defaultsh.ShaderParts.Add(new ShaderPart
            {
                ShaderSource = DefaultShader.vertexShaderSource,
                Type = shaderPartTypeValues.VERTEX
            });

            defaultsh.ShaderParts.Add(new ShaderPart
            {
                ShaderSource = DefaultShader.fragmentShaderSource,
                Type = shaderPartTypeValues.FRAGMENT
            });

            return defaultsh;
        }

        public static ComposedShader ApplyShader(string vertexShaderSource, string fragmentShaderSource)
        {
            var shader = new ComposedShader();

            shader.IsBuiltIn = true; // specifies this is a built in system shader
            shader.language = "GLSL";

            if (!string.IsNullOrEmpty(vertexShaderSource))
                shader.ShaderParts.Add(new ShaderPart
                {
                    ShaderSource = vertexShaderSource,
                    Type = shaderPartTypeValues.VERTEX
                });

            if (!string.IsNullOrEmpty(fragmentShaderSource))
                shader.ShaderParts.Add(new ShaderPart
                {
                    ShaderSource = fragmentShaderSource,
                    Type = shaderPartTypeValues.FRAGMENT
                });

            return shader;
        }

        public static ComposedShader ApplyShader(string vertexShaderSource, string fragmentShaderSource,
            string tessControlSource = "", string tessEvalSource = "", string geometryShaderSource = "")
        {
            var shader = new ComposedShader();

            shader.IsBuiltIn = true; // specifies this is a built in system shader
            shader.language = "GLSL";

            if (!string.IsNullOrEmpty(vertexShaderSource))
                shader.ShaderParts.Add(new ShaderPart
                {
                    ShaderSource = vertexShaderSource.Trim(),
                    Type = shaderPartTypeValues.VERTEX
                });

            if (!string.IsNullOrEmpty(tessControlSource))
                shader.ShaderParts.Add(new ShaderPart
                {
                    ShaderSource = tessControlSource.Trim(),
                    Type = shaderPartTypeValues.TESS_CONTROL
                });

            if (!string.IsNullOrEmpty(tessEvalSource))
                shader.ShaderParts.Add(new ShaderPart
                {
                    ShaderSource = tessEvalSource.Trim(),
                    Type = shaderPartTypeValues.TESS_EVAL
                });


            if (!string.IsNullOrEmpty(geometryShaderSource))
                shader.ShaderParts.Add(new ShaderPart
                {
                    ShaderSource = geometryShaderSource.Trim(),
                    Type = shaderPartTypeValues.GEOMETRY
                });

            if (!string.IsNullOrEmpty(fragmentShaderSource))
                shader.ShaderParts.Add(new ShaderPart
                {
                    ShaderSource = fragmentShaderSource.Trim(),
                    Type = shaderPartTypeValues.FRAGMENT
                });


            return shader;
        }

        public static int ApplyShaderPart(int shaderProgramHandle, ShaderPart part)
        {
            var type = ShaderType.VertexShader;

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

            var err = GL.GetShaderInfoLog(part.ShaderHandle).Trim();
            if (!string.IsNullOrEmpty(err))
                Console.WriteLine(err);

            GL.AttachShader(shaderProgramHandle, part.ShaderHandle);
            Console.WriteLine("ShaderPart {0} [attaching]", part.Type);

            return part.ShaderHandle;
        }

        public static int ApplyShader(string shaderSource, ShaderType type)
        {
            var shaderProgramHandle = GL.CreateProgram();

            var shaderHandle = GL.CreateShader(type);

            GL.ShaderSource(shaderHandle, shaderSource);

            GL.CompileShader(shaderHandle);

            Console.WriteLine(GL.GetShaderInfoLog(shaderHandle));

            GL.AttachShader(shaderProgramHandle, shaderHandle);
            GL.LinkProgram(shaderProgramHandle);

            Console.WriteLine(GL.GetProgramInfoLog(shaderProgramHandle));

            return shaderProgramHandle;
        }

        public static int ApplyComputeShader(string computeShaderSource)
        {
            var shaderProgramHandle = GL.CreateProgram();

            var shaderHandle = GL.CreateShader(ShaderType.ComputeShader);

            GL.ShaderSource(shaderHandle, computeShaderSource);

            GL.CompileShader(shaderHandle);

            int rvalue;
            GL.GetShader(shaderHandle, ShaderParameter.CompileStatus, out rvalue);

            if (rvalue != 1)
            {
                Console.WriteLine("Error in compiling the compute shader\n");

                var err = GL.GetShaderInfoLog(shaderHandle).Trim();

                Console.WriteLine("Compiler error:\n{0}", err);

                return -1;
            }

            GL.AttachShader(shaderProgramHandle, shaderHandle);
            GL.LinkProgram(shaderProgramHandle);
            GL.GetProgram(shaderProgramHandle, GetProgramParameterName.LinkStatus, out rvalue);

            if (rvalue != 1)
            {
                Console.WriteLine("Error in linking compute shader program");

                var err = GL.GetProgramInfoLog(shaderProgramHandle).Trim();

                Console.WriteLine("Linker error:\n{0}", err);

                return -1;
            }

            return shaderProgramHandle;
        }
    }
}