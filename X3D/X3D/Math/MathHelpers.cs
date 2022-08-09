using System;
using OpenTK;
using X3D.Core;

namespace X3D
{
    public class MathHelpers
    {
        public const float PI = (float)Math.PI; // 180 degrees
        public const float PIOver2 = PI / 2.0f; // 90 degrees
        public const float PIOver4 = PI / 4.0f; // 45 degrees
        public const float ThreePIOver2 = 3.0f * PI / 2.0f; // 270 degrees
        public const float PI_OVER_180 = (float)Math.PI / 180.0f; // radian ratio (used to convert radians to degrees)
        public const float Zenith = (float)Math.PI / 2.0f;
        public const float PI2 = 2.0f * (float)Math.PI;
        public const float PiOver180 = PI_OVER_180;
        public const float TwoPi = PI2;
        public const float EPS = 0.000001f;

        public static double FractionalPart(double n)
        {
            double fraction;
            int integer;

            //fraction = n - Math.Floor(n);
            integer = (int)Math.Truncate(n);
            fraction = n - integer;

            return fraction;
        }

        public static double RoundFractional(double n, double interval)
        {
            return Math.Round(n * interval, 0) / interval;
        }

        /// <summary>
        ///     Build a rotation matrix from the specified quaternion.
        /// </summary>
        /// <param name="q">Quaternion to translate.</param>
        /// <returns>A matrix instance.</returns>
        public static Matrix4 CreateRotation(ref Quaternion q)
        {
            var result = Matrix4.Identity;

            var X = q.X;
            var Y = q.Y;
            var Z = q.Z;
            var W = q.W;

            var xx = X * X;
            var xy = X * Y;
            var xz = X * Z;
            var xw = X * W;
            var yy = Y * Y;
            var yz = Y * Z;
            var yw = Y * W;
            var zz = Z * Z;
            var zw = Z * W;

            result.M11 = 1 - 2 * (yy + zz);
            result.M21 = 2 * (xy - zw);
            result.M31 = 2 * (xz + yw);
            result.M12 = 2 * (xy + zw);
            result.M22 = 1 - 2 * (xx + zz);
            result.M32 = 2 * (yz - xw);
            result.M13 = 2 * (xz - yw);
            result.M23 = 2 * (yz + xw);
            result.M33 = 1 - 2 * (xx + yy);
            return result;
        }

        public static float Clamp(float value, float min, float max)
        {
            return value > max ? max : value < min ? min : value;
        }

        public static float ClampCircular(float n, float min, float max)
        {
            if (n >= max) n -= max;
            if (n < min) n += max;
            return n;
        }

        public static float ClampCircularShift(float n, float min, float max)
        {
            while (n >= max) n -= max;

            while (n < min) n += max;

            return n;
        }

        public static float ClampDegrees(float n)
        {
            float min = 0;
            var max = 360.0f;

            while (n >= max) n -= max;

            while (n < min) n += max;

            return n;
        }

        public static float ClampRadians(float n)
        {
            float min = 0;
            var max = PI2;

            while (n >= max) n -= max;

            while (n < min) n += max;

            return n;
        }

        /// <summary>
        ///     Return next highest power of two.
        ///     Useful for computing valid OpenGL texture sizes.
        /// </summary>
        public static int ComputeNextHighestPowerOfTwo(int n)
        {
            int result;
            int i;

            --n;

            for (i = 1; i < 32; i <<= 1) n = n | (n >> i);

            result = n + 1;

            return result;
        }

