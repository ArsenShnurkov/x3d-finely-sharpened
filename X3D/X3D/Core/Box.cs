using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using win = System.Drawing;
using X3D.Parser;

namespace X3D
{
    /// <summary>
    /// 
    /// </summary>
    public partial class Box
    {
        private BoxGeometry _boxGeometry = new BoxGeometry();
        private Shape parentShape;

        #region Render Methods

        public override void Load()
        {
            base.Load();

            parentShape = GetParent<Shape>();

            this._boxGeometry.Load(parentShape);
        }

        public override void PreRender()
        {
            base.PreRender();
        }

        public override void Render(RenderingContext rc)
        {
            base.Render(rc);

            Vector3 zeroish = new Vector3(0.05f, 0.05f, 0.05f);

            GL.UseProgram(parentShape.CurrentShader.ShaderHandle);

            int uniformSize = GL.GetUniformLocation(parentShape.CurrentShader.ShaderHandle, "size");
            int uniformScale = GL.GetUniformLocation(parentShape.CurrentShader.ShaderHandle, "scale");

            GL.Uniform3(uniformSize, this._vec3);
            GL.Uniform3(uniformScale, zeroish);

            this._boxGeometry.Render(rc);
        }

        public override void PostRender(RenderingContext rc)
        {
            base.PostRender(rc);


        }

        #endregion

        #region Geometry

        public sealed class BoxGeometry : ShapeGeometry
        {
            public BoxGeometry()
            {
                Vertices = new Vector3[]
                {
                    new Vector3(-1.0f, -1.0f,  1.0f),
                    new Vector3( 1.0f, -1.0f,  1.0f),
                    new Vector3( 1.0f,  1.0f,  1.0f),
                    new Vector3(-1.0f,  1.0f,  1.0f),
                    new Vector3(-1.0f, -1.0f, -1.0f),
                    new Vector3( 1.0f, -1.0f, -1.0f),
                    new Vector3( 1.0f,  1.0f, -1.0f),
                    new Vector3(-1.0f,  1.0f, -1.0f)
                };

                Indices = new int[]
                {
                    // front face
                    0, 1, 2, -1,
                    2, 3, 0, -1,
                    // top face
                    3, 2, 6, -1,
                    6, 7, 3, -1,
                    // back face
                    7, 6, 5, -1,
                    5, 4, 7, -1,
                    // left face
                    4, 0, 3, -1,
                    3, 7, 4, -1,
                    // bottom face
                    0, 1, 5, -1,
                    5, 4, 0, -1,
                    // right face
                    1, 5, 6, -1,
                    6, 2, 1, -1,
                };

                Normals = new Vector3[]
                {
                    new Vector3(-1.0f, -1.0f,  1.0f),
                    new Vector3( 1.0f, -1.0f,  1.0f),
                    new Vector3( 1.0f,  1.0f,  1.0f),
                    new Vector3(-1.0f,  1.0f,  1.0f),
                    new Vector3(-1.0f, -1.0f, -1.0f),
                    new Vector3( 1.0f, -1.0f, -1.0f),
                    new Vector3( 1.0f,  1.0f, -1.0f),
                    new Vector3(-1.0f,  1.0f, -1.0f),
                };

                Colors = new int[]
                {
                    ColorToRgba32(win.Color.DarkRed),
                    ColorToRgba32(win.Color.DarkRed),
                    ColorToRgba32(win.Color.Green),
                    ColorToRgba32(win.Color.Green),
                    ColorToRgba32(win.Color.DarkRed),
                    ColorToRgba32(win.Color.DarkRed),
                    ColorToRgba32(win.Color.Blue),
                    ColorToRgba32(win.Color.Blue),
                };

                Texcoords = new Vector2[]
                {
                    new Vector2(0, 1),
                    new Vector2(1, 1),
                    new Vector2(1, 0),
                    new Vector2(0, 0),
                    new Vector2(0, 1),
                    new Vector2(1, 1),
                    new Vector2(1, 0),
                    new Vector2(0, 0),
                };

            }
    }



        #endregion
    }
}
