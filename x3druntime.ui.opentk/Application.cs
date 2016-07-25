//#define LIVE_FPS

using System;
using System.Drawing;
using System.Reflection;
using OpenTK;

using X3D;
using System.Collections.Generic;
using System.Linq;
using X3D.Engine;
using System.Runtime.InteropServices;

namespace x3druntime.ui.opentk
{
    public partial class X3DApplication
    {
        [DllImport("user32.dll")]
        private static extern int ShowCursor(bool bShow);
        private static bool systemCursorVisible = true;

        public void ToggleCursor()
        {
            if (systemCursorVisible)
            {
                ShowCursor(false);
            }
            else
            {
                ShowCursor(true);
            }
            systemCursorVisible = !systemCursorVisible;
        }


        public void ShowSupportMatrix()
        {
            Assembly asm = Assembly.GetAssembly(typeof(X3D.Parser.XMLParser));

            Type[] types = (new List<Type>(asm.GetTypes()))
                .Where(t => t.IsSubclassOf(typeof(SceneGraphNode)))
                .OrderBy(t => t.FullName).ToArray();

            Console.WriteLine("-- Support Matrix (Informal indicator of available nodes in X3D engine) --");
            int w = 4, h = types.Length / w;


            string[,] grid = new string[h, w];
            int r = 0, c = 0;

            for (int i = 0; i < types.Length && c < h; i++)
            {
                int j = i;
                Type t = types[i];

                grid[c, r] = t.FullName;

                if (c < h)
                {
                    if (i > 0 && (j + 1) % w == 0)
                    {
                        if (c < h) c++;
                        r = 0;
                    }
                    else
                    {
                        r++;
                    }
                }
                else
                {
                    break;
                }
            }

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    string s = grid[y, x];

                    if (!string.IsNullOrEmpty(s))
                    {
                        Console.SetCursorPosition(x * 25, y + 4);

                        Console.Write((x > 0 ? " " : "") + "{0}", s);
                    }
                }
            }
            Console.Write("\n");
            Console.WriteLine("-- EO Support Matrix --");
        }


        public sealed class App
        {

            public static string MapPath(string relativePath)
            {
                return System.IO.Path.GetFullPath(relativePath);
            }

            public static string Path
            {
                get
                {
                    //return System.Windows.Forms.Application.StartupPath+"\\";
                    return (new System.IO.FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location)).Directory.FullName + "\\";
                }
            }
        }

        public static string AppInfo
        {
            get
            {
                Assembly asm;
                AssemblyProductAttribute productName;
                AssemblyVersionAttribute ver;
                AssemblyDescriptionAttribute desc;

                asm = Assembly.GetAssembly(typeof(X3D.Parser.XMLParser));
                productName = (AssemblyProductAttribute)Attribute.GetCustomAttribute(asm, typeof(AssemblyProductAttribute));
                //ver=(AssemblyVersionAttribute)Attribute.GetCustomAttribute(asm,typeof(AssemblyVersionAttribute));
                desc = (AssemblyDescriptionAttribute)Attribute.GetCustomAttribute(asm, typeof(AssemblyDescriptionAttribute));

                string version = asm.GetName().Version.ToString();
                return productName.Product + " " + version + " \"" + desc.Description + "\"";
            }
        }


        private DateTime before;
        private DateTime after;
        private DateTime _prev;
        private System.Threading.Timer tmrTitleUpdate;
        private string title;

        const int TITLE_UPDATE_INTERVAL = 2000;

        private void UpdateTitle(FrameEventArgs e)
        {
#if LIVE_FPS
            draw_time=DateTime.Now.Subtract(_prev).Milliseconds;
            fps=GetFps(e.Time);

            string dt=draw_time.ToString();
            if(dt.Length<2) {
                if(dt.Length==1) {
                    dt=" "+dt;
                }
                else {
                    dt="  ";
                }
            }
            title=fps.ToString()+"f/s "+dt+"ms";
#else
            if (tmrTitleUpdate == null)
            {
                tmrTitleUpdate = new System.Threading.Timer(new System.Threading.TimerCallback(
                    (object obj) =>
                    {
                        draw_time = DateTime.Now.Subtract(_prev).Milliseconds;
                        fps = GetFps(e.Time);

                        string dt = draw_time.ToString();
                        if (dt.Length < 2)
                        {
                            if (dt.Length == 1)
                            {
                                dt = " " + dt;
                            }
                            else
                            {
                                dt = "  ";
                            }
                        }
                        title = fps.ToString() + "f/s " + dt + "ms";
                    }
                ), null, 0, TITLE_UPDATE_INTERVAL);
            }
#endif

            string view;

            if (Viewpoint.CurrentViewpoint == null)
            {
                view = Viewpoint.VIEWPOINT_DEFAULT_DESCRIPTION;
            }
            else
            {
                view = Viewpoint.CurrentViewpoint.description;
            }

            // update world time a bit faster:
            WorldTime = DateTime.Now.Subtract(time_at_init);

            string pos = string.Format("{0}, {1}, {2}", ActiveCamera.Position.X, ActiveCamera.Position.Y, ActiveCamera.Position.Z);
            string fileName = System.IO.Path.GetFileName(SceneManager.BaseURL);
            string @base = string.Format("({0}) ({1})", fileName, SceneManager.GetMIMETypeString(SceneManager.BaseMIME));

            this.window.Title = string.Format("X3D Runtime {0} {1} vwt [{2}] Viewpoint: {3} {4}", 
                title, 
                WorldTime, 
                pos, 
                view,
                @base);
        }

        private int GetFps(double time)
        {
            _fps = (int)(1.0 / time);

            return _fps;
        }
    }
}
