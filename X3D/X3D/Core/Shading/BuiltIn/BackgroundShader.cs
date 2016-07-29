using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace X3D.Core.Shading
{
    public class BackgroundShader
    {

        private static string @base = AppDomain.CurrentDomain.BaseDirectory + "..\\..\\Shaders\\";

        public static string vertexShaderSource = ShaderCompiler.GetShaderSource("BackgroundVertexShader.shader", @base);
        public static string fragmentShaderSource = ShaderCompiler.GetShaderSource("BackgroundFragmentShader.shader", @base);

    }
}