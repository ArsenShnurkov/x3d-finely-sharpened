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

    /// <summary>
    /// Interleave all geometry for single API simplicity for now.
    /// Later include additional support for indexing geometry.
    /// </summary>
    public class Buffering
    {
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
                                               out int vbo_interleaved3, out int NumVerticies)
        {
            Vertex[] _interleaved3 = geometry.ToArray();


            Console.WriteLine("Buffering Verticies..");

            GL.GenBuffers(1, out vbo_interleaved3);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_interleaved3); // InterleavedArrayFormat.T2fC4fN3fV3f
            GL.BufferData<Vertex>(BufferTarget.ArrayBuffer, (IntPtr)(_interleaved3.Length * Vertex.SizeInBytes), _interleaved3, BufferUsageHint.StaticDraw);

            Console.WriteLine("[done]");


            // STRIDE each float is 4 bytes
            // TC    C          P         N
            // [1 1] [1 1 1 1]  [1 1 1]   [1 1 1]
            //     8        24  28    36  40   48

            NumVerticies = _interleaved3.Length;

            return vbo_interleaved3;
        }

        public static void Interleave(BoundingBox _bbox,
            out int vbo_interleaved3, out int NumVerticies,
            out int vbo_interleaved4, out int NumVerticies4,
            int[] _indices, int[] _texIndices,
            Vector3[] _coords, Vector2[] _texCoords, Vector3[] _normals, 
            int[] _colorIndicies, float[] colors,
            int? restartIndex = -1, bool genTexCoordPerVertex = true, bool colorPerVertex = true, 
            bool coloring = false, bool texturing = false)
        {
            int FACE_RESTART_INDEX = 2;

            // INTERLEAVE FACE SET
            Console.WriteLine("Interleaving {0} indicies", _indices.Length);

            int faceSetIndex = 0;
            int faceSetValue, texSetValue = -1, colSetValue = -1;
            int faceType = 0;
            List<int> faceset = new List<int>();
            List<int> texset = new List<int>();
            List<int> colset = new List<int>();
            List<Vertex> verticies2 = new List<Vertex>();
            List<Vertex> verticies3 = new List<Vertex>(); // buffer verticies of different face types separatly
            List<Vertex> verticies4 = new List<Vertex>();
            Vertex v;
            Vector4 c;
            float tmp;

            if (restartIndex.HasValue)
            {
                // and put verticies of type 4 in another buffer
                for (int coordIndex = 0; coordIndex < _indices.Length; coordIndex++)
                {
                    faceSetValue = _indices[coordIndex];

                    if (_texIndices != null && _texIndices.Length > 0)
                        texSetValue = _texIndices[coordIndex];

                    if (_texIndices != null && _texIndices.Length > 0)
                        colSetValue = _texIndices[coordIndex];

                    if (faceSetValue == restartIndex.Value)
                    {
                        for (int k = 0; k < faceType; k++)
                        {
                            v = Vertex.Zero;
                            v.Position = _coords[faceset[k]];

                            // Flip Z and Y
                            tmp = v.Position.Z;
                            v.Position.Z = -v.Position.Y;
                            v.Position.Y = tmp;

                            if (texset != null && texset.Count > 0 && _texCoords != null && _texCoords.Length > 0)
                            {
                                v.TexCoord = _texCoords[texset[k]];
                            }
                            else if (genTexCoordPerVertex && texturing && _bbox != null)
                            {
                                //v.TexCoord = MathHelpers.uv(v.Position.x);
                                v = MathHelpers.uv(_bbox, new Vertex[] { v }, at_origin: false)[0];
                            }

                            if (coloring)
                            {
                                if (colorPerVertex)
                                {

                                }
                                else
                                {
                                    // color per face


                                }
                            }

                            if (_normals != null && _normals.Length > 0)
                            {
                                v.Normal = _normals[faceset[k]];
                            }

                            switch (faceType)
                            {
                                case 3:
                                    verticies3.Add(v);
                                    break;

                                case 4:
                                    verticies4.Add(v);
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
                    else
                    {
                        faceType++;
                        faceset.Add(faceSetValue);

                        if (_texIndices != null && _texIndices.Length > 0)
                            texset.Add(texSetValue);
                    }
                }
            }
            else
            {
                // NO RESTART INDEX, assume new face is at every 3rd value / i = 2

                if(_indices.Length == 4)
                {
                    FACE_RESTART_INDEX = 4; // 0-3 Quad
                }
                else if (_indices.Length == 3)
                {
                    FACE_RESTART_INDEX = 3; // 0-3 Triangle
                }
                else
                {
                    FACE_RESTART_INDEX = 3;
                }

                for (int coordIndex = 0; coordIndex < _indices.Length; coordIndex++)
                {
                    faceSetValue = _indices[coordIndex];
                    faceset.Add(faceSetValue);
                    faceType++;

                    if (_texIndices != null)
                    {
                        texSetValue = _texIndices[coordIndex];
                        texset.Add(texSetValue);
                    }

                    if (coordIndex > 0 && (coordIndex + 1) % FACE_RESTART_INDEX == 0)
                    {
                        for (int k = 0; k < faceType; k++)
                        {
                            switch (faceType)
                            {
                                case 3:
                                    v = Vertex.Zero;
                                    v.Position = _coords[faceset[k]];

                                    // Flip Z and Y
                                    tmp = v.Position.Z;
                                    v.Position.Z = -v.Position.Y;
                                    v.Position.Y = tmp;

                                    if (texset != null && texset.Count > 0 && _texCoords != null && _texCoords.Length > 0)
                                    {
                                        v.TexCoord = _texCoords[texset[k]];
                                    }
                                    else if (genTexCoordPerVertex && texturing && _bbox != null)
                                    {
                                        //v.TexCoord = MathHelpers.uv(v.Position.x);
                                        v = MathHelpers.uv(_bbox, new Vertex[] { v }, at_origin: false)[0];
                                    }

                                    if (_normals != null && _normals.Length > 0)
                                    {
                                        v.Normal = _normals[faceset[k]];
                                    }

                                    verticies3.Add(v);
                                    break;

                                case 4:
                                    v = Vertex.Zero;
                                    v.Position = _coords[faceset[k]];

                                    // Flip Z and Y
                                    tmp = v.Position.Z;
                                    v.Position.Z = -v.Position.Y;
                                    v.Position.Y = tmp;

                                    if (texset != null && texset.Count > 0 && _texCoords != null && _texCoords.Length > 0)
                                    {
                                        v.TexCoord = _texCoords[texset[k]];
                                    }
                                    else if (genTexCoordPerVertex && texturing && _bbox != null)
                                    {
                                        v = MathHelpers.uv(_bbox, new Vertex[] { v }, at_origin: false)[0];
                                    }

                                    if (_normals != null && _normals.Length > 0)
                                    {
                                        v.Normal = _normals[faceset[k]];
                                    }
                                    //v.Position = new Vector4(_coords[faceset[k]]);

                                    verticies4.Add(v);
                                    break;

                                case 2:
                                    v = Vertex.Zero;
                                    //v.Position = new Vector3(_coords[faceset[k]]);

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

            //BUG: buffering more than one object seems to overwrite ealier buffers
            NumVerticies = NumVerticies4 = 0;
            vbo_interleaved3 = vbo_interleaved4 = - 1;

            // BUFFER GEOMETRY
            if (verticies3.Count > 0) BufferShaderGeometry(verticies3, out vbo_interleaved3, out NumVerticies);
            if(verticies4.Count > 0) BufferShaderGeometry(verticies4, out vbo_interleaved4, out NumVerticies4);

            Console.WriteLine("Expanded to {0}", NumVerticies + NumVerticies4);
        }

    }
}
