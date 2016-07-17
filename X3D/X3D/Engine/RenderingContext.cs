using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Input;
using X3D.Engine;

namespace X3D
{
    public class RenderingContext
    {
        public Matricies matricies = new Matricies();
        public TestCamera cam;
        public double Time;

        private Stack<Matricies> transformHierarchy = new Stack<Matricies>();

        public KeyboardDevice Keyboard { get; set; }

        public void Translate(Vector3 translation)
        {
            matricies.modelview *= Matrix4.CreateTranslation(translation);
        }

        public void Rotate(Vector4 rotation)
        {

        }

        public void Rotate(Vector4 rotation, Vector3 centerOfRotation)
        {
            
        }

        public void Scale(Vector3 scale)
        {
            matricies.Scale = Vector3.Multiply(matricies.Scale, scale);
        }

        public void Scale(Vector3 scale, Vector4 scaleOrientation)
        {
            matricies.Scale = Vector3.Multiply(matricies.Scale, scale);
        }

        public void PushMatricies()
        {
            transformHierarchy.Push(matricies);
        }

        public void PopMatricies()
        {
            matricies = transformHierarchy.Pop();
        }


    }
}
