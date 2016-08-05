#version 420

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
in vec4 worldPos;
in vec4 camera;
out vec4 FragColor;

uniform sampler2D _MainTex;
uniform int coloringEnabled;
uniform int texturingEnabled;
uniform int lightingEnabled;

uniform int materialsEnabled;
uniform int materialsCount;

struct X3DMaterial
{
	vec4 diffuseColor;
	vec4 emissiveColor; // emissive colors are visible even if no light sources are directed at the surface
	vec4 specularColor;
	float ambientIntensity; // specifies how much ambient light this surface shall reflect
	float shininess;
	float transparency;
};

const int MATERIALS_LIMIT = 10; // finely-sharpened imposes a limit of 10 materials per object

layout(std140, binding = 0) uniform X3DMaterialBlock
{
	X3DMaterial materials[MATERIALS_LIMIT];
};

vec3 ads(){
	vec3 Ka = vec3(0.0, 0.0, 0.0);
	vec3 Kd = vec3(0.0, 0.0, 0.0);
	vec3 Ks = vec3(0.9, 0.9, 0.9);

	vec3 v = normalize(-vPosition);
	vec3 lightIntensity = vec3(0.9, 0.9, 0.9);
	vec3 lightPosition = vec3(100, 100, 100);
	vec3 n = normalize(N);
	vec3 s = normalize(lightPosition - vPosition);
	
	vec3 h = normalize(v + s);
	float Shininess = 40.00001;

	return lightIntensity *
		(Ka +
			Kd * max(dot(s, n), 0.0) +
			Ks * pow(max(dot(h, n), 0.0), Shininess));
}

vec3 spotlight() {
	vec3 Ka = vec3(0.0, 0.0, 0.0);
	vec3 Kd = vec3(0.0, 0.8, 0.0);
	vec3 Ks = vec3(0.9, 0.9, 0.9);

	vec3 spot_intensity = vec3(0.9, 0.9, 0.9);
	vec3 spot_direction = vec3(0,1,0);
	vec3 spot_position = vec3(10, 10, 10);
	float spot_cutoff = 89;
	float Shininess = 0.01001;
	float spot_exponent = 2.9001;

	vec3 s = normalize(spot_position - vPosition);
	float angle = acos( dot(-s, spot_direction) );
	float cutoff = radians( clamp (spot_cutoff, 0.0, 90.0) );
	vec3 ambient = spot_intensity * Ka;

	if (angle < cutoff) {
		float spotFactor = pow(dot(-s, spot_direction),
			spot_exponent);

		vec3 v = normalize(vec3(-vPosition));
		vec3 h = normalize(v + s);

		return ambient +
			spotFactor * spot_intensity * (

				Kd * max(dot(s, N), 0.0) +
				Ks * pow(max(dot(h, N), 0.0), Shininess)
				);
	}
	else {
		return ambient;
	}
}

