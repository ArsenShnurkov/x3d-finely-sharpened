using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using X3D.Parser;
using System.Xml.Serialization;
using OpenTK.Input;
using X3D.Engine.Shading;
using X3D.Engine.Shading.DefaultUniforms;

namespace X3D
{
    /// <summary>
    /// http://www.web3d.org/documents/specifications/19775-1/V3.3/Part01/components/geometry3D.html#ElevationGrid
    /// </summary>
    public partial class ElevationGrid
    {
        private int _vbo_interleaved;
        private int NumVerticies;
        private Shape parentShape;
        private List<Vertex> geometry = new List<Vertex>();

        private bool RGBA = false, RGB = false, coloring = false, texturing = false, generateColorMap = false;

        [XmlIgnore]
        public float[,] HeightMap;
        [XmlIgnore]
        public Vector4[,] ColorMap;
        [XmlIgnore]
        private float MaxHeight;
        [XmlIgnore]
        private float MinHeight;
        [XmlIgnore]
        private int mapSize;

        private float TessLevelInner = 3;
        private float TessLevelOuter = 2;
        private TessShaderUniforms Uniforms = new TessShaderUniforms();
        private Color colorNode;
        private ColorRGBA colorRGBANode;
        private float[] color;
        private bool _isLoaded = false;

        #region Rendering Methods

        public override void PostDescendantDeserialization()
        {
            base.PostDescendantDeserialization();

            parentShape = GetParent<Shape>();
            colorNode = (Color)this.Children.FirstOrDefault(n => n.GetType() == typeof(Color));
            colorRGBANode = (ColorRGBA)this.Children.FirstOrDefault(n => n.GetType() == typeof(ColorRGBA));



            

            RGBA = colorRGBANode != null;
            RGB = colorNode != null;
            coloring = RGBA || RGB;
            generateColorMap = coloring;

            if (RGB && !RGBA)
            {
                color = Helpers.Floats(colorNode.color);
            }
            else if (RGBA && !RGB)
            {
                color = Helpers.Floats(colorRGBANode.color);
            }

            height_mapping();
            BuildElevationGeometry();

            Buffering.BufferShaderGeometry(geometry, parentShape, out _vbo_interleaved, out NumVerticies);


            GL.UseProgram(parentShape.shaderProgramHandle);

            int uniformSize = GL.GetUniformLocation(parentShape.shaderProgramHandle, "size");
            int uniformScale = GL.GetUniformLocation(parentShape.shaderProgramHandle, "scale");

            var size = new Vector3(1, 1, 1);
            var scale = new Vector3(1, 1, 1);

            GL.Uniform3(uniformSize, size);
            GL.Uniform3(uniformScale, scale);


            if (parentShape != null)
            {
                // TESSELLATION
                //parentShape.IncludeTesselationShaders(QuadTessShader.tessControlShader, QuadTessShader.tessEvalShader, string.Empty);
            }

            Uniforms.Modelview = GL.GetUniformLocation(parentShape.shaderProgramHandle, "modelview");
            Uniforms.Projection = GL.GetUniformLocation(parentShape.shaderProgramHandle, "projection");
            Uniforms.NormalMatrix = GL.GetUniformLocation(parentShape.shaderProgramHandle, "normalmatrix");
            Uniforms.LightPosition = GL.GetUniformLocation(parentShape.shaderProgramHandle, "LightPosition");
            Uniforms.AmbientMaterial = GL.GetUniformLocation(parentShape.shaderProgramHandle, "AmbientMaterial");
            Uniforms.DiffuseMaterial = GL.GetUniformLocation(parentShape.shaderProgramHandle, "DiffuseMaterial");
            Uniforms.TessLevelInner = GL.GetUniformLocation(parentShape.shaderProgramHandle, "TessLevelInner");
            Uniforms.TessLevelOuter = GL.GetUniformLocation(parentShape.shaderProgramHandle, "TessLevelOuter");

            _isLoaded = true;
        }

        public override void Load()
        {
            base.Load();
        }

        public override void Render(RenderingContext rc)
        {
            base.Render(rc);

            if (!_isLoaded) return;

            GL.UseProgram(parentShape.shaderProgramHandle);


            Matrix3 NormalMatrix = new Matrix3(rc.matricies.modelview); // NormalMatrix = M4GetUpper3x3(ModelviewMatrix);
            Vector4 lightPosition = new Vector4(0.25f, 0.25f, 1f, 0f);

            GL.UniformMatrix3(Uniforms.NormalMatrix, false, ref NormalMatrix);
            GL.Uniform1(Uniforms.TessLevelInner, TessLevelInner);
            GL.Uniform1(Uniforms.TessLevelOuter, TessLevelOuter);
            GL.Uniform3(Uniforms.LightPosition, 1, ref lightPosition.X);
            GL.Uniform3(Uniforms.AmbientMaterial, Helpers.ToVec3(OpenTK.Graphics.Color4.Aqua)); // 0.04f, 0.04f, 0.04f
            GL.Uniform3(Uniforms.DiffuseMaterial, 0.0f, 0.75f, 0.75f);

            GL.PatchParameter(PatchParameterInt.PatchVertices, 16);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo_interleaved); // InterleavedArrayFormat.T2fC4fN3fV3f
            GL.DrawArrays(PrimitiveType.Quads, 0, NumVerticies); // Patches Quads Points  Lines

            // Testing of tessellation parameters
            if (rc.Keyboard[Key.Right]) TessLevelInner++;
            if (rc.Keyboard[Key.Left]) TessLevelInner = TessLevelInner > 1 ? TessLevelInner - 1 : 1;
            if (rc.Keyboard[Key.Up]) TessLevelOuter++;
            if (rc.Keyboard[Key.Down]) TessLevelOuter = TessLevelOuter > 1 ? TessLevelOuter - 1 : 1;
        }

