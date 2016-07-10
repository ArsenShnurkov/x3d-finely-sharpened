using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace X3D
{
    public class Camera
    {
        int Width, Height; // window viewport size
        public Matrix4 projectionMatrix = Matrix4.Identity;
        public Matrix4 cameraViewMatrix = Matrix4.Identity;

        public Vector3 Pos = new Vector3(0, 0, -2);
        public Vector3 Dir = new Vector3(0, 0, 1);
        public Vector3 Up = Vector3.UnitY;

        public Vector3 Scale = new Vector3(1f, 1f, 1f);

        public Camera(int viewportWidth, int viewportHeight)
        {
            viewportSize(viewportWidth, viewportHeight);
        }

        public void viewportSize(int viewportWidth, int viewportHeight)
        {
            this.Width = viewportWidth;
            this.Height = viewportHeight;
            float aspectRatio = Width / (float)Height;

            projectionMatrix = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver4, aspectRatio, 0.01f, 1000.0f);

        }

        public void setupGLRenderMatrix()
        {
            // setup projection
            GL.Viewport(0, 0, Width, Height);
            //GL.MatrixMode(MatrixMode.Projection);
            //GL.LoadMatrix(ref projectionMatrix);

            // create and setup camera view matrix
            cameraViewMatrix = Matrix4.LookAt(Pos, Pos + Dir, Up);
            
            //GL.MatrixMode(MatrixMode.Modelview);
            //GL.LoadMatrix(ref cameraViewMatrix);
        }

        public void Dolly(float distance)
        {
            Pos += distance * Dir;
        }
        public void PanXY(float x, float y)
        {
            Pos += new Vector3(x, y, 0);
        }

        public void OrbitXY(float x, float y)
        {
            // TBD                 
            Scale.X = Scale.X + x *.02f;
            Scale.Y = Scale.Y + y * .02f;
            

        }

    }
}
