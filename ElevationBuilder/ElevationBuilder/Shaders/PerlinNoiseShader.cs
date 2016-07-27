namespace X3D.Core.Shading
{
    public class PerlinNoiseShader
    {
        public static string vertexShaderSource = ShaderHelpers.getShaderSource("PerlinNoiseVertexShader.shader");
        public static string fragmentShaderSource = ShaderHelpers.getShaderSource("PerlinNoiseFragmentShader.shader");
    }
}