using OpenTK.Graphics.OpenGL;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using X3D.Engine;

//TODO: Refactor. Keep all texturing related functionality coupled together.
//TODO: Dont forget about the ImageTexture base classes and TextureProperties

namespace X3D
{
    public partial class ImageTexture
    {
        private GLTexture _texture = null;

        #region Rendering Methods

        public override void Load()
        {
            base.Load();

            string _url = url.FirstOrDefault();

            if (!string.IsNullOrEmpty(_url))
            {
                _url = _url.Replace("\"", "");
                _url = SceneManager.CurrentLocation + "\\" + _url;

                _texture = GLTexture.LoadTexture(_url);
            }
        }

        public override void Render(RenderingContext rc)
        {
            base.Render(rc);

            if (_texture != null)
            {
                _texture.Bind();
            }
        }

        #endregion

        protected class GLTexture : IDisposable
        {
            public static GLTexture LoadTexture(string file)
            {
                GLTexture texImage;

                texImage = GLTexture.GetTextureImage(file);

                if (texImage == null)
                {
                    return null;
                }

                texImage.Bind();

                //TODO: parse X3D TextureProperties node

                //  Set linear filtering mode.
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Linear);

                //GL.TexGen(TextureCoordName.S, TextureGenParameter.TextureGenMode, GL_OBJECT_LINEAR);
                //GL.TexGen(TextureCoordName.T, TextureGenParameter.TextureGenMode, GL_OBJECT_LINEAR);

                //GL.TexParameter(TextureTarget.Texture2D,TextureParameterName.TextureWrapS,(int)TextureWrapMode.Repeat);
                //GL.TexParameter(TextureTarget.Texture2D,TextureParameterName.TextureWrapT,(int)TextureWrapMode.Repeat);

                //gl.TexParameter(OpenGL.GL_TEXTURE_2D,OpenGL.GL_TEXTURE_BORDER_COLOR,new float[]{1.0f,1.0f,1.0f});
                //gl.TexParameter(OpenGL.GL_TEXTURE_2D,OpenGL.GL_TEXTURE_WRAP_S,OpenGL.GL_CLAMP_TO_EDGE);
                //gl.TexParameter(OpenGL.GL_TEXTURE_2D,OpenGL.GL_TEXTURE_WRAP_T,OpenGL.GL_CLAMP_TO_EDGE);
                //gl.TexParameter(OpenGL.GL_TEXTURE_2D,OpenGL.GL_TEXTURE_WRAP_S,OpenGL.GL_CLAMP);
                //gl.TexParameter(OpenGL.GL_TEXTURE_2D,OpenGL.GL_TEXTURE_WRAP_T,OpenGL.GL_CLAMP);

                GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int)TextureEnvMode.Modulate); /*
            GL.TexEnv(TextureEnvTarget.TextureEnv,TextureEnvParameter.TextureEnvMode,(int)TextureEnvMode.Decal);// */

                //GL.TexParameter(Target,TextureParameterName.TextureWrapS,(int)TextureWrapMode.Repeat);
                //GL.TexParameter(Target,TextureParameterName.TextureWrapT,(int)TextureWrapMode.Repeat);


                // The GL_MODULATE attribute allows you to apply effects such as lighting and coloring to your texture
                // If you do not want lighting and coloring to effect your texture 
                // and you would like to display the texture unchanged when coloring is applied replace GL_MODULATE with GL_DECAL
                texImage.Upload();
                texImage.Dispose();

                if (GL.GetError() != ErrorCode.NoError)
                {
                    throw new Exception("Error loading texture " + file);
                }

                GL.BindTexture(TextureTarget.Texture2D, 0); /* unbind the image so we dont affect something that doesnt require it */

