#define APPLY_BACKDROP // When defined, sets Background to scene backdrop

using System;
using System.Collections.Generic;
using System.Linq;
using win = System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using X3D.Parser;
using X3D.Core;
using X3D.Core.Shading;

namespace X3D
{
    /// <summary>
    /// http://www.web3d.org/documents/specifications/19775-1/V3.3/Part01/components/enveffects.html#Background
    /// </summary>
    public partial class Background
    {
        #region Private Fields

        private int tex_cube;
        //private int NumVerticiesCube, NumVerticiesOuter, NumVerticiesInner;
        //private int _vbo_interleaved_cube, _vbo_interleaved_inner, _vbo_interleaved_outer;
        private GeometryHandle cubeHandle, innerHandle, outerHandle;
        private CubeGeometry _cube = new CubeGeometry();
        private ComposedShader _shaderOuter;
        private ComposedShader _shaderInner;
        private ComposedShader _shaderInnerCube;
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
        private Vector3 bboxOuter, bboxInner;
        Vector3 min, max;

        private Vector3[] colors;
        private float[] angles;
        private bool skydomeComputed = false;
        private int skydomeTexture = -1;
        private Vector3[] skydomeColors;

        bool texture2d;
        Vector3 size = new Vector3(1, 1, 1);
        Vector3 scale = new Vector3(2, 2, 2);

        #endregion

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
/* 
GL_TEXTURE_CUBE_MAP_POSITIVE_X	Right
GL_TEXTURE_CUBE_MAP_NEGATIVE_X	Left
GL_TEXTURE_CUBE_MAP_POSITIVE_Y	Top
GL_TEXTURE_CUBE_MAP_NEGATIVE_Y	Bottom
GL_TEXTURE_CUBE_MAP_POSITIVE_Z	Back
GL_TEXTURE_CUBE_MAP_NEGATIVE_Z	Front    */
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

            bool? rotCW = null;

            if(side_target == TextureTarget.TextureCubeMapPositiveY) // Top
            {
                rotCW = false; // CCW
            }
            else if (side_target == TextureTarget.TextureCubeMapNegativeY) // Bottom
            {
                rotCW = true; // CW
            }

            if (ImageTexture.GetTextureImageFromMFString(url, out image, out width, out height, false, rotCW))
            {
                imgRect = new win.Rectangle(0, 0, width, height);
                pixelData = image.LockBits(imgRect, win.Imaging.ImageLockMode.ReadOnly,
                                           System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                pTexImage = pixelData.Scan0;

                // copy image data into 'target' side of cube map
                GL.TexImage2D(side_target, 0, PixelInternalFormat.Rgba, width, height, 0,
                    PixelFormat.Bgra, PixelType.UnsignedByte, pTexImage);

                image.UnlockBits(pixelData);

                return true;
            }

            return false;
        }

        #endregion

        #region Public Methods

