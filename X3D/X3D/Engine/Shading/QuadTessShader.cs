using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace X3D.Engine.Shading
{
    public class QuadTessShader
    {
        public static string tessControlShader = @"
#version 420 core
layout(vertices = 16) out;

#define ID gl_InvocationID

void main()
{
    gl_TessLevelInner[0] = 4;
    gl_TessLevelInner[1] = 4;

    gl_TessLevelOuter[0] = 4;
    gl_TessLevelOuter[1] = 4;
    gl_TessLevelOuter[2] = 4;
    gl_TessLevelOuter[3] = 4;

    gl_out[ID].gl_Position = gl_in[ID].gl_Position;
}
";

        public static string tessEvalShader = @"
#version 420 core
layout(quads, equal_spacing, cw) in;
uniform mat4 projection;
uniform mat4 modelview;
uniform vec3 scale;

float B(int i, float u)
{
    const vec4 bc = vec4(1, 3, 3, 1);

    return bc[i] * pow(u, i) * pow(1.0 - u, 3 - i);
}

void main()
{
    vec4 paccum = vec4(0.0);
    float u = gl_TessCoord.x;
    float v = gl_TessCoord.y;

    for(int j = 0; j < 4; j++)
    {
        for(int i = 0; i < 4; i++)
        {
            paccum += B(i, u) * B(j, v) * gl_in[4 * j + i].gl_Position;
            //paccum += gl_in[4 * j + i].gl_Position;
        }
    }
    gl_Position = projection * modelview * vec4(scale * paccum.xyz, 1.0);
}
";

    }
}
