using System;
using OpenTK;

public static class QuaternionExtensions
{
    public static Vector3 ExtractPitchYawRoll(Quaternion q)
    {
        Vector3 v;
        double pitch;
        double yaw;
        double roll;

        roll = Math.Atan2(2 * q.Y * q.W - 2 * q.X * q.Z, 1 - 2 * q.Y * q.Y - 2 * q.Z * q.Z);
        pitch = Math.Atan2(2 * q.X * q.W - 2 * q.Y * q.Z, 1 - 2 * q.X * q.X - 2 * q.Z * q.Z);
        yaw = Math.Asin(2 * q.X * q.Y + 2 * q.Z * q.W);

        v = new Vector3
        {
            X = (float)pitch,
            Y = (float)yaw,
            Z = (float)roll
        };

        return v;
    }

    public static Vector3 Rotate(Quaternion q, Vector3 v)
    {
        // conjugate(this) * [v,0] * this
        var c = new Quaternion(q.X, q.Y, q.Z, q.W);
        c.Conjugate();

        //q = c;

        var _w = q.W;
        var _z = q.Z;
        var _y = q.Y;
        var _x = q.X;
        var tiw = _w;
        var tiz = -_z;
        var tiy = -_y;
        var tix = -_x;
        var tx = tiw * v.X + tix * 0.0f + tiy * v.Z - tiz * v.Y;
        var ty = tiw * v.Y + tiy * 0.0f + tiz * v.X - tix * v.Z;
        var tz = tiw * v.Z + tiz * 0.0f + tix * v.Y - tiy * v.X;
        var tw = tiw * 0.0f - tix * v.X - tiy * v.Y - tiz * v.Z;

        //v = new Vector3();
        v.Z = tw * _z + tz * _w + tx * _y - ty * _x;
        v.Y = tw * _y + ty * _w + tz * _x - tx * _z;
        v.X = tw * _x + tx * _w + ty * _z - tz * _y;


        return v;
    }

    public static Quaternion EulerToQuat(float roll, float pitch, float yaw)
    {
        float cr, cp, cy, sr, sp, sy, cpcy, spsy;
        // calculate trig identities
        cr = (float)Math.Cos(roll / 2);
        cp = (float)Math.Cos(pitch / 2);
        cy = (float)Math.Cos(yaw / 2);
        sr = (float)Math.Sin(roll / 2);
        sp = (float)Math.Sin(pitch / 2);
        sy = (float)Math.Sin(yaw / 2);
        cpcy = cp * cy;
        spsy = sp * sy;

        var quat = new Quaternion();

        quat.W = cr * cpcy + sr * spsy;
        quat.X = sr * cpcy - cr * spsy;
        quat.Y = cr * sp * cy + sr * cp * sy;
        quat.Z = cr * cp * sy - sr * sp * cy;

        quat.Normalize();
        quat.Conjugate();
        quat.Normalize();

        return quat;
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
    public static Quaternion QuaternionFromEulerAnglesRad(Vector3 rotation)
    {
        return QuaternionFromEulerAnglesRad(rotation.X, rotation.Y, rotation.Z);
    }

    public static Quaternion QuaternionFromEulerAnglesRad(float yaw, float pitch, float roll)
    {
        var q = new Quaternion(pitch, yaw, roll);

        q.Normalize();
        q.Conjugate();
        q.Normalize();

        return q;
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