        public override void Load()
        {
            base.Load();

            int i;
            int a, b;
            PackedGeometry _pack;

            cubeHandle = GeometryHandle.Zero;
            innerHandle = GeometryHandle.Zero;
            outerHandle = GeometryHandle.Zero;

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

                // Assign colors with matching angles
                colors = new Vector3[groundColors.Length + skyColors.Length];
                for(i=0; i < skyColors.Length; i++)
                    colors[i] = skyColors[i];
                for (i = skyColors.Length; i < skyColors.Length + groundColors.Length; i++)
                    colors[i] = groundColors[i - skyColors.Length];
                angles = new float[groundAngles.Length + skyAngles.Length + 2];
                angles[0] = 0;
                for (i = 0; i < skyAngles.Length; i++)
                    angles[i + 1] = skyAngles[i];
                angles[skyAngles.Length + 1] = 0;
                for (i = 0; i < groundAngles.Length; i++)
                    angles[i + skyAngles.Length + 2] = 1.5f + groundAngles[i];
                


                groundDivisor = (1.0f / groundColors.Length) * (float)Math.PI; // how many colors divided over 90 degrees (lower hemisphere)
                skyDivisor = (1.0f / groundColors.Length) * (float)Math.PI; // how many colors divided over 90 degrees (upper hemisphere)

                // SKYDOME
                // outer sphere (sky)

                scaleSky = Vector3.One * 6.0f; // slightly bigger than ground hemisphere
                _shaderOuter = ShaderCompiler.ApplyShader(BackgroundShader.vertexShaderSource, // Make use of the BackgroundShader for Skydome Linear Interpolation
                                                 BackgroundShader.fragmentShaderSource);
                _shaderOuter.Link();

                List<Vertex> geometryOuterSphere = BuildSphereGeometryQuads(60, Vector3.Zero, 1.0f);
                Buffering.BufferShaderGeometry(geometryOuterSphere, out outerHandle.vbo4, out outerHandle.NumVerticies4);

                min = Vector3.Zero;
                max = Vector3.Zero;
                BoundingBox.CalculateBoundingBox(geometryOuterSphere, out max, out min);
                bboxOuter = max - min;

                // inner hemisphere (ground)

                //scaleGround = Vector3.One * 5.6f;
                //_shaderInner = ShaderCompiler.ApplyShader(BackgroundShader.vertexShaderSource,
                //                                 BackgroundShader.fragmentShaderSource);
                //_shaderInner.Link();

                //List<Vertex> geometryInnerHemisphere = BuildHemisphereGeometryQuads(60, new Vector3(0, 0.0f,0), 1.0f, false);
                //Buffering.BufferShaderGeometry(geometryInnerHemisphere, out innerHandle.vbo4, out innerHandle.NumVerticies4);

                //min = Vector3.Zero;
                //max = Vector3.Zero;
                //BoundingBox.CalculateBoundingBox(geometryInnerHemisphere, out max, out min);
                //bboxInner = max - min;

                
                skydomeTexture = MakeSkydomeTexture();
            }

            if (generateCube)
            {
                tex_cube = createCubeMapFromURIs();

                // SKYBOX
                // innermost skybox

                scaleCube = Vector3.One * 3.1f;

                _shaderInnerCube = ShaderCompiler.ApplyShader(CubeMapBackgroundShader.vertexShaderSource,
                                                 CubeMapBackgroundShader.fragmentShaderSource);
                _shaderInnerCube.Link();

                _pack = new PackedGeometry();
                _pack._indices = _cube.Indices;
                _pack._coords = _cube.Vertices;
                _pack.Texturing = true;
                //_pack._colorIndicies = _boxGeometry.Colors;
                _pack._texCoords = _cube.Texcoords;
                _pack.restartIndex = -1;

                _pack.Interleave();

                // BUFFER GEOMETRY
                cubeHandle = _pack.CreateHandle();
            }

        }

        public override void Render(RenderingContext rc)
        {
            base.Render(rc);

            //GL.Disable(EnableCap.DepthTest);

            if (generateSkyAndGround)
            {
                RenderSkydome(rc);
            }

            if (generateCube)
            {
                RenderSkybox(rc);
            }
        }

        #endregion

        #region Private Methods

