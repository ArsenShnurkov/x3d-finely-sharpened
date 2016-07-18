#define APPLY_BACKDROP // When defined, sets Background to scene backdrop

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
using X3D.Core.Shading.DefaultUniforms;

namespace X3D
{
    public partial class Background
    {
        private int tex_cube;
        private int NumVerticiesCube, NumVerticiesOuter, NumVerticiesInner;
        private int _vbo_interleaved_cube, _vbo_interleaved_inner, _vbo_interleaved_outer;
        private CubeGeometry _cube = new CubeGeometry();
        private Shape _shapeOuter;
        private Shape _shapeInner;
        private Shape _shapeInnerCube;
        private Vector3 scaleSky;
        private Vector3 scaleGround;
        private Vector3 scaleCube;
        private bool generateSkyAndGround = true;
        private bool generateCube = false;

        private Vector3[] groundColors;
        private Vector3[] skyColors;
        private float[] groundAngles;
        private float[] skyAngles;
        private float groundDivisor;
        private float skyDivisor;

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

            generateCube = !(string.IsNullOrEmpty(frontUrl) || string.IsNullOrEmpty(backUrl)
                || string.IsNullOrEmpty(topUrl) || string.IsNullOrEmpty(bottomUrl)
                || string.IsNullOrEmpty(leftUrl) || string.IsNullOrEmpty(rightUrl));

            //TODO: replace cube sides that arent available with transparent sides 

            generateSkyAndGround = !(string.IsNullOrEmpty(groundColor) || string.IsNullOrEmpty(skyColor)
                || string.IsNullOrEmpty(groundAngle) || string.IsNullOrEmpty(skyAngle));


            // TODO: later render both skydome and skybox together
            // Alpha values in skybox should provide a way to see through to skydome.
            // Skycolor sphere should be slightly larger than groundcolor hemisphere
            // and finally skybox should fit and be smaller than groundcolor hemisphere.

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

                // SKYDOME
                // outer sphere (sky)

                scaleSky = Vector3.One * 6.0f; // slightly bigger than ground hemisphere
                _shapeOuter = new Shape();
                _shapeOuter.Load();
                _shapeOuter.IncludeDefaultShader(DefaultShader.vertexShaderSource,
                                                 DefaultShader.fragmentShaderSource);
                _shapeOuter.CurrentShader.Use();
                List<Vertex> geometryOuterSphere = BuildSphereGeometryQuads(60, Vector3.Zero, 1.0f);
                Buffering.BufferShaderGeometry(geometryOuterSphere, out _vbo_interleaved_outer, out NumVerticiesOuter);

                // inner hemisphere (ground)

                scaleGround = Vector3.One * 5.6f;
                _shapeInner = new Shape();
                _shapeInner.Load();
                _shapeInner.IncludeDefaultShader(DefaultShader.vertexShaderSource,
                                                 DefaultShader.fragmentShaderSource);
                List<Vertex> geometryInnerHemisphere = BuildHemisphereGeometryQuads(60, new Vector3(0, 0.0f,0), 1.0f, false);
                Buffering.BufferShaderGeometry(geometryInnerHemisphere, out _vbo_interleaved_inner, out NumVerticiesInner);
            }

