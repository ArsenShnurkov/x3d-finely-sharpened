using OpenTK;

public static class VectorExt
{
    public static Vector3 Cross(this Vector3 left, Vector3 right)
    {
        return VectorExtensions.Cross(left, right);
    }
}

public class VectorExtensions
{
    public VectorExtensions(Vector3 vec3)
    {
        Vector3 = vec3;
    }

    public Vector3 Vector3 { get; private set; }


    /// <summary>
    ///     Caclulate the cross (vector) product of two vectors
    /// </summary>
    /// <param name="left">First operand</param>
    /// <param name="right">Second operand</param>
    /// <returns>The cross product of the two inputs</returns>
    public static Vector3 Cross(Vector3 left, Vector3 right)
    {
        Vector3 result;
        Cross(ref left, ref right, out result);
        return result;
    }

    /// <summary>
    ///     Caclulate the cross (vector) product of two vectors
    /// </summary>
    /// <param name="left">First operand</param>
    /// <param name="right">Second operand</param>
    /// <returns>The cross product of the two inputs</returns>
    /// <param name="result">The cross product of the two inputs</param>
    public static void Cross(ref Vector3 left, ref Vector3 right, out Vector3 result)
    {
        result = new Vector3(left.Y * right.Z - left.Z * right.Y,
            left.Z * right.X - left.X * right.Z,
            left.X * right.Y - left.Y * right.X);
    }

    public static implicit operator VectorExtensions(Vector3 vec3)
    {
        return new VectorExtensions(vec3);
    }

    public static VectorExtensions operator -(VectorExtensions vector, VectorExtensions value)
    {
        vector.Vector3 = new Vector3(vector.Vector3.X - value.Vector3.X,
            vector.Vector3.Y - value.Vector3.Y,
            vector.Vector3.Z - value.Vector3.Z);
        return vector;
    }

    #region double

    public static VectorExtensions operator *(VectorExtensions vector, double scale)
    {
        vector.Vector3 = new Vector3(vector.Vector3.X * (float)scale,
            vector.Vector3.Y * (float)scale,
            vector.Vector3.Z * (float)scale);

        return vector;
    }

    public static VectorExtensions operator +(VectorExtensions vector, double value)
    {
        vector.Vector3 = new Vector3(vector.Vector3.X + (float)value,
            vector.Vector3.Y + (float)value,
            vector.Vector3.Z + (float)value);
        return vector;
    }

    public static VectorExtensions operator -(VectorExtensions vector, double value)
    {
        vector.Vector3 = new Vector3(vector.Vector3.X - (float)value,
            vector.Vector3.Y - (float)value,
            vector.Vector3.Z - (float)value);
        return vector;
    }

    #endregion

    #region float

    public static VectorExtensions operator *(VectorExtensions vector, float scale)
    {
        vector.Vector3 = new Vector3(vector.Vector3.X * scale, vector.Vector3.Y * scale, vector.Vector3.Z * scale);

        return vector;
    }

    public static VectorExtensions operator +(VectorExtensions vector, float value)
    {
        vector.Vector3 = new Vector3(vector.Vector3.X + value,
            vector.Vector3.Y + value,
            vector.Vector3.Z + value);
        return vector;
    }

    public static VectorExtensions operator -(VectorExtensions vector, float value)
    {
        vector.Vector3 = new Vector3(vector.Vector3.X - value,
            vector.Vector3.Y - value,
            vector.Vector3.Z - value);
        return vector;
    }

    #endregion
}