        private int MakeSkydomeTexture()
        {
            int i;
            int texture;
            ColorInterpolator interpolator;
            Stack<Vector3> colors;
            Stack<float> angles;    
            List<Vector3> _sky;
            int numColors;
            float fraction;

            numColors = 255;
            texture = -1;
            skydomeColors = null;
            colors = new Stack<Vector3>();
            angles = new Stack<float>();

            if (skyColors.Length > 0 || groundColors.Length > 0)
            {
                //colors.Push(skyColors.Last());
                for (i = 0; i < skyColors.Length; i++)
                {
                    colors.Push(skyColors[i]);
                }
                

                angles.Push(0);
                for (i = 0; i < skyAngles.Length; i++)
                {
                    angles.Push(skyAngles[i]);
                }
                

                if (groundAngles.Length >= 0 || groundColors.Length == 1)
                {
                    if (angles.Last() < MathHelpers.PIOver2)
                    {
                        angles.Push(MathHelpers.PIOver2 - MathHelpers.EPS);

                        colors.Push(colors.Last());
                    }

                    for (i = groundAngles.Length - 1; i >= 0; i--)
                    {
                        if ((i == groundAngles.Length - 1) && (Math.PI - groundAngles[i] <= MathHelpers.PIOver2))
                        {
                            angles.Push(MathHelpers.PIOver2);
                            colors.Push(groundColors[groundColors.Length - 1]);
                        }

                        angles.Push(MathHelpers.PI - groundAngles[i]);
                        colors.Push(groundColors[i + 1]);
                    }

                    if (groundAngles.Length == 0 && groundColors.Length == 1)
                    {
                        angles.Push(MathHelpers.PIOver2);
                        colors.Push(groundColors[0]);
                    }

                    angles.Push(MathHelpers.PI);
                    colors.Push(groundColors[0]);
                }
                else
                {
                    if (angles.Last() < Math.PI)
                    {
                        angles.Push(MathHelpers.PI);
                        colors.Push(colors.Last());
                    }
                }

                var lst = angles.ToList();
                for (i = 0; i < angles.Count; i++)
                {
                    // Normalise angles to [0, 1] to convert to fractions for interpolator
                    lst[i] /= MathHelpers.PI; 
                }
                angles = new Stack<float>(lst);

                // INTERPOLATE
                interpolator = new ColorInterpolator();
                interpolator.Keys = angles.ToArray();
                interpolator.KeyValues = colors.ToArray();

                _sky = new List<Vector3>();

                numColors = MathHelpers.ComputeNextHighestPowerOfTwo(_sky.Count);
                numColors = (numColors < 512) ? 512 : numColors;

                for (i = 0; i < numColors; i++)
                {
                    fraction = ((float)i / (float)(numColors - 1));

                    interpolator.set_fraction = fraction;

                    _sky.Add(interpolator.value_changed);
                }

                _sky.Reverse();


                //skydomeColors = _sky.ToArray();

                // Build a texture since texturing supports more colors
                byte[] pixels;
                pixels = Imaging.CreateImageRGBA(_sky.ToArray(), this.transparency);

                GL.CreateTextures(TextureTarget.Texture2D, 1, out texture);
                GL.BindTexture(TextureTarget.Texture2D, texture);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

                GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, 1, pixels.Length / 4, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pixels);
                GL.BindTexture(TextureTarget.Texture2D, 0);

                //byte[] pixelsTest;
                //pixelsTest = Imaging.CreateImageARGB(tmp, this.transparency);
                //win.Bitmap bmp = Imaging.CreateImageFromBytesARGB(pixelsTest, 1, N);
                //bmp.Save(@"D:\test.png");


                skydomeComputed = true;
            }

