// original source for shader tessellation adapted from Philip Rideout http://prideout.net/blog/?p=48

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace X3D.Engine.Shading
{
    public class TriangleTessShader
    {
        public static string tessControlShader = @"
#version 420 core
layout(vertices = 3) out;
in vec3 vPosition[];
out vec3 tcPosition[];
uniform float TessLevelInner;
uniform float TessLevelOuter;

#define ID gl_InvocationID

void main()
{
    tcPosition[ID] = vPosition[ID];
    if (ID == 0) {
        gl_TessLevelInner[0] = TessLevelInner;
        gl_TessLevelOuter[0] = TessLevelOuter;
        gl_TessLevelOuter[1] = TessLevelOuter;
        gl_TessLevelOuter[2] = TessLevelOuter;
    }
}
";

        public static string tessEvalShader = @"
#version 420 core
layout(triangles, equal_spacing, cw) in;
in vec3 tcPosition[];
out vec3 tePosition;
out vec3 tePatchDistance;
uniform mat4 projection;
uniform mat4 modelview;
uniform vec3 scale;
uniform float camscale;
uniform vec3 size;
uniform vec3 X3DScale;
void main()
{
    vec3 p0 = gl_TessCoord.x * tcPosition[0];
    vec3 p1 = gl_TessCoord.y * tcPosition[1];
    vec3 p2 = gl_TessCoord.z * tcPosition[2];
    tePatchDistance = gl_TessCoord;
    tePosition = X3DScale * camscale * scale * size * normalize(p0 + p1 + p2);
    gl_Position = projection * modelview * vec4(tePosition, 1);
}
";
        public static string geometryShaderSource = @"
#version 420 core
uniform mat4 modelview;
uniform mat3 normalmatrix;
layout(triangles) in;
layout(triangle_strip, max_vertices = 3) out;

in vec3 tePosition[3];
in vec3 tePatchDistance[3];

out vec3 gFacetNormal;
out vec3 gPatchDistance;
out vec3 gTriDistance;
out vec2 gFacetTexCoord;

#define M_PI 3.1415926535897932384626433832795

void main()
{
    vec3 A = tePosition[2] - tePosition[0];
    vec3 B = tePosition[1] - tePosition[0];
    gFacetNormal = normalmatrix  * normalize(cross(A, B));
    gFacetTexCoord = vec2(asin(gFacetNormal.x)/M_PI + 0.5 , asin(gFacetNormal.y) / M_PI + 0.5 ); // Sphere TexCoord

    gPatchDistance = tePatchDistance[0];
    //gTriDistance = vec3(1, 0, 0);
    gl_Position = gl_in[0].gl_Position; 
    EmitVertex();

    gPatchDistance = tePatchDistance[1];
    //gTriDistance = vec3(0, 1, 0);
    gl_Position = gl_in[1].gl_Position; 
    EmitVertex();

    gPatchDistance = tePatchDistance[2];
    //gTriDistance = vec3(0, 0, 1);
    gl_Position = gl_in[2].gl_Position; 
    EmitVertex();

    EndPrimitive();
}
";
    }
}
