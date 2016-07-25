using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace X3D
{
    public class Matricies
    {
        public Matrix4 worldview;
        public Matrix4 modelview;
        public Matrix4 projection;

        public Quaternion orientation;

        public Vector3 Scale = Vector3.One; // Vector3.One accumulated scale
        //private Quaternion _orientation; // TODO: use Quaternions to model rotation
    }
}
