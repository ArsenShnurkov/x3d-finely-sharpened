using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using X3D.Parser;
using X3D.Core.Shading;

namespace X3D
{
    public partial class Cylinder
    {
        #region Rendering Methods

        private int vbo;
        private int verts;
        private Shape parentShape;


        public override void Load()
        {
            base.Load();

            List<Vertex> geometry = new List<Vertex>();
            Vector3 p, a, b, c;
            Vector2 uv;
            int numVertices = 80;
            float phi = 0;
            float angleSegment = (2.0f * (float)Math.PI) / (numVertices);
            float geoScale = 0.065f;
            float geoHeight = this.height * geoScale;
            float xRadius = this.radius * geoScale;
            float yRadius = this.radius * geoScale;

            // Shaft
            for (int j = 0; j <= numVertices + 1; j++)
            {
                a = new Vector3((float)Math.Cos(phi), 0, 0);
                b = new Vector3(0, 0, (float)Math.Sin(phi));
                c = new Vector3(0, j % 2 == 0 ? geoHeight : -geoHeight, 0);

                p = (a * xRadius + b * yRadius) + c;
                uv = MathHelpers.uv((p.X / geoHeight) * 1.0f, (p.Y / radius) * 1.0f);

                geometry.Add(new Vertex(p, uv)); 

                phi += angleSegment;
            }

            if (this.top)
            {

            }

            if (this.bottom)
            {

            }

            parentShape = GetParent<Shape>();

            Buffering.BufferShaderGeometry(geometry, parentShape, out vbo, out verts);
        }

        public override void Render(RenderingContext rc)
        {
            base.Render(rc);


            GL.UseProgram(parentShape.CurrentShader.ShaderHandle);
            int uniformSize = GL.GetUniformLocation(parentShape.CurrentShader.ShaderHandle, "size");
            int uniformScale = GL.GetUniformLocation(parentShape.CurrentShader.ShaderHandle, "scale");
            var size = new Vector3(1, 1, 1);
            var scale = new Vector3(1.0f, 1.0f, 1.0f);
            GL.Uniform3(uniformSize, size);
            GL.Uniform3(uniformScale, scale);


            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.DrawArrays(PrimitiveType.TriangleStrip, 0, verts);
        }

        #endregion
    }
}
