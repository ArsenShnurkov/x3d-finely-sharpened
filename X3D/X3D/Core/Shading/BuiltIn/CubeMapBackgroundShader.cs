namespace X3D.Core.Shading
{
    public class CubeMapBackgroundShader
    {
        public static string vertexShaderSource = @"
        #version 420 core
        layout(location = 0) in vec3 position;
        layout(location = 1) in vec3 normal;
        layout(location = 2) in vec4 color;
        layout(location = 3) in vec2 texcoord;

        uniform mat4 modelview;
        uniform mat4 projection;
        uniform float camscale;
        uniform vec3 size;
        uniform vec3 scale;
        uniform vec3 X3DScale;

        out vec3 vPosition;

        void main()
        {
            mat4 model = projection * modelview;
            vPosition = scale * size * position;

	        gl_Position = model * vec4(vPosition, 1.0);
        }
        ";

        public static string fragmentShaderSource = @"
        #version 420 core

        in vec3 vPosition;
        out vec4 FragColor;

        uniform samplerCube cube_texture;

        void main()
        {

            FragColor = texture (cube_texture, vPosition);
            
        }

        ";
    }
}