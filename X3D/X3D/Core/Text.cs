﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using X3D.Core;
using X3D.Core.Shading;
using System.Drawing.Drawing2D;
using X3D.Parser;

namespace X3D
{
    public partial class Text
    {
        private Shape parentShape;
        private FontStyle fontStyle;
        private List<Vertex> verts = new List<Vertex>();
        private int NumVerticies;
        private int vbo;

        private Dictionary<string, ImageTexture> stringTextures = new Dictionary<string, ImageTexture>();

        private int Width { get; set; }
        private int Height { get; set; }

        private Vector3 Offset { get; set; }
        private Font Font { get; set; }
        private System.Drawing.Color ForeColor { get; set; }
        private System.Drawing.Color BackColor { get; set; }

        #region Rendering Methods

        public override void Load()
        {
            base.Load();
            int line = 0;
            float newLinePositionY;
            List<Vertex> geometry = new List<Vertex>();
            Vertex v;

            parentShape = GetParent<Shape>();
            fontStyle = this.FontStyle;

            float z = 0f;
            float h = 0.5f;

            string combined = string.Empty;

            if(this._strings.Count == 0)
            {
                combined = "Abc";
            }

            ForeColor = System.Drawing.Color.FromArgb(255, 255, 255, 255);
            BackColor = System.Drawing.Color.FromArgb(0, 0, 0, 0);

            string family;
            float size;

            foreach (string text in this._strings)
            {
                family = X3DTypeConverters.removeQuotes(FontStyle.family.First());
                family = string.IsNullOrEmpty(family) ? "Times New Roman" : family;
                size = FontStyle.size * 10;
                Font = new Font(family, size);

                newLinePositionY = (line * 0.5f);


                if (line == 0)
                {
                    v = new Vertex()
                    {
                        Position = new Vector3(0f, 0f + newLinePositionY, z),
                        TexCoord = new Vector2(0f, 1f)
                    };
                    geometry.Add(v);

                    v = new Vertex()
                    {
                        Position = new Vector3(1.0f, 0f + newLinePositionY, z),
                        TexCoord = new Vector2(1f, 1f)
                    };
                    geometry.Add(v);

                    v = new Vertex()
                    {
                        Position = new Vector3(1.0f, h + newLinePositionY, z),
                        TexCoord = new Vector2(1f, 0f)
                    };
                    geometry.Add(v);

                    v = new Vertex()
                    {
                        Position = new Vector3(0f, h + newLinePositionY, z),
                        TexCoord = new Vector2(0f, 0f)
                    };
                    geometry.Add(v);
                }

                combined += text + (line < this._strings.Count ? "\n" : "");

                line++;
            }

            UpdateString(combined);

            parentShape.CurrentShader.Use();

            Buffering.BufferShaderGeometry(geometry, parentShape, out vbo, out NumVerticies);
        }

        public override void Render(RenderingContext rc)
        {
            base.Render(rc);

            var size = new Vector3(1, 1, 1);
            var scale = new Vector3(1, 1, 1);
            //var scale = new Vector3(0.04f, 0.04f, 0.04f);

            if (parentShape.ComposedShaders.Any(s => s.Linked))
            {
                if (parentShape.CurrentShader != null)
                {
                    rc.PushMatricies();

                    parentShape.CurrentShader.Use();

                    int uniformSize = GL.GetUniformLocation(parentShape.CurrentShader.ShaderHandle, "size");
                    int uniformScale = GL.GetUniformLocation(parentShape.CurrentShader.ShaderHandle, "scale");

                    var stringTexture = stringTextures.First();

                    stringTexture.Value.Render(rc);

                    GL.Uniform3(uniformSize, size);
                    GL.Uniform3(uniformScale, scale);
                    rc.matricies.Scale = scale;

                    GL.Enable(EnableCap.Blend);
                    GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);


                    //GL.ActiveTexture(TextureUnit.Texture0);
                    //parentShape.SetSampler(0);
                    GL.BindTexture(TextureTarget.Texture2D, stringTexture.Value.Index);
                    GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
                    GL.DrawArrays(PrimitiveType.Quads, 0, NumVerticies);


                    GL.Disable(EnableCap.Blend);
                    
                    stringTexture.Value.Unbind();

                    rc.PopMatricies();
                }
            }
        }

        #endregion

        #region Private Methods

        private void UpdateString(String text)
        {
            Brush b;
            PointF p;
            Rectangle boundingbox;
            Bitmap bmp;

            boundingbox = CalculateBoundingBox(text);

            bmp = new Bitmap(boundingbox.Width, boundingbox.Height);

            bmp.MakeTransparent(BackColor);

            if(FontStyle.Justify.FirstOrDefault() == "MIDDLE")
            {
                p = new PointF((float)Offset.X + (boundingbox.Width / 8.0f), (float)Offset.Y);
            }
            else
            {
                p = new PointF((float)Offset.X, (float)Offset.Y);
            }

            
            
            b = new SolidBrush(ForeColor);

            using (Graphics g2D = Graphics.FromImage(bmp))
            {
                g2D.Clear(BackColor);
                g2D.DrawString(text, Font, b, p, StringFormat.GenericDefault);
            }

            bmp.RotateFlip(RotateFlipType.RotateNoneFlipX);

            ImageTexture texture = ImageTexture.CreateTextureFromImage(bmp, boundingbox);

            if (stringTextures.ContainsKey(text))
            {
                stringTextures[text].FreeTexture();
                stringTextures[text].Dispose();

                stringTextures[text] = texture;
            }
            else
            {
                stringTextures.Add(text, texture);
            }
        }

        private Rectangle CalculateBoundingBox(String text)
        {
            var bmp = new Bitmap(1, 1);
            SizeF size;

            using (Graphics g2D = Graphics.FromImage(bmp))
            {
                size = g2D.MeasureString(text, Font);
            }

            Width = (int)(size.Width);
            Height = (int)(size.Height);

            return new Rectangle(0, 0, Width, Height);
        }

        #endregion


        //public static void _DrawText(string text, Font font, System.Drawing.Color color, Point position)
        //{// this method is currently not working
        //    float r = (float)color.R / 0xff, g = (float)color.G / 0xff, b = (float)color.B / 0xff;
        //    //GL.DrawText(position.X,position.Y,r,g,b,font.FontFamily.Name,font.Size,info);

        //    //Font fonty=new Font(FontFamily.GenericSansSerif,18.0f);
        //    OpenTK.Graphics.TextPrinter printer = new OpenTK.Graphics.TextPrinter(OpenTK.Graphics.TextQuality.High);

        //    printer.Begin();

        //    printer.Print(text, font, color);
        //    printer.End();
        //}
    }
}