// Description: Perlin noise renderer for heightmap generation.
//    Informal:  Renders Perlin Noise using a GLSL shader program into a framebuffer and out to a GDI Bitmap.
//      Formal:  After the perlin shader has executed, the output image from the framebuffer is exported to a GDI Bitmap.
//     Authors:  The shader integrated into x3d-finely-sharpened by Gerallt Franke
//               The original source of the shader: http://www.kamend.com/2012/06/perlin-noise-and-glsl/

using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using X3D.Core;
using X3D.Core.Shading;
using X3D.Core.Shading.DefaultUniforms;
using System.Drawing;
using System.Drawing.Imaging;

namespace X3D.Engine
{
    /// <summary>
    /// Perlin noise renderer for heightmap generation
    /// </summary>
    public class PerlinNoise
    {
        public float Height { get; set; }
        public Vector3 Position { get; set; }

        public Bitmap Image { get; private set; }

        private int NumVerticies, vbo;
        private ComposedShader CurrentShader;
        private ShaderUniformsPNCT uniforms = new ShaderUniformsPNCT();

        /// <summary>
        /// Pseudo random seed generator for noise function.
        /// </summary>
        private static Random seed = new Random();

        public delegate void RenderBlock();

        public PerlinNoise()
        {
            this.Position = new Vector3(-1f, -1f, 0f);
            this.Height = 2.0f;
        }

        public Bitmap GetPerlinNoise(RenderingContext rc)
        {
            GL.BindTexture(TextureTarget.Texture2D, 0);

            Bitmap b = TakeRenderingContextScreenshot(0, 0, 800, 600, () =>
            {

                GL.Enable(EnableCap.Texture2D);

                Vector3 size = new Vector3(1, 1, 1);
                Vector3 scale = new Vector3(1, 1, 1);

                //GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
                CurrentShader.Use();
                CurrentShader.SetFieldValue("scale", scale);
                CurrentShader.SetFieldValue("size", size);
                CurrentShader.SetFieldValue("seed", (float)seed.NextDouble()); //  (float)rc.Time * 0.001f

                var identity = Matrix4.Identity;
                CurrentShader.SetFieldValue("modelview", ref identity);

                GL.DepthMask(false);
                GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
                Buffering.ApplyBufferPointers(CurrentShader);
                GL.DrawArrays(PrimitiveType.Quads, 0, NumVerticies);
                GL.DepthMask(true);

            });

            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.UseProgram(0);

            this.Image = b;

            return b;
        }

        public void Load()
        {
            Vertex v;
            List<Vertex> geometry = new List<Vertex>();

            v = new Vertex()
            {
                Position = new Vector3(0f, 0f, 0) + Position,
                TexCoord = new Vector2(0f, 1f)
            };
            geometry.Add(v);

            v = new Vertex()
            {
                Position = new Vector3(this.Height, 0f, 0) + Position,
                TexCoord = new Vector2(1f, 1f)
            };
            geometry.Add(v);

            v = new Vertex()
            {
                Position = new Vector3(this.Height, this.Height, 0) + Position,
                TexCoord = new Vector2(1f, 0f)
            };
            geometry.Add(v);

            v = new Vertex()
            {
                Position = new Vector3(0f, this.Height, 0) + Position,
                TexCoord = new Vector2(0f, 0f)
            };

            geometry.Add(v);



            //Bitmap bmpCross = ImageTexture.CreateBitmap(System.Drawing.Color.Black, 100, 100);
            //GraphicsUnit gunit = GraphicsUnit.Pixel;
            //var bounds = bmpCross.GetBounds(ref gunit);
            //this._image = ImageTexture.CreateTextureFromImage(bmpCross, bounds);


            //var @default = ShaderCompiler.BuildDefaultShader();
            //@default.Link();
            //@default.Use();

            var @default = ShaderCompiler.ApplyShader(PerlinNoiseShader.vertexShaderSource, PerlinNoiseShader.fragmentShaderSource);
            @default.Link();
            @default.Use();

            CurrentShader = @default;

            Buffering.BufferShaderGeometry(geometry, out vbo, out NumVerticies);
        }


