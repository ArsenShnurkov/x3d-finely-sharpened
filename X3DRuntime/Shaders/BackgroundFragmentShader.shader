#version 420 core

// [Skydome Linear Interpolator]
// [  For X3D Background Node  ]
// Fragment Shader
//      by Gerallt Franke Copyright 2013 - 2016

in vec3 vPosition;
out vec4 FragColor;

uniform int skyColors;
uniform float skyColor[255 * 3];
uniform float skyAngle[255];
uniform int isGround;
uniform vec3 bbox;

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
/// Computes spherical linear interpolaton between two points p0 p1
/// </summary>
vec3 slerp(vec3 from, vec3 to, float ratio) 
{
	vec3 average;
	float slerpRange;
	float slerpRangePhi;

	slerpRange = dot(normalize(from), normalize(to));

	slerpRangePhi = acos(slerpRange * 3.14159 / 180.0);

	average = (from * sin((1 - ratio) * slerpRangePhi)
		+ (to   * sin(ratio * slerpRangePhi)) / sin(slerpRangePhi));

	return average;
}

/// <summary>
/// Computes linear interpolation between two points p0 p1
/// </summary>
vec3 lerp(vec3 from, vec3 p1, float ratio)
{
	return vec3(from + (p1 - from) * ratio);
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

	pitch_ratio = (vPosition.y / bbox.y); // ratio where the vertex is around the sphere


	i = int(mod(pitch_ratio, skyColors)); // index into the sky colors
	//i = int(pitch_ratio * skyColors);
	j = i > skyColors ? i : i + 1;

	angle = skyAngle[i];
	next_angle = skyAngle[j];

	seg_from = selectSkyColor(i);
	seg_to = selectSkyColor(j);
	
	//BUG: this isnt quite how the Background node is meant to be implemented. 
	// Need to take into account the skyAngles so color ranges are placed where they are meant to be.

	//i = int(mod(angle / PI2, skyColors));

	//float angSegment = lerpf();

	sky = slerp(seg_from, seg_to, pitch_ratio);

	//sky = selectSkyColor(5);

	FragColor = vec4(sky, 1.0);
}