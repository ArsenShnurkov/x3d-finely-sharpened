using System.Collections.Generic;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using X3D.Core;
using X3D.Core.Shading;
using X3D.Core.Shading.DefaultUniforms;
using X3D.Properties;

namespace X3D.Engine
{
    public class Crosshair
    {
        private ImageTexture _crosshair;
        private ComposedShader CurrentShader;

        private int NumVerticies, vbo;
        private readonly Vector3 scale = new Vector3(0.8f, 0.8f, 0.8f);

        private readonly Vector3 size = new Vector3(0.11f, 0.13f, 0.12f);
        private ShaderUniformsPNCT uniforms = new ShaderUniformsPNCT();

        public Crosshair()
        {
            Position = new Vector3(-0.2f, -0.2f, 0f);
            Height = 1.0f;
        }

        public float Height { get; set; }
        public Vector3 Position { get; set; }

        public void Load()
        {
            Vertex v;
            var geometry = new List<Vertex>();

            v = new Vertex
            {
                Position = new Vector3(0f, 0f, 0) + Position,
                TexCoord = new Vector2(0f, 1f)
            };
            geometry.Add(v);

            v = new Vertex
            {
                Position = new Vector3(1.0f, 0f, 0) + Position,
                TexCoord = new Vector2(1f, 1f)
            };
            geometry.Add(v);

            v = new Vertex
            {
                Position = new Vector3(1.0f, Height, 0) + Position,
                TexCoord = new Vector2(1f, 0f)
            };
            geometry.Add(v);

            v = new Vertex
            {
                Position = new Vector3(0f, Height, 0) + Position,
                TexCoord = new Vector2(0f, 0f)
            };

            geometry.Add(v);


            var bmpCross = Resources.crosshair;
            var gunit = GraphicsUnit.Pixel;
            var bounds = bmpCross.GetBounds(ref gunit);
            _crosshair = ImageTexture.CreateTextureFromImage(bmpCross, bounds);

            var @default = ShaderCompiler.ApplyShader(CrosshairShader.vertexShaderSource,
                CrosshairShader.fragmentShaderSource);
            @default.Link();
            @default.Use();

            CurrentShader = @default;

            Buffering.BufferShaderGeometry(geometry, out vbo, out NumVerticies);
        }

        public void Render(RenderingContext rc)
        {
            CurrentShader.Use();
            CurrentShader.SetFieldValue("scale", scale);
            CurrentShader.SetFieldValue("size", size);

            var identity = Matrix4.Identity;
            CurrentShader.SetFieldValue("modelview", ref identity);

            GL.Enable(EnableCap.Texture2D);
            GL.DepthMask(false);
            GL.BindTexture(TextureTarget.Texture2D, _crosshair.Index);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            Buffering.ApplyBufferPointers(CurrentShader);
            GL.DrawArrays(PrimitiveType.Quads, 0, NumVerticies);
            GL.DepthMask(true);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }
    }
}