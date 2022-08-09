using System;

namespace X3D.Core.Shading
{
    public class PerlinNoiseShader
    {
        private static readonly string @base = AppDomain.CurrentDomain.BaseDirectory + "..\\..\\Shaders\\";

        public static string vertexShaderSource =
            ShaderCompiler.GetShaderSource("PerlinNoiseVertexShader.shader", @base);

        public static string fragmentShaderSource =
            ShaderCompiler.GetShaderSource("PerlinNoiseFragmentShader.shader", @base);
    }
}