using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using X3D.Core;
using X3D.Parser;
using X3D.Core.Shading;
using OpenTK.Graphics.OpenGL4;

namespace X3D
{
    /// <summary>
    /// http://www.web3d.org/documents/specifications/19775-1/V3.3/Part01/components/rendering.html#PointSet
    /// </summary>
    public partial class PointSet
    {
        #region Fields

        internal PackedGeometry _pack;

        public PrimitiveType PrimativeType = PrimitiveType.LineLoop;

        #endregion

        #region Public Static Methods

        public static PointSet CreateFromVertexSet(List<Vertex> verticies)
        {
            PointSet ps;
            Coordinate coord;
            Color colors;
            Vector3[] points;

            ps = new PointSet();
            coord = new Coordinate();
            colors = new Color();

            points = verticies.Select(v => v.Position).ToArray();
            colors.color = X3DTypeConverters.ToString(verticies.Select(v => v.Color.Xyz).ToArray());
            coord.point = X3DTypeConverters.ToString(points);

            coord.Parent = ps;
            colors.Parent = ps;
            ps.Children.Add(colors);
            ps.Children.Add(coord);

            ps.PrimativeType = PrimitiveType.Points;

            return ps;
        }

        #endregion

        #region Public Methods

        public override void CollectGeometry(
                        RenderingContext rc,
                        out GeometryHandle handle,
                        out BoundingBox bbox,
                        out bool coloring,
                        out bool texturing)
        {
            bbox = BoundingBox.Zero;

            

            // INTERLEAVE
            _pack = PackedGeometry.Pack(this);

            coloring = _pack.Coloring;
            texturing = _pack.Texturing;
            bbox = _pack.bbox;

            // BUFFER GEOMETRY
            handle = _pack.CreateHandle();
        }

        #endregion
    }
}
