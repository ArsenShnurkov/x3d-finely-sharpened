﻿//#define APPLY_BACKDROP // When defined, sets Background to scene backdrop

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

        private Vector3[] groundColors;
        private Vector3[] skyColors;
        private float[] groundAngles;
        private float[] skyAngles;
        private float groundDivisor;
        private float skyDivisor;
        private bool hemispheresEnabled = false;

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

        #endregion

        public override void Load()
        {
            base.Load();

            generateSkyAndGround = string.IsNullOrEmpty(frontUrl) || string.IsNullOrEmpty(backUrl)
                || string.IsNullOrEmpty(topUrl) || string.IsNullOrEmpty(bottomUrl)
                || string.IsNullOrEmpty(leftUrl) || string.IsNullOrEmpty(rightUrl);

            if (generateSkyAndGround)
            {
                // Sphere
                // interpolate colors from groundColor and skyColor over hemispheres using specified sky and ground angles
                this.groundColors = X3DTypeConverters.MFVec3f(groundColor);
                this.groundAngles = X3DTypeConverters.Floats(groundAngle);
                this.skyColors = X3DTypeConverters.MFVec3f(skyColor);
                this.skyAngles = X3DTypeConverters.Floats(skyAngle);

                groundDivisor = (1.0f / groundColors.Length) * (float)Math.PI; // how many colors divided over 90 degrees (lower hemisphere)
                skyDivisor = (1.0f / groundColors.Length) * (float)Math.PI; // how many colors divided over 90 degrees (upper hemisphere)

                hemispheresEnabled = true;
            }
            else
            {
                tex_cube = createCubeMapFromURIs();
            }


            _shape = new Shape();
            _shape.Load();


            if (hemispheresEnabled)
            {
                _shape.IncludeDefaultShader(DefaultShader.vertexShaderSource,
                            DefaultShader.fragmentShaderSource);

                _shape.CurrentShader.Use();


                List<Vertex> geometry = BuildSphereGeometryQuads(60, Vector3.Zero, 10.0f);

                Buffering.BufferShaderGeometry(geometry, _shape, out _vbo_interleaved, out NumVerticies);
            }
            else
            {
                _shape.IncludeDefaultShader(CubeMapBackgroundShader.vertexShaderSource,
                            CubeMapBackgroundShader.fragmentShaderSource);

                _shape.CurrentShader.Use();

                Buffering.Interleave(_shape, null, out _vbo_interleaved, out NumVerticies,
                    out _vbo_interleaved4, out NumVerticies4,
                    _cube.Indices, _cube.Indices, _cube.Vertices, _cube.Texcoords, _cube.Normals, null, null);
            }

        }

        public override void Render(RenderingContext rc)
        {
            base.Render(rc);
            this._shape.Render(rc);

            var size = new Vector3(1, 1, 1);
            var scale = new Vector3(1, 1, 1);

            _shape.CurrentShader.Use();
#if APPLY_BACKDROP
            _shape.CurrentShader.SetFieldValue("modelview", Matrix4.Identity);
#endif
            _shape.CurrentShader.SetFieldValue("scale", scale);
            _shape.CurrentShader.SetFieldValue("size", size);
            _shape.CurrentShader.SetFieldValue("coloringEnabled", 1);
            _shape.CurrentShader.SetFieldValue("texturingEnabled", 0);

            bool texture2d = GL.IsEnabled(EnableCap.Texture2D);

            if (!texture2d)
                GL.Enable(EnableCap.Texture2D);

#if APPLY_BACKDROP
            GL.DepthMask(false);
#endif
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.TextureCubeMap, tex_cube);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo_interleaved);
            if (hemispheresEnabled)
            {
                GL.DrawArrays(PrimitiveType.Quads, 0, NumVerticies);
            }
            else
            {
                GL.DrawArrays(PrimitiveType.Triangles, 0, NumVerticies);
            }

#if APPLY_BACKDROP
            GL.DepthMask(true);
