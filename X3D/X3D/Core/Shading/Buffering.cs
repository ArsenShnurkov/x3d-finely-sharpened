//TODO: dont unpack indicies or transform them if it is not required. we want to save both time and space if at all possible.
// todo implememt optimisations; minimal unpacking/transformation of geometry.
// todo: primativeRestartIndex()


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using X3D.Core;

namespace X3D.Core.Shading
{
    using Verticies = List<Vertex>;
    using Mesh = List<List<Vertex>>; // Mesh will contain faces made up of either or Triangles, and Quads
    using DefaultUniforms;
    using Parser;

    /// <summary>
    /// Interleave all geometry for single API simplicity for now.
    /// Later include additional support for indexing geometry.
    /// </summary>
    public class Buffering
    {
        public static bool BufferMaterials(ComposedShader shader, List<ShaderMaterial> materials, string name)
        {
            int uboMaterial;
            int uboIndex; // Index to use for the buffer binding. All binding indicies start from 0
            ShaderMaterial[] _materials; 
            ShaderMaterial[] src;

            _materials = new ShaderMaterial[16]; // finely-sharpened imposes a limit of 16 of materials per object
            src = materials.ToArray();
            Array.Copy(src, 0, _materials, 0, src.Length);

            uboIndex = 0;
            uboMaterial = GL.GenBuffer();

            GL.BindBuffer(BufferTarget.UniformBuffer, uboMaterial);

            // Allocate Memory Request
            GL.BufferData<ShaderMaterial>(BufferTarget.UniformBuffer,
                (IntPtr)(_materials.Length * ShaderMaterial.Size),
                _materials, BufferUsageHint.StaticDraw);


            // Bind the created Uniform Buffer to the Buffer Index
            GL.BindBufferRange(BufferRangeTarget.UniformBuffer,
                               uboIndex,
                               uboMaterial,
                               (IntPtr)0, 
                               _materials.Length * ShaderMaterial.Size
                                );


            // Cleanup
            GL.BindBuffer(BufferTarget.UniformBuffer, 0);

            return true;
        }

        public static bool BufferMaterial(ComposedShader shader, ShaderMaterial material, string name, out int uboMaterial)
        {
            int uboIndex; // Index to use for the buffer binding (All good things start at 0 )
            //int uniformBlockLocation;

            uboIndex = 0; 

            //uniformBlockLocation = GL.GetUniformBlockIndex(shader.ShaderHandle, name);

            //if(UniformBlockLocation > -1)
            //{
            //    GL.UniformBlockBinding(shader.ShaderHandle, uniformBlockLocation, uboIndex);
            //}


            uboMaterial = GL.GenBuffer();

            GL.BindBuffer(BufferTarget.UniformBuffer, uboMaterial);

            // Allocate Memory Request
            GL.BufferData(BufferTarget.UniformBuffer, 
                (IntPtr)(ShaderMaterial.Size), (IntPtr)(null), BufferUsageHint.DynamicDraw);

            // Bind the created Uniform Buffer to the Buffer Index
            GL.BindBufferRange(BufferRangeTarget.UniformBuffer,
                               uboIndex, 
                               uboMaterial, 
                               (IntPtr)0, ShaderMaterial.Size
                                );


            GL.BufferSubData<ShaderMaterial>(BufferTarget.UniformBuffer, (IntPtr)0, ShaderMaterial.Size, ref material);
            GL.BindBuffer(BufferTarget.UniformBuffer, 0);

            return true;
        }

        /// <summary>
        /// Precondition: Apply Buffer Pointers right before GL.DrawArrays or GL.DrawElements, 
        /// requires default pointers to be bound in the shader after linking.
        /// </summary>
        public static void ApplyBufferPointers(ComposedShader shader)
        {
            // Set pointers to shader vertex attributes 
            shader.SetPointer("position", VertexAttribType.Position); // vertex position
            shader.SetPointer("normal", VertexAttribType.Normal); // vertex normal
            shader.SetPointer("color", VertexAttribType.Color); // vertex color
            shader.SetPointer("texcoord", VertexAttribType.TextureCoord); // vertex texCoordinate
        }

