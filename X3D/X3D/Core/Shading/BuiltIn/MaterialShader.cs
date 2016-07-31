using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace X3D.Core.Shading
{
    public class MaterialShader
    {

        private static string @base = AppDomain.CurrentDomain.BaseDirectory + "..\\..\\Shaders\\";

        public static string vertexShaderSource = ShaderCompiler.GetShaderSource("MaterialVertexShader.shader", @base);
        public static string fragmentShaderSource = ShaderCompiler.GetShaderSource("MaterialFragmentShader.shader", @base);

    }
}
