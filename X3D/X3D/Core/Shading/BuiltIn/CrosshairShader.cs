using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace X3D.Core.Shading
{
    public class CrosshairShader
    {
        public static string vertexShaderSource = @"
#version 400

layout(location = 0) in vec3 position;
layout(location = 1) in vec3 normal;
layout(location = 2) in vec4 color;
layout(location = 3) in vec2 texcoord;

uniform mat4 modelview;
uniform vec3 size;
uniform vec3 scale;

out lowp vec2 uv;
out vec3 vPosition;
out vec4 vColorToDiscard;

vec4 threshold = vec4(0.1, 0.1, 0.1, 1.0); // transparency threshold

void main() 
{

    vPosition = scale * size * position;
	gl_Position = modelview * vec4(vPosition, 1.0);

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

    if (c.r < vColorToDiscard.x && c.g < vColorToDiscard.y && c.b < vColorToDiscard.z) 
        discard;
}
";
    }
}
