using System;
using System.IO;
using System.Windows.Forms;
using OpenTK.Graphics;
using OpenTK.Platform;
using X3D.Engine;

namespace X3D
{
    public class Program
    {
        public static string X3DExamplesDirectory =
            Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory + "..\\..\\..\\x3d-examples\\");

        private static GraphicsContext context;
        private static IWindowInfo windowInfo;
        private static Control control;

        [STAThread]
        public static void Main(string[] args)
        {
            OpenFileDialog fileui;
            DialogResult result;
            string x3dFile;
            SceneGraph graphTarget;
            SceneManager scene;
            X3DMIMEType mime;
            Stream compiled;
            StreamWriter writer;
            string tmpFile;

            CreateDummyContext();

            fileui = new OpenFileDialog();
            fileui.Multiselect = false;
            fileui.Title = "Select X3D Scene to compile to standalone executable";
            fileui.Filter =
                "X3D Files (*.x3d)|*.x3d|XML Files (*.xml)|*.xml|X3D Binary Files (*.x3db)|*.x3db|Classic VRML Files (*.x3dv)|*.x3dv|VRML Files (*.wrl)|*.wrl|All Files (*.*)|*.*";

            if (Directory.Exists(X3DExamplesDirectory))
                fileui.InitialDirectory = X3DExamplesDirectory;
            else
                fileui.InitialDirectory = "C:\\";

            while (true)
            {
                result = fileui.ShowDialog();

                if (result == DialogResult.OK || result == DialogResult.Yes)
                {
                    x3dFile = fileui.FileName;

                    if (File.Exists(x3dFile)) break;
                }
            }

            // COMPILE x3d to standalone platform specific executable
            // currently only supporting the windows platform

            mime = SceneManager.GetMIMETypeByURL(x3dFile);

            SceneManager.BaseURL = x3dFile;
            SceneManager.BaseMIME = mime;
            Script.EngineEnabled = false;

            scene = SceneManager.fromURL(x3dFile, mime);
            graphTarget = scene.SceneGraph;

            if (graphTarget != null)
            {
                compiled = X3DCompiler.Compile(graphTarget);

                tmpFile = Path.GetTempFileName() + ".exe";

                Console.WriteLine("Saving to {0}" + tmpFile);

                writer = new StreamWriter(compiled);

                using (var fileStream = File.Create(tmpFile))
                {
                    compiled.Position = 0;
                    compiled.Seek(0, SeekOrigin.Begin);
                    compiled.CopyTo(fileStream);
                }
            }
            else
            {
                Console.WriteLine("Could not build scene graph from X3D file");
            }

            DestroyDummyContext();
        }

        #region Dummy Context

        public static void CreateDummyContext()
        {
            IntPtr hWnd;

            control = new Control();
            hWnd = control.Handle;
            windowInfo = Utilities.CreateWindowsWindowInfo(hWnd);
            context = new GraphicsContext(GraphicsMode.Default, windowInfo);
            context.MakeCurrent(windowInfo);

            context.LoadAll();
        }

        public static void DestroyDummyContext()
        {
            context.Dispose();
            windowInfo.Dispose();
            control.Dispose();
        }

        #endregion
    }
}