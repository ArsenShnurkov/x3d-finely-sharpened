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
            X3D root = new X3D();
            string xml;

            // Build a big chessboard as a test, then save test to x3d file

            elevation = builder.BuildFlatPlane(100, 100, 1f, 1f, false, colorSequencer);

            root.Children.Add(elevation);

            // Build XML from all the SceneGraphNodes
            serializer = new X3DSceneGraphXMLSerializer(root);
            xml = serializer.Serialize();


            Console.WriteLine("\n~~~ X3D Gemerated Below ~~~\n");
            Console.WriteLine(xml);

            File.WriteAllText("D:\\out.x3d", xml);

            Console.ReadLine();
        }

        public static Vector3 colorSequencer(int face, int vertex)
        {
            Vector3 color = new Vector3();

            if(vertex % 2 == 0)
            {
                color = Vector3.Zero; // black
                color = new Vector3(OpenTK.Graphics.Color4.DarkGreen.R,
                                    OpenTK.Graphics.Color4.DarkGreen.G,
                                    OpenTK.Graphics.Color4.DarkGreen.B);
            }
            else
            {
                color = Vector3.One; // white
            }

            return color;
        }


    }
}