        /// <summary>d
        /// Precondition: Apply Buffer Pointers right before GL.DrawArrays or GL.DrawElements
        /// </summary>
        [ObsoleteAttribute("use ApplyBufferPointers(ComposedShader) instead")]
        public static void ApplyBufferPointers(ShaderUniformsPNCT uniforms)
        {
            if (uniforms.a_position != -1)
            {
                GL.EnableVertexAttribArray(uniforms.a_position); // vertex position
                GL.VertexAttribPointer(uniforms.a_position, 3, VertexAttribPointerType.Float, false, Vertex.Stride, (IntPtr)0);
            }

            if (uniforms.a_normal != -1)
            {
                GL.EnableVertexAttribArray(uniforms.a_normal); // vertex normal
                GL.VertexAttribPointer(uniforms.a_normal, 3, VertexAttribPointerType.Float, false, Vertex.Stride, (IntPtr)(Vector3.SizeInBytes));
            }

            if (uniforms.a_color != -1)
            {
                GL.EnableVertexAttribArray(uniforms.a_color); // vertex color
                GL.VertexAttribPointer(uniforms.a_color, 4, VertexAttribPointerType.Float, false, Vertex.Stride, (IntPtr)(Vector3.SizeInBytes + Vector3.SizeInBytes));
            }


            if (uniforms.a_texcoord != -1)
            {
                GL.EnableVertexAttribArray(uniforms.a_texcoord); // vertex texCoordinate
                GL.VertexAttribPointer(uniforms.a_texcoord, 2, VertexAttribPointerType.Float, false, Vertex.Stride, (IntPtr)(Vector3.SizeInBytes + Vector3.SizeInBytes + Vector4.SizeInBytes));
            }
        }

        public static GeometryHandle BufferShaderGeometry(PackedGeometry packSet)
        {
            GeometryHandle handle;

            handle = new GeometryHandle();

            if (packSet.interleaved3.Count > 0)
            {
                Buffering.BufferShaderGeometry(packSet.interleaved3, out handle.vbo3, out handle.NumVerticies3);
            }

            if (packSet.interleaved4.Count > 0)
            {
                Buffering.BufferShaderGeometry(packSet.interleaved4, out handle.vbo4, out handle.NumVerticies4);
            }

            return handle;
        }

        public static int BufferShaderGeometry(Mesh geometries, out int verts)
        {

            int numBuffers = geometries.Count;
            int buffers;


            GL.GenBuffers(1, out buffers);

            int totalSize = 0;

            for (int i = 0; i < numBuffers; i++)
            {
                totalSize += geometries[i].Count * Vertex.SizeInBytes;
            }

            //GL.BindBuffer(BufferTarget.ArrayBuffer, buffers[0]);
            // GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(totalSize*2), IntPtr.Zero, BufferUsageHint.StaticDraw);
            //GL.BindBuffer(BufferTarget.ArrayBuffer, buffers[1]);
            //GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)totalSize, IntPtr.Zero, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ArrayBuffer, buffers);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(totalSize), IntPtr.Zero, BufferUsageHint.StaticDraw);

            verts = 0;

            int offset = 0;
            for (int i = 0; i < numBuffers; i++)
            {
                Vertex[] _interleaved3 = geometries[i].ToArray();
                verts += _interleaved3.Length;


                int stride = BlittableValueType.StrideOf(_interleaved3); // Vertex.SizeInBytes;
                int size = _interleaved3.Length * stride;

                Console.WriteLine("Buffering Verticies..");
                GL.BufferSubData<Vertex>(BufferTarget.ArrayBuffer, (IntPtr)offset, (IntPtr)(size), _interleaved3);
                Console.WriteLine("[done]");

                offset += size;

            }

            return buffers;
        }

