//#define VSYNC_ACTIVE

using System;
using System.Collections.Generic;

using System.ComponentModel;
using System.Threading;

using System.IO;

using System.Windows.Forms;
using OpenTK;
using System.Text;
using OpenTK.Graphics;

namespace x3druntime.ui.opentk
{

    public class X3DProgram
    {
        private const int EXIT_SUCCESS = 0;
        private static VSyncMode VSync;

        [STAThread]
        public static int Main(string[] args)
        {
#if VSYNC_ACTIVE
            VSync=VSyncMode.Off;
#else
            VSync = VSyncMode.On;
#endif

            LoadBrowser();

            return EXIT_SUCCESS;
        }

        private static void LoadBrowser()
        {
            //BackgroundWorker bw;
            //AutoResetEvent closureEvent;
            X3DBrowser browser;
            string url;

            url = App.SelectFile();

            //closureEvent = new AutoResetEvent(false);
            //bw = new BackgroundWorker();

            //bw.WorkerReportsProgress = true;
            //bw.WorkerSupportsCancellation = true;
            //bw.DoWork += new DoWorkEventHandler((object sender, DoWorkEventArgs e) =>
            //{
                browser = new X3DBrowser(VSync, url, Resolution.Size800x600, new GraphicsMode(32, 16, 0, 4));
                browser.Title = "Initilising..";
#if VSYNC_ACTIVE
                browser.Run();
#else
                browser.Run(60);
#endif
            //});
            //bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler((object sender, RunWorkerCompletedEventArgs e) =>
            //{
            //    closureEvent.Set();
            //});

            //bw.RunWorkerAsync();
            //closureEvent.WaitOne();
        }
    }

}