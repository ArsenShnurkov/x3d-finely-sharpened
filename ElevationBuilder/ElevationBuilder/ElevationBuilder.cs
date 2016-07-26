using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using X3D.Parser;
using System.Reflection;

namespace X3D
{

    /// <summary>
    /// Computes a color given vertex and or (x, z) coordinates of a plane.
    /// </summary>
    public delegate Vector3 ElevationColorSequencerDelegate(int face, int vertex, int x, int z);

    /// <summary>
    /// Computes a height value given (x,z) coordinates of a plane.
    /// </summary>
    public delegate float HeightComputationDelegate(int x, int z);

    public interface IElevationBuilder
    {


    }

    public class ElevationBuilder : IElevationBuilder
    {


        public ElevationGrid BuildCheckerboard(int xDimension, int zDimension,
                                            float xSpacing, float zSpacing, 
                                            Vector3 colorOdd, Vector3 colorEven)
        {
            ElevationGrid g;

            g = BuildFlatPlane(xDimension, zDimension, xSpacing, zSpacing, false, (int face, int vertex, int x, int z) => {
                Vector3 color = new Vector3();

                if (vertex % 2 == 0)
                {
                    color = colorEven;
                }
                else
                {
                    color = colorOdd;
                }

                return color;
            });

            return g;
        }

        public ElevationGrid BuildFlatPlane(int xDimension, int zDimension, 
                                            float xSpacing, float zSpacing, 
                                            bool colorPerVertex,
                                            ElevationColorSequencerDelegate colorSequencer)
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

            for (int x = 0; x < xDimension; x++)
            {
                for (int z = 0; z < zDimension; z++)
                {
                    if (coordIndex % 4 == 0 && coordIndex > 0)
                    {
                        faceIndex++;
                    }


                    g.height += " 0";
                    color = colorSequencer(faceIndex, coordIndex, x, z);
                    colorNode.color += X3DTypeConverters.ToString(color) + ", ";

                    coordIndex++;
                }
            }

            colorNode.color = colorNode.color.TrimStart();
            colorNode.color = colorNode.color.Substring(0, colorNode.color.Length - 2);

            g.height = g.height.TrimStart();
            g.Children.Add(colorNode);

            return g;
        }

        public ElevationGrid BuildVaryingHeightMap(int xDimension, int zDimension, 
                                                   float xSpacing, float zSpacing,
                                                   bool colorPerVertex,
                                                   ElevationColorSequencerDelegate colorSequencer,
                                                   HeightComputationDelegate geometrySequencer)
        {
            ElevationGrid g = new ElevationGrid();
            Color colorNode = new Color();
            int numHeights = xDimension * zDimension;
            int faceIndex = 0;
            Vector3 color;
            int coordIndex = 0;

            g.colorPerVertex = colorPerVertex;
            g.xDimension = xDimension.ToString();
            g.zDimension = zDimension.ToString();
            g.xSpacing = xSpacing;
            g.zSpacing = zSpacing;
            g.height = string.Empty;
            colorNode.color = string.Empty;

            for (int x = 0; x < xDimension; x++)
            {
                for (int z = 0; z < zDimension; z++)
                {
                    if (coordIndex % 4 == 0 && coordIndex > 0)
                    {
                        faceIndex++;
                    }

                    g.height += string.Format(" {0}", geometrySequencer(x, z));

                    if(colorSequencer != null)
                    {
                        color = colorSequencer(faceIndex, coordIndex, x, z);
                        colorNode.color += X3DTypeConverters.ToString(color) + ", ";
                    }

                    coordIndex++;
                }
            }

            colorNode.color = colorNode.color.TrimStart();
            colorNode.color = colorNode.color.Substring(0, colorNode.color.Length - 2);

            g.height = g.height.TrimStart();
            g.Children.Add(colorNode);

            return g;
        }

        public void BuildShapeDom(out X3D root, out Shape shape)
        {
            head head = new head();
            meta meta = new meta();
            Scene scene = new Scene();

            root = new X3D();
            shape = new Shape();

            meta.name = "generator";
            meta.content = AppInfo;

            scene.Children.Add(shape);
            head.Children.Add(meta);
            
            root.Children.Add(scene);
            root.Children.Add(head);
        }

        private static string AppInfo
        {
            get
            {
                Assembly asm;
                AssemblyProductAttribute productName;
                AssemblyVersionAttribute ver;
                AssemblyDescriptionAttribute desc;

                asm = Assembly.GetAssembly(typeof(Parser.XMLParser));
                productName = (AssemblyProductAttribute)Attribute.GetCustomAttribute(asm, typeof(AssemblyProductAttribute));
                //ver=(AssemblyVersionAttribute)Attribute.GetCustomAttribute(asm,typeof(AssemblyVersionAttribute));
                desc = (AssemblyDescriptionAttribute)Attribute.GetCustomAttribute(asm, typeof(AssemblyDescriptionAttribute));

                string version = asm.GetName().Version.ToString();
                return productName.Product + "_" + version + "_graph_builder";
            }
        }
    }
}
