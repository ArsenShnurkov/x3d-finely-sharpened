using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using win = System.Drawing;
using System.IO;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System.Xml.Serialization;
using X3D.Parser;
using X3D.Engine;
using X3D.Core;
using X3D.Core.Shading;

namespace X3D
{
    public partial class Background
    {
        private int tex_cube;
        private int NumVerticies, NumVerticies4;
        private int _vbo_interleaved, _vbo_interleaved4;
        private CubeGeometry _cube = new CubeGeometry();
        private Shape _shape;
        private bool generateSkyAndGround = true;

        //[XmlIgnore]
        //public ComposedShader CurrentShader = null;


        public int createCubeMap()
        {
            int tex_cube;

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.GenTextures(1, out tex_cube);

            List<string> textureUrls = new List<string>{ frontUrl, backUrl, topUrl, bottomUrl, leftUrl, rightUrl };

            for (int i = 0; i < 6; i++)
            {
                loadCubeMapSide(tex_cube, TextureTarget.TextureCubeMapPositiveX + i, textureUrls[i]);
            }

            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            GL.BindTexture(TextureTarget.TextureCubeMap, 0);

            return tex_cube;
        }

        private bool loadCubeMapSide(int texture, TextureTarget side_target, string url)
        {
            GL.BindTexture(TextureTarget.TextureCubeMap, texture);

            int width, height;
            win.Bitmap image;
            win.Imaging.BitmapData pixelData;
            win.Rectangle imgRect;
            IntPtr pTexImage;

            if (ImageTexture.GetTextureImageFromMFString(url, out image, out width, out height))
            {
                imgRect = new win.Rectangle(0, 0, width, height);
                pixelData = image.LockBits(imgRect, win.Imaging.ImageLockMode.ReadOnly, 
                                           System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                pTexImage = pixelData.Scan0;

                // copy image data into 'target' side of cube map
                GL.TexImage2D(side_target, 0, PixelInternalFormat.Rgba, width, height, 0,
                    PixelFormat.Bgra, PixelType.UnsignedByte, pTexImage);

                return true;
            }

            return false;
        }

        public int createCubeMapFromColors()
        {
            //int tex_cube;

            //GL.ActiveTexture(TextureUnit.Texture0);
            //GL.GenTextures(1, out tex_cube);

            //List<string> textureUrls = new List<string> { frontUrl, backUrl, topUrl, bottomUrl, leftUrl, rightUrl };

            //for (int i = 0; i < 6; i++)
            //{
            //    loadCubeMapSide(tex_cube, TextureTarget.TextureCubeMapPositiveX + i, textureUrls[i]);
            //}

            //GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Linear);
            //GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            //GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge);
            //GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            //GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            //GL.BindTexture(TextureTarget.TextureCubeMap, 0);

            //return tex_cube;
        }

        public override void Load()
        {
            base.Load();

            if(string.IsNullOrEmpty(frontUrl) || string.IsNullOrEmpty(backUrl) 
                || string.IsNullOrEmpty(topUrl) || string.IsNullOrEmpty(bottomUrl)
                || string.IsNullOrEmpty(leftUrl) || string.IsNullOrEmpty(rightUrl))
            {
                generateSkyAndGround = true;

                tex_cube = createCubeMapFromColors();

            }
            else
            {
                generateSkyAndGround = false;

                tex_cube = createCubeMap();
            }
            

            _shape = new Shape();
            _shape.Load();
            _shape.IncludeDefaultShader(CubeMapBackgroundShader.vertexShaderSource, 
                                        CubeMapBackgroundShader.fragmentShaderSource);

            Buffering.Interleave(_shape, null, out _vbo_interleaved, out NumVerticies,
                out _vbo_interleaved4, out NumVerticies4,
                _cube.Indices, _cube.Indices, _cube.Vertices, _cube.Texcoords, _cube.Normals, null, null);
        }

        public override void Render(RenderingContext rc)
        {
            base.Render(rc);
            this. _shape.Render(rc);

            var size = new Vector3(1, 1, 1);
            var scale = new Vector3(1, 1, 1);

            _shape.CurrentShader.Use();
            _shape.CurrentShader.SetFieldValue("modelview", Matrix4.Identity);

            bool texture2d = GL.IsEnabled(EnableCap.Texture2D);

            if (!texture2d)
                GL.Enable(EnableCap.Texture2D);

            GL.DepthMask(false);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.TextureCubeMap, tex_cube);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo_interleaved);
            GL.DrawArrays(PrimitiveType.Triangles, 0, NumVerticies);
            GL.DepthMask(true);

            if (!texture2d)
                GL.Disable(EnableCap.Texture2D);
        }


        public sealed class CubeGeometry : ShapeGeometry
        {
            public CubeGeometry()
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


    }
}