        public static int BufferShaderGeometry(Verticies geometry,
                                               out int vbo_interleaved, out int NumVerticies)
        {
            Vertex[] _interleaved = geometry.ToArray();


            Console.WriteLine("Buffering Verticies..");

            vbo_interleaved = GL.GenBuffer();
            //GL.GenBuffers(1, out vbo_interleaved);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_interleaved); // InterleavedArrayFormat.T2fC4fN3fV3f
            GL.BufferData<Vertex>(BufferTarget.ArrayBuffer, 
                (IntPtr)(_interleaved.Length * Vertex.SizeInBytes), 
                _interleaved, BufferUsageHint.StaticDraw);

            Console.WriteLine("[done]");


            // Each vertex in the interleaved structure is defined as follows..
            //
            // In x3d-finely-sharpened, each field 'f' is a floating point value. 
            // The size of a float is 4 bytes, the size of a vertex structure is calculated as follows:
            //
            // 'p' position field is 12 bytes (3 fields * 4 bytes = 12 bytes) 
            // 'n' normal field is 12 bytes (3 fields * 4 bytes = 12 bytes) 
            // 'c' color field is 16 bytes (4 fields * 4 bytes = 16 bytes) 
            // 'tc' texture coordinate field is 8 bytes (2 fields * 4 bytes = 8 bytes) 

            // The STRIDE is the number of bytes between each interleaved structure
            //  stride = Pf + Nf + Cf + TCf
            //  stride = 12 + 12 + 16 + 8
            //  stride = 48 bytes
            //
            //  Luckily this new implementation doesnt require calculation of stride
            //  however it is important to keep to this structure. 
            //  Floating point values must used uniformally for each field. 
            //  'c' color field must be composed of 4 floating point values 
            //  otherwise if there is no alpha channel OpenGL pads the color to 16 bytes 
            //  automatically. This creates uneven structures and results in undesired operation.
            //
            //  If any new fields need to be added to the Vertex structure,
            //  this model needs updating to reflect the changes 
            //  new fields must be floating point values, 
            //  finally Vertex.SizeInBytes should be checked to see 
            //  if it returns the same value as the calculation above. 
            //
            // P        N        C          TC
            // [f f f]  [f f f]  [f f f f]  [f f]
            //      12  13   24  25     40  41 50

            NumVerticies = _interleaved.Length;

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            return vbo_interleaved;
        }

        /* 
            Buffering.Interleave(out this.bbox,
                 out this.interleaved3,
                 out this.interleaved4,
                 this._indices,
                 this._texIndices,
                 this._coords,
                 this._texCoords,
                 null,
                 this._colorIndicies,
                 this.color,
                 this.restartIndex,
                 false,
                 true,
                 this.Coloring,
                 this.Texturing, 
                 calcBounds
                 );
        */

        public static void Interleave(
            ref PackedGeometry pack,
            bool genTexCoordPerVertex = true,
            bool colorPerVertex = true,
            bool calcBounds = true)
        {
            int FACE_RESTART_INDEX = 2;

            // INTERLEAVE FACE SET
            Console.WriteLine("Interleaving {0} indicies", pack._indices.Length);

            int faceSetIndex = 0;
            int faceSetValue, texSetValue = -1, colSetValue = -1;
            int faceType = 0;
            List<int> faceset = new List<int>();
            List<int> texset = new List<int>();
            List<int> colset = new List<int>();
            List<Vertex> verticies2 = new List<Vertex>();
            pack.interleaved3 = new List<Vertex>(); // buffer verticies of different face types separatly
            pack.interleaved4 = new List<Vertex>();
            Vertex v;
            //Vector4 c;
            float tmp;
            Vector3 maximum;
            Vector3 minimum;

            maximum = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            minimum = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);

            //REFACTOR

