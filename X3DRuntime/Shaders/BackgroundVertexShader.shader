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

varying vec3 normalVec;

out vec3 vPosition;
out vec3 N;

void main()
{
	mat4 model = projection * modelview;

	vPosition = X3DScale * camscale * scale * size * position;

	gl_Position = model * vec4(vPosition, 1.0);

	N = normalize(gl_NormalMatrix * gl_Normal);
	normalVec = normalize(normal);
}