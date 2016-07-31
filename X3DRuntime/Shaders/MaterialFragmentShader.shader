#version 420 core

// [Material Blender]
// [  For X3D Shape Node  ]
// Fragment Shader
//      by Gerallt Franke Copyright 2013 - 2016

in vec3 vPosition;
out vec4 FragColor;

void main()
{
	vec3 material_color = vec3(0, 1, 1);

	FragColor = vec4(material_color, 1.0);
}