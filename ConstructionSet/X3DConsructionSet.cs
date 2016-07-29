﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using X3D.Engine;

namespace X3D.ConstructionSet
{
    public class X3DConsructionSet : IConstructionSet
    {
        public IElevationBuilder ElevationBuilder { get; set; }

        public IPerlinNoiseGenerator GetPerlinNoiseProvider()
        {
            PerlinNoise generator;

            generator = new PerlinNoise();
            generator.Load();

            return generator;
        }

        public static IConstructionSet GetConstructionSetProvider()
        {
            IConstructionSet constructionSet = new X3DConsructionSet();
            ElevationBuilder elevationProvider = new ElevationBuilder();

            // Makes use of whatever OpenGL context invokes this construction set

            constructionSet.ElevationBuilder = elevationProvider;

            return constructionSet;
        }
    }
}
