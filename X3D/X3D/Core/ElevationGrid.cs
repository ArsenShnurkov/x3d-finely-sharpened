using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Xml.Serialization;
using OpenTK;
using X3D.Core;
using X3D.Core.Shading;
using X3D.Core.Shading.DefaultUniforms;
using X3D.Engine;
using X3D.Parser;

namespace X3D
{
    /// <summary>
    ///     http://www.web3d.org/documents/specifications/19775-1/V3.3/Part01/components/geometry3D.html#ElevationGrid
    /// </summary>
    public partial class ElevationGrid
    {
        private bool _isLoaded;
        private float[] color;

        [XmlIgnore] public Vector4[,] ColorMap;

        private Color colorNode;
        private ColorRGBA colorRGBANode;
        private bool generateColorMap, generateTexCoordMap;

        [XmlIgnore] public float[,] HeightMap;

        [XmlIgnore] private int mapSize;

        [XmlIgnore] internal float MaxHeight;

        [XmlIgnore] internal float MinHeight;

        private Shape parentShape;

        private bool RGBA, RGB, coloring, texturing;
        private TextureCoordinate texCoordinateNode;

        [XmlIgnore] public Vector2[,] TexCoordMap;

        private TessShaderUniforms Uniforms = new TessShaderUniforms();

        [XmlAttributeAttribute]
        [DefaultValue(true)]
        public bool construct { get; set; }

        public override void CollectGeometry(
            RenderingContext rc,
            out GeometryHandle handle,
            out BoundingBox bbox,
            out bool Coloring,
            out bool Texturing)
        {
            List<Vertex> geometry;


            handle = GeometryHandle.Zero;
            bbox = BoundingBox.Zero;


            parentShape = GetParent<Shape>();
            colorNode = (Color)Children.FirstOrDefault(n => n.GetType() == typeof(Color));
            colorRGBANode = (ColorRGBA)Children.FirstOrDefault(n => n.GetType() == typeof(ColorRGBA));
            texCoordinateNode =
                (TextureCoordinate)Children.FirstOrDefault(n => n.GetType() == typeof(TextureCoordinate));

            RGBA = colorRGBANode != null;
            RGB = colorNode != null;
            coloring = RGBA || RGB;
            generateColorMap = coloring;

            texturing = texCoordinateNode != null || parentShape.texturingEnabled;
            generateTexCoordMap = texturing;

            Coloring = coloring;
            Texturing = texturing;

            if (RGB && !RGBA)
                color = X3DTypeConverters.Floats(colorNode.color);
            else if (RGBA && !RGB) color = X3DTypeConverters.Floats(colorRGBANode.color);

            _isLoaded = true;


            ////
            ////
            ////


            if (construct)
            {
                var sx = xSpacing;
                var sz = zSpacing;

                IConstructionSet constructionSet;
                IPerlinNoiseGenerator perlinProvider;
                Bitmap largePerlinImage;

                constructionSet = SceneManager.GetCurrentConstructionSet();
                perlinProvider = constructionSet.GetPerlinNoiseProvider();

                var generated = constructionSet.ElevationBuilder.BuildHeightmapFromGenerator(
                    rc,
                    perlinProvider,
                    out largePerlinImage, 40, 40, 20, 20, 20); // build a rather large height map

                largePerlinImage.Dispose();

                var genColorNode = (Color)generated.Children.FirstOrDefault(n => n.GetType() == typeof(Color));
                RGB = coloring = generateColorMap = true;
                height = generated.height;
                color = X3DTypeConverters.Floats(genColorNode.color);
                colorPerVertex = generated.colorPerVertex;
                normalPerVertex = generated.normalPerVertex;
                Children = generated.Children;
                xSpacing = generated.xSpacing;
                zSpacing = generated.zSpacing;
                xDimension = generated.xDimension;
                zDimension = generated.zDimension;
            }


            if (!_isLoaded)
            {
                parentShape = GetParent<Shape>();

                texturing = parentShape.texturingEnabled;
                generateTexCoordMap = texturing && texCoordinateNode == null;

                _isLoaded = true;
            }


            height_mapping();

            bbox = BoundingBox.CalcBoundingBox(this);


            geometry = BuildElevationGeometryQuads(bbox);

            Buffering.BufferShaderGeometry(geometry, out handle.vbo4, out handle.NumVerticies4);
        }

        #region Rendering Methods

