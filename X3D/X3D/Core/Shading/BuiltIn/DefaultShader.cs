using System;

namespace X3D.Core.Shading
{
    public class DefaultShader
    {
        private static readonly string @base = AppDomain.CurrentDomain.BaseDirectory + "..\\..\\Shaders\\";

        public static string vertexShaderSource = ShaderCompiler.GetShaderSource("DefaultVertexShader.shader", @base);

        public static string fragmentShaderSource =
            ShaderCompiler.GetShaderSource("DefaultFragmentShader.shader", @base);
    }
}