        private void height_mapping()
        {
            int row, col, index;
            float heightValue;

            this.HeightMap = new float[this._xDimension, this._zDimension];
            this.ColorMap = new Vector4[this._xDimension, this._zDimension];

            this.mapSize = this._xDimension * this._zDimension;

            if (this.height == null)
            {
                throw new Exception("The height[] node is null when the requested map size is " + mapSize.ToString() + " cells");
            }

            if (mapSize > this.height.Length)
            {
                throw new Exception("Number of values in height[] (" + height.Length + ") is less than the requested map size of (" + mapSize.ToString() + ")");
            }

            row = 0;
            col = 0;
            index = 0;
            this.MaxHeight = float.MinValue;
            this.MinHeight = float.MaxValue;



            while (index < mapSize)
            {
                if (row == this._xDimension)
                {
                    row = 0;
                    col++;
                }

                if (col == this._zDimension)
                {
                    col = 0;
                }

                heightValue = heights[index];
                this.HeightMap[row, col] = heightValue;

                if (RGBA)
                {
                    if (generateColorMap && (index * 4 + 2 < color.Length))
                    {
                        this.ColorMap[row, col] = new Vector4(color[index * 4], color[index * 4 + 1], color[index * 4 + 2], color[index * 4 + 3]);
                    }
                }
                else
                {
                    if (generateColorMap && (index * 3 + 2 < color.Length))
                    {
                        this.ColorMap[row, col] = new Vector4(color[index * 3], color[index * 3 + 1], color[index * 3 + 2], 1.0f);
                    }
                }


                //if (generateNormalMap && (index * 3 + 2 < normalVectors.Length))
                //{
                //    this.NormalMap[row, col] = new Normal(new float[] { normalVectors[index * 3], normalVectors[index * 3 + 1], normalVectors[index * 3 + 2] });
                //}

                //if (generateTexCoordMap && (index * 3 + 2 < texCoordinate.point.Length))
                //{
                //    this.TexCoordMap[row, col] = new float[] { texCoordinate.point[index * 3], texCoordinate.point[index * 3 + 1], texCoordinate.point[index * 3 + 2] };
                //}

                this.MaxHeight = Math.Max(MaxHeight, heightValue);
                this.MinHeight = Math.Min(MinHeight, heightValue);

                row++;
                index++;
            }
        }

        public void BuildElevationGeometry()
        {
            int row, col;
            int index;
            float gx, gz;
            int xDim, zDim;
            Vector3 p;
            Vector4 c;
            Vertex v1;
            Vertex v2;
            Vertex v3;
            Vertex v4;

            geometry = new List<Vertex>();

            row = 0;
            col = 0;
            index = 0;
            gx = 0;
            gz = 0;
            xDim = this._xDimension - 1;
            zDim = this._zDimension - 1;
            //this.coloring = false;

            for (row = 0; row < xDim; row++)
            {
                for (col = 0; col < zDim; col++)
                {
                    gx = row * xSpacing;
                    gz = col * zSpacing;

                    // Construct a grid cell with 1 Quadrilateral using the 4 verticies
                    p = new Vector3(gx, HeightMap[row, col], gz);
                    v1 = new Vertex()
                    {
                        Position = p
                    };

                    p = new Vector3(gx, HeightMap[row, col + 1], gz + zSpacing);
                    v2 = new Vertex()
                    {
                        Position = p
                    };

                    p = new Vector3(gx + xSpacing, HeightMap[row + 1, col + 1], gz + zSpacing);
                    v3 = new Vertex()
                    {
                        Position = p
                    };

                    p = new Vector3(gx + xSpacing, HeightMap[row + 1, col], gz);
                    v4 = new Vertex()
                    {
                        Position = p
                    };
                    
                    // COLORING
                    if (this.coloring)
                    {
                        int collength;
                        if (this.RGBA)
                        {
                            collength = 4;
                        }
                        else
                        {
                            collength = 3;
                        }

                        if (color.Length / collength >= xDim * zDim)
                        {
                            /* Assume that there are 3 or 4 colors per grid cell, 
                                * it is also valid to assume that colorPerVertex may be false */

                            // Auto generate colors based on height value?

                            if (colorPerVertex)
                            {
                                c = new Vector4(this.ColorMap[row, col]);
                                v1.Color = c;

                                c = new Vector4(this.ColorMap[row, col + 1]);
                                v2.Color = c;

                                c = new Vector4(this.ColorMap[row + 1, col + 1]);
                                v3.Color = c;

                                c = new Vector4(this.ColorMap[row + 1, col]);
                                v4.Color = c;
                            }
                            else
                            {
                                // Color per face
                                if (RGB)
                                {
                                    if (index * 3 + 2 < color.Length)
                                    {
                                        /* There are 4 verticies in the quadrilateral that must have the same color */
                                        c = new Vector4(color[index * 3], color[index * 3 + 1], color[index * 3 + 2], 1.0f);
                                        v1.Color = v2.Color = v3.Color = v4.Color = c;
                                    }
                                }
                                else if(RGBA)
                                {
                                    if (index * 4 + 2 < color.Length)
                                    {
                                        /* There are 4 verticies in the quadrilateral that must have the same color */
                                        c = new Vector4(color[index * 4], color[index * 4 + 1], color[index * 4 + 2], color[index * 4 + 3]);
                                        v1.Color = v2.Color = v3.Color = v4.Color = c;
                                    }
                                }
                            }
                        }
                    }

                    geometry.Add(v1);
                    geometry.Add(v2);
                    geometry.Add(v3);
                    geometry.Add(v4);

                    index++;
                }
            }


        }

        #endregion
    }
}