        private void height_mapping()
        {
            int row, col, index;
            float heightValue;

            HeightMap = new float[_xDimension, _zDimension];
            ColorMap = new Vector4[_xDimension, _zDimension];
            TexCoordMap = new Vector2[_xDimension, _zDimension];
            mapSize = _xDimension * _zDimension;

            if (height == null)
                throw new Exception("The height[] node is null when the requested map size is " + mapSize + " cells");

            if (mapSize > height.Length)
                throw new Exception("Number of values in height[] (" + height.Length +
                                    ") is less than the requested map size of (" + mapSize + ")");

            row = 0;
            col = 0;
            index = 0;
            MaxHeight = float.MinValue;
            MinHeight = float.MaxValue;


            while (index < mapSize)
            {
                if (row == _xDimension)
                {
                    row = 0;
                    col++;
                }

                if (col == _zDimension) col = 0;

                heightValue = heights[index];
                HeightMap[row, col] = heightValue;

                if (RGBA)
                {
                    if (generateColorMap && index * 4 + 2 < color.Length)
                        ColorMap[row, col] = new Vector4(color[index * 4], color[index * 4 + 1], color[index * 4 + 2],
                            color[index * 4 + 3]);
                }
                else
                {
                    if (generateColorMap && index * 3 + 2 < color.Length)
                        ColorMap[row, col] = new Vector4(color[index * 3], color[index * 3 + 1], color[index * 3 + 2],
                            1.0f);
                }


                //if (generateNormalMap && (index * 3 + 2 < normalVectors.Length))
                //{
                //    this.NormalMap[row, col] = new Normal(new float[] { normalVectors[index * 3], normalVectors[index * 3 + 1], normalVectors[index * 3 + 2] });
                //}

                if (generateTexCoordMap && texCoordinateNode != null && index * 2 + 2 < texCoordinateNode.point.Length)
                    TexCoordMap[row, col] = new Vector2(texCoordinateNode.point[index * 3],
                        texCoordinateNode.point[index * 3 + 1]); // texCoordinateNode.point[index * 3 + 2]

                MaxHeight = Math.Max(MaxHeight, heightValue);
                MinHeight = Math.Min(MinHeight, heightValue);

                row++;
                index++;
            }
        }

        public List<Vertex> BuildElevationGeometryQuads(BoundingBox bbox)
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
            bool auto_texcoords;

            auto_texcoords = !(texCoordinateNode != null && texCoordinateNode.point != null &&
                               texCoordinateNode.point.Length > 0);
            //auto_normals = !(this.normalVectorsNode != null && normalVectorsNode.Length > 0);

            var geometry = new List<Vertex>();

            row = 0;
            col = 0;
            index = 0;
            gx = 0;
            gz = 0;
            xDim = _xDimension - 1;
            zDim = _zDimension - 1;
            //this.coloring = false;

            for (row = 0; row < xDim; row++)
            for (col = 0; col < zDim; col++)
            {
                gx = row * xSpacing;
                gz = col * zSpacing;

                // Construct a grid cell with 1 Quadrilateral using the 4 verticies
                p = new Vector3(gx, HeightMap[row, col], gz);
                v1 = new Vertex
                {
                    Position = p
                };

                p = new Vector3(gx, HeightMap[row, col + 1], gz + zSpacing);
                v2 = new Vertex
                {
                    Position = p
                };

                p = new Vector3(gx + xSpacing, HeightMap[row + 1, col + 1], gz + zSpacing);
                v3 = new Vertex
                {
                    Position = p
                };

                p = new Vector3(gx + xSpacing, HeightMap[row + 1, col], gz);
                v4 = new Vertex
                {
                    Position = p
                };

                if (auto_texcoords)
                {
                    /* Calculate texture coordinates for the Quadrilateral based on the ElevationGrid parameters */
                    var VT = MathHelpers.uv(bbox, new[] { v1, v2, v3, v4 }, false);
                    v1 = VT[0];
                    v2 = VT[1];
                    v3 = VT[2];
                    v4 = VT[3];
                }
                else
                {
                    //TODO: collect texture coordinates
                    v1.TexCoord = TexCoordMap[row, col];
                    v2.TexCoord = TexCoordMap[row, col + 1];
                    v3.TexCoord = TexCoordMap[row + 1, col + 1];
                    v4.TexCoord = TexCoordMap[row + 1, col];
                    /* Note there is no texCoordIndex field for an ElevationGrid */
                }

                // COLORING
                if (coloring)
                {
                    int collength;
                    if (RGBA)
                        collength = 4;
                    else
                        collength = 3;

                    if (color.Length / collength >= xDim * zDim)
                    {
                        /* Assume that there are 3 or 4 colors per grid cell, 
                                * it is also valid to assume that colorPerVertex may be false */
                        // Auto generate colors based on height value?
                        if (colorPerVertex)
                        {
                            c = new Vector4(ColorMap[row, col]);
                            v1.Color = c;

                            c = new Vector4(ColorMap[row, col + 1]);
                            v2.Color = c;

                            c = new Vector4(ColorMap[row + 1, col + 1]);
                            v3.Color = c;

                            c = new Vector4(ColorMap[row + 1, col]);
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
                            else if (RGBA)
                            {
                                if (index * 4 + 2 < color.Length)
                                {
                                    /* There are 4 verticies in the quadrilateral that must have the same color */
                                    c = new Vector4(color[index * 4], color[index * 4 + 1], color[index * 4 + 2],
                                        color[index * 4 + 3]);
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

            return geometry;
        }

        #endregion
    }
}