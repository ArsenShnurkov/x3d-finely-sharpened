using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace X3D.Engine
{
    public interface IConstructionSet
    {
        IElevationBuilder ElevationBuilder { get; set; }

        IPerlinNoiseGenerator GetPerlinNoiseProvider();
    }
}
