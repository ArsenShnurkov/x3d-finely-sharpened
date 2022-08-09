// TODO: calculate texcoords using spherical equation in shader

using OpenTK;
using X3D.Core.Shading;

namespace X3D
{
    /// <summary>
    ///     A Sphere with underlying geometry implemented as an Iso-Sphere.
    ///     Uses an inbuilt tessellator to subdivide icosahedron into a sphere approximation.
    /// </summary>
    public partial class Sphere
    {
        internal PackedGeometry _pack;

        private Shape parentShape;

        public override void CollectGeometry(
            RenderingContext rc,
            out GeometryHandle handle,
            out BoundingBox bbox,
            out bool coloring,
            out bool texturing)
        {
            handle = GeometryHandle.Zero;
            bbox = BoundingBox.Zero;
            coloring = false;
            texturing = false;

            parentShape = GetParent<Shape>();

            _pack = new PackedGeometry();
            _pack.restartIndex = -1;
            _pack._indices = Faces;
            _pack._coords = Verts;
            _pack.bbox = BoundingBox.CalculateBoundingBox(Verts);

            _pack.Interleave();

            // BUFFER GEOMETRY
            handle = Buffering.BufferShaderGeometry(_pack);
        }

        #region Rendering Methods

        public override void Render(RenderingContext rc)
        {
            //var scale = new Vector3(0.04f, 0.04f, 0.04f);

            //rc.cam.Scale = scale;
        }

        #endregion

        #region Icosahedron Geometry

        private int[] Faces =
        {
            2, 1, 0, -1,
            3, 2, 0, -1,
            4, 3, 0, -1,
            5, 4, 0, -1,
            1, 5, 0, -1,
            11, 6, 7, -1,
            11, 7, 8, -1,
            11, 8, 9, -1,
            11, 9, 10, -1,
            11, 10, 6, -1,
            1, 2, 6, -1,
            2, 3, 7, -1,
            3, 4, 8, -1,
            4, 5, 9, -1,
            5, 1, 10, -1,
            2, 7, 6, -1,
            3, 8, 7, -1,
            4, 9, 8, -1,
            5, 10, 9, -1,
            1, 6, 10, -1
        };

        //TODO: scale values correctly
        private Vector3[] Verts =
        {
            new Vector3(0.000f, 0.000f, 1.000f),
            new Vector3(0.894f, 0.000f, 0.447f),
            new Vector3(0.276f, 0.851f, 0.447f),
            new Vector3(-0.724f, 0.526f, 0.447f),
            new Vector3(-0.724f, -0.526f, 0.447f),
            new Vector3(0.276f, -0.851f, 0.447f),
            new Vector3(0.724f, 0.526f, -0.447f),
            new Vector3(-0.276f, 0.851f, -0.447f),
            new Vector3(-0.894f, 0.000f, -0.447f),
            new Vector3(-0.276f, -0.851f, -0.447f),
            new Vector3(0.724f, -0.526f, -0.447f),
            new Vector3(0.000f, 0.000f, -1.000f)
        };

        #endregion
    }
}