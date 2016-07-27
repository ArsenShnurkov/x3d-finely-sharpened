#version 400

// original source: http://www.kamend.com/2012/06/perlin-noise-and-glsl/

layout(location = 0) in vec3 position;
layout(location = 1) in vec3 normal;
layout(location = 2) in vec4 color;
layout(location = 3) in vec2 texcoord;

uniform mat4 modelview;
uniform vec3 size;
uniform vec3 scale;

out lowp vec2 uv;
out vec3 vPosition;

void main()
{

	vPosition = scale * size * position;
	gl_Position = modelview * vec4(vPosition, 1.0);

	uv = texcoord;
}