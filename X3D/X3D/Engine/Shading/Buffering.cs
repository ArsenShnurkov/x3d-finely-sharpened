using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using X3D.Parser;

namespace X3D.Engine.Shading
{
    public class Buffering
    {
        public static int BufferShaderGeometry(List<List<Vertex>> geometries, Shape parentShape, out int verts)
        {
            GL.UseProgram(parentShape.shaderProgramHandle);

            parentShape.uniforms.a_position = GL.GetAttribLocation(parentShape.shaderProgramHandle, "position");
            parentShape.uniforms.a_normal = GL.GetAttribLocation(parentShape.shaderProgramHandle, "normal");
            parentShape.uniforms.a_color = GL.GetAttribLocation(parentShape.shaderProgramHandle, "color");
            parentShape.uniforms.a_texcoord = GL.GetAttribLocation(parentShape.shaderProgramHandle, "texcoord");

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

                if (parentShape.uniforms.a_position != -1)
                {
                    GL.EnableVertexAttribArray(parentShape.uniforms.a_position); // vertex position
                    GL.VertexAttribPointer(parentShape.uniforms.a_position, 3, VertexAttribPointerType.Float, false, Vertex.Stride, (IntPtr)0);
                }

                if (parentShape.uniforms.a_normal != -1)
                {
                    GL.EnableVertexAttribArray(parentShape.uniforms.a_normal); // vertex normal
                    GL.VertexAttribPointer(parentShape.uniforms.a_normal, 3, VertexAttribPointerType.Float, false, Vertex.Stride, (IntPtr)(Vector3.SizeInBytes));
                }

                if (parentShape.uniforms.a_color != -1)
                {
                    GL.EnableVertexAttribArray(parentShape.uniforms.a_color); // vertex color
                    GL.VertexAttribPointer(parentShape.uniforms.a_color, 4, VertexAttribPointerType.Float, false, Vertex.Stride, 0);
                }


                if (parentShape.uniforms.a_texcoord != -1)
                {
                    GL.EnableVertexAttribArray(parentShape.uniforms.a_texcoord); // vertex texCoordinate
                    GL.VertexAttribPointer(parentShape.uniforms.a_texcoord, 2, VertexAttribPointerType.Float, false, Vertex.Stride, (IntPtr)(Vector3.SizeInBytes + Vector3.SizeInBytes));
                }

            }

            return buffers;
        }

        public static int BufferShaderGeometry(List<Vertex> geometry, Shape parentShape,
                                               out int vbo_interleaved3, out int NumVerticies)
        {
            Vertex[] _interleaved3 = geometry.ToArray();


            GL.UseProgram(parentShape.shaderProgramHandle);
            parentShape.uniforms.a_position = GL.GetAttribLocation(parentShape.shaderProgramHandle, "position");
            parentShape.uniforms.a_normal = GL.GetAttribLocation(parentShape.shaderProgramHandle, "normal");
            parentShape.uniforms.a_color = GL.GetAttribLocation(parentShape.shaderProgramHandle, "color");
            parentShape.uniforms.a_texcoord = GL.GetAttribLocation(parentShape.shaderProgramHandle, "texcoord");


            Console.WriteLine("Buffering Verticies..");

            GL.GenBuffers(1, out vbo_interleaved3);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_interleaved3);
            GL.BufferData<Vertex>(BufferTarget.ArrayBuffer, (IntPtr)(_interleaved3.Length * Vertex.SizeInBytes), _interleaved3, BufferUsageHint.StaticDraw);

            Console.WriteLine("[done]");


