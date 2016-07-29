using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace X3D.Engine
{
    public interface IHeightMapGenerator
    {
        Bitmap GenerateHeightMap(RenderingContext rc);
    }
}
