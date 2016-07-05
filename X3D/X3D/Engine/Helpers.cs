using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics.OpenGL;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;

namespace X3D.Parser
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Vertex
    { // mimic InterleavedArrayFormat.T2fC4fN3fV3f
      /// <summary>
      /// Required
      /// </summary>
        public Vector3 Position;
        public Vector3 Normal;
        //public Vector4 Color;
        public Vector2 TexCoord;
        
        public static Vertex Zero
        {
            get
            {
                Vertex v = new Vertex();
                v.TexCoord = Vector2.Zero;
                v.Normal = Vector3.Zero;
                return v;
            }
        }


        public static readonly int SizeInBytes = Vector2.SizeInBytes + Vector4.SizeInBytes + Vector3.SizeInBytes + Vector3.SizeInBytes;

        public static readonly int Stride = Marshal.SizeOf(default(Vertex));
    }

    public class Helpers
    {

        public static void Interleave(out int vbo_interleaved, out int NumVerticies,
            int[] _indices, int[] _texIndices, 
            Vector3[] _coords, Vector2[] _texCoords, Vector3[] _normals, int? restartIndex = -1)
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

                                    if (texset != null && texset.Count > 0 && _texCoords != null && _texCoords.Length > 0)
                                    {
                                        v.TexCoord = _texCoords[texset[k]];
                                    }

                                    if(_normals != null && _normals.Length > 0)
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


            Vertex[] _interleaved = verticies3.ToArray();
            //TODO: figure out how to render type 4


            GL.UseProgram(Shape.shaderProgramHandle);

            int a_position = GL.GetAttribLocation(Shape.shaderProgramHandle, "position");
            int a_normal = GL.GetAttribLocation(Shape.shaderProgramHandle, "normal");
            int a_color = GL.GetAttribLocation(Shape.shaderProgramHandle, "color");
            int a_texcoord = GL.GetAttribLocation(Shape.shaderProgramHandle, "texcoord");



            Console.WriteLine("Buffering Verticies..");

            GL.GenBuffers(1, out vbo_interleaved);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_interleaved);
            GL.BufferData<Vertex>(BufferTarget.ArrayBuffer, (IntPtr)(_interleaved.Length * Vertex.SizeInBytes), _interleaved, BufferUsageHint.StaticDraw);

            Console.WriteLine("[done]");


            // STRIDE each float is 4 bytes
            // [1 1] [1 1 1 1]  [1 1 1]   [1 1 1]
            //     8        24  28    36  40   48

            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_interleaved); // InterleavedArrayFormat.T2fC4fN3fV3f
            
            if(a_position != -1)
            {
                GL.EnableVertexAttribArray(a_position); // vertex position
                GL.VertexAttribPointer(a_position, 3, VertexAttribPointerType.Float, false, Vertex.Stride, (IntPtr)0);
            }

            if (a_normal != -1)
            {
                GL.EnableVertexAttribArray(a_normal); // vertex normal
                GL.VertexAttribPointer(a_normal, 3, VertexAttribPointerType.Float, false, Vertex.Stride, (IntPtr)(Vector3.SizeInBytes));
            }

            if (a_color!= -1)
            {
                //GL.EnableVertexAttribArray(a_color); // vertex color
                //GL.VertexAttribPointer(a_color, 4, VertexAttribPointerType.Float, false, Vertex.Stride, 0);
            }


            if (a_texcoord != -1)
            {
                GL.EnableVertexAttribArray(a_texcoord); // vertex texCoordinate
                GL.VertexAttribPointer(a_texcoord, 2, VertexAttribPointerType.Float, false, Vertex.Stride, (IntPtr)(Vector3.SizeInBytes + Vector3.SizeInBytes));
            }

            //GL.ColorPointer(4, ColorPointerType.Float, InterleavedVertexData.size_in_bytes, (IntPtr)0);
            //GL.VertexPointer(3, VertexPointerType.Float, InterleavedVertexData.size_in_bytes, (IntPtr)(COLOR_COORD_SIZE * sizeof(float)));
            //GL.TexCoordPointer(2, TexCoordPointerType.Float, InterleavedVertexData.size_in_bytes, (IntPtr)((COLOR_COORD_SIZE + VERTEX_COORD_SIZE) * sizeof(float)));
            //GL.NormalPointer(NormalPointerType.Float, InterleavedVertexData.size_in_bytes, (IntPtr)((COLOR_COORD_SIZE + VERTEX_COORD_SIZE + TEXTURE_COORD_SIZE) * sizeof(float)));



            NumVerticies = _interleaved.Length;

            Console.WriteLine("Expanded to {0}", NumVerticies);
        }

        public static string ToString(Vector3 sfVec3f)
        {
            return string.Format("{0} {1} {2}", sfVec3f.X, sfVec3f.Y, sfVec3f.Z);
        }

        /// <summary>
        /// Relocate this elsewhere
        /// </summary>
        public static int ApplyShader(string vertexShaderSource, string fragmentShaderSource)
        {
            int shaderProgramHandle = GL.CreateProgram();

            int vertexShaderHandle = GL.CreateShader(ShaderType.VertexShader);
            int fragmentShaderHandle = GL.CreateShader(ShaderType.FragmentShader);

            GL.ShaderSource(vertexShaderHandle, vertexShaderSource);
            GL.ShaderSource(fragmentShaderHandle, fragmentShaderSource);

            GL.CompileShader(vertexShaderHandle);
            GL.CompileShader(fragmentShaderHandle);

            Console.WriteLine(GL.GetShaderInfoLog(vertexShaderHandle));
            Console.WriteLine(GL.GetShaderInfoLog(fragmentShaderHandle));

            GL.AttachShader(shaderProgramHandle, vertexShaderHandle);
            GL.AttachShader(shaderProgramHandle, fragmentShaderHandle);
            GL.LinkProgram(shaderProgramHandle);

            Console.WriteLine(GL.GetProgramInfoLog(shaderProgramHandle));

            //GL.UseProgram(shaderProgramHandle);

            return shaderProgramHandle;
        }

        public static int[] ParseIndicies(string value)
        {
            List<int> indicies = new List<int>();

            Regex regMFInt32 = new Regex("\\S+\\S?");
            var mc = regMFInt32.Matches(value);
            int v;

            foreach (Match m in mc)
            {
                v = int.Parse(m.Value);
                //if (v != -1)
                //{
                    indicies.Add(v);
                //}
            }

            return indicies.ToArray();
        }

        public static Vector3 SFVec3(string value)
        {
            float[] values = value.Split(' ').Select(s => float.Parse(s)).ToArray();
            return new Vector3(values[0], values[1], values[2]);
        }
        public static Vector3 SFVec3f(string value)
        {
            Regex regMFInt32 = new Regex("[+-]?\\d+\\.\\d+");
            MatchCollection mc = regMFInt32.Matches(value);
            
            return new Vector3(float.Parse(mc[0].Value), 
                               float.Parse(mc[1].Value), 
                               float.Parse(mc[2].Value));
        }

        public static float[] Floats(string value)
        {
            Regex regMFInt32 = new Regex("([+-]?[0-9]+[.]?[0-9]?)+"); // [+-]?\\d+\\.\\d+
            MatchCollection mc = regMFInt32.Matches(value);
            List<float> floats = new List<float>();

            foreach (Match m in mc)
            {
                floats.Add(float.Parse(m.Value));
            }

            return floats.ToArray();
        }

        public static float SFVec1f(string value)
        {
            Regex regMFInt32 = new Regex("[+-]?\\d+\\.\\d+");
            MatchCollection mc = regMFInt32.Matches(value);

            return Floats(value).FirstOrDefault();
        }

        public static Vector2[] MFVec2f(string value)
        {
            List<Vector2> coords = new List<Vector2>();

            Regex regMFInt32 = new Regex("[+-]?\\d+\\.\\d?[^,]+");
            var mc = regMFInt32.Matches(value);

            foreach (Match m in mc)
            {
                float[] arr = Floats(m.Value);

                coords.Add(new Vector2(arr[0], arr[1]));
            }

            return coords.ToArray();
        }

        public static Vector3[] MFVec3f(string value)
        {
            List<Vector3> coords = new List<Vector3>();
            Regex regMFInt32 = new Regex(value.Contains(',') ? "\\S+\\S?\\s+\\S+\\S?\\s+\\S+\\S?" : "\\S+\\S?\\s+\\S+\\S?\\s+\\S+\\S?\\s+");
            var mc = regMFInt32.Matches(value + " ");

            List<float> lst = new List<float>();
            float[] vec3;

            int i = 0;
            foreach (Match m in mc)
            {
                vec3 = Floats(m.Value.Replace(",", ""));

                switch(vec3.Length)
                {
                    case 3:
                        coords.Add(new Vector3(vec3[0], vec3[1], vec3[2]));
                        break;
                    case 4:
                        //coords.Add(new Vector4(vec3[0], vec3[1], vec3[2], vec3[3]));
                        throw new Exception("error found a coordinate of size 4 in a size 3 coordinate set");
                        //break;
                }
                

            }

            return coords.ToArray();
        }

        public static float[] ParseCoords(string value)
        {
            List<float> coords = new List<float>();

            Regex regMFInt32 = new Regex("[+-]?\\d+\\.\\d?[^,]+");
            var mc = regMFInt32.Matches(value);

            foreach (Match m in mc)
            {
                coords.AddRange(Floats(m.Value));
            }

            return coords.ToArray();
        }

        

        /// <summary>
        /// Unpacks the indicies given an index set and coordinate set
        /// </summary>
        /// <param name="index_set"></param>
        /// <param name="coords"></param>
        /// <param name="stride"></param>
        /// <returns></returns>
        public static float[] Unpack(int[] index_set, float[] coords, int stride)
        {
            int i = 0, j, k;
            float[] arr = new float[index_set.Length * stride];

            foreach (int index in index_set)
            {
                k = stride * index;

                for (j = 0; j < stride; j++)
                {
                    arr[i + j] = coords[k + j];
                }

                i += stride;
            }

            return arr;
        }
        /// <summary>
        /// Unpacks the indicies given an index set and coordinate set
        /// </summary>
        /// <param name="index_set"></param>
        /// <param name="coords"></param>
        /// <param name="stride"></param>
        /// <returns></returns>
        public static int[] Unpack(int[] index_set, int[] colors, int stride)
        {
            int i = 0, j, k;
            int[] arr = new int[index_set.Length * stride];

            foreach (int index in index_set)
            {
                k = stride * index;

                for (j = 0; j < stride; j++)
                {
                    arr[i + j] = colors[k + j];
                }

                i += stride;
            }

            return arr;
        }
        /// <summary>
        /// Unpacks the indicies given an index set and coordinate set
        /// </summary>
        /// <param name="index_set"></param>
        /// <param name="coords"></param>
        /// <param name="stride"></param>
        /// <returns></returns>
        public static Vector3[] Unpack(int[] index_set, Vector3[] coords)
        {
            int stride = 1;

            int i = 0, j;
            Vector3[] arr = new Vector3[index_set.Length * stride];

            foreach (int index in index_set)
            {

                arr[i] = coords[index];

                i += stride;
            }

            return arr;
        }
    }
}
