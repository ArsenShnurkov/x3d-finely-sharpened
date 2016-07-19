﻿//#define VSYNC_ACTIVE

using System;
using System.Collections.Generic;

using System.ComponentModel;
using System.Threading;

using System.IO;

using System.Windows.Forms;
using OpenTK;
using System.Text;
using OpenTK.Graphics;
using System.Runtime.InteropServices;

namespace x3druntime.ui.opentk
{

    public class X3DProgram : IWin32Window
    {
        private const int EXIT_SUCCESS = 0;
        private static VSyncMode VSync;
        private static BackgroundWorker bw;
        private static AutoResetEvent closureEvent;
        private static bool restartRequired = false;
        private static bool quitRequired = false;
        private static X3DBrowser browser;
        private static string url;

        #region WINDOWS PLATFORM

        public static X3DProgram CurrentProgram = new X3DProgram();

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();

        public IntPtr Handle
        {
            get { return GetConsoleWindow(); }
        }

        #endregion

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

        public static void Quit()
        {
            quitRequired = true;
            restartRequired = false;
            browser.Close();
            closureEvent.Set();
        }

        public static void Restart()
        {
            restartRequired = true;
            browser.Close();
            closureEvent.Set();
        }

        private static void LoadBrowser()
        {
            url = App.SelectFile();

            closureEvent = new AutoResetEvent(false);
            bw = new BackgroundWorker();

            bw.WorkerReportsProgress = true;
            bw.WorkerSupportsCancellation = true;
            bw.DoWork += new DoWorkEventHandler((object sender, DoWorkEventArgs e) =>
            {
                browser = new X3DBrowser(VSync, url, Resolution.Size800x600, new GraphicsMode(32, 16, 0, 4));
                browser.Title = "Initilising..";
#if VSYNC_ACTIVE
                browser.Run();
#else
                browser.Run(60);
#endif
            });

            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler((object sender, RunWorkerCompletedEventArgs e) =>
            {
                closureEvent.Set();
            });

            bw.RunWorkerAsync();
            closureEvent.WaitOne();

            if (!quitRequired && restartRequired)
            {
                restartRequired = false;
                LoadBrowser();
            }

            if (quitRequired)
            {
                bw.CancelAsync();
                //System.Windows.Forms.Application.Exit();
                System.Threading.Thread.CurrentThread.Abort();
            }
        }
    }

}