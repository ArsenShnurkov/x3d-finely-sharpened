using System.Drawing;
using OpenTK;
using X3D.ConstructionSet;

namespace X3D.Engine
{
    public interface IElevationBuilder
    {
        ElevationGrid BuildHeightmapFromGenerator(RenderingContext rc, IHeightMapGenerator generator,
            out Bitmap largePerlinImage, int x, int z, int h, float sx, float sz);

        ElevationGrid BuildHeightmapFromTexture(float xSpacing, float zSpacing, Bitmap texture, float maxHeight = 1.0f);

        ElevationGrid BuildCheckerboard(int xDimension, int zDimension, float xSpacing, float zSpacing,
            Vector3 colorOdd, Vector3 colorEven);

        ElevationGrid BuildFlatPlane(int xDimension, int zDimension, float xSpacing, float zSpacing,
            bool colorPerVertex, ElevationColorSequencerDelegate colorSequencer);

        ElevationGrid BuildVaryingHeightMap(int xDimension, int zDimension, float xSpacing, float zSpacing,
            bool colorPerVertex, ElevationColorSequencerDelegate colorSequencer,
            HeightComputationDelegate geometrySequencer);

        Shape BuildShapeNodeWithScene();
    }
}