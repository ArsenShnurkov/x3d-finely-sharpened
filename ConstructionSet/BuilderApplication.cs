using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading;
using X3D.Engine;
using X3D.Parser;

namespace X3D.ConstructionSet
{
    public class BuilderApplication : GameWindow
    {
        private const int FRAMES_TO_RENDER = 1; // set to -1 to disable frame limit

        #region Fields and Properties

        public static IConstructionSet ConstructionSet;

        public static string X3DExamplesDirectory = System.IO.Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory + "..\\..\\..\\..\\x3d-examples\\");
        public static string TempFileLocation = Path.GetTempPath();
        public static string X3dGenOutputFileLocation = TempFileLocation + "test_frame_out_{0}.x3d";
        public static string PngGenOutputFileLocation = TempFileLocation + "test_frame_out_perlin_{0}.png";


        private static Random r = new Random();
        private static double Rand() { return r.NextDouble(); }
        private static double degToRad(double degr) { return (Math.PI * degr) / 180.0; }

        private SceneCamera ActiveCamera;
        private static PerlinNoise _perlin;
        private static Bitmap largePerlinImage;
        private int frame = 0;
        
        public OpenTK.Input.KeyboardDevice Keyboard { get { return this.InputDriver.Keyboard[0]; } }
        public OpenTK.Input.MouseDevice Mouse { get { return this.InputDriver.Mouse[0]; } }
        public OpenTK.Input.JoystickDevice Joystick { get { return this.InputDriver.Joysticks[0]; } }

        #endregion

        public static void ExecuteBuildTasks(RenderingContext rc)
        {
            Console.WriteLine("Builds an ElevationGrid given parameters outputting in X3D XML encoding");

            
            ElevationBuilder builder = new ElevationBuilder();
            ElevationGrid elevation;
            X3D root;
            Shape shape;
            Image newImage;
            Bitmap bmp;

            ConstructionSet.ElevationBuilder = builder;

            // Setup scene graph
            builder.BuildShapeDom(out root, out shape);

            // below are the generation options for ElevationGrid

            // builds a big chessboard
            //elevation = builder.BuildCheckerboard(100, 100, 8f, 8f, Vector3.One, new Vector3(OpenTK.Graphics.Color4.DarkGreen.R, OpenTK.Graphics.Color4.DarkGreen.G, OpenTK.Graphics.Color4.DarkGreen.B));

            // builds elevation based on a sine function:
            //elevation = builder.BuildVaryingHeightMap(100, 100, 8f, 8f, false, colorSequencer, sineGeometrySequencer);

            // build mountain
            //elevation = builder.BuildVaryingHeightMap(100, 100, 1.0f, 1.0f, false, colorSequencer, mountainGeometrySequencer);

            // build mountain from hight points in texture:
            //bmp = (Bitmap)Image.FromFile(X3DExamplesDirectory + "heightmaptest1.png");
            //bmp = (Bitmap)Image.FromFile(X3DExamplesDirectory + "heightmaptest2.jpg");
            //bmp = (Bitmap)Image.FromFile("D:\\test_frame_out.png");
            //newImage = bmp.GetThumbnailImage(100, 100, null, IntPtr.Zero); // scale image to make processing easier
            //bmp.Dispose();
            //bmp = (Bitmap)newImage;
            //elevation = builder.BuildHeightmapFromTexture(20, 20, bmp, 400.0f); // build a rather large height map

            // build mountain on the fly using perlin noise shader: (requires OpenGL 4.x)
            Console.WriteLine("Using perlin noise generator");
            elevation = builder.BuildHeightmapFromGenerator(rc, _perlin, out largePerlinImage, 40, 40, 20, 20, 20); // build a rather large height map

            outputX3D(root, shape, elevation);
        }

        private static void outputX3D(X3D root, Shape shape, ElevationGrid elevation)
        {
            X3DSceneGraphXMLSerializer serializer;
            string xml;

            shape.Children.Add(elevation);

            // Build XML from all the SceneGraphNodes
            serializer = new X3DSceneGraphXMLSerializer(root);
            xml = serializer.Serialize();

            Console.WriteLine("\nDone.");

            //Console.WriteLine("\n~~~ X3D Generated Below ~~~\n");
            //Console.WriteLine(xml);

            // Save output XML to x3d file
            string fileX3d, filePng;

            fileX3d = newIndexedFile(X3dGenOutputFileLocation);
            filePng = newIndexedFile(PngGenOutputFileLocation);

            File.WriteAllText(fileX3d, xml);
            largePerlinImage.Save(filePng);

            Console.WriteLine("See Auto Generated X3D file in {0}", fileX3d);
            Console.WriteLine("See Auto Generated perlin noise texture in {0}", filePng);
            Console.ReadLine();
        }

