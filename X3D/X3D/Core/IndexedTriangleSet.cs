﻿using X3D.Core.Shading;

namespace X3D
{
    public partial class IndexedTriangleSet
    {
        internal PackedGeometry _pack;

        public override void CollectGeometry(
            RenderingContext rc,
            out GeometryHandle handle,
            out BoundingBox bbox,
            out bool coloring,
            out bool texturing)
        {
            handle = GeometryHandle.Zero;
            bbox = BoundingBox.Zero;

            // INTERLEAVE
            _pack = PackedGeometry.Pack(this);

            coloring = _pack.Coloring;
            texturing = _pack.Texturing;
            bbox = _pack.bbox;

            // BUFFER GEOMETRY
            handle = _pack.CreateHandle();
        }
    }
}