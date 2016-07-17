using System;
using OpenTK;

public class MatrixExtensions
{
    public MatrixExtensions(Matrix4 mat4) { Matrix4 = mat4; }
    public Matrix4 Matrix4 { get; private set; }

    public static implicit operator MatrixExtensions(Matrix4 mat4)
    {
        return new MatrixExtensions(mat4);
    }

    //   public static Matrix4 LookAt(Vector3 eye, Vector3 center, Vector3 up) 
    //{
    //	Matrix4 @default = Matrix4.Zero;

    //	return LookAt(eye, center, up, @default);
    //}

    ///**
    //* Generates a look-at matrix with the given eye position, focal point, and up axis
    //*
    //* @param {vec3} eye Position of the viewer
    //* @param {vec3} center Point the viewer is looking at
    //* @param {vec3} up vec3 pointing "up"
    //* @param {mat4} [dest] mat4 frustum matrix will be written into
    //*
    //* Returns result if specified, a new mat4 otherwise
    //*/
    //public static Matrix4 LookAt(Vector3 eye, Vector3 center, Vector3 up, Matrix4 @default) 
    //{
    //	Matrix4 result = @default;

    //	if(result == null) result = Matrix4.Zero;

    //	double x0, x1, x2, y0, y1, y2, z0, z1, z2, len;

    //	if (eye.X == center.X && eye.Y == center.Y && eye.Z == center.Z) 
    //	{
    //		result = Matrix4.Identity;
    //		return result;
    //	}

    //	//static Vector3 direction(eye, center, z);
    //	z0 = eye.X - center.X;
    //	z1 = eye.Y - center.Y;
    //	z2 = eye.Z - center.Z;

    //	// normalize (no check needed for 0 because of early return)
    //	len = 1.0 / Math.Sqrt(z0 * z0 + z1 * z1 + z2 * z2);
    //	z0 *= len;
    //	z1 *= len;
    //	z2 *= len;

    //	//static Vector3 normalize(static Vector3 cross(up, z, x));
    //	x0 = up.Y * z2 - up.Z * z1;
    //	x1 = up.Z * z0 - up.X * z2;
    //	x2 = up.X * z1 - up.Y * z0;
    //	len = Math.Sqrt(x0 * x0 + x1 * x1 + x2 * x2);
    //	if (double.IsNaN(len)) 
    //	{
    //		x0 = 0.0;
    //		x1 = 0.0;
    //		x2 = 0.0;
    //	} 
    //	else 
    //	{
    //		len = 1.0 / len;
    //		x0 *= len;
    //		x1 *= len;
    //		x2 *= len;
    //	}

    //	//static Vector3 normalize(static Vector3 cross(z, x, y));
    //	y0 = z1 * x2 - z2 * x1;
    //	y1 = z2 * x0 - z0 * x2;
    //	y2 = z0 * x1 - z1 * x0;

    //	len = Math.Sqrt(y0 * y0 + y1 * y1 + y2 * y2);

    //	if (double.IsNaN(len)) 
    //	{
    //		y0 = 0.0;
    //		y1 = 0.0;
    //		y2 = 0.0;
    //	} 
    //	else 
    //	{
    //		len = 1.0 / len;
    //		y0 *= len;
    //		y1 *= len;
    //		y2 *= len;
    //	}
    //	result.M11 = x0;
    //	result.M12 = y0;
    //	result.M13 = z0;
    //	result.M14 = 0.0;
    //	result.M21 = x1;
    //	result.M22 = y1;
    //	result.M23 = z1;
    //	result.M24 = 0.0;
    //	result.M31 = x2;
    //	result.M32 = y2;
    //	result.M33 = z2;
    //	result.M34 = 0.0;
    //	result.M41 = -(x0 * eye.X + x1 * eye.Y + x2 * eye.Z);
    //	result.M42 = -(y0 * eye.X + y1 * eye.Y + y2 * eye.Z);
    //	result.M43 = -(z0 * eye.X + z1 * eye.Y + z2 * eye.Z);
    //	result.M44 = 1.0;

    //	/*
    //	result.storage[0] = x0;
    //	result.storage[1] = y0;
    //	result.storage[2] = z0;
    //	result.storage[3] = 0.0;
    //	result.storage[4] = x1;
    //	result.storage[5] = y1;
    //	result.storage[6] = z1;
    //	result.storage[7] = 0.0;
    //	result.storage[8] = x2;
    //	result.storage[9] = y2;
    //	result.storage[10] = z2;
    //	result.storage[11] = 0.0;
    //	result.storage[12] = -(x0 * eye.x + x1 * eye.y + x2 * eye.z);
    //	result.storage[13] = -(y0 * eye.x + y1 * eye.y + y2 * eye.z);
    //	result.storage[14] = -(z0 * eye.x + z1 * eye.y + z2 * eye.z);
    //	result.storage[15] = 1.0; 
    //	*/

    //	return result;
    //}

    public static Vector3 Transform(Matrix4 m, Vector3 v)
    {
        //float x_ =  (storage[0] * arg.storage[0]) + (storage[4] * arg.storage[1]) + (storage[8] * arg.storage[2]) + storage[12];
        //float y_ =  (storage[1] * arg.storage[0]) + (storage[5] * arg.storage[1]) + (storage[9] * arg.storage[2]) + storage[13];
        //float z_ =  (storage[2] * arg.storage[0]) + (storage[6] * arg.storage[1]) + (storage[10] * arg.storage[2]) + storage[14];
        float x_ = (m.M11 * v.X) + (m.M14 * v.Y) + (m.M23 * v.Z) + m.M32;
        float y_ = (m.M12 * v.X) + (m.M21 * v.Y) + (m.M24 * v.Z) + m.M33;
        float z_ = (m.M13 * v.X) + (m.M22 * v.Y) + (m.M31 * v.Z) + m.M34;
        v.X = x_;
        v.Y = y_;
        v.Z = z_;
        return v;
    }
}