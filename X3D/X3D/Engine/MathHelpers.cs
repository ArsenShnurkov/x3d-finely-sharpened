using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace X3D
{

    public class MathHelpers
    {
        public const float PI_OVER_180 = (float)Math.PI / 180.0f;
        public const float TWO_PI = 2.0f * (float)Math.PI;

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

        public static Vector3 Perp(Vector3 v)
        {
            float min = Math.Abs(v.X);
            Vector3 cardinalAxis = Vector3.UnitX;

            if (Math.Abs(v.Y) < min)
            {
                min = Math.Abs(v.Y);
                cardinalAxis = Vector3.UnitY;
            }

            if (Math.Abs(v.Z) < min)
            {
                cardinalAxis = Vector3.UnitZ;
            }

            return Vector3.Cross(v, cardinalAxis);
        }
    }
}
