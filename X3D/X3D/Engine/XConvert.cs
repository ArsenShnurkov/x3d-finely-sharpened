using System;
using X3D.Spec;
using X3D.Geometry3D;

namespace X3D.Engine {
    /// <summary>
    /// Converter algorithms useful for converting between different GeometryNode types
    /// </summary>
    public sealed class XConvert { //:IGeometryConvertable {
        //TODO: implement converter algorithms for specific GeometryNode types
        public static IndexedFaceSet ToIndexedFaceSet(X3DComposedGeometryNode node) {
            throw new NotImplementedException();
        }

        public static ElevationGrid ToElevationGrid(X3DComposedGeometryNode node) { //TODO: blender doesnt export terrain to height maps
            throw new NotImplementedException();
        }

        //TODO: Other GeometryNode types
    }
}