        /// <summary>
        ///     Calculate all the Texture Coordinates given input verticies and a bounding box
        /// </summary>
        public static Vertex[] uv(BoundingBox boundingBox, Vertex[] verticies, bool at_origin = false)
        {
            var dx = boundingBox.Width / 2;
            var dz = boundingBox.Depth / 2;
            Vertex v;
            Vector2 t;

            for (var i = 0; i < verticies.Length; i++)
            {
                v = verticies[i];

                if (at_origin)
                    t = new Vector2((verticies[i].Position.X - dx) / boundingBox.Width,
                        (verticies[i].Position.Z - dz) / boundingBox.Depth);
                else
                    t = new Vector2(verticies[i].Position.X / boundingBox.Width,
                        verticies[i].Position.Z / boundingBox.Depth);

                v.TexCoord = t;
                verticies[i] = v;
            }

            return verticies;
        }

        /// <summary>
        ///     Calculates the Maximum component within the vector.
        /// </summary>
        public static float Max(Vector3 vector)
        {
            float result;

            result = Math.Max(vector.X, Math.Max(vector.Y, vector.Z));

            return result;
        }

        /// <summary>
        ///     Calculates the Minimum component within the vector.
        /// </summary>
        public static float Min(Vector3 vector)
        {
            float result;

            result = Math.Min(vector.X, Math.Min(vector.Y, vector.Z));

            return result;
        }

        /// <summary>
        ///     Calculates the Maximum Vector
        /// </summary>
        public static Vector3 Max(Vector3 vector1, Vector3 vector2)
        {
            Vector3 result;

            result = new Vector3
            {
                X = Math.Max(vector1.X, vector2.X),
                Y = Math.Max(vector1.Y, vector2.Y),
                Z = Math.Max(vector1.Z, vector2.Z)
            };

            return result;
        }

        /// <summary>
        ///     Calculates the Minimum Vector
        /// </summary>
        public static Vector3 Min(Vector3 vector1, Vector3 vector2)
        {
            Vector3 result;

            result = new Vector3
            {
                X = Math.Min(vector1.X, vector2.X),
                Y = Math.Min(vector1.Y, vector2.Y),
                Z = Math.Min(vector1.Z, vector2.Z)
            };

            return result;
        }

        /// <summary>
        ///     Calculates the absolute value of each component in the specified vector
        /// </summary>
        public static Vector3 Abs(Vector3 vector)
        {
            Vector3 result;

            result = new Vector3
            {
                X = Math.Abs(vector.X),
                Y = Math.Abs(vector.Y),
                Z = Math.Abs(vector.Z)
            };

            return result;
        }

        public static Vector2 uv(float u, float v)
        {
            return new Vector2(u, v);
        }

        public static Vector2 uv(Vector3 xyz, float radius, float sectorIndex)
        {
            return Vector2.Zero;
        }

        public static Vector2 uv(Vector3 xyz, float radius)
        {
            float u, v;


            var radian = radius * (Math.PI / 180.0f);

            var xcos = (float)Math.Cos(radian);
            var ysin = (float)Math.Sin(radian);

            //x = xcos * c.r + c.pos.x;
            //y = ysin * c.r + c.pos.y;

            u = xcos * 0.5f + 0.5f;
            v = ysin * 0.5f + 0.5f;


            return new Vector2(u, v);
        }

        /// <summary>
        ///     Computes the facet Texture Coordinate for a Sphere given the Sphere's facet normal.
        /// </summary>
        [Obsolete("Perform this calculation in a Vertex or Geometry shader for increased performance.")]
        public static Vector2 uv(Vector3 facetNormal)
        {
            return new Vector2((float)(Math.Asin(facetNormal.X) / Math.PI + 0.5),
                (float)(Math.Asin(facetNormal.Y) / Math.PI + 0.5));
        }

