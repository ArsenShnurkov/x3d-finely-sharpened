using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace X3D.Runtime
{
    public class App
    {
        public static string X3DExamplesDirectory =
            Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory + "..\\..\\..\\x3d-examples\\");

        public static string SelectFile()
        {
            OpenFileDialog fileui;
            var url = string.Empty;

            #region WINDOWS PLATFORM

            // Hook into OpenFileDialog to get URLs

            uint pid = 0;
            uint tid = 0;
            using (var p = Process.GetCurrentProcess())
            {
                GetWindowThreadProcessId(p.MainWindowHandle, out pid);
            }

            var hHook = SetWinEventHook(EVENT_OBJECT_VALUECHANGE, EVENT_OBJECT_INVOKED,
                IntPtr.Zero, CallWinEventProc, pid, tid, WINEVENT_OUTOFCONTEXT);

            #endregion

            fileui = new OpenFileDialog();
            fileui.Title = "Open Scene File (Pick a file on local file system or a URL)";
            fileui.Filter =
                "X3D Files (*.x3d)|*.x3d|XML Files (*.xml)|*.xml|X3D Binary Files (*.x3db)|*.x3db|Classic VRML Files (*.x3dv)|*.x3dv|VRML Files (*.wrl)|*.wrl|All Files (*.*)|*.*";

            if (Directory.Exists(X3DExamplesDirectory))
                fileui.InitialDirectory = X3DExamplesDirectory;
            else
                fileui.InitialDirectory = "C:\\";

            OpenFileCancelButtonPressed = false;
            OpenFileCancelURLDownload = false;
            OpenFileDialogURL = "";

            //fileui.ShowDialog(X3DProgram.CurrentProgram); // the Console is the owner of the dialog

            fileui.ValidateNames = false;
            fileui.DereferenceLinks = false;
            fileui.CheckFileExists = false;
            fileui.RestoreDirectory = true;
            fileui.Multiselect = false;

            var result = fileui.ShowDialog(X3DProgram.CurrentProgram);

            if (result == DialogResult.OK)
            {
                var temporaryInternetFilesDir = Environment.GetFolderPath(Environment.SpecialFolder.InternetCache);
                if (!string.IsNullOrEmpty(temporaryInternetFilesDir) &&
                    fileui.FileName.StartsWith(temporaryInternetFilesDir, StringComparison.InvariantCultureIgnoreCase))
                    // the file is in the Temporary Internet Files directory, very good chance it has been downloaded...
                    url = OpenFileDialogURL;
                else
                    url = fileui.FileName;
            }
            else if (result == DialogResult.Cancel && OpenFileCancelURLDownload)
            {
                url = OpenFileDialogURL;
            }

            UnhookWinEvent(hHook);
            fileui.Dispose();

            if (OpenFileCancelButtonPressed || string.IsNullOrEmpty(url.Trim())) url = null;

            return url;
        }

        #region WINDOWS PLATFORM

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc,
            WinEventProc lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

        [DllImport("user32.dll")]
        private static extern bool UnhookWinEvent(IntPtr hWinEventHook);

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Auto)]
        private static extern int
            SendMessage3(IntPtr hwndControl, uint Msg, int wParam, StringBuilder strBuffer); // get text

        [DllImport("user32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Auto)]
        private static extern int SendMessage4(IntPtr hwndControl, uint Msg, int wParam, int lParam); // text length

        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        public static extern IntPtr GetParent(IntPtr hWnd);

        public static void CloseWindow(IntPtr hwnd)
        {
            SendMessage4(hwnd, WM_CLOSE, 0, 0);
        }

        private const uint WM_CLOSE = 0x0010;

        private const int WINEVENT_OUTOFCONTEXT = 0;

        /// <summary>
        ///     If a textbox or scrollbar has changed
        /// </summary>
        private const uint EVENT_OBJECT_VALUECHANGE = 0x800E;

        /// <summary>
        ///     If a button has been clicked
        /// </summary>
        private const uint EVENT_OBJECT_INVOKED = 0x8013;

        private static string OpenFileDialogURL = "";
        private static bool OpenFileCancelURLDownload;
        private static bool OpenFileCancelButtonPressed;
        private static readonly WinEventProc CallWinEventProc = EventCallback;

        private delegate void WinEventProc(IntPtr hWinEventHook, int iEvent, IntPtr hWnd, int idObject, int idChild,
            int dwEventThread, int dwmsEventTime);

        private static int GetTextBoxTextLength(IntPtr hTextBox)
        {
            // helper for GetTextBoxText
            uint WM_GETTEXTLENGTH = 0x000E;
            var result = SendMessage4(hTextBox, WM_GETTEXTLENGTH,
                0, 0);
            return result;
        }

        private static string GetTextBoxText(IntPtr hTextBox)
        {
            try
            {
                uint WM_GETTEXT = 0x000D;
                var len = GetTextBoxTextLength(hTextBox);
                if (len <= 0) return string.Empty; // no text
                var sb = new StringBuilder(len + 1);
                SendMessage3(hTextBox, WM_GETTEXT, len + 1, sb);
                return sb.ToString();
            }
            catch
            {
                return string.Empty;
            }
        }

        private static void EventCallback(IntPtr hWinEventHook, int iEvent, IntPtr hWnd, int idObject, int idChild,
            int dwEventThread, int dwmsEventTime)
        {
            var sb1 = new StringBuilder(256);
            GetClassName(hWnd, sb1, sb1.Capacity);
            var eventClass = sb1.ToString();

            switch (eventClass)
            {
                case "Button":

                    var label = GetTextBoxText(hWnd).ToLower();

                    if (label.Contains("open") || (label.Contains("ok") && OpenFileCancelURLDownload))
                    {
                        var winHwnd = GetParent(hWnd);

                        // Cancel window quickly to prevent download and cache of URL that OpenFileDialog does
                        CloseWindow(winHwnd);
                    }
                    else if (label.Contains("cancel"))
                    {
                        OpenFileCancelButtonPressed = true;
                    }

                    break;
                case "Edit":

                    var sb = new StringBuilder(512);
                    GetWindowText(hWnd, sb, sb.Capacity);
                    OpenFileDialogURL = sb.ToString();
                    OpenFileCancelURLDownload = true;

                    break;
            }

            if (sb1.ToString() == "Edit")
            {
            }
        }

        #endregion
    }
}