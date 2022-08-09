using OpenTK;

namespace X3D
{
    public class Matricies
    {
        public Matrix4 modelview;

        public Quaternion orientation;
        public Matrix4 projection;

        public Vector3 Scale = Vector3.One; // Vector3.One accumulated scale

        public Matrix4 worldview;
        //private Quaternion _orientation; // TODO: use Quaternions to model rotation
    }
}