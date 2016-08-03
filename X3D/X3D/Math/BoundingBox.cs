using OpenTK;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using X3D.Core;
using X3D.Core.Shading;

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

        /// <summary>
        /// Calculates the Maximum bounding box size 
        /// </summary>
        public static BoundingBox Max(BoundingBox box1, BoundingBox box2)
        {
            BoundingBox result;

            result = new BoundingBox()
            {
                Width = Math.Max(box1.Width, box2.Width),
                Height = Math.Max(box1.Height, box2.Height),
                Depth = Math.Max(box1.Depth, box2.Depth)
            };

            return result;
        }

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
        /// Calculates a bounding box of a String given the specified Font type.
        /// </summary>
        public static BoundingBox CalculateBoundingBox(String text, Font font)
        {
            Bitmap bmp = new Bitmap(1, 1);
            SizeF size;

            using (Graphics g2D = Graphics.FromImage(bmp))
            {
                size = g2D.MeasureString(text, font);
            }

            return new BoundingBox()
            {
                Width = (int)(size.Width),
                Height = (int)(size.Height),
                Depth = 0
            };
        }

        public static BoundingBox CalculateBoundingBox(PackedGeometry pack)
        {
            BoundingBox result;
            List<Vertex> interleaved;

            interleaved = new List<Vertex>();
            interleaved.AddRange(pack.interleaved3);
            interleaved.AddRange(pack.interleaved4);
            
            result = CalculateBoundingBox(interleaved);

            return result;
        }

        public static BoundingBox CalculateBoundingBox(IEnumerable<Vector3> vectors)
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

        public static void CalculateBoundingBox(IEnumerable<Vector3> vectors, out Vector3 max, out Vector3 min)
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

        public static BoundingBox CalculateBoundingBox(IEnumerable<Vertex> vectors)
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

        public static void CalculateBoundingBox(IEnumerable<Vertex> vectors, out Vector3 max, out Vector3 min)
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

        public static BoundingBox Zero
        {
            get
            {
                return new BoundingBox(0, 0, 0, 0, 0);
            }
        }

    }
}
