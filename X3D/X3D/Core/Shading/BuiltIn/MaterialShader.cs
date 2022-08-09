using System;

namespace X3D.Core.Shading
{
    public class MaterialShader
    {
        private static readonly string @base = AppDomain.CurrentDomain.BaseDirectory + "..\\..\\Shaders\\";

        public static string vertexShaderSource = ShaderCompiler.GetShaderSource("MaterialVertexShader.shader", @base);

        public static string fragmentShaderSource =
            ShaderCompiler.GetShaderSource("MaterialFragmentShader.shader", @base);
    }
}