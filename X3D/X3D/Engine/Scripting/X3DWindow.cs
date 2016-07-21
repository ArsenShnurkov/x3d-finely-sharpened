using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace X3D.Engine
{
    /// <summary>
    /// Window used by Scripting component 
    /// (via V8.Net Engine)
    /// </summary>
    public class X3DWindow
    {
        public static X3DWindow Current;

        public View screen { get; set; }

        internal static void SetView(View view)
        {
            Current = new X3DWindow();
            Current.screen = view;
        }
    }
}