            return texture;
        }

        private void RenderSkydome(RenderingContext rc)
        {
            rc.PushMatricies();

            texture2d = GL.IsEnabled(EnableCap.Texture2D);

            if (texture2d)
                GL.Disable(EnableCap.Texture2D);

            Matrix4 mat4;
            // Outer sky Sphere
            //this._shapeOuter.Render(rc);

            _shaderOuter.Use();



#if APPLY_BACKDROP
            mat4 = rc.cam.GetWorldOrientation();
            _shaderOuter.SetFieldValue("modelview", ref mat4);
#endif
            //_shaderOuter.SetFieldValue("scale", scaleSky);
            _shaderOuter.SetFieldValue("scale", size);
            _shaderOuter.SetFieldValue("size", size);
            if (skydomeColors != null && skydomeColors.Length > 0) { 
                _shaderOuter.SetFieldValue("skyColors", this.skydomeColors.Length);//_shaderOuter.SetFieldValue("skyColors", this.colors.Length);
                _shaderOuter.SetFieldValue("skyColor", this.skydomeColors, 255 * 3);
            }
            //_shaderOuter.SetFieldValue("skyColor", this.colors, 255 * 3);
            _shaderOuter.SetFieldValue("skyAngle", this.angles, 255);
            
            _shaderOuter.SetFieldValue("isGround", 0);
            _shaderOuter.SetFieldValue("bbox", bboxOuter);
            _shaderOuter.SetFieldValue("max", max);
            _shaderOuter.SetFieldValue("min", min);

            _shaderOuter.SetFieldValue("projection", ref rc.matricies.projection);
            _shaderOuter.SetFieldValue("camscale", rc.cam.Scale.X);
            _shaderOuter.SetFieldValue("X3DScale", rc.matricies.Scale);

#if APPLY_BACKDROP
            GL.DepthMask(false);
#endif
            if (skydomeComputed)
            {
                GL.BindTexture(TextureTarget.Texture2D, skydomeTexture);
            }

            GL.BindBuffer(BufferTarget.ArrayBuffer, outerHandle.vbo4);
            Buffering.ApplyBufferPointers(_shaderOuter);
            GL.DrawArrays(PrimitiveType.Quads, 0, outerHandle.NumVerticies4);

            if (skydomeComputed)
            {
                GL.BindTexture(TextureTarget.Texture2D, 0);
            }

#if APPLY_BACKDROP
            GL.DepthMask(true);
#endif
            rc.PopMatricies();


            //            rc.PushMatricies();

            //            // Inner ground Hemisphere
            //            //RenderCube(rc);

            //            _shaderInner.Use();
            //#if APPLY_BACKDROP
            //            mat4 = rc.cam.GetWorldOrientation();
            //            _shaderInner.SetFieldValue("modelview", ref mat4);
            //#endif
            //            _shaderInner.SetFieldValue("scale", scaleGround);
            //            _shaderInner.SetFieldValue("size", size);
            //            _shaderOuter.SetFieldValue("skyColor", this.skydomeColors, 255 * 3);
            //            //_shaderInner.SetFieldValue("skyColor", this.colors, 255 * 3);
            //            _shaderInner.SetFieldValue("skyAngle", this.angles, 255);
            //            _shaderInner.SetFieldValue("skyColors", this.colors.Length);
            //            _shaderInner.SetFieldValue("isGround", 1);
            //            _shaderInner.SetFieldValue("bbox", bboxInner);

            //            _shaderInner.SetFieldValue("projection", ref rc.matricies.projection);
            //            _shaderInner.SetFieldValue("camscale", rc.cam.Scale.X);
            //            _shaderInner.SetFieldValue("X3DScale", rc.matricies.Scale);

            //#if APPLY_BACKDROP
            //            GL.DepthMask(false);
            //#endif
            //            GL.BindBuffer(BufferTarget.ArrayBuffer, innerHandle.vbo4);
            //            Buffering.ApplyBufferPointers(_shaderInner);
            //            GL.DrawArrays(PrimitiveType.Quads, 0, innerHandle.NumVerticies4);

            //#if APPLY_BACKDROP
            //            GL.DepthMask(true);
            //#endif


            //            rc.PopMatricies();

            if (texture2d)
                GL.Enable(EnableCap.Texture2D);
        }

        private void RenderSkybox(RenderingContext rc)
        {
            rc.PushMatricies();

            // Inner cubemapped skybox 
            //this._shapeInnerCube.Render(rc);

            _shaderInnerCube.Use();
#if APPLY_BACKDROP
            Matrix4 mat4 = rc.cam.GetWorldOrientation();
            _shaderInnerCube.SetFieldValue("modelview", ref mat4);
#endif
            _shaderInnerCube.SetFieldValue("scale", scaleCube);
            _shaderInnerCube.SetFieldValue("size", size);
            _shaderInnerCube.SetFieldValue("coloringEnabled", 0);
            _shaderInnerCube.SetFieldValue("texturingEnabled", 1);
            _shaderInnerCube.SetFieldValue("lightingEnabled", 0);

            _shaderInnerCube.SetFieldValue("projection", ref rc.matricies.projection);
            _shaderInnerCube.SetFieldValue("camscale", rc.cam.Scale.X); 
            _shaderInnerCube.SetFieldValue("X3DScale", rc.matricies.Scale); 


            texture2d = GL.IsEnabled(EnableCap.Texture2D);

            if (!texture2d)
                GL.Enable(EnableCap.Texture2D);

#if APPLY_BACKDROP
            GL.DepthMask(false);
#endif

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.TextureCubeMap, tex_cube);
            GL.BindBuffer(BufferTarget.ArrayBuffer, cubeHandle.vbo3);
            Buffering.ApplyBufferPointers(_shaderInnerCube);
            GL.DrawArrays(PrimitiveType.Triangles, 0, cubeHandle.NumVerticies3);

#if APPLY_BACKDROP
            GL.DepthMask(true);
#endif

            if (!texture2d)
                GL.Disable(EnableCap.Texture2D);

            rc.PopMatricies();
        }

        private Vector4 GetLatitudeFillColorSky(float latitudeRatio, int segments)
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

        private Vector4 GetLatitudeFillColorGround(float latitudeRatio, int segments)
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


        private List<Vertex> BuildHemisphereGeometryQuads(int n, Vector3 center, float radius, bool up = false)
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

        private List<Vertex> BuildSphereGeometryQuads(int n, Vector3 center, float radius)
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

        #endregion

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
