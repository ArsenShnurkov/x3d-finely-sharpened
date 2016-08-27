//#define VSYNC_OFF

using System;
using System.Threading;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using X3D.Engine;

namespace X3D.Runtime
{
    public class X3DProgram : IWin32Window
    {

        #region WINDOWS PLATFORM

        public static X3DProgram CurrentProgram = new X3DProgram();

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();

        public IntPtr Handle
        {
            get { return GetConsoleWindow(); }
        }

        #endregion

        #region Private Static Fields

        private const int EXIT_SUCCESS = 0;
        private static VSyncMode VSync;
        private static AutoResetEvent closureEvent;
        private static bool restartRequired = false;
        private static bool quitRequired = false;
        private static X3DBrowser browser;
        public static string url;

        #endregion

        #region Public Static Methods

        [STAThread]
        public static int Main(string[] args)
        {

            LoadBrowserWithURL();

            return EXIT_SUCCESS;
        }

        public static void Quit()
        {
            browser.Dispose();

            quitRequired = true;
            restartRequired = false;
            browser.Close();
            closureEvent.Set();
        }

        public static void Restart()
        {
            browser.Dispose();

            restartRequired = true;
            browser.Close();
            closureEvent.Set();
        }

        public static SceneGraph LoadX3D(string x3dFile, bool scriptingEnabled = true)
        {
            SceneManager scene;
            X3DMIMEType mime;

            // precondition: needs a graphics context

            mime = SceneManager.GetMIMETypeByURL(x3dFile);

            SceneManager.BaseURL = x3dFile;
            SceneManager.BaseMIME = mime;
            Script.EngineEnabled = scriptingEnabled;

            scene = SceneManager.fromURL(x3dFile, mime);

            return scene.SceneGraph;
        }

        public static async Task LoadBrowserWithGraphAsync(SceneGraph graph)
        {
            await Task.Run(() =>
            {
                browser = new X3DBrowser(
                    OpenTK.VSyncMode.On,
                    graph,
                    Resolution.Size800x600,
                    new GraphicsMode(32, 16, 0, 4)
                );

                browser.Title = "Initilising";

                browser.Run(60);
            });
        }

        public static async Task LoadBrowserWithURLAsync()
        {
#if VSYNC_OFF
            VSync = VSyncMode.Off;
#else
            VSync = VSyncMode.On;
#endif

            await Task.Run(() =>
            {
                browser = new X3DBrowser(VSync, url, Resolution.Size800x600, new GraphicsMode(32, 16, 0, 4));
                browser.Title = "Initilising..";
#if VSYNC_OFF
                browser.Run();
#else
                browser.Run(60);
#endif
            });
        }

        #endregion

        #region Private Static Methods

        private static void LoadBrowserWithURL()
        {
            url = App.SelectFile();

            closureEvent = new AutoResetEvent(false);

            var task = Task.Run(LoadBrowserWithURLAsync);
            
            closureEvent.WaitOne();

            if (!quitRequired && restartRequired)
            {
                restartRequired = false;
                LoadBrowserWithURL();
            }

            if (quitRequired)
            {
                task.ConfigureAwait(false);
                //th.Abort();
                //bw.CancelAsync();
                //System.Windows.Forms.Application.Exit();
                //System.Threading.Thread.CurrentThread.Abort();
                //Environment.Exit(0);
                //Application.Exit();
                System.Diagnostics.Process.GetCurrentProcess().Kill(); // Fast !
            }
        }

        #endregion
    }
}