#endif

            if (!texture2d)
                GL.Disable(EnableCap.Texture2D);
        }
        public Vector4 GetLatitudeFillColor(float latitudeRatio, int segments)
        {
            //NOT COMPLETED
            //http://www.web3d.org/documents/specifications/19775-1/V3.3/Part01/components/enveffects.html#Concepts
            Vector3[] colorPalette;
            float colorSelectionRatio;
            int colorIndex = 0;

            if (latitudeRatio < 0.5f)
            {
                colorPalette = skyColors;

                foreach(float angle in skyAngles)
                {
                    if(angle < (latitudeRatio * MathHelpers.PI2))
                    {
                        colorSelectionRatio = angle / MathHelpers.PI;
                        colorIndex = (int)Math.Round((colorPalette.Length - 1.0f) * colorSelectionRatio);
                    }
                }
            }
            else
            {
                colorPalette = groundColors;
                colorSelectionRatio = latitudeRatio;
                colorIndex = (int)Math.Round((colorPalette.Length - 1.0f) * colorSelectionRatio);
            }

            

            colorSelectionRatio = latitudeRatio;

            

            return new Vector4(colorPalette[colorIndex], 1.0f);
        }
        public List<Vertex> BuildSphereGeometryQuads(int n, Vector3 center, float radius)
        {
            List<Vertex> geometry = new List<Vertex>();

            float theta = 0.0f;
            float theta2 = 0.0f;
            float phi = 0.0f;
            float phi2 = 0.0f;
            float segments = n;

            float cosT = 0.0f;
            float cosT2 = 0.0f;
            float cosP = 0.0f;
            float cosP2 = 0.0f;

            float sinT = 0.0f;
            float sinT2 = 0.0f;
            float sinP = 0.0f;
            float sinP2 = 0.0f;
            int vertexIndex = 0;
            int expectedNumVerticies = n * n * 4;
            float latitudeRatio;

            List<Vertex> current = new List<Vertex>(4);

            for (float lat = 0; lat < segments; lat++)
            {
                latitudeRatio = (lat / segments);
                phi = (float)Math.PI * latitudeRatio;
                phi2 = (float)Math.PI * ((lat + 1.0f) / segments);

                cosP = (float)Math.Cos(phi);
                cosP2 = (float)Math.Cos(phi2);
                sinP = (float)Math.Sin(phi);
                sinP2 = (float)Math.Sin(phi2);

                Vector4 latitudeColor = GetLatitudeFillColor(latitudeRatio, n);

                for (float lon = 0; lon < segments; lon++)
                {
                    current = new List<Vertex>(4);
                    theta = MathHelpers.PI2 * (lon / segments);
                    theta2 = MathHelpers.PI2 * ((lon + 1.0f) / segments);

                    cosT = (float)Math.Cos(theta);
                    cosT2 = (float)Math.Cos(theta2);
                    sinT = (float)Math.Sin(theta);
                    sinT2 = (float)Math.Sin(theta2);


                    current.Add(
                        new Vertex()
                        {
                            Position = new Vector3(
                                cosT * sinP,
                                cosP,
                                sinT * sinP
                            ),
                            Color = latitudeColor
                        }
                    );
                    vertexIndex++;

                    current.Add(
                        new Vertex()
                        {
                            Position = new Vector3(
                                cosT * sinP2,
                                cosP2,
                                sinT * sinP2
                            ),
                            Color = latitudeColor
                        }
                    );
                    vertexIndex++;

                    current.Add(
                        new Vertex()
                        {
                            Position = new Vector3(
                                 cosT2 * sinP2,
                                 cosP2,
                                 sinT2 * sinP2
                            ),
                            Color = latitudeColor
                        }
                    );
                    vertexIndex++;

                    current.Add(
                        new Vertex()
                        {
                            Position = new Vector3(
                                 cosT2 * sinP,
                                 cosP,
                                 sinT2 * sinP
                            ),
                            Color = latitudeColor
                        }
                    );
                    vertexIndex++;

                    geometry.AddRange(current);
                }

            }

            return geometry;
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
