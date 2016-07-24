using System;
using OpenTK;

public static class QuaternionExtensions
{
	public static Vector3 Rotate(Quaternion q, Vector3 v) 
	{
        // conjugate(this) * [v,0] * this
        Quaternion c = new Quaternion(q.X, q.Y, q.Z, q.W);
        c.Conjugate();

        //q = c;

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

        //v = new Vector3();
		v.Z = tw * _z + tz * _w + tx * _y - ty * _x;
		v.Y = tw * _y + ty * _w + tz * _x - tx * _z;
		v.X = tw * _x + tx * _w + ty * _z - tz * _y;


		return v;
	}

    //public static Quaternion angle2quat(float yaw, float pitch, float roll)
    //{
    //    Quaternion q = new Quaternion();

    //    q = [cang(:, 1).* cang(:, 2).* cang(:, 3) + sang(:, 1).* sang(:, 2).* sang(:, 3), ...
    //        cang(:, 1).* cang(:, 2).* sang(:, 3) - sang(:, 1).* sang(:, 2).* cang(:, 3), ...
    //        cang(:, 1).* sang(:, 2).* cang(:, 3) + sang(:, 1).* cang(:, 2).* sang(:, 3), ...
    //        sang(:, 1).* cang(:, 2).* cang(:, 3) - cang(:, 1).* sang(:, 2).* sang(:, 3)];


    //    return q;
    //}

    public static Quaternion QuaternionFromEulerAnglesRad(float yaw, float pitch, float roll)
    {
        return new Quaternion(pitch, yaw, roll);

        //Quaternion q = new Quaternion();

        //float hRoll = roll * 0.5f;
        //float shr = (float)Math.Sin(hRoll);
        //float chr = (float)Math.Cos(hRoll);
        //float hPitch = pitch * 0.5f;
        //float shPitch = (float)Math.Sin(hPitch);
        //float chPitch = (float)Math.Cos(hPitch);
        //float hYaw = yaw * 0.5f;
        //float shYaw = (float)Math.Sin(hYaw);
        //float chYaw = (float)Math.Cos(hYaw);
        //float chy_shp = chYaw * shPitch;
        //float shy_chp = shYaw * chPitch;
        //float chy_chp = chYaw * chPitch;
        //float shy_shp = shYaw * shPitch;

        //q.X = (chy_shp * chr) + (shy_chp * shr);  // cos(yaw/2) * sin(pitch/2) * cos(roll/2) + sin(yaw/2) * cos(pitch/2) * sin(roll/2)
        //q.Y  = (shy_chp * chr) - (chy_shp * shr); // sin(yaw/2) * cos(pitch/2) * cos(roll/2) - cos(yaw/2) * sin(pitch/2) * sin(roll/2)
        //q.Z = (chy_chp * shr) - (shy_shp * chr);  // cos(yaw/2) * cos(pitch/2) * sin(roll/2) - sin(yaw/2) * sin(pitch/2) * cos(roll/2)
        //q.W = (chy_chp * chr) + (shy_shp * shr);  // cos(yaw/2) * cos(pitch/2) * cos(roll/2) + sin(yaw/2) * sin(pitch/2) * sin(roll/2)

        //return q;
    }
}