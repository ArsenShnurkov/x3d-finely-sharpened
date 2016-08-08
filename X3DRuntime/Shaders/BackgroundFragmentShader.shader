#version 420 core

// [Skydome Linear Interpolator]
// [  For X3D Background Node  ]
// Fragment Shader
//      by Gerallt Franke Copyright 2013 - 2016

varying vec3 normalVec;

in vec3 vPosition;
in vec3 N;
out vec4 FragColor;

uniform sampler2D _MainTex;

uniform int skyColors;
uniform float skyColor[255 * 3];
uniform float skyAngle[255];
uniform int isGround;
uniform vec3 bbox;
uniform vec3 max;
uniform vec3 min;

/// <summary>
/// Chooses between sky colors given an index.
/// </summary>
vec3 selectSkyColor(int index)
{
	vec3 color;

	color = vec3(skyColor[index * 3], skyColor[index * 3 + 1 ], skyColor[index * 3 + 2]);

	return color;
}

// INTERPOLATION functions

/// <summary>
/// Computes spherical linear interpolaton between two points 'from' 'to'
/// </summary>
vec3 slerp(vec3 from, vec3 to, float ratio) 
{
	vec3 average;
	float slerpRange;
	float slerpRangePhi;

	slerpRange = dot(normalize(from), normalize(to));

	slerpRangePhi = acos(slerpRange * 3.1415926535897931 / 180.0);

	average = (from * sin((1 - ratio) * slerpRangePhi)
		+ (to   * sin(ratio * slerpRangePhi)) / sin(slerpRangePhi));

	return average;
}

/// <summary>
/// Computes linear interpolation between two points 'from' 'to'
/// </summary>
vec3 lerp(vec3 from, vec3 to, float ratio)
{
	return vec3(from + (to - from) * ratio);
}

/// <summary>
/// Linear interpolation between two floating point values
/// </summary>
float lerpf(float from, float to, float ratio)
{
	return (from + (to - from) * ratio);
}

void main()
{
	vec3 sky;           // the resultant interpolated color value derived from the yielded sky colors
	vec3 seg_from;      // the color to interpolate from
	vec3 seg_to;        // the color to interpolate to
	float angle;        // sky or ground angle
	float next_angle;        // next sky or ground angle
	float pitch_ratio;   // Pitch anglular ratio of current vertex
	int i;              // Color index yelding what the sky color is for a segment that needs interpolating.
	int j;
	float skyPitchRatio;
	float PI = 3.14159;
	float PI2 = 2 * PI;
	vec4 texture_color;

	pitch_ratio = (max.y - vPosition.y) / bbox.y; // ratio where the vertex is around the sphere

	//i = int(pitch_ratio * skyColors);

	//sky = selectSkyColor(i);

	//FragColor = vec4(sky, 1.0);


	vec2 uv_sphere = vec2(0, 0);
	uv_sphere.y = pitch_ratio;

	texture_color = texture2D(_MainTex, uv_sphere);
	
	FragColor = texture_color;

	
}