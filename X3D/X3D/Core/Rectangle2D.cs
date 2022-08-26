using OpenTK;
using X3D.Core.Shading;
using win = System.Drawing;

namespace X3D
{
    public partial class Rectangle2D
    {
        internal PackedGeometry _pack;
        private Shape parentShape;

        public override void CollectGeometry(
            RenderingContext rc,
            out GeometryHandle handle,
            out BoundingBox bbox,
            out bool Coloring,
            out bool Texturing)
        {
            handle = GeometryHandle.Zero;
            bbox = BoundingBox.Zero;
            Texturing = true;
            Coloring = true;

            parentShape = GetParent<Shape>();


            _pack = new PackedGeometry();
            /* 
        glColor3d(1,0,0);
        glVertex3f(-1,-1,-10);
        glColor3d(1,1,0);
        glVertex3f(1,-1,-10);
        glColor3d(1,1,1);
        glVertex3f(1,1,-10);
        glColor3d(0,1,1);
        glVertex3f(-1,1,-10);
             */
            _pack._indices = new[] { 0, 1, 2, 3, -1 };
            _pack._coords = new[]
            {
                new Vector3(-1, -1, 0),
                new Vector3(1, -1, 0),
                new Vector3(1, 1, 0),
                new Vector3(-1, 1, 0)
            };
            _pack._texCoords = new[]
            {
                new Vector2(0, 0),
                new Vector2(0, 1),
                new Vector2(1, 0),
                new Vector2(1, 1)
            };
            //_pack._colorIndicies = ;

            _pack.restartIndex = -1;

            _pack.Interleave();

            // BUFFER GEOMETRY
            handle = _pack.CreateHandle();
        }
    }
}