            if (generateCube)
            {
                tex_cube = createCubeMapFromURIs();

                // SKYBOX
                // innermost skybox

                scaleCube = Vector3.One * 3.1f;
                _shapeInnerCube = new Shape();
                _shapeInnerCube.Load();

                _shapeInnerCube.IncludeDefaultShader(CubeMapBackgroundShader.vertexShaderSource,
                                                     CubeMapBackgroundShader.fragmentShaderSource);

                int a, b;
                Buffering.Interleave(null, out _vbo_interleaved_cube, out NumVerticiesCube,
                    out a, out b,
                    _cube.Indices, _cube.Indices, _cube.Vertices, _cube.Texcoords, _cube.Normals, null, null);
            }

        }

        public override void Render(RenderingContext rc)
        {
            base.Render(rc);

            bool texture2d;
            var size = new Vector3(1, 1, 1);
            var scale = new Vector3(1, 1, 1);

            //GL.Disable(EnableCap.DepthTest);

            if (generateSkyAndGround)
            {
                rc.PushMatricies();

                texture2d = GL.IsEnabled(EnableCap.Texture2D);

                if (texture2d)
                    GL.Disable(EnableCap.Texture2D);

                // Outer sky Sphere
                this._shapeOuter.Render(rc);

                _shapeOuter.CurrentShader.Use();

                Matrix4 mat4;

#if APPLY_BACKDROP
                mat4 = rc.cam.GetWorldOrientation();
                _shapeOuter.CurrentShader.SetFieldValue("modelview", ref mat4);
#endif
                _shapeOuter.CurrentShader.SetFieldValue("scale", scaleSky);
                _shapeOuter.CurrentShader.SetFieldValue("size", size);
                _shapeOuter.CurrentShader.SetFieldValue("coloringEnabled", 1);
                _shapeOuter.CurrentShader.SetFieldValue("texturingEnabled", 0);

#if APPLY_BACKDROP
                GL.DepthMask(false);
#endif
                GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo_interleaved_outer);
                Buffering.ApplyBufferPointers(_shapeOuter.uniforms);
                GL.DrawArrays(PrimitiveType.Quads, 0, NumVerticiesOuter);

#if APPLY_BACKDROP
                GL.DepthMask(true);
#endif
                rc.PopMatricies();


                rc.PushMatricies();

                // Inner ground Hemisphere
                this._shapeInner.Render(rc);

                _shapeInner.CurrentShader.Use();
#if APPLY_BACKDROP
                mat4 = rc.cam.GetWorldOrientation();
                _shapeInner.CurrentShader.SetFieldValue("modelview", ref mat4);
#endif
                _shapeInner.CurrentShader.SetFieldValue("scale", scaleGround);
                _shapeInner.CurrentShader.SetFieldValue("size", size);
                _shapeInner.CurrentShader.SetFieldValue("coloringEnabled", 1);
                _shapeInner.CurrentShader.SetFieldValue("texturingEnabled", 0);

#if APPLY_BACKDROP
                GL.DepthMask(false);
#endif
                GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo_interleaved_inner);
                Buffering.ApplyBufferPointers(_shapeInner.uniforms);
                GL.DrawArrays(PrimitiveType.Quads, 0, NumVerticiesInner);

#if APPLY_BACKDROP
                GL.DepthMask(true);
#endif

                if (texture2d)
                    GL.Enable(EnableCap.Texture2D);

                rc.PopMatricies();
            }

            if (generateCube)
            {
                rc.PushMatricies();

                // Inner cubemapped skybox 
                this._shapeInnerCube.Render(rc);

                _shapeInnerCube.CurrentShader.Use();
#if APPLY_BACKDROP
                Matrix4 mat4 = rc.cam.GetWorldOrientation();
                _shapeInnerCube.CurrentShader.SetFieldValue("modelview", ref mat4);
#endif
                _shapeInnerCube.CurrentShader.SetFieldValue("scale", scaleCube);
                _shapeInnerCube.CurrentShader.SetFieldValue("size", size);
                _shapeInnerCube.CurrentShader.SetFieldValue("coloringEnabled", 0);
                _shapeInnerCube.CurrentShader.SetFieldValue("texturingEnabled", 1);


                texture2d = GL.IsEnabled(EnableCap.Texture2D);

                if (!texture2d)
                    GL.Enable(EnableCap.Texture2D);

#if APPLY_BACKDROP
                GL.DepthMask(false);
#endif

                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.TextureCubeMap, tex_cube);
                GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo_interleaved_cube);
                Buffering.ApplyBufferPointers(_shapeInnerCube.uniforms);
                GL.DrawArrays(PrimitiveType.Triangles, 0, NumVerticiesCube);

