using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using X3D.Core;

namespace X3D
{
    public class MathHelpers
    {
        public const float PI = (float)Math.PI;
        public const float PI_OVER_180 = (float)Math.PI / 180.0f;
        public const float Zenith = (float)Math.PI / 2.0f;
        public const float PI2 = 2.0f * (float)Math.PI;
        public const float PiOver180 = PI_OVER_180;
        public const float TwoPi = PI2;

        public static float Clamp(float value, float min, float max)
        {
            return value > max ? max : (value < min ? min : value);
        }
        public static float ClampCircular(float n, float min, float max)
        {
            if (n >= max) n -= max;
            if (n < min) n += max;
            return n;
        }

        public static BoundingBox CalcBoundingBox(IndexedTriangleSet its, int? restartIndex)
        {
            BoundingBox box;
            int i;
            int facesetPointer;
            float maxwidth;
            float minwidth;
            float maxdepth;
            float mindepth;
            float maxheight;
            float minheight;
            Vector3 v;

            int FACE_RESTART_INDEX = 2; // 0 - 2

            box = new BoundingBox();

            maxwidth = float.MinValue;
            maxdepth = float.MinValue;
            maxheight = float.MinValue;
            minwidth = float.MaxValue;
            mindepth = float.MaxValue;
            minheight = float.MaxValue;

            if (its.coordinate != null && its.coordinate.point != null)
            {
                for (i = 0; i < its._indicies.Length; i++)
                {
                    facesetPointer = its._indicies[i];

                    if (restartIndex.HasValue)
                    {
                        if (facesetPointer != restartIndex.Value)
                        {
                            if (!((facesetPointer * 3 < its.coordinate.point.Length) &&
                                (facesetPointer * 3 + 1 < its.coordinate.point.Length) &&
                                (facesetPointer * 3 + 2 < its.coordinate.point.Length)) || facesetPointer < 0)
                            {
                                /* If an index specified in coordIndex is invalid or not related to coordinate.point, we need to skip it */
                                /* At this point we could display a warning message for each one of these errors 
                                    * indicating the particular cordIndex and the maximum range exceeded */
                                continue;
                            }

                            v = its._coords[facesetPointer];

                            maxwidth = Math.Max(v.X, maxwidth);
                            minwidth = Math.Min(v.X, minwidth);

                            maxdepth = Math.Max(v.Z, maxdepth);
                            mindepth = Math.Min(v.Z, mindepth);

                            minheight = Math.Max(v.Y, minheight);
                            minheight = Math.Min(v.Y, minheight);
                        }
                    }
                    else
                    {
                        // NO RESTART INDEX, assume new face is at every 3rd value / i = 2

                        if (facesetPointer > 0 && facesetPointer % FACE_RESTART_INDEX == 0)
                        {
                            if (!((facesetPointer * 3 < its.coordinate.point.Length) &&
                                (facesetPointer * 3 + 1 < its.coordinate.point.Length) &&
                                (facesetPointer * 3 + 2 < its.coordinate.point.Length)) || facesetPointer < 0)
                            {
                                /* If an index specified in coordIndex is invalid or not related to coordinate.point, we need to skip it */
                                /* At this point we could display a warning message for each one of these errors 
                                    * indicating the particular cordIndex and the maximum range exceeded */
                                continue;
                            }

                            v = its._coords[facesetPointer];

                            maxwidth = Math.Max(v.X, maxwidth);
                            minwidth = Math.Min(v.X, minwidth);

                            maxdepth = Math.Max(v.Z, maxdepth);
                            mindepth = Math.Min(v.Z, mindepth);

                            maxheight = Math.Max(v.Y, maxheight);
                            minheight = Math.Min(v.Y, minheight);
                        }
                    }
                }
            }

            box.Width = Math.Abs(maxwidth) + Math.Abs(minwidth);
            box.Height = Math.Abs(maxheight) + Math.Abs(minheight);
            box.Depth = Math.Abs(maxdepth) + Math.Abs(mindepth);

            if (box.Height == 0) box.Height = 1.0f;

            return box;
        }

