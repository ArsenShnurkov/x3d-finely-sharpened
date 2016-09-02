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


uniform mat4 model;
uniform sampler2D _MainTex;
uniform int coloringEnabled;
uniform int texturingEnabled;
uniform int lightingEnabled;
uniform int headlightEnabled;

uniform vec3 sceneCameraPosition;
uniform vec3 forward;
uniform vec3 lookat;
uniform vec3 up;
uniform vec3 left;
uniform vec2 orientation;

uniform vec3 calib1; // for calibration
uniform vec3 calib2;

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

const int MATERIALS_LIMIT = 16; // finely-sharpened imposes a limit of 16 materials per object
const vec3 black = vec3(1, 1, 1);
const vec3 yellow = vec3(1, 1, 1);
const vec3 white = vec3(.9, .9, 1);

layout(std140, binding = 0) uniform X3DMaterialBlock
{
	X3DMaterial materials[MATERIALS_LIMIT];
};

float toDegrees(float radians) {
	return (radians / (2 * 3.1428)) * 360.0;
}

float toRadians(float degrees) {
	return (degrees / 360.0) * (2 * 3.1428);
}

vec3 ads(vec4 col_accum){
	vec3 result;
	vec3 Ka = vec3(0.5, 0.5, 0.5);
	vec3 Kd = col_accum.xyz; //vec3(0.0, 0.0, 0.0);
	vec3 Ks = vec3(0.9, 0.9, 0.0);
	vec3 normal;

	if (length(N) > 0) {
		normal = N;
	}
	if (length(normalVec) > 0) {
		normal = normalVec;
	}

	vec3 fragPosition = vec3(model * vec4(vPosition, 1));
	
	vec3 v = normalize(-fragPosition);
	vec3 lightIntensity = vec3(0.8, 0.8, 0.8);
	vec3 lightPosition = fragPosition;
	vec3 n = normalize(normal);
	vec3 s = normalize(lightPosition - fragPosition);
	
	vec3 h = normalize(v + s);
	float Shininess = 40.00001;

	/*return lightIntensity *
		(Ka +
			Kd * max(dot(s, n), 0.0) +
			Ks * pow(max(dot(h, n), 0.0), Shininess));*/

	if (texturingEnabled == 1)
	{
		lightIntensity = vec3(1, 1, 1);
	}

	result = lightIntensity *
		(col_accum.xyz * max(dot(s, n), 0.0)
			+ (Ks * pow(max(dot(h, n), 0.0), Shininess))
			);

	return result;
}

vec3 spotlight() {
	vec3 Ka = black;
	vec3 Kd = white;
	vec3 Ks = white;
	vec3 normal;

	vec3 fragPosition = vec3(model * vec4(vPosition, 1));

	vec3 spot_intensity = vec3(0.9, 0.9, 0.9);
	vec3 spot_direction = lookat;
	vec3 spot_position = sceneCameraPosition;

	vec3 surfaceToLight = spot_position - fragPosition;

	if (length(N) > 0) {
		normal = N;
	}

	if (length(normalVec) > 0) {
		normal = normalVec;
	}

	//calculate the cosine of the angle of incidence
	float maxBrightness = length(surfaceToLight) * length(normal);
	float brightness = (dot(normal, surfaceToLight) / maxBrightness);
	brightness = 1.0 - clamp(brightness, 0, 1);


	float spot_cutoff = 90;
	float Shininess = 0.7;
	float spot_exponent = 0.9001;

	vec3 s = normalize(spot_position - fragPosition);

	float angleX = acos( dot(-s, spot_direction) );
	float angleY = asin( dot(-s, spot_direction));

	float cutoffX = radians( clamp (spot_cutoff, 0.0, 90.0) );
	float cutoffY = radians( clamp (spot_cutoff, 0.0, 90.0) );

	vec3 ambient = spot_intensity * Ka;

	return brightness * Kd;

	//if (angleX < cutoffX && angleY < cutoffY) 
	{
		float spotFactor = pow(dot(-s, spot_direction), spot_exponent);

		vec3 v = normalize(vec3(-fragPosition));
		vec3 h = normalize(v + s);

		return ambient +
			spotFactor * spot_intensity * (

				Kd * max(dot(s, normal), 0.0) +
				Ks * pow(max(dot(h, normal), 0.0), Shininess)
			);
	}
	//else 
	//{
		//return ambient;
	//}
}

