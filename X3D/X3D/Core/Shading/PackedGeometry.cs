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

    /// <summary>
    /// A Mesh of primatives, hence "Packed Geometry", 
    /// of which primativies are usually interleavable using coresponding coordinate indicies.
    /// </summary>
    public class PackedGeometry
    {
        public bool Texturing;
        public bool Coloring;
        public bool RGBA;
        public bool RGB;
        public bool generateColorMap;
        public bool colorPerVertex;
        public BoundingBox bbox;
        public int? restartIndex;

        public int[] _indices;
        public int[] _texIndices;
        public int[] _colorIndicies;
        public Vector2[] _texCoords;
        public Vector3[] _coords;
        public Vector2[] _coords2f;
        public float[] color;
        public float[] normals;

        public Verticies interleaved3 = new Verticies();
        public Verticies interleaved4 = new Verticies();

        private const int RESTART_INDEX = -1;

        public void Interleave(bool calcBounds = true)
        {
            if (this._indices == null) return;

            PackedGeometry pack = this;

            Buffering.Interleave(ref pack, genTexCoordPerVertex: false, colorPerVertex: true, calcBounds: calcBounds);
        }

        public GeometryHandle CreateHandle()
        {
            GeometryHandle handle;

            handle = Buffering.BufferShaderGeometry(this);

            return handle;
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

                packed.Interleave();
            }

            return packed;
        }

        public static PackedGeometry Pack(IndexedLineSet ils)
        {
            PackedGeometry packed;
            Coordinate coordinate;

            packed = new PackedGeometry();
            //packed.Texturing = ifs.texCoordinate != null;// || parentShape.texturingEnabled;

            coordinate = (Coordinate)ils.ChildrenWithAppliedReferences.FirstOrDefault(n => n.GetType() == typeof(Coordinate));

            packed.RGBA = false;
            packed.RGB = false;
            packed.Coloring = false;
            packed.generateColorMap = false;

            if (coordinate != null && !string.IsNullOrEmpty(ils.coordIndex))
            {
                packed._indices = X3DTypeConverters.ParseIndicies(ils.coordIndex);
                packed._coords = X3DTypeConverters.MFVec3f(coordinate.point);

                if (ils.coordIndex.Contains(RESTART_INDEX.ToString()))
                {
                    packed.restartIndex = RESTART_INDEX;
                }

                packed.Interleave();
            }

            return packed;
        }

        public static PackedGeometry Pack(LineSet lineSet)
        {
            PackedGeometry packed;
            Coordinate coordinate;

            packed = new PackedGeometry();
            //packed.Texturing = ifs.texCoordinate != null;// || parentShape.texturingEnabled;

            coordinate = (Coordinate)lineSet.ChildrenWithAppliedReferences.FirstOrDefault(n => n.GetType() == typeof(Coordinate));

            packed.RGBA = false;
            packed.RGB = false;
            packed.Coloring = false;
            packed.generateColorMap = false;

            if (coordinate != null)
            {
                if(lineSet.vertexCount == 2)
                    packed._coords2f = X3DTypeConverters.MFVec2f(coordinate.point);
                else packed._coords = X3DTypeConverters.MFVec3f(coordinate.point);

                packed.restartIndex = null;

                packed.Interleave();
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
            }

            return packed;
        }
    }
}