#if APPLY_BACKDROP
                GL.DepthMask(true);
#endif

                if (!texture2d)
                    GL.Disable(EnableCap.Texture2D);

                rc.PopMatricies();
            }
        }

        public Vector4 GetLatitudeFillColorSky(float latitudeRatio, int segments)
        {
            //NOT COMPLETED
            //http://www.web3d.org/documents/specifications/19775-1/V3.3/Part01/components/enveffects.html#Concepts
            Vector3[] colorPalette;
            float colorSelectionRatio;
            int colorIndex = 0;

            colorPalette = skyColors;

            //if (latitudeRatio < 0.5f)
            //{
                

            //    foreach(float angle in skyAngles)
            //    {
            //        if(angle < (latitudeRatio * MathHelpers.PI2))
            //        {
            //            colorSelectionRatio = angle / MathHelpers.PI;
            //            colorIndex = (int)Math.Round((colorPalette.Length - 1.0f) * colorSelectionRatio);
            //        }
            //    }
            //}
            //else
            //{
            //    colorSelectionRatio = latitudeRatio;
            //    colorIndex = (int)Math.Round((colorPalette.Length - 1.0f) * colorSelectionRatio);
            //}

            

            colorSelectionRatio = latitudeRatio;
            colorIndex = (int)Math.Round((colorPalette.Length - 1.0f) * colorSelectionRatio);


            if (colorPalette == null || colorPalette.Length == 0)
            {
                return Vector4.One;
            }

            return new Vector4(colorPalette[colorIndex], 1.0f);
        }

        public Vector4 GetLatitudeFillColorGround(float latitudeRatio, int segments)
        {
            //NOT COMPLETED
            //http://www.web3d.org/documents/specifications/19775-1/V3.3/Part01/components/enveffects.html#Concepts
            Vector3[] colorPalette;
            float colorSelectionRatio;
            int colorIndex = 0;
            float angle;
            float sphereAngle;
            float anglePrev = 0.0f;
            float angleNext;

            colorPalette = groundColors;

            if (colorPalette == null || colorPalette.Length == 0)
            {
                return Vector4.One;
            }

            //latitudeRatio = 0.5f - latitudeRatio;
            sphereAngle = (latitudeRatio) * MathHelpers.PI;

            angleNext = groundAngles.First();

            if (sphereAngle >= 0 && sphereAngle <= angleNext)
            {
                colorIndex = 0;
            }
            else
            {
                anglePrev = angleNext;
                float[] gangles = groundAngles.Skip(1).ToArray();

                for (int i = 1; i < gangles.Length; i++)
                {
                    angle = (i > 0 ? gangles[i] : 0.0f);


                    if (sphereAngle >= anglePrev && sphereAngle <= angle)
                    {
                        colorSelectionRatio = angle / MathHelpers.PI2;
                        colorIndex = (int)Math.Round((colorPalette.Length - 1.0f) * colorSelectionRatio);
                    }

                    anglePrev = angle;
                }
            }




            return new Vector4(colorPalette[colorIndex], 1.0f);
        }


        public List<Vertex> BuildHemisphereGeometryQuads(int n, Vector3 center, float radius, bool up = false)
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

           

            for (float lat = 0; lat < segments; lat++)
            {
                latitudeRatio = (lat / segments);

                if (latitudeRatio >= 0.5) break;

                phi = (float)Math.PI * latitudeRatio;
                phi2 = (float)Math.PI * ((lat + 1.0f) / segments);

                cosP = (float)Math.Cos(phi) * radius *  (up ? 1f : -1f);
                cosP2 = (float)Math.Cos(phi2) * radius * (up ? 1f : -1f);
                sinP = (float)Math.Sin(phi) * radius;
                sinP2 = (float)Math.Sin(phi2) * radius;

                //Vector3 a = new Vector3((float)Math.Cos(phi), 0, 0);
                //Vector3 b = new Vector3(0, 0, (float)Math.Sin(phi));
                Vector3 height = new Vector3(0, 0.0f, 0) + center;

                Vector4 latitudeColor = GetLatitudeFillColorGround(latitudeRatio, n);

                for (float lon = 0; lon < segments; lon++)
                {
                    theta = MathHelpers.PI2 * (lon / segments);
                    theta2 = MathHelpers.PI2 * ((lon + 1.0f) / segments);

                    cosT = (float)Math.Cos(theta) * radius;
                    cosT2 = (float)Math.Cos(theta2) * radius;
                    sinT = (float)Math.Sin(theta) * radius;
                    sinT2 = (float)Math.Sin(theta2) * radius;


                    geometry.Add(
                        new Vertex()
                        {
                            Position = (new Vector3(
                                cosT * sinP,
                                cosP,
                                sinT * sinP
                            ))+ height,
                            Color = latitudeColor
                        }
                    );
                    vertexIndex++;

                    geometry.Add(
                        new Vertex()
                        {
                            Position = (new Vector3(
                                cosT * sinP2,
                                cosP2,
                                sinT * sinP2
                            )) + height,
                            Color = latitudeColor
                        }
                    );
                    vertexIndex++;

                    geometry.Add(
                        new Vertex()
                        {
                            Position = (new Vector3(
                                 cosT2 * sinP2,
                                 cosP2,
                                 sinT2 * sinP2
                            )) + height,
                            Color = latitudeColor
                        }
                    );
                    vertexIndex++;

                    geometry.Add(
                        new Vertex()
                        {
                            Position = (new Vector3(
                                 cosT2 * sinP,
                                 cosP,
                                 sinT2 * sinP
                            ))+height,
                            Color = latitudeColor
                        }
                    );
                    vertexIndex++;
                }

            }

            return geometry;
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

            Vector3 height = new Vector3(0, 0.0f, 0) + center;

            for (float lat = 0; lat < segments; lat++)
            {
                latitudeRatio = (lat / segments);
                phi = (float)Math.PI * latitudeRatio;
                phi2 = (float)Math.PI * ((lat + 1.0f) / segments);

                cosP = (float)Math.Cos(phi) * radius;
                cosP2 = (float)Math.Cos(phi2) * radius;
                sinP = (float)Math.Sin(phi) * radius;
                sinP2 = (float)Math.Sin(phi2) * radius;

                Vector4 latitudeColor = GetLatitudeFillColorSky(latitudeRatio, n);

                for (float lon = 0; lon < segments; lon++)
                {
                    theta = MathHelpers.PI2 * (lon / segments);
                    theta2 = MathHelpers.PI2 * ((lon + 1.0f) / segments);

                    cosT = (float)Math.Cos(theta) * radius;
                    cosT2 = (float)Math.Cos(theta2) * radius;
                    sinT = (float)Math.Sin(theta) * radius;
                    sinT2 = (float)Math.Sin(theta2) * radius;


                    geometry.Add(
                        new Vertex()
                        {
                            Position = (new Vector3(
                                cosT * sinP,
                                cosP,
                                sinT * sinP
                            )) + height,
                            Color = latitudeColor
                        }
                    );
                    vertexIndex++;

                    geometry.Add(
                        new Vertex()
                        {
                            Position = (new Vector3(
                                cosT * sinP2,
                                cosP2,
                                sinT * sinP2
                            )) + height,
                            Color = latitudeColor
                        }
                    );
                    vertexIndex++;

                    geometry.Add(
                        new Vertex()
                        {
                            Position = (new Vector3(
                                 cosT2 * sinP2,
                                 cosP2,
                                 sinT2 * sinP2
                            )) + height,
                            Color = latitudeColor
                        }
                    );
                    vertexIndex++;

                    geometry.Add(
                        new Vertex()
                        {
                            Position = (new Vector3(
                                 cosT2 * sinP,
                                 cosP,
                                 sinT2 * sinP
                            )) +height,
                            Color = latitudeColor
                        }
                    );
                    vertexIndex++;
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
