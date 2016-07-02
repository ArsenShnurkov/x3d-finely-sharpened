using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace x3druntime.ui.opentk
{
    public class App
    {
        public static string X3DExamplesDirectory = System.IO.Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory + "..\\..\\..\\x3d-examples\\");

        public static string SelectFile()
        {
            System.Windows.Forms.OpenFileDialog fileui;

            fileui = new System.Windows.Forms.OpenFileDialog();
            fileui.Title = "Open Scene File";
            fileui.Filter = "X3D Files (*.x3d)|*.x3d|XML Files (*.xml)|*.xml|X3D Binary Files (*.x3db)|*.x3db|Classic VRML Files (*.x3dv)|*.x3dv|VRML Files (*.wrl)|*.wrl|All Files (*.*)|*.*";

            

            if (System.IO.Directory.Exists(X3DExamplesDirectory))
            {
                fileui.InitialDirectory = X3DExamplesDirectory;
            }
            else
            {
                fileui.InitialDirectory = "C:\\";
            }

            fileui.ShowDialog();

            return fileui.FileName;
        }
    }
}
