//TODO: provide these cutom shader variables for compatibility with x3dom: http://doc.x3dom.org/tutorials/lighting/customShader/

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
out vec3 N;





//attribute vec3 position;
//attribute vec3 normal;
//attribute vec2 texcoord;
//attribute vec3 tangent;
//attribute vec3 binormal;
//uniform mat4 modelViewMatrix;
//uniform mat4 modelViewMatrixInverse;
//uniform mat4 modelViewProjectionMatrix;

varying vec3 fragNormal;
varying vec3 fragEyeVector;
varying vec2 fragTexCoord;
varying vec3 fragTangent;
varying vec3 fragBinormal;

void main()
{
	vec4 eye = vec4(modelViewMatrixInverse * vec4(0., 0., 0., 1.));
	fragEyeVector = position - eye.xyz;

	fragNormal = normal;
	fragTangent = tangent;
	fragBinormal = binormal;

	//mat3 matrix = mat3(normalize(normal), normalize(tangent), normalize(binormal));
	//fragNormalTS = transpose(matrix) * fragNormal;
	//fragEyeTS = transpose(matrix) * fragEyeVector;

	fragTexCoord = vec2(texcoord.x, 1.0 - texcoord.y);
	vPosition = position;
	gl_Position = modelViewProjectionMatrix * vec4(position, 1.0);
}