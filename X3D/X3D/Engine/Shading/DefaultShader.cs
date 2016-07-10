using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace X3D.Engine.Shading
{
    public class DefaultShader
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
uniform int coloringEnabled;
uniform int texturingEnabled;

varying vec3 lightVec; 
varying vec3 eyeVec; 
varying vec3 normalVec;

out vec4 vColor;
out lowp vec2 uv;
out vec3 vPosition;

void main()
{
    mat4 model = projection * modelview;

    vPosition = X3DScale * camscale * scale * size * position;
	gl_Position = model * vec4(vPosition, 1.0);
    vColor = color;

	//gl_TexCoord[0] = gl_MultiTexCoord0; 
	normalVec = normalize(normal); // gl_Normal

	vec4 eyePos = gl_ModelViewMatrixInverse * vec4(0., 0., 0., 1.); 
	eyeVec = normalize(eyePos.xyz - position.xyz);

	vec4 lightPos = modelview * vec4(1.0, 0.0, 0.0, 1.0); // gl_ModelViewMatrixInverse  gl_LightSource[0].position.xyz
	lightVec = normalize(lightPos.xyz - position.xyz);

    uv = texcoord;
}
";

        public static string fragmentShaderSource = @"
#version 420 core
 
varying vec3 lightVec; 
varying vec3 eyeVec; 
varying vec3 normalVec;

in vec3 gFacetNormal;
in vec3 gTriDistance;
in vec3 gPatchDistance;
in float gPrimitive;
in vec2 gFacetTexCoord; 

in vec2 uv;
in vec4 vColor;
out vec4 FragColor;

uniform sampler2D _MainTex;
uniform vec3 specular = vec3(.7, .7, .7); 
uniform float ambient = 0.2;

uniform vec3 LightPosition;
uniform vec3 DiffuseMaterial;
uniform vec3 AmbientMaterial;
uniform int coloringEnabled;
uniform int texturingEnabled;

float amplify(float d, float scale, float offset)
{
    d = scale * d + offset;
    d = clamp(d, 0, 1);
    d = 1 - exp2(-2*d*d);
    return d;
}

void main()
{

    vec4 texture_color;

    if(texturingEnabled == 1)
    {
        if(length(uv) == 0)
        {
            texture_color = texture2D(_MainTex, gFacetTexCoord);
        }
        else 
        {
            texture_color = texture2D(_MainTex, uv);
        }
    }

    // PHONG SHADING TEST
	//vec3 texCol = vec3(0.1, 0.1, 0.1); 
	//vec3 halfVec = normalize( eyeVec + lightVec );
	//float ndotl = max( dot( lightVec, normalVec ), 0.0 ); 
	//float ndoth = (ndotl > 0.0) ? pow(max( dot( halfVec, normalVec ), 0.0 ), 128.) : 0.0;  
	//vec3 color = 0.2 * ambient + ndotl * texCol + ndoth * specular;

    //FragColor = vec4(color, 1.0);	
    //FragColor = vec4(color, 1.0) +  vColor / 2;
	//FragColor = vec4(0.5, 0.8, 1.0, 1.0);
    //FragColor = vColor;

    //vec4 op = texture_color + vColor / 2;
    //op = op + vec4(color, 1.0) / 2;

    //FragColor = op;



    // TexCoords from tessellation
    vec3 N = normalize(gFacetNormal);
    vec3 L = LightPosition;
    float df = abs(dot(N, L));
    vec3 color = AmbientMaterial + df * DiffuseMaterial;

    float d1 = min(min(gTriDistance.x, gTriDistance.y), gTriDistance.z);
    float d2 = min(min(gPatchDistance.x, gPatchDistance.y), gPatchDistance.z);
    color = amplify(d1, 40, -0.5) * amplify(d2, 60, -0.5) * color;

    vec4 col_accum;

    col_accum = vec4(color, 1.0) / 2;

    if(texturingEnabled == 1 && coloringEnabled == 1)
    {
        col_accum = texture_color;
        col_accum = col_accum + vColor / 2;
    }
    else if (coloringEnabled == 1)
    {
        col_accum = vColor;
    }   
    else if (texturingEnabled == 1)
    {
        col_accum = col_accum + texture_color / 2;
    }  
    else 
    {
        col_accum = vec4(0.0, 0, 0, 1.0);
    }
 
    FragColor = col_accum;
}

";
    }
}
