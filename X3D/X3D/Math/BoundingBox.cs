using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using X3D.Core;

namespace X3D
{
    public class BoundingBox
    {
        public float Width, Height, Depth;

        public BoundingBox()
        {
            this.Width = 0f;
            this.Height = 0f;
            this.Depth = 0f;
        }

        public BoundingBox(float x, float y, float w, float h, float d)
        {
            this.Width = w;
            this.Height = h;
            this.Depth = d;
        }

        public static BoundingBox CalculateBoundingBox(List<Vector3> vectors)
        {
            Vector3 max;
            Vector3 min;

            max = Vector3.Zero;
            min = Vector3.Zero;

            foreach (Vector3 vector in vectors)
            {
                if (vector.X > max.X) max.X = vector.X;
                if (vector.Y > max.Y) max.Y = vector.Y;
                if (vector.Z > max.Z) max.Z = vector.Z;

                if (vector.X < min.X) min.X = vector.X;
                if (vector.Y < min.Y) min.Y = vector.Y;
                if (vector.Z < min.Z) min.Z = vector.Z;
            }

            return new BoundingBox()
            {
                Width = max.X,
                Height = max.Y,
                Depth = max.Z
            };
        }

        public static void CalculateBoundingBox(List<Vector3> vectors, out Vector3 max, out Vector3 min)
        {
            max = Vector3.Zero;
            min = Vector3.Zero;

            foreach (Vector3 vector in vectors)
            {
                if (vector.X > max.X) max.X = vector.X;
                if (vector.Y > max.Y) max.Y = vector.Y;
                if (vector.Z > max.Z) max.Z = vector.Z;

                if (vector.X < min.X) min.X = vector.X;
                if (vector.Y < min.Y) min.Y = vector.Y;
                if (vector.Z < min.Z) min.Z = vector.Z;
            }
        }

        public static BoundingBox CalculateBoundingBox(List<Vertex> vectors)
        {
            Vector3 vector;
            Vector3 max;
            Vector3 min;

            max = Vector3.Zero;
            min = Vector3.Zero;

            foreach (Vertex verticy in vectors)
            {
                vector = verticy.Position;

                if (vector.X > max.X) max.X = vector.X;
                if (vector.Y > max.Y) max.Y = vector.Y;
                if (vector.Z > max.Z) max.Z = vector.Z;

                if (vector.X < min.X) min.X = vector.X;
                if (vector.Y < min.Y) min.Y = vector.Y;
                if (vector.Z < min.Z) min.Z = vector.Z;
            }

            return new BoundingBox()
            {
                Width = max.X,
                Height = max.Y,
                Depth = max.Z
            };
        }

        public static void CalculateBoundingBox(List<Vertex> vectors, out Vector3 max, out Vector3 min)
        {
            Vector3 vector;

            max = Vector3.Zero;
            min = Vector3.Zero;
           
            foreach (Vertex verticy in vectors)
            {
                vector = verticy.Position;

                if (vector.X > max.X) max.X = vector.X;
                if (vector.Y > max.Y) max.Y = vector.Y;
                if (vector.Z > max.Z) max.Z = vector.Z;

                if (vector.X < min.X) min.X = vector.X;
                if (vector.Y < min.Y) min.Y = vector.Y;
                if (vector.Z < min.Z) min.Z = vector.Z;
            }
        }
    }
}