        /// <summary>
        /// Finds a new file given the specified path format 
        /// that is postfixed with an index that does not exist on the file system.
        /// </summary>
        private static string newIndexedFile(string pathFormat)
        {
            string name = string.Empty;

            // find a file name that does not exist
            for (int i = 0; i < int.MaxValue; i++)
            {
                name = string.Format(pathFormat, i);

                if (!File.Exists(name))
                {
                    break;
                }
            }

            return name;
        }

        #region Geometry and Color Sequencers

        public static float mountainGeometrySequencer(int x, int z)
        {

            if (x == 0)
            {
                x = 1;
            }

            double pdRat1 = 360.0 / (double)x;

            double rad1 = degToRad((double)z * pdRat1);

            double y = 0.2f * Rand() * Math.Sin(x + z);

            y = 10.2f * Rand() * Math.Sin(rad1);

            y = -(y * 10.0 / 3.0) + 1.0;

            return (float)y;
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

            if (vertex % 2 == 0)
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

        #endregion

        #region Constructors

        public BuilderApplication(VSyncMode VSync, Resolution res, GraphicsMode mode) : base(res.Width, res.Height, mode)
        {
            ActiveCamera = new SceneCamera(this.Width, this.Height);
        }

        #endregion

        #region Rendering Methods

        protected override void OnLoad(EventArgs e)
        {
            this.Visible = false; // no need to display anything, we only need the graphics context.

            Console.WriteLine("LOAD <Perlin Noise Generator>");
            Console.Title = "Perlin Noise Generator";
            int[] t = new int[2];
            GL.GetInteger(GetPName.MajorVersion, out t[0]);
            GL.GetInteger(GetPName.MinorVersion, out t[1]);
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("OpenGL Version " + t[0] + "." + t[1]);


            // INITILISE SCENE

            GL.ClearColor(0.0f, 0.0f, 0.0f, 0.0f);           // Black Background
            GL.ClearDepth(1.0f);                            // Depth Buffer Setup
            GL.Enable(EnableCap.DepthTest);                 // Enables Depth Testing
            GL.DepthFunc(DepthFunction.Lequal);             // The Type Of Depth Testing To Do
            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);  // Really Nice Perspective Calculations

            DateTime time_before = DateTime.Now;

            View view = View.CreateViewFromWindow(this);

            // Get perlin noise output
            _perlin = new PerlinNoise();
            _perlin.Load();

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("loading time: " + DateTime.Now.Subtract(time_before).TotalMilliseconds.ToString() + "ms");
            Console.ForegroundColor = ConsoleColor.Yellow;
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            if (frame >= FRAMES_TO_RENDER) return;

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.ClearColor(System.Drawing.Color.White);
            GL.Enable(EnableCap.DepthTest);
            GL.DepthMask(true);
            GL.DepthFunc(DepthFunction.Lequal);
            GL.PointSize(6.0f);

            //TODO: improve current Camera implementation

            ActiveCamera.ApplyTransformations(); // TEST new camera implementation

            // TODO: steroscopic mode

            RenderingContext rc = new RenderingContext();
            rc.View = View.CreateViewFromWindow(this);
            rc.Time = e.Time;
            rc.matricies.worldview = Matrix4.Identity;
            rc.matricies.projection = ActiveCamera.Projection;

            rc.matricies.modelview = Matrix4.Identity;
            rc.matricies.orientation = Quaternion.Identity;
            rc.cam = ActiveCamera;
            rc.Keyboard = this.Keyboard;

            // Apply the current Viewpoint
            Viewpoint.Apply(rc, Viewpoint.CurrentViewpoint);

            //this._perlin.Render(rc);

            ExecuteBuildTasks(rc);

            frame++;

            SwapBuffers();
        }


        protected override void OnResize(EventArgs e)
        {
            // not used.
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            // not used.
        }

        #endregion

        #region Public Static Methods

        public static BuilderApplication CreateBuilderApplication()
        {
            VSyncMode VSync;
            AutoResetEvent closureEvent;
            BackgroundWorker bw;
            BuilderApplication builder = null;

            VSync = VSyncMode.Off;
            closureEvent = new AutoResetEvent(false);
            bw = new BackgroundWorker();

            bw.WorkerReportsProgress = true;
            bw.WorkerSupportsCancellation = true;
            bw.DoWork += new DoWorkEventHandler((object sender, DoWorkEventArgs e) =>
            {
                builder = new BuilderApplication(VSync, Resolution.Size800x600, new GraphicsMode(32, 16, 0, 4));
                builder.Title = "Initilising Construction Set..";
                builder.Visible = false;

                if (VSync == VSyncMode.Off)
                {
                    builder.Run(60);
                }
                else
                {
                    builder.Run();
                }
            });

            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler((object sender, RunWorkerCompletedEventArgs e) =>
            {
                closureEvent.Set();
            });

            bw.RunWorkerAsync();
            //closureEvent.WaitOne();

            return builder;
        }

        public static string AppInfo
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

        #endregion
    }
}
