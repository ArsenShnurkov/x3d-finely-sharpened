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
        public SceneCamera cam;
        public double Time;

        /// <summary>
        /// Monoscopic View
        /// </summary>
        public View View;
        public View StereoscopicLeft = null;
        public View StereoscopicRight = null;

        public KeyboardDevice Keyboard { get; set; }

        private Stack<Matricies> transformHierarchy = new Stack<Matricies>();

        #region Public Methods

        public void TranslateWorldview(Vector3 translation)
        {
            matricies.worldview  *= Matrix4.CreateTranslation(translation);
            //matricies.modelview *= Matrix4.CreateTranslation(translation);
        }

        public void RotateWorldview(Vector4 rotation, Vector3 centerOfRotation)
        {

        }

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

        #endregion
    }
}