vec4 applyMaterials()
{
	X3DMaterial material;
	vec4 blended;
	vec3 Light0;
	vec3 R;
	vec4 Iamb;
	vec4 Idiff;
	vec4 Ispec;
	vec3 E;
	vec4 ambientColor;

	float lightAttenuationMax;
	float lightAttenuationMin;
	float d;
	float attenuation;
	float Light_Cone_Min;
	float Light_Cone_Max;
	float LdotS;
	float CosI;
	vec4 Light_Intensity;

	//float depth = (length(camera.xyz - vPosition) - 1.0) / 10.0;

	d = 0.;
	Light_Cone_Min = 3.14 / 6.0;
	Light_Cone_Max = 3.14 / 4.0;
	lightAttenuationMax = 1.0;
	lightAttenuationMin = 0.0;
	Light_Intensity = vec4(0.1, 0.1, 0.1, 1.0);
	blended = vec4(0, 0, 0, 0);


	for (int i = 0; i < materialsCount; i++)
	{
		material = materials[i];

		ambientColor = material.diffuseColor * material.ambientIntensity;
		
		E = normalize(-vPosition); // we are in Eye Coordinates, so EyePos is (0,0,0) 
								   //Light0 = normalize(gl_LightSource[i].position.xyz - vPosition);
		
		Light0 = normalize(-eyeVec.xyz);

		R = normalize(-reflect(Light0, N));

		Iamb = ambientColor;

		Idiff = material.diffuseColor * max(dot(N, Light0), 0.0);
		//Idiff = material.diffuseColor;
		Idiff = clamp(Idiff, 0.0, 1.0);

		Ispec = material.specularColor * pow(max(dot(R, E), 0.0), 0.3 * material.shininess);
		//Ispec = material.specularColor;
		Ispec = clamp(Ispec, 0.0, 1.0);






		// Hermite interpolation for smooth variations of light level

		//attenuation = smoothstep(lightAttenuationMax, lightAttenuationMin, d);

		// Adjust attenuation based on light cone.

		//vec3 S = normalize(Light0);

		//vec3 L = normalize(Light0);
		//vec3 E = normalize(attrib_Fragment_Eye);
		//vec3 H = normalize(L + E);



		//LdotS = dot(-L, S);
		//CosI = Light_Cone_Min - Light_Cone_Max;

		//attenuation *= clamp((LdotS - Light_Cone_Max) / CosI, 0.0, 1.0);


		//blended += (Iamb + Idiff + Ispec) * Light_Intensity;
		blended += (Iamb + Idiff + Ispec) * material.emissiveColor;


		//blended += (Iamb + Idiff + Ispec) * depth;
		//blended += (Iamb + Idiff + Ispec) * Light_Intensity * attenuation;
		//blended += Idiff;
		//blended.w += material.transparency;

		blended.w = material.transparency;
		//blended.w = 1.0;


		//float depth = (length(camera.xyz - vPosition) - 1.0) / 49.0;
		//blended = vec4(depth, depth, depth, 1.0);


		//blended = material.diffuseColor;

		//blended = mix(blended, material.diffuseColor, 0.5);
		//blended = blended + material.diffuseColor / 2;
	}

	//blended = vec4(0.69803923, 0.5176471, 0.03137255, 1.0);

	//return materials[0].test2;

	//return vec4(X3DMaterial.test, 1.0);
	//return vec4(materials[0].diffuseColor, 1.0);
	//return vec4(1, 0, 0, 1.0);

	return blended;
}

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
	vec4 material_color;
	vec4 col_accum;
	vec4 finalColor;

	texture_color = vec4(0, 0, 0, 1);
	finalColor = vec4(0.0, 0.0, 0.0, 1.0);

	// TEXTURING
	if (texturingEnabled == 1)
	{
		if (length(gFacetTexCoord) != 0)
		{
			texture_color = texture2D(_MainTex, gFacetTexCoord);
		}
		else
		{
			texture_color = texture2D(_MainTex, uv);
		}
	}

	col_accum = texture_color;

	// TESSELLATION edge collorings
	vec3 Nf = normalize(gFacetNormal);
	vec3 L = vec3(0.25, 0.25, 1.00);
	float df = abs(dot(Nf, L));
	vec3 color = (df * col_accum.xyz);

	float d1 = min(min(gTriDistance.x, gTriDistance.y), gTriDistance.z);
	float d2 = min(min(gPatchDistance.x, gPatchDistance.y), gPatchDistance.z);
	color = amplify(d1, 40, -0.5) * amplify(d2, 60, -0.5) * color;
	col_accum = col_accum + vec4(color, 1.0) / 2;

	// COLORING
	if (coloringEnabled == 1)
	{
		col_accum = vColor + col_accum / 2;
	}

	// MATERIALS
	if (materialsEnabled == 1)
	{
		// MaterialFragmentShader.shader should be linked in so we can use the functions it provides.


		material_color = applyMaterials();

		col_accum = col_accum + material_color / 2;

		//col_accum = material_color;
	}
	else {
		//col_accum = vec4(1, 0, 0, 1);
	}

	vec4 Ads1 = vec4(ads(), 1.0);

	if (lightingEnabled == 1) 
	{
		//col_accum = col_accum + Ads1 / 2;
	}
	



	//col_accum = col_accum + vec4(spotlight(), 1.0) / 2;

	//col_accum = Ads1;

	//col_accum = vec4(1, 0, 0, 1);

	FragColor = col_accum;

}