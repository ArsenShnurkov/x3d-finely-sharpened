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
out vec4 worldPos;

out vec4 camera;

void main()
{
	mat4 model = projection * modelview;

	vPosition = X3DScale * camscale * scale * size * position;
	worldPos = model * vec4(vPosition, 1.);

	gl_Position = worldPos;

	vColor = color;

	//gl_TexCoord[0] = gl_MultiTexCoord0; 
	normalVec = normalize(normal); // gl_Normal
	N = normalize(gl_NormalMatrix * gl_Normal);

	vec4 eyePos = gl_ModelViewMatrixInverse * vec4(0., 0., 0., 1.);
	camera = eyePos;
	eyeVec = normalize(eyePos.xyz - position.xyz);

	vec4 lightPos = modelview * vec4(1., 0., 0., 1.); // gl_ModelViewMatrixInverse  gl_LightSource[0].position.xyz
	lightVec = normalize(lightPos.xyz - position.xyz);

	uv = texcoord;
}