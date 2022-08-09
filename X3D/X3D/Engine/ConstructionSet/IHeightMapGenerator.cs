using System.Drawing;

namespace X3D.Engine
{
    public interface IHeightMapGenerator
    {
        Bitmap GenerateHeightMap(RenderingContext rc);
    }
}