vec4 applyMaterials(vec4 col_accum)
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
	vec3 normal;
	//float depth = (length(camera.xyz - vPosition) - 1.0) / 10.0;

	d = 0.;
	Light_Cone_Min = 3.14 / 6.0;
	Light_Cone_Max = 3.14 / 4.0;
	lightAttenuationMax = 1.0;
	lightAttenuationMin = 0.0;
	Light_Intensity = vec4(0.4, 0.4, 0.4, 1.0);
	blended = vec4(0, 0, 0, 0);
	normal = normalVec; // N

	if (length(N) > 0) {
		normal = N;
	}

	if (length(normalVec) > 0) {
		normal = normalVec;
	}

	vec3 fragPosition = vec3(model * vec4(vPosition, 1));

	for (int i = 0; i < materialsCount; i++)
	{
		material = materials[i];

		ambientColor = material.diffuseColor * material.ambientIntensity;
		
		E = normalize(-fragPosition); // we are in Eye Coordinates, so EyePos is (0,0,0) 
								   //Light0 = normalize(gl_LightSource[i].position.xyz - vPosition);
		
		Light0 = normalize(-fragPosition.xyz);

		float cosTheta = clamp(dot(normal, Light0), 0, 1);

		R = normalize(-reflect(Light0, normal));

		Iamb = ambientColor;



		




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

		if (lightingEnabled == 1) 
		{
			// APPLY LIGHTING 
			// Apply the ambient, diffuse, specular, and emissive colors

			vec3 lightPosition = fragPosition;

			vec3 s = normalize(lightPosition - fragPosition);
			vec3 v = normalize(vec3(-fragPosition));
			vec3 h = normalize(v + s);
			float Shininess = 0.1;

			//Idiff = material.diffuseColor * max(dot(normal, Light0), 0.0);
			Idiff = material.diffuseColor * max(dot(s, normal), 0.0);
			//Idiff = material.diffuseColor;
			Idiff = clamp(Idiff, 0.0, 1.0);

			//Ispec = material.specularColor * pow(max(dot(R, E), 0.0), material.shininess);
			Ispec = material.specularColor * pow(max(dot(h, normal), 0.0), material.shininess);

			//Ispec = material.specularColor;
			Ispec = clamp(Ispec, 0.0, 1.0);

			//blended += (Iamb + Idiff + Ispec) * depth;
			//blended += (Iamb + Idiff + Ispec) * Light_Intensity * attenuation;
			//blended += (Iamb + Idiff + Ispec) * Light_Intensity;

			//blended += normalize((Iamb + Idiff + Ispec) * material.emissiveColor);
			//blended += normalize((Iamb + Idiff + Ispec));

			//blended += normalize((Iamb + Idiff + Ispec) * material.emissiveColor);




			//float distance = dot(sceneCameraPosition, fragPosition);

			//blended += Idiff * Ispec * cosTheta / (distance*distance);

			blended += (Ispec / 8) + ((Idiff + Iamb)) + material.emissiveColor; // Light_Intensity

			//blended += (((Iamb + Ispec / 2.0)) + Idiff) + material.emissiveColor;

			//blended += (Iamb + Light_Intensity * (Ispec * Idiff)) * material.emissiveColor;

			/*blended += Iamb + vec4(.3,.3,.3, 1.0) * Light_Intensity 
				* (
					Idiff * max(dot(s, normal), 0.0) +
					Ispec * pow(max(dot(h, normal), 0.0), Shininess)
				);*/

			//blended += max(Ispec * Idiff, Iamb) * material.emissiveColor;

			//blended = vec4(1, 0, 0, 1);
		}
		else 
		{
			Idiff = material.diffuseColor;
			Idiff = clamp(Idiff, 0.0, 1.0);

			// Since lighting is disabled
			// Apply the diffuse color component only.
			//blended += Idiff + material.emissiveColor;
			blended += material.emissiveColor * Idiff;
			
			//blended = vec4(1, 0, 0, 1.0);
		}


		//blended.w += material.transparency;
		//blended.w = 1.0;
		blended.w = 1.0 - material.transparency;
		
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

vec4 headlight(vec4 col_accum)
{
	vec4 out_color;
	vec3  lightPosition;
	vec3  lightDirection;
	float lightRange;
	float lightCosInnerAngle;
	float lightCosOuterAngle;
	vec3  lightDiffuseColor;
	float lightDiffuseIntensity;
	vec3  lightSpecularColor;
	float lightSpecularIntensity;
	float matSpecularPower = 1.0;
	vec3 cameraPosition;
	vec3 headlight_color;
	vec3 fragPosition;
	vec3 normal;
	float distanceAttenuation;
	float spotEffect;
	float beamDistance;
	float distToLight;
	float diffuseLight;
	vec3 diffuse;
	float specularLight;

	if (length(N) > 0) {
		normal = N;
	}

	if (length(normalVec) > 0) {
		normal = normalVec;
	}

	fragPosition = vec3(model * vec4(vPosition, 1));

	cameraPosition = sceneCameraPosition;
	lightPosition = eyeVec;//normalize(-sceneCameraPosition);
	lightDirection = lookat;

	lightRange = 900.0;
	lightCosInnerAngle = orientation.x + toRadians(56.464);
	lightCosOuterAngle = orientation.y - toRadians(56.06);

	lightDiffuseColor = white;
	lightDiffuseIntensity = 0.9;
	lightSpecularColor = white;
	lightSpecularIntensity = 0.9;
	headlight_color = white;

	vec3 L = lightPosition - fragPosition;
	
	distToLight = length(L);
	L = normalize(L);

	beamDistance = dot(L, -lightDirection);
	spotEffect = smoothstep(lightCosOuterAngle, lightCosInnerAngle, -beamDistance);

	distanceAttenuation = smoothstep(lightRange, 0.0f, distToLight);

	//vec3 N = normalize(normalVec);

	diffuseLight = max(dot(normalVec, L), 0.0f); // normalVec
	diffuse = (diffuseLight * lightDiffuseColor) * lightDiffuseIntensity;

	specularLight = 0.0f;

	if (matSpecularPower > 0.0f) 
	{
		vec3 V = normalize(cameraPosition - fragPosition);
		vec3 H = normalize(L + V);
		vec3 R = reflect(-L, normal);

		specularLight = pow(clamp(dot(R, H), 0.0f, 1.0f), matSpecularPower);
	}
	vec3 specular = (specularLight * lightSpecularColor) * lightSpecularIntensity;

	vec3 finalColor = (headlight_color) * spotEffect * distanceAttenuation;

	out_color = vec4(finalColor, 1.0f);

	return out_color;
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
	finalColor = vec4(0, 0, 0, 1);

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


		material_color = applyMaterials(col_accum);

		//col_accum = material_color;

		//col_accum = vec4(1, 0, 0, 1);

		col_accum = col_accum / 2.0 + material_color;
	}
	else {
		//col_accum = vec4(1, 0, 0, 1);
	}

	vec4 Ads1 = vec4(ads(col_accum), 1.0);

	if (lightingEnabled == 1) 
	{
		//col_accum = col_accum + Ads1 / 2.0;
	}
	
	if (headlightEnabled == 1)
	{
		//col_accum = col_accum + headlight(col_accum) / 2.0;
		//col_accum = col_accum + vec4(spotlight(), 1.0) / 2.0;

		col_accum = col_accum + Ads1 / 2.0;
	}
	

	//

	//col_accum = Ads1;

	//col_accum = vec4(1, 0, 0, 1);

	FragColor = col_accum;

}