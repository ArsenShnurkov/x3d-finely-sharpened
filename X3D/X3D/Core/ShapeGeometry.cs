using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using X3D.Parser;

namespace X3D
{
    public abstract class ShapeGeometry
    {
        public int NumElements;
        private int NumVerticies;

        private int _vbo_interleaved;
        private Shape parentShape;

        #region Geometry

        public Vector3[] Vertices { get; set; }

        public Vector3[] Normals { get; set; }

        public Vector2[] Texcoords { get; set; }

        public int[] Indices { get; set; }

        public int[] Colors { get; set; }

        #endregion Geometry

        #region Rendering Methods

        public void Load(Shape parentShape)
        {
            this.parentShape = parentShape;

            Helpers.Interleave(parentShape, out _vbo_interleaved, out NumVerticies, 
                this.Indices, this.Indices, this.Vertices, this.Texcoords, this.Normals);
        }

        public void Render(RenderingContext rc)
        {
            GL.UseProgram(parentShape.shaderProgramHandle);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo_interleaved);
            GL.DrawArrays(PrimitiveType.Triangles, 0, NumVerticies);
        }

        #endregion

        /// <summary>
        /// Converts a System.Drawing.Color to a System.Int32.
        /// </summary>
        /// <param name="c">The System.Drawing.Color to convert.</param>
        /// <returns>A System.Int32 containing the R, G, B, A values of the
        /// given System.Drawing.Color in the Rbga32 format.</returns>
        public static int ColorToRgba32(System.Drawing.Color c)
        {
            return (int)((c.A << 24) | (c.B << 16) | (c.G << 8) | c.R);
        }

    }

}