                return texImage;
            }

            public int Width;
            public int Height;
            public int Index;

            public void Bind()
            {
                GL.ActiveTexture(TextureUnit.Texture0);

                if (!GL.IsEnabled(EnableCap.Texture2D))
                {
                    GL.Enable(EnableCap.Texture2D);
                }

                GL.BindTexture(TextureTarget.Texture2D, this.Index);
            }

            public void FreeTexture()
            {
                GL.DeleteTexture(Index);
            }

            // more consts can be found here: https://github.com/adobe/GLS3D/blob/master/libGLconsts.as
            public const int GL_EYE_LINEAR = 0x2400;
            public const int GL_OBJECT_LINEAR = 0x2401;
            public const int GL_SPHERE_MAP = 0x2402;
            public const int GL_NORMAL_MAP = 0x8511;
            public const int GL_REFLECTION_MAP = 0x8512;



            private void Upload()
            {
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
                    Width, Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, this);

                //gl.TexImage2D(OpenGL.GL_TEXTURE_2D,0,(int)OpenGL.GL_RGBA,
                //Width,Height,0,OpenGL.GL_BGRA,OpenGL.GL_UNSIGNED_BYTE,pixelData.Scan0);
            }

            #region Windows specific

            public IntPtr pTexImage;
            private Bitmap image;
            private BitmapData pixelData;

            /// <summary>
            /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            public void Dispose()
            {
                image.UnlockBits(pixelData);
                image.Dispose();
                pTexImage = IntPtr.Zero;
            }

            public static implicit operator IntPtr(GLTexture TextureImage)
            {
                return TextureImage.pTexImage;
            }

            public static GLTexture GetTextureImage(string file)
            { // If !WEBGL use this method
                GLTexture texImage;
                Rectangle imgRect;

                int[] textureMaxSize;
                int glTexWidth;
                int glTexHeight;

                texImage = new GLTexture();

                //TODO: this method might not be "port friendly to something like JSIL/webgl"

                if (SceneManager.IsMFString(file))
                {
                    string[] urls;
                    object resource;
                    bool actually_loaded_something;

                    actually_loaded_something = false;
                    urls = SceneManager.GetMFString(file);

                    foreach (string url in urls)
                    {
                        if (SceneManager.FetchSingle(url, out resource))
                        {
                            Stream s;

                            s = (Stream)resource;
                            texImage.image = new Bitmap(s);
                            s.Close();
                            actually_loaded_something = true;
                            break;
                        }
                    }

                    if (!actually_loaded_something)
                    {
                        texImage.image = Properties.Resources.ErrorTexture;
                    }
                }
                else
                {
                    texImage.image = new Bitmap(file);
                }

                if (texImage.image == null)
                {
                    return null;
                }

                /*	Get the maximum texture size supported by OpenGL: */
                textureMaxSize = new int[] { 0 };
                GL.GetInteger(GetPName.MaxTextureSize, textureMaxSize);
                //gl.GetInteger(OpenGL.GL_MAX_TEXTURE_SIZE,textureMaxSize);

                /*	Find the target width and height sizes, which is just the highest
                 *	posible power of two that'll fit into the image. */
                glTexWidth = textureMaxSize[0];
                glTexHeight = textureMaxSize[0];
                for (int size = 1; size <= textureMaxSize[0]; size *= 2)
                {
                    if (texImage.image.Width < size)
                    {
                        glTexWidth = size / 2;
                        break;
                    }
                    if (texImage.image.Width == size)
                        glTexWidth = size;

                }

                for (int size = 1; size <= textureMaxSize[0]; size *= 2)
                {
                    if (texImage.image.Height < size)
                    {
                        glTexHeight = size / 2;
                        break;
                    }
                    if (texImage.image.Height == size)
                        glTexHeight = size;
                }

                if (texImage.image.Width != glTexWidth || texImage.image.Height != glTexHeight)
                {
                    /* Scale the image according to OpenGL requirements */
                    Image newImage = texImage.image.GetThumbnailImage(glTexWidth, glTexHeight, null, IntPtr.Zero);

                    texImage.image.Dispose();
                    texImage.image = (Bitmap)newImage;
                }

                //if(file.ToLower().EndsWith(".bmp")) {
                texImage.image.RotateFlip(RotateFlipType.RotateNoneFlipY); //TODO: figure out more efficient code

                /* Another way to rotate texture on draw()
                gl.MatrixMode(OpenGL.GL_TEXTURE);
                gl.LoadIdentity();
                gl.Scale(1.0f,-1.0f,1.0f);
                gl.MatrixMode(OpenGL.GL_MODELVIEW);
                 */
                //}
                imgRect = new Rectangle(0, 0, texImage.image.Width, texImage.image.Height);
                texImage.pixelData = texImage.image.LockBits(imgRect, ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                texImage.pTexImage = texImage.pixelData.Scan0;
                texImage.Width = texImage.image.Width;
                texImage.Height = texImage.image.Height;

                return texImage;
            }

            public static GLTexture GetTextureImage2(string file)
            {
                //  Try and load the bitmap. Return false on failure.
                Bitmap image = new Bitmap(file);
                if (image == null)
                    return null;

                //	Get the maximum texture size supported by OpenGL.
                int[] textureMaxSize = { 0 };
                GL.GetInteger(GetPName.MaxTextureSize, textureMaxSize);
                //gl.GetInteger(OpenGL.GL_MAX_TEXTURE_SIZE,textureMaxSize);

                //	Find the target width and height sizes, which is just the highest
                //	posible power of two that'll fit into the image.
                int targetWidth = textureMaxSize[0];
                int targetHeight = textureMaxSize[0];

                for (int size = 1; size <= textureMaxSize[0]; size *= 2)
                {
                    if (image.Width < size)
                    {
                        targetWidth = size / 2;
                        break;
                    }
                    if (image.Width == size)
                        targetWidth = size;

                }

                for (int size = 1; size <= textureMaxSize[0]; size *= 2)
                {
                    if (image.Height < size)
                    {
                        targetHeight = size / 2;
                        break;
                    }
                    if (image.Height == size)
                        targetHeight = size;
                }

                //  If need to scale, do so now.
                if (image.Width != targetWidth || image.Height != targetHeight)
                {
                    //  Resize the image.
                    Image newImage = image.GetThumbnailImage(targetWidth, targetHeight, null, IntPtr.Zero);

                    //  Destory the old image, and reset.
                    image.Dispose();
                    image = (Bitmap)newImage;
                }

                //  Lock the image bits (so that we can pass them to OGL).
                BitmapData bitmapData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height),
                    ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);


                //	Bind our texture object (make it the current texture).
                //this.Bind();

                //  Set the image data.
                //gl.TexImage2D(OpenGL.GL_TEXTURE_2D,0,(int)OpenGL.GL_RGBA,
                //    width,height,0,OpenGL.GL_BGRA,OpenGL.GL_UNSIGNED_BYTE,
                //    bitmapData.Scan0);

                GLTexture glt = new GLTexture();
                glt.Height = image.Height;
                glt.Width = image.Width;
                glt.pixelData = bitmapData;
                glt.image = image;
                glt.pTexImage = bitmapData.Scan0;

                //  Unlock the image.
                //image.UnlockBits(bitmapData);

                //  Dispose of the image file.
                //image.Dispose();


                return glt;
            }

            #endregion
        }
    }
}
