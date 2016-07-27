using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace X3D
{
    public class ShaderHelpers
    {
        public static string getShaderSource(string fileAbsolutePath)
        {
            string @base = System.IO.Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory + "..\\..\\Shaders\\");

            return File.ReadAllText(@base + fileAbsolutePath);
        }
    }
}
