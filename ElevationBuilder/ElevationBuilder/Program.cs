using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using OpenTK;
using X3D.Engine;
using System.Reflection;
using X3D.Parser;
using OpenTK.Graphics;

namespace X3D
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("ElevationBuilder by Gerallt Franke 2013 - 2016");
            Console.WriteLine("Builds an ElevationGrid given parameters outputting in X3D XML encoding");

            X3DSceneGraphXMLSerializer serializer;
            ElevationBuilder builder = new ElevationBuilder();
            ElevationGrid elevation;
            X3D root;
            Shape shape;
            string xml;

            // Scene Graph
            builder.BuildShapeDom(out root, out shape);

            // builds a big chessboard
            //elevation = builder.BuildCheckerboard(100, 100, 8f, 8f, Vector3.One, new Vector3(OpenTK.Graphics.Color4.DarkGreen.R, OpenTK.Graphics.Color4.DarkGreen.G, OpenTK.Graphics.Color4.DarkGreen.B));

            // builds elevation based on a sine function:
            elevation = builder.BuildVaryingHeightMap(100, 100, 8f, 8f, false, colorSequencer, sineGeometrySequencer);

            shape.Children.Add(elevation);


            // Build XML from all the SceneGraphNodes
            serializer = new X3DSceneGraphXMLSerializer(root);
            xml = serializer.Serialize();

            Console.WriteLine("\n~~~ X3D Generated Below ~~~\n");
            Console.WriteLine(xml);

            // then save output XML to x3d file
            File.WriteAllText("D:\\out.x3d", xml);

            Console.ReadLine();
        }

        public static float sineGeometrySequencer(int x, int z)
        {

            if (x == 0)
            {
                x = 1;
            }

            double pdRat1 = 360.0 / (double)x;

            double rad1 = degToRad((double)z * pdRat1);

            double y = 0.2f * Rand() * Math.Sin(x + z);

            y = 1.2f * Rand() * Math.Sin(rad1);

            y = -(y * 10.0 / 3.0) + 1.0;

            return (float)y;
        }

        public static Vector3 colorSequencer(int face, int vertex, int x, int z)
        {
            Vector3 color = new Vector3();

            if(vertex % 2 == 0)
            {
                Color4 col = OpenTK.Graphics.Color4.DarkGray;
                color = new Vector3(col.R, col.G, col.B);
            }
            else
            {
                Color4 col = OpenTK.Graphics.Color4.Silver;
                color = new Vector3(col.R, col.G, col.B);
            }

            return color;
        }

        private static Random r = new Random();
        private static double Rand(){ return r.NextDouble(); }
        private static double degToRad(double degr){ return (Math.PI * degr) / 180.0; }
    }
}