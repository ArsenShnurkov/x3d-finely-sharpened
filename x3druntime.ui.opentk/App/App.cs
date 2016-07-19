using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using OpenTK;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace x3druntime.ui.opentk
{
    public class App
    {
        public static string X3DExamplesDirectory = System.IO.Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory + "..\\..\\..\\x3d-examples\\");

        public static string SelectFile()
        {
            OpenFileDialog fileui;
            string url = string.Empty;

            #region WINDOWS PLATFORM

            // Hook into OpenFileDialog to get URLs

            uint pid = 0;
            uint tid = 0;
            using (var p = Process.GetCurrentProcess())
                GetWindowThreadProcessId(p.MainWindowHandle, out pid);

            var hHook = SetWinEventHook(EVENT_OBJECT_VALUECHANGE, EVENT_OBJECT_VALUECHANGE, IntPtr.Zero, CallWinEventProc, pid, tid, WINEVENT_OUTOFCONTEXT);


            #endregion

            fileui = new OpenFileDialog();
            fileui.Title = "Open Scene File (Pick a file on local file system or a URL)";
            fileui.Filter = "X3D Files (*.x3d)|*.x3d|XML Files (*.xml)|*.xml|X3D Binary Files (*.x3db)|*.x3db|Classic VRML Files (*.x3dv)|*.x3dv|VRML Files (*.wrl)|*.wrl|All Files (*.*)|*.*";

            if (System.IO.Directory.Exists(X3DExamplesDirectory))
            {
                fileui.InitialDirectory = X3DExamplesDirectory;
            }
            else
            {
                fileui.InitialDirectory = "C:\\";
            }

            //fileui.ShowDialog(X3DProgram.CurrentProgram); // the Console is the owner of the dialog

            fileui.ValidateNames = false;
            fileui.DereferenceLinks = false;
            fileui.CheckFileExists = false;
            fileui.RestoreDirectory = true;
            fileui.Multiselect = false;

            if (fileui.ShowDialog(X3DProgram.CurrentProgram) == DialogResult.OK)
            {

                string temporaryInternetFilesDir = Environment.GetFolderPath(System.Environment.SpecialFolder.InternetCache);
                if (!string.IsNullOrEmpty(temporaryInternetFilesDir) &&
                            fileui.FileName.StartsWith(temporaryInternetFilesDir, StringComparison.InvariantCultureIgnoreCase))
                {
                    // the file is in the Temporary Internet Files directory, very good chance it has been downloaded...

                    url = OpenFileDialogURL;
                }
                else
                {
                    url = fileui.FileName;
                }
            }

            UnhookWinEvent(hHook);
            fileui.Dispose();

            return url;
        }

        #region WINDOWS PLATFORM

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventProc lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);
        [DllImport("user32.dll")]
        private static extern bool UnhookWinEvent(IntPtr hWinEventHook);
        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);


        private const int WINEVENT_OUTOFCONTEXT = 0;
        private const uint EVENT_OBJECT_VALUECHANGE = 0x800E;

        private static String OpenFileDialogURL = "";
        private static WinEventProc CallWinEventProc = new WinEventProc(EventCallback);
        private delegate void WinEventProc(IntPtr hWinEventHook, int iEvent, IntPtr hWnd, int idObject, int idChild, int dwEventThread, int dwmsEventTime);
        private static void EventCallback(IntPtr hWinEventHook, int iEvent, IntPtr hWnd, int idObject, int idChild, int dwEventThread, int dwmsEventTime)
        {
            StringBuilder sb1 = new StringBuilder(256);
            GetClassName(hWnd, sb1, sb1.Capacity);
            if (sb1.ToString() == "Edit")
            {
                StringBuilder sb = new StringBuilder(512);
                GetWindowText(hWnd, sb, sb.Capacity);
                OpenFileDialogURL = sb.ToString();
            }
        }

        #endregion
    }
}
