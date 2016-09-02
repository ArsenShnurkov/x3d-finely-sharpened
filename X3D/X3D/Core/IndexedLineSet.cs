﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using X3D.Core;
using X3D.Parser;
using X3D.Core.Shading;

namespace X3D
{
    /// <summary>
    /// http://www.web3d.org/documents/specifications/19775-1/V3.3/Part01/components/rendering.html#LineSet
    /// </summary>
    public partial class IndexedLineSet
    {
        #region Fields

        internal PackedGeometry _pack;

        #endregion

        #region Public Static Methods

        public static IndexedLineSet CreateFromVertexSet(List<Vertex> verticies)
        {
            IndexedLineSet ils;
            Coordinate coord;
            Vector3[] points;
            int[] indicies;
            int i;

            ils = new IndexedLineSet();
            coord = new Coordinate();

            points = verticies.Select(v => v.Position).ToArray();

            i = -1;
            indicies = verticies.Select(v => ++i).ToArray();

            coord.point = X3DTypeConverters.ToString(points);

            ils.coordIndex = X3DTypeConverters.ToString(indicies);
            ils.Children.Add(coord);

            return ils;
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
