//TODO: provide these cutom shader variables for compatibility with x3dom: http://doc.x3dom.org/tutorials/lighting/customShader/

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

const int MATERIALS_LIMIT = 10; // finely-sharpened imposes a limit of 10 materials per object
const vec3 black = vec3(1, 1, 1);
const vec3 yellow = vec3(1, 1, 1);
const vec3 white = vec3(.9, .9, 1);

layout(std140, binding = 0) uniform X3DMaterialBlock
{
	X3DMaterial materials[MATERIALS_LIMIT];
};

#ifdef GL_ES
precision highp float;
#endif

uniform sampler2D tex;
uniform samplerCube cube;
uniform sampler2D bump;
varying vec3 fragNormal;
varying vec3 fragEyeVector;
varying vec2 fragTexCoord;
varying vec3 fragTangent;
varying vec3 fragBinormal;

void main()
{
	vec3 eye = normalize(fragEyeVector);

	vec3 normal = normalize(fragNormal);
	vec3 tangent = normalize(fragTangent);
	vec3 binormal = normalize(fragBinormal);

	vec4 texCol = texture2D(tex, fragTexCoord);
	vec3 bumpCol = texture2D(bump, fragTexCoord).rgb;

	vec3 tsn = 2.0 * (normalize(bumpCol) - 0.5);
	tsn = tsn.z * normal + tsn.y * tangent + tsn.x * binormal;
	normal = -normalize(tsn);

	vec3 cubecoord = reflect(eye, normal);
	vec4 cubeCol = textureCube(cube, cubecoord);

	float p = max(0.1, dot(normal, eye));
	texCol.rgb *= p;
	texCol.rgb += max(0.0, pow(p, 128.0)) * vec3(0.8);

	texCol.rgb = clamp(texCol.rgb, 0.0, 1.0);

	gl_FragColor = mix(texCol, cubeCol, 0.35);
}