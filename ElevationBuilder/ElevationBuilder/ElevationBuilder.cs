using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace X3D
{
    public delegate Vector3 ElevationColorSequencer(int face, int vertex);

    public class ElevationBuilder
    {


        public ElevationGrid BuildFlatPlane(int xDimension, int zDimension, 
                                            float xSpacing, float zSpacing, bool colorPerVertex,
                                            ElevationColorSequencer colorSequencer)
        {
            ElevationGrid g = new ElevationGrid();
            Color colorNode = new Color();
            int numHeights = xDimension * zDimension;
            int[] heights = new int[numHeights];
            int faceIndex = 0;
            int coordIndex = 0;
            Vector3 color;

            g.colorPerVertex = colorPerVertex;
            g.xDimension = xDimension.ToString();
            g.zDimension = zDimension.ToString();
            g.xSpacing = xSpacing;
            g.zSpacing = zSpacing;
            g.height = string.Empty;
            colorNode.color = string.Empty;

            for (coordIndex = 0; coordIndex < numHeights; coordIndex++)
            {
                if(coordIndex % 4 == 0 && coordIndex > 0)
                {
                    faceIndex++;
                }

                g.height += " 0";

                color = colorSequencer(faceIndex, coordIndex);

                colorNode.color += string.Format(" {0} {1} {2}, ", color.X, color.Y, color.Z);
            }

            colorNode.color = colorNode.color.TrimStart();
            colorNode.color = colorNode.color.Substring(0, colorNode.color.Length - 2);

            g.height = g.height.TrimStart();

            
            g.Children.Add(colorNode);

            return g;
        }
    }
}