        public static Vector3 ToHSV(Vector3 color)
        {
            Vector3 result; // the resultant color converted to HSV space
            float hue; // degrees [0, 360]
            float saturation; // [0, 1]
            float value; // [0, 1]
            float min,
                max,
                delta;
            Vector3 c;

            c = color * 255;

            var _color = System.Drawing.Color.FromArgb(255, (int)c.X, (int)c.Y, (int)c.Z);
            hue = _color.GetHue();
            saturation = _color.GetSaturation();
            value = _color.GetBrightness() * 255;
            return new Vector3(hue, saturation, value);


            //if (c.X == 255 && c.Y == 255 && c.Z == 255)
            //{
            //    hue = 0;
            //    saturation = 0;
            //    value = 100;

            //    return new Vector3(hue, saturation, value);
            //}

            min = Min(c);
            max = Max(c);

            value = max;
            delta = max - min;

            if (max != 0)
            {
                if (delta == 0)
                {
                    if (min == 255)
                        saturation = 0;
                    else
                        saturation = min / 255;
                }
                else
                {
                    saturation = delta / max;
                }
            }
            else
            {
                saturation = 0;
                hue = 0;
                value = 0;

                return new Vector3(hue, saturation, value);
            }

            if (c.X == max) // red is dominant
                hue = (c.Y - c.Z) / delta; // between yellow & magenta
            else if (c.Y == max) // green is dominant
                hue = 2 + (c.Z - c.X) / delta; // between cyan & yellow
            else // blue is dominant
                hue = 4 + (c.X - c.Y) / delta; // between magenta & cyan

            hue *= 60;

            if (hue < 0)
                hue += 360;

            if (float.IsNaN(hue)) hue = 0;

            result = new Vector3(hue, saturation, value);

            return result;
        }

        public static Vector3 FromHSV(Vector3 hsv)
        {
            Vector3 result; // the resultant HSV value converted to RGB space
            float hue;
            float saturation;
            float value;
            int i;
            float f;
            float v;
            float p;
            float q;
            float t;

            hue = hsv.X;
            saturation = hsv.Y;
            value = hsv.Z;

            i = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            f = hue / 60f - (float)Math.Floor(hue / 60f);

            v = value;
            p = value * (1 - saturation);
            q = value * (1 - f * saturation);
            t = value * (1 - (1 - f) * saturation);

            switch (i)
            {
                default:
                    result = new Vector3(v, p, q);
                    break;
                case 0:
                    result = new Vector3(v, t, p);
                    break;
                case 1:
                    result = new Vector3(q, v, p);
                    break;
                case 2:
                    result = new Vector3(p, v, t);
                    break;
                case 3:
                    result = new Vector3(p, q, v);
                    break;
                case 4:
                    result = new Vector3(t, p, v);
                    break;
            }

            result /= 255.0f;

            return result;
        }

        /// <summary>
        ///     Computes spherical linear interpolaton between two points 'from' 'to'
        /// </summary>
        public static Vector3 Slerp(Vector3 from, Vector3 to, float ratio)
        {
            Vector3 average;
            float slerpRange;
            float slerpRangePhi;


            slerpRange = Vector3.Dot(from.Normalized(), to.Normalized());

            slerpRangePhi = (float)Math.Acos(slerpRange * PI / 180.0);

            average = from * (float)Math.Sin((1 - ratio) * slerpRangePhi)
                      + to * (float)Math.Sin(ratio * slerpRangePhi) / (float)Math.Sin(slerpRangePhi);

            return average;
        }

        /// <summary>
        ///     Computes linear interpolation between two points 'from' 'to'
        ///     and returns the resultant interpolated value that lies within the specified range.
        /// </summary>
        public static Vector3 Lerp(Vector3 from, Vector3 to, float ratio)
        {
            //return (from * (1.0f - ratio)) + (to * ratio);
            return from + (to - from) * ratio;
        }

        public static Vector3 Perp(Vector3 v)
        {
            var min = Math.Abs(v.X);
            var cardinalAxis = Vector3.UnitX;

            if (Math.Abs(v.Y) < min)
            {
                min = Math.Abs(v.Y);
                cardinalAxis = Vector3.UnitY;
            }

            if (Math.Abs(v.Z) < min) cardinalAxis = Vector3.UnitZ;

            return Vector3.Cross(v, cardinalAxis);
        }
    }
}