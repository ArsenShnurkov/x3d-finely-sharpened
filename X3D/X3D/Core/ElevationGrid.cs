using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using X3D.Parser;
using System.Xml.Serialization;

namespace X3D
{
    public partial class ElevationGrid
    {
        private int _vbo_interleaved;
        private int NumVerticies;
        private Shape parentShape;
        private List<Vertex> geometry = new List<Vertex>();

        private bool RGBA = false, coloring = false, texturing = false;

        [XmlIgnore]
        public float[,] HeightMap;
        private float MaxHeight;
        private float MinHeight;
        private int mapSize;


        #region Rendering Methods

        public override void Load()
        {
            base.Load();

            height_mapping();
            BuildElevationGeometry();

            parentShape = GetParent<Shape>();

            Helpers.BufferShaderGeometry(geometry, parentShape, out _vbo_interleaved, out NumVerticies);


            GL.UseProgram(parentShape.shaderProgramHandle);

            int uniformSize = GL.GetUniformLocation(parentShape.shaderProgramHandle, "size");
            int uniformScale = GL.GetUniformLocation(parentShape.shaderProgramHandle, "scale");

            var size = new Vector3(1, 1, 1);
            var scale = new Vector3(1, 1, 1);

            GL.Uniform3(uniformSize, size);
            GL.Uniform3(uniformScale, scale);
        }

        public override void Render(RenderingContext rc)
        {
            base.Render(rc);

            GL.UseProgram(parentShape.shaderProgramHandle);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo_interleaved); // InterleavedArrayFormat.T2fC4fN3fV3f
            GL.DrawArrays(PrimitiveType.Quads, 0, NumVerticies); // Quads Points  Lines
        }

        private void height_mapping()
        {
            int row, col, index;
            float heightValue;
            //bool generateColorMap;
            //bool generateNormalMap;
            //bool generateTexCoordMap;

            this.HeightMap = new float[this._xDimension, this._zDimension];
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

                //if (RGBA)
                //{
                //    if (generateColorMap && (index * 4 + 2 < color.Length))
                //    {
                //        this.ColorMap[row, col] = new Color(new float[] { color[index * 4], color[index * 4 + 1], color[index * 4 + 2], color[index * 4 + 3] });
                //    }
                //}
                //else
                //{
                //    if (generateColorMap && (index * 3 + 2 < color.Length))
                //    {
                //        this.ColorMap[row, col] = new Color(new float[] { color[index * 3], color[index * 3 + 1], color[index * 3 + 2] });
                //    }
                //}


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

            geometry = new List<Vertex>();

            row = 0;
            col = 0;
            index = 0;
            gx = 0;
            gz = 0;
            xDim = this._xDimension - 1;
            zDim = this._zDimension - 1;

            for (row = 0; row < xDim; row++)
            {
                for (col = 0; col < zDim; col++)
                {
                    gx = row * xSpacing;
                    gz = col * zSpacing;

                    /* Construct a grid cell with 2 Triangles out of the 4 verticies */

                    // Construct a grid cell with 1 Quadrilateral using the 4 verticies
                    geometry.Add(new Vertex( new Vector3(gx, HeightMap[row, col], gz)
                                /*  .    .
                                    * [.]   .   */));
                    geometry.Add(new Vertex(new Vector3(gx, HeightMap[row, col + 1], gz + zSpacing)
                                /* .
                                    * . ___ [.] */));
                    geometry.Add(new Vertex(new Vector3(gx + xSpacing, HeightMap[row + 1, col + 1], gz + zSpacing)
                                /* .  /[.]
                                    * . /  .    */));
                    geometry.Add(new Vertex(new Vector3(gx + xSpacing, HeightMap[row + 1, col], gz)
                                /* [.]   .
                                    *  .    .   */));

                    index++;
                }
            }


        }

        #endregion
    }
}
