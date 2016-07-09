using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Input;

namespace X3D
{
    public class RenderingContext
    {
        public Matricies matricies = new Matricies();
        public Camera cam;
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

        public void Scale(Vector3 scale)
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
