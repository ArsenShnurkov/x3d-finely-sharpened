namespace X3D.Core.Shading
{
    public class ColorReplaceShader
    {
        public static string vertexShaderSource = @"
#version 400

layout(location = 0) in vec3 position;
layout(location = 1) in vec3 normal;
layout(location = 2) in vec4 color;
layout(location = 3) in vec2 texcoord;

//vec4 threshold = vec4(0.7, 0.7, 0.7, 1.0); // transparency threshold
uniform vec4 threshold;

uniform mat4 modelview;
uniform mat4 projection;
uniform float camscale;
uniform vec3 size;
uniform vec3 scale;
uniform vec3 X3DScale;
uniform int coloringEnabled;
uniform int texturingEnabled;

varying vec3 lightVec; 
varying vec3 eyeVec; 
varying vec3 normalVec;

out vec4 vColor;
out lowp vec2 uv;
out vec3 vPosition;
out vec4 vColorToDiscard;

void main() 
{
    mat4 model = projection * modelview;

    vPosition = X3DScale * camscale * scale * size * position;
	gl_Position = model * vec4(vPosition, 1.0);
    vColor = color;

	normalVec = normalize(normal); // gl_Normal

	vec4 eyePos = gl_ModelViewMatrixInverse * vec4(0., 0., 0., 1.); 
	eyeVec = normalize(eyePos.xyz - position.xyz);

	vec4 lightPos = modelview * vec4(1.0, 0.0, 0.0, 1.0); // gl_ModelViewMatrixInverse  gl_LightSource[0].position.xyz
	lightVec = normalize(lightPos.xyz - position.xyz);

    uv = texcoord;
    vColorToDiscard = threshold;
}
";

        public static string fragmentShaderSource = @"
#version 400

in vec2 uv;
in vec4 vColorToDiscard;
uniform sampler2D _MainTex;

void main() 
{
    vec4 c = texture2D(_MainTex, uv);

    gl_FragColor = c;

    // color discard threshold

    vec4 thres = vec4(0.1, 0.1, 0.1, 1.0);
    //vec4 thres = vColorToDiscard;

    if (c.r < thres.x && c.g < thres.y && c.b < thres.z) 
        discard;
}
";
    }
}