            // STRIDE each float is 4 bytes
            // [1 1] [1 1 1 1]  [1 1 1]   [1 1 1]
            //     8        24  28    36  40   48

            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_interleaved3); // InterleavedArrayFormat.T2fC4fN3fV3f

            if (parentShape.uniforms.a_position != -1)
            {
                GL.EnableVertexAttribArray(parentShape.uniforms.a_position); // vertex position
                GL.VertexAttribPointer(parentShape.uniforms.a_position, 3, VertexAttribPointerType.Float, false, Vertex.Stride, (IntPtr)0);
            }

            if (parentShape.uniforms.a_normal != -1)
            {
                GL.EnableVertexAttribArray(parentShape.uniforms.a_normal); // vertex normal
                GL.VertexAttribPointer(parentShape.uniforms.a_normal, 3, VertexAttribPointerType.Float, false, Vertex.Stride, (IntPtr)(Vector3.SizeInBytes));
            }

            if (parentShape.uniforms.a_color != -1)
            {
                GL.EnableVertexAttribArray(parentShape.uniforms.a_color); // vertex color
                GL.VertexAttribPointer(parentShape.uniforms.a_color, 4, VertexAttribPointerType.Float, false, Vertex.Stride, 0);
            }


            if (parentShape.uniforms.a_texcoord != -1)
            {
                GL.EnableVertexAttribArray(parentShape.uniforms.a_texcoord); // vertex texCoordinate
                GL.VertexAttribPointer(parentShape.uniforms.a_texcoord, 2, VertexAttribPointerType.Float, false, Vertex.Stride, (IntPtr)(Vector3.SizeInBytes + Vector3.SizeInBytes));
            }

            //GL.ColorPointer(4, ColorPointerType.Float, InterleavedVertexData.size_in_bytes, (IntPtr)0);
            //GL.VertexPointer(3, VertexPointerType.Float, InterleavedVertexData.size_in_bytes, (IntPtr)(COLOR_COORD_SIZE * sizeof(float)));
            //GL.TexCoordPointer(2, TexCoordPointerType.Float, InterleavedVertexData.size_in_bytes, (IntPtr)((COLOR_COORD_SIZE + VERTEX_COORD_SIZE) * sizeof(float)));
            //GL.NormalPointer(NormalPointerType.Float, InterleavedVertexData.size_in_bytes, (IntPtr)((COLOR_COORD_SIZE + VERTEX_COORD_SIZE + TEXTURE_COORD_SIZE) * sizeof(float)));

            NumVerticies = _interleaved3.Length;

            return vbo_interleaved3;
        }

        public static void Interleave(Shape parentShape,
            out int vbo_interleaved3, out int NumVerticies,
            int[] _indices, int[] _texIndices,
            Vector3[] _coords, Vector2[] _texCoords, Vector3[] _normals, int? restartIndex = -1, bool genTexCoordPerVertex = true)
        {
            const int FACE_RESTART_INDEX = 2;

            // INTERLEAVE FACE SET
            Console.WriteLine("Interleaving {0} indicies", _indices.Length);

            int faceSetIndex = 0;
            int faceSetValue, texSetValue = -1;
            int faceType = 0;
            List<int> faceset = new List<int>();
            List<int> texset = new List<int>();
            List<Vertex> verticies2 = new List<Vertex>();
            List<Vertex> verticies3 = new List<Vertex>(); // buffer verticies of different face types separatly
            List<Vertex> verticies4 = new List<Vertex>();
            Vertex v;

            if (restartIndex.HasValue)
            {
                // and put verticies of type 4 in another buffer
                for (int coordIndex = 0; coordIndex < _indices.Length; coordIndex++)
                {
                    faceSetValue = _indices[coordIndex];

                    if (_texIndices != null && _texIndices.Length > 0)
                        texSetValue = _texIndices[coordIndex];

                    if (faceSetValue == restartIndex.Value)
                    {
                        for (int k = 0; k < faceType; k++)
                        {
                            switch (faceType)
                            {
                                case 3:
                                    v = Vertex.Zero;
                                    v.Position = _coords[faceset[k]];

                                    // Flip Z and Y
                                    float tmp;
                                    tmp = v.Position.Z;
                                    v.Position.Z = -v.Position.Y;
                                    v.Position.Y = tmp;

                                    if (texset != null && texset.Count > 0 && _texCoords != null && _texCoords.Length > 0)
                                    {
                                        v.TexCoord = _texCoords[texset[k]];
                                    }
                                    else if (genTexCoordPerVertex)
                                    {
                                        //v.TexCoord = MathHelpers.uv(v.Position.x);
                                    }

                                    if (_normals != null && _normals.Length > 0)
                                    {
                                        v.Normal = _normals[faceset[k]];
                                    }

                                    verticies3.Add(v);
                                    break;

                                case 4:
                                    v = Vertex.Zero;
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

                    if (faceSetValue > 0 && faceSetValue % FACE_RESTART_INDEX == 0)
                    {
                        for (int k = 0; k < faceType; k++)
                        {
                            switch (faceType)
                            {
                                case 3:
                                    v = Vertex.Zero;
                                    v.Position = _coords[faceset[k]];

                                    // Flip Z and Y
                                    float tmp;
                                    tmp = v.Position.Z;
                                    v.Position.Z = -v.Position.Y;
                                    v.Position.Y = tmp;

                                    if (texset != null && texset.Count > 0 && _texCoords != null && _texCoords.Length > 0)
                                    {
                                        v.TexCoord = _texCoords[texset[k]];
                                    }

                                    if (_normals != null && _normals.Length > 0)
                                    {
                                        v.Normal = _normals[faceset[k]];
                                    }

                                    verticies3.Add(v);
                                    break;

                                case 4:
                                    v = Vertex.Zero;
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

            //TODO: figure out how to render type 4

            // BUFFER GEOMETRY
            BufferShaderGeometry(verticies3, parentShape, out vbo_interleaved3, out NumVerticies);

            Console.WriteLine("Expanded to {0}", NumVerticies);
        }

    }
}
