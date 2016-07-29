using OpenTK;
using System;
using System.Drawing;
using X3D.Engine;
using X3D.Parser;

namespace X3D.ConstructionSet
{



    public class ElevationBuilder : IElevationBuilder
    {

        public ElevationGrid BuildHeightmapFromGenerator(RenderingContext rc,
                                                      IHeightMapGenerator generator, 
                                                      out Bitmap largePerlinImage, 
                                                      int x, int z, int h, 
                                                      float sx, 
                                                      float sz)
        {
            ElevationGrid g;
            Bitmap noiseBitmap;
            Image newImage;

            Console.WriteLine("Rendering noise texture (in a shader)");
            noiseBitmap = largePerlinImage = generator.GenerateHeightMap(rc);

            Console.WriteLine("Processed into GDI Bitmap and cropping down to ({0}, {1}) for performace", x, z);
            newImage = noiseBitmap.GetThumbnailImage(x, z, null, IntPtr.Zero); // scale image to make processing easier
            //noiseBitmap.Dispose();
            noiseBitmap = (Bitmap)newImage;

            Console.WriteLine("Going through each depth pixel to generate an ElevationGrid");
            g = BuildHeightmapFromTexture(sx, sz, noiseBitmap, h); 

            return g;
        }

        public ElevationGrid BuildHeightmapFromTexture(
                                            float xSpacing, float zSpacing, 
                                            Bitmap texture, 
                                            float maxHeight = 1.0f)
        {
            ElevationGrid g = new ElevationGrid();
            Color colorNode = new Color();
            int xDimension, zDimension;
            int numHeights;
            int faceIndex = 0;
            int coordIndex = 0;
            float height;
            System.Drawing.Color color;
            Vector3 vColor;
            Vector3 vNTSCConversionWeight;
            float grayscale;

            g.colorPerVertex = true;
            g.xSpacing = xSpacing;
            g.zSpacing = zSpacing;
            g.height = string.Empty;
            colorNode.color = string.Empty;

            xDimension = texture.Width;
            zDimension = texture.Height;

            numHeights = xDimension * zDimension;

            g.xDimension = xDimension.ToString();
            g.zDimension = zDimension.ToString();

            Console.WriteLine("Building height map from texture (this could take a while)");

            for (int x = 0; x < xDimension; x++)
            {
                for (int z = 0; z < zDimension; z++)
                {
                    if (coordIndex % 4 == 0 && coordIndex > 0)
                    {
                        faceIndex++;
                    }

                    color = texture.GetPixel(x, z);


                    // Convert to grayscale using NTSC conversion weights
                    vColor = new Vector3(color.R / 255f, color.G / 255f, color.B / 255f);
                    vNTSCConversionWeight = new Vector3(0.299f, 0.587f, 0.114f);
                    grayscale = Vector3.Dot(vColor, vNTSCConversionWeight);


                    // Convert color to height value
                    height = (float)(grayscale) * maxHeight;

                    g.height += string.Format(" {0}", height);
                    colorNode.color += X3DTypeConverters.ToString(vColor) + ", ";

                    updateProgress(coordIndex, numHeights);

                    coordIndex++;
                }
            }

            colorNode.color = colorNode.color.Trim();
            colorNode.color = colorNode.color.Substring(0, colorNode.color.Length - 2);

            g.height = g.height.TrimStart();
            g.Children.Add(colorNode);

            return g;
        }


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

            Console.WriteLine("Building height map flat plane (this could take a while)");

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

                    updateProgress(coordIndex, numHeights);

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

            Console.WriteLine("Building varying height map (this could take a while)");

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

                    updateProgress(coordIndex, numHeights);

                    coordIndex++;
                }
            }

            colorNode.color = colorNode.color.TrimStart();
            colorNode.color = colorNode.color.Substring(0, colorNode.color.Length - 2);

            g.height = g.height.TrimStart();
            g.Children.Add(colorNode);

            return g;
        }

        public Shape BuildShapeNodeWithScene()
        {
            X3D root;
            Shape shape;

            BuildShapeDom(out root, out shape);

            return shape;
        }

        private void updateProgress(int coordIndex, int numHeights)
        {

            float percent = ((float)(coordIndex + 1) / (float)numHeights) * 100;

            if (percent > 0 && percent % 1.0f == 0)
            {
                //Console.SetCursorPosition(0, 15);
                
                Console.Write(" {0}%", percent);
            }
        }

        internal void BuildShapeDom(out X3D root, out Shape shape)
        {
            head head = new head();
            meta meta = new meta();
            Scene scene = new Scene();

            root = new X3D();
            shape = new Shape();

            meta.name = "generator";
            meta.content = BuilderApplication.AppInfo;

            scene.Children.Add(shape);
            head.Children.Add(meta);
            
            root.Children.Add(scene);
            root.Children.Add(head);
        }
    }
}
