namespace X3D.Core.Shading
{
    public class QuadTessShader
    {
        public static string tessControlShader = @"
#version 430 core
layout(vertices = 16) out;
in vec3 vPosition[];
out vec3 tcPosition[];
uniform float TessLevelInner;
uniform float TessLevelOuter;

#define ID gl_InvocationID

void main()
{
    tcPosition[ID] = vPosition[ID];

    float inner;
    float outer;

    inner = TessLevelInner > 0 ? TessLevelInner : 137; // 3
    outer = TessLevelInner > 0 ? TessLevelInner : 115; // 2

    if (ID == 0) {
        gl_TessLevelInner[0] = inner;
        gl_TessLevelInner[1] = inner;
        gl_TessLevelOuter[0] = outer;
        gl_TessLevelOuter[1] = outer;
        gl_TessLevelOuter[2] = outer;
        gl_TessLevelOuter[3] = outer;
    }
}
";

        public static string tessEvalShader = @"
#version 430 core
layout (quads) in;
in vec3 tcPosition[];
out vec3 tePosition;
out vec4 tePatchDistance;
uniform mat4 projection;
uniform mat4 modelview;
uniform vec3 scale;
uniform float camscale;
uniform vec3 size;
uniform vec3 X3DScale;
uniform mat4 B;
uniform mat4 BT;
void main(void)
{
    // TRIANGLES 1
    //vec4 p1 = mix(gl_in[1].gl_Position,gl_in[0].gl_Position,gl_TessCoord.x);
    //vec4 p2 = mix(gl_in[2].gl_Position,gl_in[3].gl_Position,gl_TessCoord.x);
    //gl_Position = mix(p1, p2, gl_TessCoord.y);


    // TRIANGLES 2
    //gl_Position=(gl_TessCoord.x*gl_in[0].gl_Position+gl_TessCoord.y*gl_in[1].gl_Position+gl_TessCoord.z*gl_in[2].gl_Position);


    // QUADS 1
    float u = gl_TessCoord.x, v = gl_TessCoord.y;

    mat4 Px = mat4(
        tcPosition[0].x, tcPosition[1].x, tcPosition[2].x, tcPosition[3].x, 
        tcPosition[4].x, tcPosition[5].x, tcPosition[6].x, tcPosition[7].x, 
        tcPosition[8].x, tcPosition[9].x, tcPosition[10].x, tcPosition[11].x, 
        tcPosition[12].x, tcPosition[13].x, tcPosition[14].x, tcPosition[15].x );

    mat4 Py = mat4(
        tcPosition[0].y, tcPosition[1].y, tcPosition[2].y, tcPosition[3].y, 
        tcPosition[4].y, tcPosition[5].y, tcPosition[6].y, tcPosition[7].y, 
        tcPosition[8].y, tcPosition[9].y, tcPosition[10].y, tcPosition[11].y, 
        tcPosition[12].y, tcPosition[13].y, tcPosition[14].y, tcPosition[15].y );

    mat4 Pz = mat4(
        tcPosition[0].z, tcPosition[1].z, tcPosition[2].z, tcPosition[3].z, 
        tcPosition[4].z, tcPosition[5].z, tcPosition[6].z, tcPosition[7].z, 
        tcPosition[8].z, tcPosition[9].z, tcPosition[10].z, tcPosition[11].z, 
        tcPosition[12].z, tcPosition[13].z, tcPosition[14].z, tcPosition[15].z );

    mat4 cx = B * Px * BT;
    mat4 cy = B * Py * BT;
    mat4 cz = B * Pz * BT;

    vec4 U = vec4(u*u*u, u*u, u, 1);
    vec4 V = vec4(v*v*v, v*v, v, 1);

    float x = dot(cx * V, U);
    float y = dot(cy * V, U);
    float z = dot(cz * V, U);
    tePosition =  vec3(x, y, z);

    tePatchDistance = vec4(u, v, 1-u, 1-v);
    tePosition = X3DScale * camscale * scale * size * vec3(x, y, z);
    gl_Position = projection * modelview * vec4(tePosition, 1);
}

";

        //public static string geometryShaderSource = null;
        public static string geometryShaderSource = @"
#version 420 core
uniform mat4 modelview;
uniform mat3 normalmatrix;
uniform float bboxMaxWidth;
uniform float bboxMaxDepth;
uniform float bboxMaxHeight;

layout (triangles, max_vertices = 3) in;
layout (triangle_strip, max_vertices = 3) out; // triangle_strip

in vec3 tePosition[3];
in vec4 tePatchDistance[3];

out vec3 gFacetNormal;
out vec3 gPatchDistance;
out vec3 gTriDistance;
out vec2 gFacetTexCoord;

void main()
{


    vec3 A = tePosition[2] - tePosition[0];
    vec3 B = tePosition[1] - tePosition[0];

    gFacetNormal = normalmatrix  * normalize(cross(A, B));
    gFacetTexCoord  = vec2((tePosition[0].x / bboxMaxWidth) * 1.0, (tePosition[0].z / bboxMaxDepth) * 1.0); // ElevationGrid TexCoord  


    gl_Position = gl_in[0].gl_Position; 
    EmitVertex();


    gl_Position = gl_in[1].gl_Position; 
    EmitVertex();


    gl_Position = gl_in[2].gl_Position; 
    EmitVertex();

    //gl_Position = gl_in[3].gl_Position; 
    //EmitVertex();

    EndPrimitive();
}
";
    }
}