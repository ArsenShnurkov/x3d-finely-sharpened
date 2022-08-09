using System;

namespace X3D.Core.Shading
{
    public class BackgroundShader
    {
        private static readonly string @base = AppDomain.CurrentDomain.BaseDirectory + "..\\..\\Shaders\\";

        public static string vertexShaderSource =
            ShaderCompiler.GetShaderSource("BackgroundVertexShader.shader", @base);

        public static string fragmentShaderSource =
            ShaderCompiler.GetShaderSource("BackgroundFragmentShader.shader", @base);
    }
}