        public static BoundingBox CalcBoundingBox(IndexedFaceSet ifs, int? restartIndex)
        {
            BoundingBox box;
            int i;
            int facesetPointer;
            float maxwidth;
            float minwidth;
            float maxdepth;
            float mindepth;
            float maxheight;
            float minheight;
            Vector3 v;

            int FACE_RESTART_INDEX = 2;

            box = new BoundingBox();

            maxwidth = float.MinValue;
            maxdepth = float.MinValue;
            maxheight = float.MinValue;
            minwidth = float.MaxValue;
            mindepth = float.MaxValue;
            minheight = float.MaxValue;

            if (ifs.coordinate != null && ifs.coordinate.point != null)
            {
                for (i = 0; i < ifs._indices.Length; i++)
                {
                    facesetPointer = ifs._indices[i];

                    if (restartIndex.HasValue)
                    {
                        if (facesetPointer != restartIndex.Value)
                        {
                            if (!((facesetPointer * 3 < ifs.coordinate.point.Length) &&
                                (facesetPointer * 3 + 1 < ifs.coordinate.point.Length) &&
                                (facesetPointer * 3 + 2 < ifs.coordinate.point.Length)) || facesetPointer < 0)
                            {
                                /* If an index specified in coordIndex is invalid or not related to coordinate.point, we need to skip it */
                                /* At this point we could display a warning message for each one of these errors 
                                    * indicating the particular cordIndex and the maximum range exceeded */
                                continue;
                            }
                            
                            v = ifs._coords[facesetPointer];

                            maxwidth = Math.Max(v.X, maxwidth);
                            minwidth = Math.Min(v.X, minwidth);

                            maxdepth = Math.Max(v.Z, maxdepth);
                            mindepth = Math.Min(v.Z, mindepth);

                            minheight = Math.Max(v.Y, minheight);
                            minheight = Math.Min(v.Y, minheight);
                        }
                    }
                    else
                    {
                        // NO RESTART INDEX, assume new face is at every 3rd value / i = 2

                        if (ifs._indices.Length == 4)
                        {
                            FACE_RESTART_INDEX = 3; // 0-3 Quad
                        }
                        else if (ifs._indices.Length == 3)
                        {
                            FACE_RESTART_INDEX = 2; // 0-3 Triangle
                        }
                        else
                        {
                            FACE_RESTART_INDEX = 2;
                        }

                        if (facesetPointer > 0 && facesetPointer % FACE_RESTART_INDEX == 0)
                        {
                            if (!((facesetPointer * 3 < ifs.coordinate.point.Length) &&
                                (facesetPointer * 3 + 1 < ifs.coordinate.point.Length) &&
                                (facesetPointer * 3 + 2 < ifs.coordinate.point.Length)) || facesetPointer < 0)
                            {
                                /* If an index specified in coordIndex is invalid or not related to coordinate.point, we need to skip it */
                                /* At this point we could display a warning message for each one of these errors 
                                    * indicating the particular cordIndex and the maximum range exceeded */
                                continue;
                            }

                            v = ifs._coords[facesetPointer];

                            maxwidth = Math.Max(v.X, maxwidth);
                            minwidth = Math.Min(v.X, minwidth);

                            maxdepth = Math.Max(v.Z, maxdepth);
                            mindepth = Math.Min(v.Z, mindepth);

                            maxheight = Math.Max(v.Y, maxheight);
                            minheight = Math.Min(v.Y, minheight);
                        }
                    }
                }
            }

            box.Width = Math.Abs(maxwidth) + Math.Abs(minwidth);
            box.Height = Math.Abs(maxheight) + Math.Abs(minheight);
            box.Depth = Math.Abs(maxdepth) + Math.Abs(mindepth);

            if (box.Height == 0) box.Height = 1.0f;

            return box;
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

        /// <summary>
        /// Computes the facet Texture Coordinate for a Sphere given the Sphere's facet normal.
        /// </summary>
        [Obsolete("Perform this calculation in a Vertex or Geometry shader for increased performance.")]
        public static Vector2 uv(Vector3 facetNormal)
        {
            return new Vector2((float)(Math.Asin(facetNormal.X) / Math.PI + 0.5),
                (float)(Math.Asin(facetNormal.Y) / Math.PI + 0.5));
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
