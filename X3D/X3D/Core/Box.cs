using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using win = System.Drawing;

namespace X3D
{

    public partial class Box
    {
        private BoxGeometry _boxGeometry = new BoxGeometry();

        #region Render Methods

        public override void Load()
        {
            base.Load();
            
            this._boxGeometry.Load();
        }

        public override void PreRender()
        {
            base.PreRender();
        }

        public override void Render(FrameEventArgs e)
        {
            base.Render(e);

            this._boxGeometry.Render();
        }

        public override void PostRender()
        {
            base.PostRender();
            
        }

        #endregion

        public void RenderShape()
        {

        }

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
                    0, 1, 2, 2, 3, 0,
                    // top face
                    3, 2, 6, 6, 7, 3,
                    // back face
                    7, 6, 5, 5, 4, 7,
                    // left face
                    4, 0, 3, 3, 7, 4,
                    // bottom face
                    0, 1, 5, 5, 4, 0,
                    // right face
                    1, 5, 6, 6, 2, 1,
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
                    ColorToRgba32(win.Color.Gold),
                    ColorToRgba32(win.Color.Gold),
                    ColorToRgba32(win.Color.DarkRed),
                    ColorToRgba32(win.Color.DarkRed),
                    ColorToRgba32(win.Color.Gold),
                    ColorToRgba32(win.Color.Gold),
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
