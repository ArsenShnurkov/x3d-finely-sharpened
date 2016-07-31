#version 420 core

#define MAX_LIGHTS 3

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
in vec3 N;
in vec3 vPosition;
out vec4 FragColor;

uniform sampler2D _MainTex;
uniform vec3 specular = vec3(.7, .7, .7);
uniform float ambient = 0.2;

uniform vec3 LightPosition;
uniform vec3 DiffuseMaterial;
uniform vec3 AmbientMaterial;
uniform int coloringEnabled;
uniform int texturingEnabled;
uniform int materialsEnabled;

float amplify(float d, float scale, float offset)
{
	d = scale * d + offset;
	d = clamp(d, 0, 1);
	d = 1 - exp2(-2 * d*d);
	return d;
}

void main()
{

	vec4 texture_color;

	if (texturingEnabled == 1)
	{
		if (length(uv) == 0)
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
	vec3 Nf = normalize(gFacetNormal);
	vec3 L = LightPosition;
	float df = abs(dot(Nf, L));
	vec3 color = AmbientMaterial + df * DiffuseMaterial;

	float d1 = min(min(gTriDistance.x, gTriDistance.y), gTriDistance.z);
	float d2 = min(min(gPatchDistance.x, gPatchDistance.y), gPatchDistance.z);
	color = amplify(d1, 40, -0.5) * amplify(d2, 60, -0.5) * color;

	vec4 col_accum;

	col_accum = vec4(color, 1.0) / 2;

	if (texturingEnabled == 1 && coloringEnabled == 1)
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




	// PHONG SHADING
	//vec3 Light0 = normalize(gl_LightSource[0].position.xyz - vPosition);   
	//vec3 E = normalize(-vPosition); // we are in Eye Coordinates, so EyePos is (0,0,0)  
	//vec3 R = normalize(-reflect(Light0,N));  

	//vec4 Iamb = gl_FrontLightProduct[0].ambient;     
	//vec4 Idiff = gl_FrontLightProduct[0].diffuse * max(dot(N,Light0), 0.0);
	//Idiff = clamp(Idiff, 0.0, 1.0);     

	//vec4 Ispec = gl_FrontLightProduct[0].specular * pow(max(dot(R,E),0.0),0.3 * gl_FrontMaterial.shininess);
	//Ispec = clamp(Ispec, 0.0, 1.0); 
	//col_accum = col_accum + ( gl_FrontLightModelProduct.sceneColor + Iamb + Idiff + Ispec) / 2;






	vec4 finalColor = vec4(0.0, 0.0, 0.0, 0.0);

	for (int i = 0; i < MAX_LIGHTS; i++)
	{
		vec3 Light0 = normalize(gl_LightSource[i].position.xyz - vPosition);
		vec3 E = normalize(-vPosition); // we are in Eye Coordinates, so EyePos is (0,0,0) 
		vec3 R = normalize(-reflect(Light0, N));

		vec4 Iamb = gl_FrontLightProduct[i].ambient;
		vec4 Idiff = gl_FrontLightProduct[i].diffuse * max(dot(N, Light0), 0.0);
		Idiff = clamp(Idiff, 0.0, 1.0);

		vec4 Ispec = gl_FrontLightProduct[i].specular * pow(max(dot(R, E), 0.0), 0.3*gl_FrontMaterial.shininess);
		Ispec = clamp(Ispec, 0.0, 1.0);

		finalColor += Iamb + Idiff + Ispec;
	}

	col_accum = col_accum + (gl_FrontLightModelProduct.sceneColor + finalColor) / 2;

	FragColor = col_accum;

}