            if (pack.restartIndex.HasValue)
            {
                // and put verticies of type 4 in another buffer
                for (int coordIndex = 0; coordIndex < pack._indices.Length; coordIndex++)
                {
                    faceSetValue = pack._indices[coordIndex];

                    if (pack._texIndices != null && pack._texIndices.Length > 0)
                        texSetValue = pack._texIndices[coordIndex];

                    if (pack._colorIndicies != null && pack._colorIndicies.Length > 0)
                        colSetValue = pack._colorIndicies[coordIndex];

                    if (faceSetValue == pack.restartIndex.Value)
                    {
                        for (int k = 0; k < faceType; k++)
                        {
                            v = Vertex.Zero;
                            v.Position = pack._coords[faceset[k]];

                            maximum = MathHelpers.Max(v.Position, maximum);
                            minimum = MathHelpers.Min(v.Position, minimum);

                            // Flip Z and Y
                            //tmp = v.Position.Z;
                            //v.Position.Z = -v.Position.Y;
                            //v.Position.Y = tmp;

                            if (texset != null && texset.Count > 0 && pack._texCoords != null && pack._texCoords.Length > 0)
                            {
                                v.TexCoord = pack._texCoords[texset[k]];
                            }
                            //else if (genTexCoordPerVertex && texturing && _bbox != null)
                            //{
                            //    //v.TexCoord = MathHelpers.uv(v.Position.x);
                            //    v = MathHelpers.uv(_bbox, new Vertex[] { v }, at_origin: false)[0];
                            //}

                            if (pack.Coloring)
                            {
                                if (colorPerVertex)
                                {
                                    if (colset != null && colset.Count > 0 && pack.color != null && pack.color.Length > 0)
                                    {
                                        if (colset.Count == 3)
                                        {
                                            v.Color = new Vector4(
                                                pack.color[colset[k]],
                                                pack.color[colset[k] + 1],
                                                pack.color[colset[k] + 2],
                                                1.0f
                                            );
                                        }
                                        else if (colset.Count == 4)
                                        {
                                            v.Color = new Vector4(
                                                pack.color[colset[k]],
                                                pack.color[colset[k] + 1],
                                                pack.color[colset[k] + 2],
                                                pack.color[colset[k] + 3]
                                            );
                                        }

                                    }
                                }
                                else
                                {
                                    // color per face


                                }
                            }

                            if (pack.normals != null && pack.normals.Length > 0)
                            {
                                //TODO: interleave normals

                                //v.Normal = pack.normals[faceset[k]];
                            }

                            switch (faceType)
                            {
                                case 3:
                                    pack.interleaved3.Add(v);
                                    break;
                                case 4:
                                    pack.interleaved4.Add(v);
                                    break;
                                case 2:
                                    verticies2.Add(v);
                                    break;
                            }
                        }

                        faceSetIndex++;
                        faceType = 0;
                        faceset.Clear();
                        texset.Clear();
                        colset.Clear();
                    }
                    else
                    {
                        faceType++;
                        faceset.Add(faceSetValue);

                        if (pack._texIndices != null && pack._texIndices.Length > 0)
                            texset.Add(texSetValue);

                        if (pack._colorIndicies != null && pack._colorIndicies.Length > 0)
                            colset.Add(colSetValue);
                    }
                }
            }
            else
            {
                // NO RESTART INDEX, assume new face is at every 3rd value / i = 2

                if (pack._indices.Length == 4)
                {
                    FACE_RESTART_INDEX = 4; // 0-3 Quad
                }
                else if (pack._indices.Length == 3)
                {
                    FACE_RESTART_INDEX = 3; // 0-3 Triangle
                }
                else
                {
                    FACE_RESTART_INDEX = 3;
                }

                for (int coordIndex = 0; coordIndex < pack._indices.Length; coordIndex++)
                {
                    faceSetValue = pack._indices[coordIndex];
                    faceset.Add(faceSetValue);
                    faceType++;

                    if (pack._texIndices != null)
                    {
                        texSetValue = pack._texIndices[coordIndex];
                        texset.Add(texSetValue);
                    }

                    if (coordIndex > 0 && (coordIndex + 1) % FACE_RESTART_INDEX == 0)
                    {
                        for (int k = 0; k < faceType; k++)
                        {
                            v = Vertex.Zero;
                            v.Position = pack._coords[faceset[k]];

                            maximum = MathHelpers.Max(v.Position, maximum);
                            minimum = MathHelpers.Min(v.Position, minimum);

                            // Flip Z and Y
                            //tmp = v.Position.Z;
                            //v.Position.Z = -v.Position.Y;
                            //v.Position.Y = tmp;

                            if (texset != null && texset.Count > 0 && pack._texCoords != null && pack._texCoords.Length > 0)
                            {
                                v.TexCoord = pack._texCoords[texset[k]];
                            }
                            //else if (genTexCoordPerVertex && texturing && _bbox != null)
                            //{
                            //    //v.TexCoord = MathHelpers.uv(v.Position.x);
                            //    v = MathHelpers.uv(_bbox, new Vertex[] { v }, at_origin: false)[0];
                            //}

                            if (pack.normals != null && pack.normals.Length > 0)
                            {
                                //TODO: interleave normals

                                //v.Normal = pack.normals[faceset[k]];
                            }

                            switch (faceType)
                            {
                                case 3:
                                    pack.interleaved3.Add(v);
                                    break;
                                case 4:
                                    pack.interleaved4.Add(v);
                                    break;
                                case 2:
                                    verticies2.Add(v);
                                    break;
                            }
                        }

                        faceSetIndex++;
                        faceType = 0;
                        faceset.Clear();
                        texset.Clear();
                    }
                }
            }

            if (calcBounds)
            {
                Vector3 x3dScale = new Vector3(0.06f, 0.06f, 0.06f);

                pack.bbox = new BoundingBox()
                {
                    Width = (maximum.X - minimum.X),
                    Height = (maximum.Y - minimum.Y),
                    Depth = (maximum.Z - minimum.Z),

                    Maximum = maximum,
                    Minimum = minimum
                };

                // corners
                // (min_x,min_y), (min_x,max_y), (max_x,max_y), (max_x,min_y)
            }
            else
            {
                pack.bbox = null;
            }

            //Dont buffer geometry here, just interleave
        }


    }
}
