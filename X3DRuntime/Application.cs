//#define LIVE_FPS

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using OpenTK;
using X3D.Engine;
using X3D.Parser;

namespace X3D.Runtime
{
    public partial class X3DApplication
    {
        private const int TITLE_UPDATE_INTERVAL = 2000;
        private static bool systemCursorVisible = true;


        //private DateTime before;
        //private DateTime after;
        private DateTime _prev;
        private string title;
        private Timer tmrTitleUpdate;

        public static string AppInfo
        {
            get
            {
                Assembly asm;
                AssemblyProductAttribute productName;
                //AssemblyVersionAttribute ver;
                AssemblyDescriptionAttribute desc;

                asm = Assembly.GetAssembly(typeof(XMLParser));
                productName =
                    (AssemblyProductAttribute)Attribute.GetCustomAttribute(asm, typeof(AssemblyProductAttribute));
                //ver=(AssemblyVersionAttribute)Attribute.GetCustomAttribute(asm,typeof(AssemblyVersionAttribute));
                desc = (AssemblyDescriptionAttribute)Attribute.GetCustomAttribute(asm,
                    typeof(AssemblyDescriptionAttribute));

                var version = asm.GetName().Version.ToString();
                return productName.Product + " " + version + " \"" + desc.Description + "\"";
            }
        }

        [DllImport("user32.dll")]
        private static extern int ShowCursor(bool bShow);

        public void ToggleCursor()
        {
            if (systemCursorVisible)
                ShowCursor(false);
            else
                ShowCursor(true);
            systemCursorVisible = !systemCursorVisible;
        }


        public void ShowSupportMatrix()
        {
            var asm = Assembly.GetAssembly(typeof(XMLParser));

            var types = new List<Type>(asm.GetTypes())
                .Where(t => t.IsSubclassOf(typeof(SceneGraphNode)))
                .OrderBy(t => t.FullName).ToArray();

            Console.WriteLine("-- Support Matrix (Informal indicator of available nodes in X3D engine) --");
            int w = 4, h = types.Length / w;


            var grid = new string[h, w];
            int r = 0, c = 0;

            for (var i = 0; i < types.Length && c < h; i++)
            {
                var j = i;
                var t = types[i];

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

            for (var x = 0; x < w; x++)
            for (var y = 0; y < h; y++)
            {
                var s = grid[y, x];

                if (!string.IsNullOrEmpty(s))
                {
                    Console.SetCursorPosition(x * 25, y + 4);

                    Console.Write((x > 0 ? " " : "") + "{0}", s);
                }
            }

            Console.Write("\n");
            Console.WriteLine("-- EO Support Matrix --");
        }

        private void UpdateTitle(FrameEventArgs e)
        {
#if LIVE_FPS
            draw_time = DateTime.Now.Subtract(_prev).Milliseconds;
            fps = GetFps(e.Time);

            string dt = draw_time.ToString();
            if(dt.Length<2) {
                if(dt.Length==1) {
                    dt = " "+dt;
                }
                else {
                    dt = "  ";
                }
            }
            title = fps.ToString()+"f/s "+dt+"ms";
#else
            if (tmrTitleUpdate == null)
                tmrTitleUpdate = new Timer(obj =>
                {
                    draw_time = DateTime.Now.Subtract(_prev).Milliseconds;
                    fps = GetFps(e.Time);

                    var dt = draw_time.ToString();
                    if (dt.Length < 2)
                    {
                        if (dt.Length == 1)
                            dt = " " + dt;
                        else
                            dt = "  ";
                    }

                    title = fps + "f/s " + dt + "ms";
                }, null, 0, TITLE_UPDATE_INTERVAL);
#endif

            string view;

            if (Viewpoint.CurrentViewpoint == null)
                view = Viewpoint.VIEWPOINT_DEFAULT_DESCRIPTION;
            else
                view = Viewpoint.CurrentViewpoint.description;

            // update world time a bit faster:
            WorldTime = DateTime.Now.Subtract(time_at_init);

            var pos = string.Format("{0}, {1}, {2}", ActiveCamera.Position.X, ActiveCamera.Position.Y,
                ActiveCamera.Position.Z);
            var fileName = Path.GetFileName(SceneManager.BaseURL);
            var @base = string.Format("({0}) ({1})", fileName, SceneManager.GetMIMETypeString(SceneManager.BaseMIME));

            window.Title = string.Format("X3D Runtime {0} {1} vwt [{2}] Viewpoint: {3} {4}",
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


        public sealed class App
        {
            public static string Path =>
                //return System.Windows.Forms.Application.StartupPath+"\\";
                new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.FullName + "\\";

            public static string MapPath(string relativePath)
            {
                return System.IO.Path.GetFullPath(relativePath);
            }
        }
    }
}