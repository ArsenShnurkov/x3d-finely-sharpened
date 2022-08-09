using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using GraphDebugger.OpenGL;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;

public class GfxTextNode2D
{
    private Bitmap bmp;

    private Rectangle boundingbox;
    internal MatrixCollection gfx_model;

    public Vector3 Position;
    public Vector3 Scale;
    private TextureTarget target;
    private string text;

    private int text_texture_id;

    //TODO: ensure generated bitmaps with transparent backgrounds applied actually work in opengl scene
    public GfxTextNode2D()
    {
        gfx_model = MatrixCollection.CreateInitialMatricies();

        Scale = Vector3.One;
        Position = Vector3.Zero;
        Offset = Vector2.Zero;
        text = string.Empty;

        Font = new Font("Times New Roman", 12.0f);
        ForeColor = Color.Yellow;
        BackColor = Color.Transparent;
    }

    public int Width { get; set; }
    public int Height { get; set; }

    public Vector2 Offset { get; set; }
    public Font Font { get; set; }
    public Color ForeColor { get; set; }
    public Color BackColor { get; set; }

    public string Text
    {
        get => text;
        set
        {
            if (value != text)
            {
                text = value;
                UpdateString();
            }
            else
            {
                text = value;
            }
        }
    }

    public void Render()
    {
        var aspect = Height / (float)Width;
        var h = aspect * 1.0f;

        gfx_model.ApplyLocalTransformations(Position, Scale);
        gfx_model.SetMatrixUniforms();

        GL.Enable(EnableCap.PolygonStipple);
        GL.Enable(EnableCap.Texture2D);
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactorSrc.One, BlendingFactorDest.OneMinusSrcAlpha);
        GL.BindTexture(target, text_texture_id);
        GL.Begin(BeginMode.Quads);
        GL.TexCoord2(0f, 1f);
        GL.Vertex2(0f, 0f);
        GL.TexCoord2(1f, 1f);
        GL.Vertex2(1.0f, 0f);
        GL.TexCoord2(1f, 0f);
        GL.Vertex2(1.0f, h);
        GL.TexCoord2(0f, 0f);
        GL.Vertex2(0f, h);
        GL.End();
        GL.BindTexture(target, 0);
    }

    public void Refresh()
    {
        UpdateString();
    }

    private void Init()
    {
        target = TextureTarget.Texture2D;

        bmp = new Bitmap(boundingbox.Width, boundingbox.Height);


        text_texture_id = GL.GenTexture();

        GL.BindTexture(target, text_texture_id);
        GL.TexParameter(target, TextureParameterName.TextureMagFilter, (int)All.Linear);
        GL.TexParameter(target, TextureParameterName.TextureMinFilter, (int)All.Linear);
        GL.TexImage2D(target, 0, PixelInternalFormat.Rgba, boundingbox.Width, boundingbox.Height, 0,
            PixelFormat.Bgra, PixelType.UnsignedByte, IntPtr.Zero);
    }

    /// <summary>
    ///     Upload the bitmap to OpenGL
    /// </summary>
    private void BufferImage()
    {
        BitmapData data;

        data = bmp.LockBits(boundingbox,
            ImageLockMode.ReadOnly,
            System.Drawing.Imaging.PixelFormat.Format32bppArgb);

        GL.TexImage2D(target, 0, PixelInternalFormat.Rgba, boundingbox.Width, boundingbox.Height, 0,
            PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

        bmp.UnlockBits(data);
    }

    private void CalculateBoundingBox()
    {
        var bmp = new Bitmap(1, 1);
        SizeF size;

        using (var g2D = Graphics.FromImage(bmp))
        {
            size = g2D.MeasureString(text, Font);
        }

        Width = (int)size.Width;
        Height = (int)size.Height;

        boundingbox = new Rectangle(0, 0, Width, Height);
    }

    private void UpdateString()
    {
        Brush b;
        PointF p;

        CalculateBoundingBox();

        Init();

        p = new PointF(Offset.X, Offset.Y);
        b = new LinearGradientBrush(boundingbox, ForeColor, ForeColor, LinearGradientMode.Horizontal);

        using (var g2D = Graphics.FromImage(bmp))
        {
            g2D.Clear(BackColor);
            g2D.DrawString(text, Font, b, p);
        }

        BufferImage();
    }
}