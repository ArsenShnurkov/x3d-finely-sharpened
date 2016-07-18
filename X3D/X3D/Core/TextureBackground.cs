#define APPLY_BACKDROP // When defined, sets Background to scene backdrop
//#define CULL_FACE

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
    public partial class TextureBackground
    {
        private int tex_cube;
        private int NumVerticies, NumVerticies4;
        private int _vbo_interleaved, _vbo_interleaved4;
        private CubeGeometry _cube = new CubeGeometry();
        private Shape _shape;
        private bool generateSkyAndGround = true;

        #region Private Properties

        private string frontUrl
        {
            get
            {
                ImageTexture tex;

                tex = this.Items
                    .Where(c => typeof(ImageTexture).IsInstanceOfType(c)) 
                    .Select(c=> c as ImageTexture)
                    .FirstOrDefault(c => c.containerField == "frontTexture");

                return tex.url;
            }
        }

        private string backUrl
        {
            get
            {
                ImageTexture tex;

                tex = this.Items
                    .Where(c => typeof(ImageTexture).IsInstanceOfType(c))
                    .Select(c => c as ImageTexture)
                    .FirstOrDefault(c => c.containerField == "backTexture");

                return tex.url;
            }
        }

        private string topUrl
        {
            get
            {
                ImageTexture tex;

                tex = this.Items
                    .Where(c => typeof(ImageTexture).IsInstanceOfType(c))
                    .Select(c => c as ImageTexture)
                    .FirstOrDefault(c => c.containerField == "topTexture");

                return tex.url;
            }
        }

        private string bottomUrl
        {
            get
            {
                ImageTexture tex;

                tex = this.Items
                    .Where(c => typeof(ImageTexture).IsInstanceOfType(c))
                    .Select(c => c as ImageTexture)
                    .FirstOrDefault(c => c.containerField == "bottomTexture");

                return tex.url;
            }
        }

        private string leftUrl
        {
            get
            {
                ImageTexture tex;

                tex = this.Items
                    .Where(c => typeof(ImageTexture).IsInstanceOfType(c))
                    .Select(c => c as ImageTexture)
                    .FirstOrDefault(c => c.containerField == "leftTexture");

                return tex.url;
            }
        }

        private string rightUrl
        {
            get
            {
                ImageTexture tex;

                tex = this.Items
                    .Where(c => typeof(ImageTexture).IsInstanceOfType(c))
                    .Select(c => c as ImageTexture)
                    .FirstOrDefault(c => c.containerField == "rightTexture");

                return tex.url;
            }
        }

        #endregion

        //[XmlIgnore]
        //public ComposedShader CurrentShader = null;

        #region Private Methods

        private int createCubeMapFromURIs()
        {
            int tex_cube;

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.GenTextures(1, out tex_cube);
            GL.BindTexture(TextureTarget.TextureCubeMap, tex_cube);

            List<string> textureUrls = new List<string> { frontUrl, backUrl, topUrl, bottomUrl, leftUrl, rightUrl };

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

            if (ImageTexture.GetTextureImageFromMFString(url, out image, out width, out height, flipX: true))
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

        #endregion

        public override void Load()
        {
            base.Load();

            generateSkyAndGround = string.IsNullOrEmpty(frontUrl) || string.IsNullOrEmpty(backUrl)
                || string.IsNullOrEmpty(topUrl) || string.IsNullOrEmpty(bottomUrl)
                || string.IsNullOrEmpty(leftUrl) || string.IsNullOrEmpty(rightUrl);

            if (generateSkyAndGround)
            {

            }
            else
            {
                tex_cube = createCubeMapFromURIs();
            }


            _shape = new Shape();
            _shape.Load();
            _shape.IncludeDefaultShader(CubeMapBackgroundShader.vertexShaderSource,
                                        CubeMapBackgroundShader.fragmentShaderSource);

            Buffering.Interleave(null, out _vbo_interleaved, out NumVerticies,
                out _vbo_interleaved4, out NumVerticies4,
                _cube.Indices, _cube.Indices, _cube.Vertices, _cube.Texcoords, _cube.Normals, null, null);
        }

        public override void Render(RenderingContext rc)
        {
            base.Render(rc);
            this._shape.Render(rc);

            var size = new Vector3(1, 1, 1);
            var scale = new Vector3(1, 1, 1);

            _shape.CurrentShader.Use();
#if APPLY_BACKDROP
            Matrix4 mat4 = rc.cam.GetWorldOrientation();
            _shape.CurrentShader.SetFieldValue("modelview", ref mat4);
#endif
            _shape.CurrentShader.SetFieldValue("scale", scale);
            _shape.CurrentShader.SetFieldValue("size", size);

            bool texture2d = GL.IsEnabled(EnableCap.Texture2D);

            if (!texture2d)
                GL.Enable(EnableCap.Texture2D);

#if APPLY_BACKDROP
            GL.DepthMask(false);
#endif
#if CULL_FACE
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);
#endif
            
            //GL.FrontFace(FrontFaceDirection.Cw);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.TextureCubeMap, tex_cube);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo_interleaved);
            Buffering.ApplyBufferPointers(_shape.uniforms);
            GL.DrawArrays(PrimitiveType.Triangles, 0, NumVerticies);
#if APPLY_BACKDROP
            GL.DepthMask(true);
#endif
#if CULL_FACE
            GL.Disable(EnableCap.CullFace);
#endif
            //GL.FrontFace(FrontFaceDirection.Ccw);

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
