using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using X3D;
using X3D.Engine;
using x3druntime.ui.opentk;

namespace X3D
{
    public class ProjectTemplate
    {
        internal static string buildSource(string initilizers)
        {
            string code;

            code = @"
using OpenTK;
using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using X3D;
using X3D.Engine;
using x3druntime.ui.opentk;

namespace X3D 
{
    public class Program 
    {
        private static X3DBrowser browser;
        private static Thread th = null;
        private static AutoResetEvent closureEvent = null;

        public static SceneGraph Application { get; set; }

        public static void Main(string[] args)
        {
            Console.WriteLine(" + "\"" + "X3D Compiled Application - Hello World!" + "\"" + @");
            
            "+ "\n" + initilizers + "\n" + @"

        }

        public static void RunApplication()
        {
            closureEvent = new AutoResetEvent(false);

            ThreadStart ts = new ThreadStart(() =>
            {
                browser = new X3DBrowser(
                    OpenTK.VSyncMode.On, 
                    Application, 
                    Resolution.Size800x600, 
                    new GraphicsMode(32, 16, 0, 4)
                );

                browser.Title = " + "\"" + "Initilising.." + "\"" + @";

                browser.Run(60);
            });
            th = new Thread(ts);

            th.Start();

            closureEvent.WaitOne();
        }
    }
}
";
            return code;
        }

    }
}