        public Bitmap TakeRenderingContextScreenshot(int x, int y, 
                                            int width, int height, 
                                            RenderBlock renderingBlock)
        {
            Bitmap b;
            BitmapData bits;
            Rectangle rect;

            int fboWidth = width;
            int fboHeight = height;

            uint fboHandle;
            uint colorTexture;
            uint depthRenderbuffer;

            GL.GenTextures(1, out colorTexture);
            GL.BindTexture(TextureTarget.Texture2D, colorTexture);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, fboWidth, fboHeight, 0, OpenTK.Graphics.OpenGL4.PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
            GL.BindTexture(TextureTarget.Texture2D, 0); // prevent feedback, reading and writing to the same image is a bad idea
            GL.GenRenderbuffers(1, out depthRenderbuffer);
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, depthRenderbuffer);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, (RenderbufferStorage)All.DepthComponent32, fboWidth, fboHeight);
            GL.GenFramebuffers(1, out fboHandle);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, fboHandle);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, colorTexture, 0);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, depthRenderbuffer);
            GL.DrawBuffer((DrawBufferMode)FramebufferAttachment.ColorAttachment0);

            //GL.PushAttrib(AttribMask.ViewportBit); // stores GL.Viewport() parameters
            //GL.Viewport(0, 0, fboWidth, fboHeight);

            {
                // What is executed in renderingBlock() wont be rendered on screen but in frame buffer object memory.
                renderingBlock();
            }

            //GL.ReadBuffer(ReadBufferMode.BackLeft);
            //GL.Disable(EnableCap.Texture2D);
            GL.PixelStore(PixelStoreParameter.PackAlignment, 1);
            rect = new Rectangle(0, 0, width, height);

            b = new Bitmap(width, height);
            b.MakeTransparent();
            using (Graphics g2D = Graphics.FromImage(b))
            {
                g2D.Clear(System.Drawing.Color.Black);
            }

            bits = b.LockBits(new Rectangle(0, 0, rect.Width, rect.Height),ImageLockMode.WriteOnly, 
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.ReadPixels(rect.Left, rect.Top, rect.Width, rect.Height, 
                OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, PixelType.UnsignedByte, bits.Scan0);

            b.UnlockBits(bits);


            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.DeleteFramebuffers(1, ref fboHandle);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            //b = ChangeOpacity(b, 100.0f);

            return b;
        }

        /// <summary>
        /// Takes a full screen capture.
        /// </summary>
        public Bitmap TakeScreenshot(int x, int y, int width, int height)
        {
            Rectangle rect;
            Bitmap b;
            BitmapData bits;

            GL.ReadBuffer(ReadBufferMode.BackLeft);
            GL.Disable(EnableCap.Texture2D);
            GL.PixelStore(PixelStoreParameter.PackAlignment, 1);

            rect = new Rectangle(x, y, width, height);
            b = new Bitmap(width, height);
            bits = b.LockBits(new Rectangle(0, 0, rect.Width, rect.Height), ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.ReadPixels(x, y, width, height, OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, PixelType.UnsignedByte, bits.Scan0);

            b.UnlockBits(bits);

            return b;
        }

        /// <summary>
        /// Change the opacity level of the input Bitmap
        /// </summary>
        public static Bitmap ChangeOpacity(Image img, float opacityvalue)
        {

            ImageAttributes imgAttribute;
            ColorMatrix colormatrix;
            Graphics graphics;
            Bitmap bmp;

            bmp = new Bitmap(img.Width, img.Height);
            graphics = Graphics.FromImage(bmp);
            colormatrix = new ColorMatrix();
            colormatrix.Matrix33 = opacityvalue;
            imgAttribute = new ImageAttributes();
            imgAttribute.SetColorMatrix(colormatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
            graphics.DrawImage(img, new Rectangle(0, 0, bmp.Width, bmp.Height), 0, 0, img.Width, img.Height, GraphicsUnit.Pixel, imgAttribute);
            graphics.Dispose();
             
            return bmp;
        }
    }
}
