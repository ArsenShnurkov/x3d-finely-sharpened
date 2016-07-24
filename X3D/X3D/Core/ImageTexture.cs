using OpenTK.Graphics.OpenGL4;
using OpenTK;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using X3D.Engine;
using X3D.Parser;

//TODO: later diverge texturing functionality slightly when handling MultiTexturing and Texture3D
//TODO: Refactor. Keep all texturing related functionality coupled together.
//TODO: Dont forget about the ImageTexture base classes and TextureProperties

namespace X3D
{
    public enum InternalImageType
    {
        Bitmap,
        WindowsHandle
    }

    public partial class ImageTexture : IDisposable
    {
        #region Public Static Methods

        public static ImageTexture CreateTextureFromImage(Bitmap image, Rectangle boundingbox)
        {
            ImageTexture texture = new ImageTexture();

            texture._type = InternalImageType.Bitmap;
            texture.Width = boundingbox.Width;
            texture.Height = boundingbox.Height;

            //texture.Index = GL.GenTexture();
            //GL.BindTexture(TextureTarget.Texture2D, texture.Index);
            //GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Linear);
            //GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Linear);
            //GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, boundingbox.Width, boundingbox.Height, 0,
            //    OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, PixelType.UnsignedByte, IntPtr.Zero);

            texture.pixelData = image.LockBits(boundingbox,
                    System.Drawing.Imaging.ImageLockMode.ReadOnly,
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            texture.image = image;

            // Upload done later by system automatically
            //texture.LoadTexture();

            // Append to system texture assets
            texture._textureID = SceneManager.CreateTexture(texture);

            return texture;
        }


        public static Bitmap CreateBitmap(System.Drawing.Color backgroundColor, int width, int height)
        {
            Bitmap bmp;

            bmp = new Bitmap(width, height);

            using (Graphics g2D = Graphics.FromImage(bmp))
            {
                g2D.Clear(backgroundColor);
            }


            return bmp;
        }

        public static bool GetTextureImageFromMFString(string mfstring, out Bitmap image, out int width, out int height, bool flipX = false, bool? rotCW = null)
        {
            Rectangle imgRect;

            int[] textureMaxSize;
            int glTexWidth;
            int glTexHeight;
            string[] urls;
            object resource;
            bool actually_loaded_something;

            actually_loaded_something = false;
            urls = X3DTypeConverters.GetMFString(mfstring);
            image = null;
            width = 0;
            height = 0;

            foreach (string url in urls)
            {
                if (SceneManager.FetchSingle(url, out resource))
                {
                    if (resource is Stream)
                    {
                        Stream s;

                        s = (Stream)resource;
                        image = new Bitmap(s);
                        s.Close();
                        actually_loaded_something = true;
                    }
                    else
                    {
                        throw new Exception("Resource is of unknown type, consider returning file streams instead");
                    }

                    break;
                }
            }

            if (!actually_loaded_something)
            {
                image = Properties.Resources.ErrorTexture;
            }

            if (image == null)
            {
                return false;
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
                if (image.Width < size)
                {
                    glTexWidth = size / 2;
                    break;
                }
                if (image.Width == size)
                    glTexWidth = size;

            }

            for (int size = 1; size <= textureMaxSize[0]; size *= 2)
            {
                if (image.Height < size)
                {
                    glTexHeight = size / 2;
                    break;
                }
                if (image.Height == size)
                    glTexHeight = size;
            }

            if (image.Width != glTexWidth || image.Height != glTexHeight)
            {
                /* Scale the image according to OpenGL requirements */
                Image newImage = image.GetThumbnailImage(glTexWidth, glTexHeight, null, IntPtr.Zero);

                image.Dispose();
                image = (Bitmap)newImage;
            }
            //image.RotateFlip(RotateFlipType.RotateNoneFlipY); //TODO: figure out more efficient code
            if (flipX)
            {
                image.RotateFlip(RotateFlipType.RotateNoneFlipX);
            }

            if (rotCW.HasValue)
            {
                if (rotCW.Value)
                {
                    // Clockwise by 90 degrees
                    image.RotateFlip(RotateFlipType.Rotate90FlipNone);
                }
                else
                {
                    // Counterclockwise by -90 degrees
                    image.RotateFlip(RotateFlipType.Rotate270FlipNone);
                }
            }

            /* Another way to rotate texture on draw()
            gl.MatrixMode(OpenGL.GL_TEXTURE);
            gl.LoadIdentity();
            gl.Scale(1.0f,-1.0f,1.0f);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
             */
            //}
            imgRect = new Rectangle(0, 0, image.Width, image.Height);

            width = image.Width;
            height = image.Height;

            return true;
        }

        #endregion

        private int _textureID = -1;
        private InternalImageType _type;
        private TextureTarget textureTarget = TextureTarget.Texture2D;
        private TextureUnit textureUnit = TextureUnit.Texture0; // multitexturing

        public int Width;
        public int Height;
        public int Index;
        public bool IsLoaded = false;

        // more consts can be found here: https://github.com/adobe/GLS3D/blob/master/libGLconsts.as
        public const int GL_EYE_LINEAR = 0x2400;
        public const int GL_OBJECT_LINEAR = 0x2401;
        public const int GL_SPHERE_MAP = 0x2402;
        public const int GL_NORMAL_MAP = 0x8511;
        public const int GL_REFLECTION_MAP = 0x8512;

        #region Rendering Methods

        public override void Load()
        {
            base.Load();

            if (!string.IsNullOrEmpty(url) && _textureID == -1)
            {

                this._textureID = SceneManager.CreateTexture(this);

                if (GetTextureImageFromMFString(url) == true)
                {
                    // Upload done later by system automatically
                    //this.LoadTexture();
                }
            }
        }

        public override void Render(RenderingContext rc)
        {
            base.Render(rc);

            if (this.Index != -1)
            {
                this.Bind();
            }
        }

        #endregion

        #region Public Methods

        public void LoadTexture()
        {

            this.Bind();

            //TODO: parse X3D TextureProperties node

            //  Set linear filtering mode.
            GL.TexParameter(textureTarget, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(textureTarget, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Linear);

            //GL.TexGen(TextureCoordName.S, TextureGenParameter.TextureGenMode, GL_OBJECT_LINEAR);
            //GL.TexGen(TextureCoordName.T, TextureGenParameter.TextureGenMode, GL_OBJECT_LINEAR);

            //GL.TexParameter(TextureTarget.Texture2D,TextureParameterName.TextureWrapS,(int)TextureWrapMode.Repeat);
            //GL.TexParameter(TextureTarget.Texture2D,TextureParameterName.TextureWrapT,(int)TextureWrapMode.Repeat);

            //gl.TexParameter(OpenGL.GL_TEXTURE_2D,OpenGL.GL_TEXTURE_BORDER_COLOR,new float[]{1.0f,1.0f,1.0f});
            //gl.TexParameter(OpenGL.GL_TEXTURE_2D,OpenGL.GL_TEXTURE_WRAP_S,OpenGL.GL_CLAMP_TO_EDGE);
            //gl.TexParameter(OpenGL.GL_TEXTURE_2D,OpenGL.GL_TEXTURE_WRAP_T,OpenGL.GL_CLAMP_TO_EDGE);
            //gl.TexParameter(OpenGL.GL_TEXTURE_2D,OpenGL.GL_TEXTURE_WRAP_S,OpenGL.GL_CLAMP);
            //gl.TexParameter(OpenGL.GL_TEXTURE_2D,OpenGL.GL_TEXTURE_WRAP_T,OpenGL.GL_CLAMP);

            //GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int)TextureEnvMode.Modulate); /*
            //GL.TexEnv(TextureEnvTarget.TextureEnv,TextureEnvParameter.TextureEnvMode,(int)TextureEnvMode.Decal);// */

            //GL.TexParameter(Target,TextureParameterName.TextureWrapS,(int)TextureWrapMode.Repeat);
            //GL.TexParameter(Target,TextureParameterName.TextureWrapT,(int)TextureWrapMode.Repeat);


            // The GL_MODULATE attribute allows you to apply effects such as lighting and coloring to your texture
            // If you do not want lighting and coloring to effect your texture 
            // and you would like to display the texture unchanged when coloring is applied replace GL_MODULATE with GL_DECAL
            this.Upload();
            this.Dispose();

            if (GL.GetError() != ErrorCode.NoError)
            {
                throw new Exception("Error loading texture ");
            }

            this.Unbind();

            IsLoaded = true;
        }

        public void Bind()
        {
            //GL.ActiveTexture(textureUnit);
            //textureUnit = TextureUnit.Texture0 + 2 * (this.Index - 1);
            //GL.ActiveTexture(textureUnit);

            if (!GL.IsEnabled(EnableCap.Texture2D))
            {
                GL.Enable(EnableCap.Texture2D);
            }

            GL.BindTexture(textureTarget, this.Index);
        }

        public void Unbind()
        {
            /* unbind the image so we dont affect something that doesnt require it */

            GL.BindTexture(textureTarget, 0);
        }

        public void Deactivate()
        {
            GL.BindTexture(textureTarget, 0);

            GL.ActiveTexture(textureUnit);

            if (GL.IsEnabled(EnableCap.Texture2D))
            {
                GL.Disable(EnableCap.Texture2D);
            }
        }

        public void FreeTexture()
        {
            GL.DeleteTexture(Index);
        }

        public void Upload()
        {

            if (_type == InternalImageType.WindowsHandle)
            {
                GL.TexImage2D(textureTarget, 0, PixelInternalFormat.Rgba, this.Width, this.Height, 0,
                    OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, PixelType.UnsignedByte, this.pTexImage);
            }
            else if (_type == InternalImageType.Bitmap)
            {

                //Rectangle boundingbox = new Rectangle(0,0, this.Width, this.Height);

                //this.pixelData = image.LockBits(boundingbox,
                //    System.Drawing.Imaging.ImageLockMode.ReadOnly,
                //    System.Drawing.Imaging.PixelFormat.Format32bppArgb);


                GL.TexImage2D(textureTarget, 0, PixelInternalFormat.Rgba, this.Width, this.Height, 0,
                    OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, PixelType.UnsignedByte, this.pixelData.Scan0);

                image.UnlockBits(this.pixelData);
            }



            //gl.TexImage2D(OpenGL.GL_TEXTURE_2D,0,(int)OpenGL.GL_RGBA,
            //Width,Height,0,OpenGL.GL_BGRA,OpenGL.GL_UNSIGNED_BYTE,pixelData.Scan0);
        }

        //public void Upload(System.Drawing.Imaging.BitmapData data)
        //{
        //    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, this.Width, this.Height, 0,
        //        OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
        //}

        #endregion

        #region Private Methods



        #endregion

        #region Windows Platform Specific

        private IntPtr pTexImage;
        private Bitmap image;
        private BitmapData pixelData;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            //image.UnlockBits(pixelData);
            image.Dispose();
            pTexImage = IntPtr.Zero;
        }

        public static implicit operator IntPtr(ImageTexture TextureImage)
        {
            return TextureImage.pTexImage;
        }

        private bool GetTextureImageFromMFString(string mfstring)
        {
            Rectangle imgRect;

            int[] textureMaxSize;
            int glTexWidth;
            int glTexHeight;
            string[] urls;
            object resource;
            bool actually_loaded_something;

            actually_loaded_something = false;
            urls = X3DTypeConverters.GetMFString(mfstring);

            foreach (string url in urls)
            {
                if (SceneManager.FetchSingle(url, out resource))
                {
                    if(resource is Stream)
                    {
                        Stream s;

                        s = (Stream)resource;
                        this.image = new Bitmap(s);
                        s.Close();
                        actually_loaded_something = true;
                    }
                    else
                    {
                        throw new Exception("Resource is of unknown type, consider returning file streams instead");
                    }

                    break;
                }
            }

            if (!actually_loaded_something)
            {
                this.image = Properties.Resources.ErrorTexture;
            }

            if (this.image == null)
            {
                return false;
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
                if (image.Width < size)
                {
                    glTexWidth = size / 2;
                    break;
                }
                if (image.Width == size)
                    glTexWidth = size;

            }

            for (int size = 1; size <= textureMaxSize[0]; size *= 2)
            {
                if (image.Height < size)
                {
                    glTexHeight = size / 2;
                    break;
                }
                if (image.Height == size)
                    glTexHeight = size;
            }

            if (image.Width != glTexWidth || image.Height != glTexHeight)
            {
                /* Scale the image according to OpenGL requirements */
                Image newImage = image.GetThumbnailImage(glTexWidth, glTexHeight, null, IntPtr.Zero);

                image.Dispose();
                image = (Bitmap)newImage;
            }

            //if(file.ToLower().EndsWith(".bmp")) {
            image.RotateFlip(RotateFlipType.RotateNoneFlipY); //TODO: figure out more efficient code

            /* Another way to rotate texture on draw()
            gl.MatrixMode(OpenGL.GL_TEXTURE);
            gl.LoadIdentity();
            gl.Scale(1.0f,-1.0f,1.0f);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
             */
            //}
            imgRect = new Rectangle(0, 0, image.Width, image.Height);
            pixelData = image.LockBits(imgRect, ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            pTexImage = pixelData.Scan0;
            Width = image.Width;
            Height = image.Height;
            _type = InternalImageType.WindowsHandle;

            return true;
        }

        private bool GetTextureImageFromMFString2(string mfstring)
        {
            Rectangle imgRect;

            int[] textureMaxSize;
            int glTexWidth;
            int glTexHeight;
            string[] urls;
            object resource;
            bool actually_loaded_something;

            if (X3DTypeConverters.IsMFString(mfstring))
            {

                actually_loaded_something = false;
                urls = X3DTypeConverters.GetMFString(mfstring);

                foreach (string url in urls)
                {
                    if (SceneManager.FetchSingle(url, out resource))
                    {
                        Stream s;

                        s = (Stream)resource;
                        this.image = new Bitmap(s);
                        s.Close();
                        actually_loaded_something = true;
                        break;
                    }
                }

                if (!actually_loaded_something)
                {
                    this.image = Properties.Resources.ErrorTexture;
                }
            }
            else
            {
                this.image = new Bitmap(mfstring);
            }

            if (this.image == null)
            {
                return false;
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
                if (image.Width < size)
                {
                    glTexWidth = size / 2;
                    break;
                }
                if (image.Width == size)
                    glTexWidth = size;

            }

            for (int size = 1; size <= textureMaxSize[0]; size *= 2)
            {
                if (image.Height < size)
                {
                    glTexHeight = size / 2;
                    break;
                }
                if (image.Height == size)
                    glTexHeight = size;
            }

            if (image.Width != glTexWidth || image.Height != glTexHeight)
            {
                /* Scale the image according to OpenGL requirements */
                Image newImage = image.GetThumbnailImage(glTexWidth, glTexHeight, null, IntPtr.Zero);

                image.Dispose();
                image = (Bitmap)newImage;
            }

            //if(file.ToLower().EndsWith(".bmp")) {
            image.RotateFlip(RotateFlipType.RotateNoneFlipY); //TODO: figure out more efficient code

            /* Another way to rotate texture on draw()
            gl.MatrixMode(OpenGL.GL_TEXTURE);
            gl.LoadIdentity();
            gl.Scale(1.0f,-1.0f,1.0f);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
             */
            //}
            imgRect = new Rectangle(0, 0, image.Width, image.Height);
            pixelData = image.LockBits(imgRect, ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            pTexImage = pixelData.Scan0;
            Width = image.Width;
            Height = image.Height;
            _type = InternalImageType.WindowsHandle;

            return true;
        }

        private bool GetTextureImageFromWindowsFileSystem(string file)
        {
            //  Try and load the bitmap. Return false on failure.
            Bitmap image = new Bitmap(file);
            if (image == null)
                return false;

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

            this.Height = image.Height;
            this.Width = image.Width;
            this.pixelData = bitmapData;
            this.image = image;
            this.pTexImage = bitmapData.Scan0;

            //  Unlock the image.
            //image.UnlockBits(bitmapData);

            //  Dispose of the image file.
            //image.Dispose();

            _type = InternalImageType.WindowsHandle;

            return true;
        }

        #endregion

    }
}
