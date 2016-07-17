using System;
using OpenTK;

public static class QuaternionExtensions
{
	public static Vector3 Rotate(Quaternion q, Vector3 v) 
	{
        // conjugate(this) * [v,0] * this
        float _w = q.W;
        float _z = q.Z;
        float _y = q.Y;
        float _x = q.X;
        float tiw = _w;
        float tiz = -_z;
        float tiy = -_y;
        float tix = -_x;
		float tx = tiw * v.X + tix * 0.0f + tiy * v.Z - tiz * v.Y;
        float ty = tiw * v.Y + tiy * 0.0f + tiz * v.X - tix * v.Z;
        float tz = tiw * v.Z + tiz * 0.0f + tix * v.Y - tiy * v.X;
        float tw = tiw * 0.0f - tix * v.X - tiy * v.Y - tiz * v.Z;
		v.Z = tw * _z + tz * _w + tx * _y - ty * _x;
		v.Y = tw * _y + ty * _w + tz * _x - tx * _z;
		v.X = tw * _x + tx * _w + ty * _z - tz * _y;
		return v;
	}
}