using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using X3D.Parser;

namespace X3D
{

    public class BoundingBox
    {
        public float X, Y;

        public float Width, Height, Depth;

        public BoundingBox()
        {
            this.X = 0f;
            this.Y = 0;
            this.Width = 0f;
            this.Height = 0f;
            this.Depth = 0f;
        }

        public BoundingBox(float x, float y, float w, float h, float d)
        {
            this.X = x;
            this.Y = y;
            this.Width = w;
            this.Height = h;
            this.Depth = d;
        }
    }

    public class MathHelpers
    {
        public const float PI_OVER_180 = (float)Math.PI / 180.0f;
        public const float TWO_PI = 2.0f * (float)Math.PI;

        public static BoundingBox CalcBoundingBox(ElevationGrid elevationGrid)
        {
            BoundingBox bbox;
            int xDim, zDim;

            xDim = elevationGrid._xDimension - 1;
            zDim = elevationGrid._zDimension - 1;

            bbox = new BoundingBox();

            bbox.Width = xDim * elevationGrid._xSpacing;
            bbox.Depth = zDim * elevationGrid._zSpacing;
            bbox.Height = elevationGrid.MaxHeight - elevationGrid.MinHeight;

            return bbox;
        }

        /// <summary>
        /// Calculate all the Texture Coordinates given input verticies and a bounding box
        /// </summary>
        public static Vertex[] uv(BoundingBox boundingBox, Vertex[] verticies, bool at_origin = false)
        {
            float dx = boundingBox.Width / 2;
            float dz = boundingBox.Depth / 2;
            Vertex v;
            Vector2 t;

            for (int i = 0; i < verticies.Length; i++)
            {
                v = verticies[i];

                if (at_origin)
                {
                    t = new Vector2((verticies[i].Position.X - dx) / (boundingBox.Width), 
                        (verticies[i].Position.Z - dz) / (boundingBox.Depth));
                }
                else
                {
                    t = new Vector2(verticies[i].Position.X / boundingBox.Width, 
                        verticies[i].Position.Z / boundingBox.Depth);
                }

                v.TexCoord = t;
                verticies[i] = v;
            }

            return verticies;
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
