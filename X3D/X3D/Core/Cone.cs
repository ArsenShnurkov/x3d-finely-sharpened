using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using X3D.Parser;

namespace X3D
{
    public partial class Cone
    {
        #region Rendering Methods
        private int handles;
        private int verts;
        private List<List<Vertex>> geometries;

        private Vector3 StartAxis = Vector3.UnitY ; // Axis defined as a normalized vector from base to apex. UnitY by default
        private Vector3 StartPosition; // Position of apex. (the top "sharp" point of the cone)

        public override unsafe void Load()
        {
            base.Load();


            float rd = bottomRadius; // Radius of directrix.
            int n = 10; // Number of radial "slices."

            StartPosition = new Vector3(0, 0, height); // with its position of apex at y = height / 2
            Vector3 startPos = StartPosition + (-StartAxis);
            Vector3 bottomStartPosition = StartPosition + (-StartAxis * this.height);
            Vector3 bottomStartPosition2 = StartPosition + (StartAxis * this.height);
            Vector3 e0 = MathHelpers.Perp(StartAxis);
            Vector3 e1 = Vector3.Cross(e0, StartAxis);
            float angleDirectrix = 360.0f / n * MathHelpers.PI_OVER_180;
            
            List<Vector3> pts = new List<Vector3>();
            var geometry = new List<Vertex>();
            var geometryBottomFace = new List<Vertex>();

            // calculate points around directrix
            //for (int i = 0; i < n; ++i)
            //{
            //    float phi = angleDirectrix * i;
            //    Vector3 p = bottomStartPosition + (((e0 * (float)Math.Cos(phi)) + (e1 * (float)Math.Sin(phi))) * rd);

            //    pts.Add(p);
            //}

            // cone top
            //geometry.Add(new Vertex(StartPosition));
            //for (int i = 0; i < n; ++i)
            //{
            //    geometry.Add(new Vertex(pts[i], MathHelpers.uv(1.0f, 1.0f)));
            //}
            //geometry.Add(new Vertex(pts[n - 1], MathHelpers.uv(1.0f, 1.0f)));

            //// cone bottom
            //for (int i = n - 1; i >= 0; --i)
            //{
            //    geometry.Add(new Vertex(pts[i], MathHelpers.uv(1.0f * (pts[i].Y / height), 1.0f * (pts[i].X / bottomRadius))));
            //}
            //geometry.Add(new Vertex(pts[n - 1], MathHelpers.uv(1.0f * (pts[n - 1].Y / height), 1.0f * (pts[n - 1].X / bottomRadius))));


            // draw the upper part of the cone
            //geometry.Add(new Vertex(bottomStartPosition2, MathHelpers.uv(0.0f, 0.0f)));
            //for (int phi = 0; phi < n; phi++)
            //{
            //    Vector3 p = ((e0 * (float)Math.Cos(phi)) + (e1 * (float)Math.Sin(phi))) * bottomRadius;

            //    //Vector3 pos = new Vector3((float)e0 * Math.Sin(angle) * bottomRadius, (float)Math.Cos(angle) * bottomRadius, 0);

            //    geometry.Add(new Vertex(p, MathHelpers.uv(1.0f, 1.0f)));
            //}

            // draw the base of the cone
            geometry.Add(new Vertex(bottomStartPosition2, MathHelpers.uv(0.0f, 0.0f)));
            for (int phi = 0; phi < n; phi++)
            {

                Vector3 p = bottomStartPosition + (((e0 * (float)Math.Cos(phi)) + (e1 * (float)Math.Sin(phi))) * bottomRadius);

                geometry.Add(new Vertex(p, MathHelpers.uv(1.0f * (p.Y / height), 1.0f * (p.X / bottomRadius))));
            }

            //geometry.Add(new Vertex(startPos, MathHelpers.uv(0.0f, 0.0f)));
            //for (int i = 0; i < n; i++)
            //{
            //    float phi = angleDirectrix * i;
            //    Vector3 p = bottomStartPosition + (((e0 * (float)Math.Cos(phi)) + (e1 * (float)Math.Sin(phi))) * bottomRadius);

            //    geometry.Add(new Vertex(p, MathHelpers.uv(1.0f * (p.Y / height), 1.0f * (p.X / bottomRadius))));
            //}

            geometries = new List<List<Vertex>>();
            geometries.Add(geometry);

            Helpers.BufferShaderGeometry(geometry, out handles, out verts);
        }

        public override unsafe void Render(RenderingContext rc)
        {
            base.Render(rc);

            bool bCull = !this.solid;

            if (bCull)
            {
                //GL.FrontFace(FrontFaceDirection.Ccw);
                //GL.Enable(EnableCap.CullFace);
            }
            else
            {
                //GL.Disable(EnableCap.CullFace);
            }

            GL.UseProgram(Shape.shaderProgramHandle);
            int uniformSize = GL.GetUniformLocation(Shape.shaderProgramHandle, "size");
            int uniformScale = GL.GetUniformLocation(Shape.shaderProgramHandle, "scale");
            var size = new Vector3(1, 1, 1);
            var scale = new Vector3(0.7f, 0.7f, 0.7f);
            GL.Uniform3(uniformSize, size);
            GL.Uniform3(uniformScale, scale);


            GL.BindBuffer(BufferTarget.ArrayBuffer, handles);
            GL.DrawArrays(PrimitiveType.TriangleFan, 0, verts); // TriangleFan Points
            



        }

        #endregion
    }
}
