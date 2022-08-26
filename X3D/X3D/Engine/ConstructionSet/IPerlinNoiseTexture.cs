using System.Drawing;

namespace X3D.Engine
{
    public interface IPerlinNoiseGenerator : IHeightMapGenerator
    {
        Bitmap GetPerlinNoise(RenderingContext rc);
    }
}