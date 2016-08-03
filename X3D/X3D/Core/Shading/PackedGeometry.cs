using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace X3D.Core.Shading
{
    using Verticies = List<Vertex>;
    using Mesh = List<List<Vertex>>; // Mesh will contain faces made up of either or Triangles, and Quads
    using DefaultUniforms;
    using Parser;

    public class PackedGeometry
    {
        public bool Texturing;
        public bool Coloring;
        public bool RGBA;
        public bool RGB;
        public bool generateColorMap;
        public bool colorPerVertex;
        public BoundingBox bbox;
        //public BoundingBox bbox3;
        //public BoundingBox bbox4;
        public int? restartIndex;

        public int[] _indices;
        public int[] _texIndices;
        public int[] _colorIndicies;
        public Vector2[] _texCoords;
        public Vector3[] _coords;
        public float[] color;

        public Verticies interleaved3 = new Verticies();
        public Verticies interleaved4 = new Verticies();

        private const int RESTART_INDEX = -1;

        public void Interleave()
        {
            Buffering.Interleave(out this.bbox,
                 out this.interleaved3,
                 out this.interleaved4,
                 this._indices,
                 this._texIndices,
                 this._coords,
                 this._texCoords,
                 null,
                 this._colorIndicies,
                 this.color,
                 this.restartIndex,
                 false,
                 true,
                 this.Coloring,
                 this.Texturing
                 );
        }

        public static PackedGeometry InterleavePacks(List<PackedGeometry> packs)
        {
            PackedGeometry p;

            p = new PackedGeometry();

            foreach (PackedGeometry g in packs)
            {
                p.interleaved3.AddRange(g.interleaved3);
                p.interleaved4.AddRange(g.interleaved4);
            }

            return p;
        }

        public static PackedGeometry Pack(IndexedTriangleSet its)
        {
            PackedGeometry packed;
            TextureCoordinate texCoordinate;
            Coordinate coordinate;
            Color colorNode;
            ColorRGBA colorRGBANode;

            packed = new PackedGeometry();
            //packed.Texturing = ifs.texCoordinate != null;// || parentShape.texturingEnabled;

            texCoordinate = (TextureCoordinate)its.ChildrenWithAppliedReferences.FirstOrDefault(n => n.GetType() == typeof(TextureCoordinate));
            coordinate = (Coordinate)its.ChildrenWithAppliedReferences.FirstOrDefault(n => n.GetType() == typeof(Coordinate));
            colorNode = (Color)its.ChildrenWithAppliedReferences.FirstOrDefault(n => n.GetType() == typeof(Color));
            colorRGBANode = (ColorRGBA)its.ChildrenWithAppliedReferences.FirstOrDefault(n => n.GetType() == typeof(ColorRGBA));

            packed.RGBA = colorRGBANode != null;
            packed.RGB = colorNode != null;
            packed.Coloring = packed.RGBA || packed.RGB;
            packed.generateColorMap = packed.Coloring;

            if (packed.RGB && !packed.RGBA)
            {
                packed.color = X3DTypeConverters.Floats(colorNode.color);
            }
            else if (packed.RGBA && !packed.RGB)
            {
                packed.color = X3DTypeConverters.Floats(colorRGBANode.color);
            }

            if (texCoordinate != null)
            {
                packed._texCoords = X3DTypeConverters.MFVec2f(texCoordinate.point);
            }

            if (coordinate != null && !string.IsNullOrEmpty(its.index))
            {
                packed._indices = X3DTypeConverters.ParseIndicies(its.index);
                packed._coords = X3DTypeConverters.MFVec3f(coordinate.point);

                packed.restartIndex = null;

                Buffering.Interleave(out packed.bbox,
                                     out packed.interleaved3,
                                     out packed.interleaved4,
                                     packed._indices,
                                     packed._texIndices,
                                     packed._coords,
                                     packed._texCoords,
                                     null,
                                     packed._colorIndicies,
                                     packed.color,
                                     packed.restartIndex,
                                     false,
                                     true,
                                     packed.Coloring,
                                     packed.Texturing
                                     );

                //packed.bbox = MathHelpers.CalcBoundingBox(ifs, packed.restartIndex);
                //packed.bbox3 = BoundingBox.CalculateBoundingBox(packed.interleaved3);
                //packed.bbox4 = BoundingBox.CalculateBoundingBox(packed.interleaved4);
                //packed.bbox = BoundingBox.Max(packed.bbox3, packed.bbox4);

                Buffering.Interleave(packed.bbox,
                    out packed.interleaved3,
                    out packed.interleaved4,
                    packed._indices, packed._texIndices, packed._coords,
                    packed._texCoords, null, packed._colorIndicies, packed.color,
                    packed.restartIndex, true,
                    packed.colorPerVertex, packed.Coloring, packed.Texturing);

            }

            return packed;
        }

        public static PackedGeometry Pack(IndexedFaceSet ifs)
        {
            PackedGeometry packed;
            TextureCoordinate texCoordinate;
            Coordinate coordinate;
            Color colorNode;
            ColorRGBA colorRGBANode;

            packed = new PackedGeometry();
            //packed.Texturing = ifs.texCoordinate != null;// || parentShape.texturingEnabled;

            texCoordinate = (TextureCoordinate)ifs.ChildrenWithAppliedReferences.FirstOrDefault(n => n.GetType() == typeof(TextureCoordinate));
            coordinate = (Coordinate)ifs.ChildrenWithAppliedReferences.FirstOrDefault(n => n.GetType() == typeof(Coordinate));
            colorNode = (Color)ifs.ChildrenWithAppliedReferences.FirstOrDefault(n => n.GetType() == typeof(Color));
            colorRGBANode = (ColorRGBA)ifs.ChildrenWithAppliedReferences.FirstOrDefault(n => n.GetType() == typeof(ColorRGBA));

            packed.RGBA = colorRGBANode != null;
            packed.RGB = colorNode != null;
            packed.Coloring = packed.RGBA || packed.RGB;
            packed.generateColorMap = packed.Coloring;

            if (packed.RGB && !packed.RGBA)
            {
                packed.color = X3DTypeConverters.Floats(colorNode.color);
            }
            else if (packed.RGBA && !packed.RGB)
            {
                packed.color = X3DTypeConverters.Floats(colorRGBANode.color);
            }

            if (texCoordinate != null && !string.IsNullOrEmpty(ifs.texCoordIndex))
            {
                packed._texIndices = X3DTypeConverters.ParseIndicies(ifs.texCoordIndex);
                packed._texCoords = X3DTypeConverters.MFVec2f(texCoordinate.point);
                packed.Texturing = true;
            }

            if (coordinate != null && !string.IsNullOrEmpty(ifs.coordIndex))
            {
                packed._indices = X3DTypeConverters.ParseIndicies(ifs.coordIndex);
                packed._coords = X3DTypeConverters.MFVec3f(coordinate.point);

                if (!string.IsNullOrEmpty(ifs.colorIndex))
                {
                    packed._colorIndicies = X3DTypeConverters.ParseIndicies(ifs.colorIndex);
                }

                if (ifs.coordIndex.Contains(RESTART_INDEX.ToString()))
                {
                    packed.restartIndex = RESTART_INDEX;
                }

                packed.Interleave();

                //Buffering.Interleave(out packed.bbox,
                //                     out packed.interleaved3,
                //                     out packed.interleaved4,
                //                     packed._indices,
                //                     packed._texIndices,
                //                     packed._coords,
                //                     packed._texCoords,
                //                     null,
                //                     packed._colorIndicies,
                //                     packed.color,
                //                     packed.restartIndex,
                //                     false,
                //                     true,
                //                     packed.Coloring,
                //                     packed.Texturing
                //                     );

                //packed.bbox = MathHelpers.CalcBoundingBox(ifs, packed.restartIndex);
                //packed.bbox3 = BoundingBox.CalculateBoundingBox(packed.interleaved3);
                //packed.bbox4 = BoundingBox.CalculateBoundingBox(packed.interleaved4);
                //packed.bbox = BoundingBox.Max(packed.bbox3, packed.bbox4);

                //Buffering.Interleave(packed.bbox,
                //    out packed.interleaved3,
                //    out packed.interleaved4,
                //    packed._indices, packed._texIndices, packed._coords,
                //    packed._texCoords, null, packed._colorIndicies, packed.color,
                //    packed.restartIndex, true,
                //    packed.colorPerVertex, packed.Coloring, packed.Texturing);

            }

            return packed;
        }

    }
}
