using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace X3D.Parser
{
    public class Helpers
    {
        public static int BufferShaderGeometry(List<List<Vertex>> geometries, out int verts)
        {
            GL.UseProgram(Shape.shaderProgramHandle);

            int a_position = GL.GetAttribLocation(Shape.shaderProgramHandle, "position");
            int a_normal = GL.GetAttribLocation(Shape.shaderProgramHandle, "normal");
            int a_color = GL.GetAttribLocation(Shape.shaderProgramHandle, "color");
            int a_texcoord = GL.GetAttribLocation(Shape.shaderProgramHandle, "texcoord");

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

                if (a_position != -1)
                {
                    GL.EnableVertexAttribArray(a_position); // vertex position
                    GL.VertexAttribPointer(a_position, 3, VertexAttribPointerType.Float, false, Vertex.Stride, (IntPtr)0);
                }

                if (a_normal != -1)
                {
                    GL.EnableVertexAttribArray(a_normal); // vertex normal
                    GL.VertexAttribPointer(a_normal, 3, VertexAttribPointerType.Float, false, Vertex.Stride, (IntPtr)(Vector3.SizeInBytes));
                }

                if (a_color != -1)
                {
                    //GL.EnableVertexAttribArray(a_color); // vertex color
                    //GL.VertexAttribPointer(a_color, 4, VertexAttribPointerType.Float, false, Vertex.Stride, 0);
                }


                if (a_texcoord != -1)
                {
                    GL.EnableVertexAttribArray(a_texcoord); // vertex texCoordinate
                    GL.VertexAttribPointer(a_texcoord, 2, VertexAttribPointerType.Float, false, Vertex.Stride, (IntPtr)(Vector3.SizeInBytes + Vector3.SizeInBytes));
                }

            }

            return buffers;
        }
        public static int BufferShaderGeometry(List<Vertex> geometry, out int vbo_interleaved3, out int NumVerticies)
        {
            Vertex[] _interleaved3 = geometry.ToArray();

            GL.UseProgram(Shape.shaderProgramHandle);

            int a_position = GL.GetAttribLocation(Shape.shaderProgramHandle, "position");
            int a_normal = GL.GetAttribLocation(Shape.shaderProgramHandle, "normal");
            int a_color = GL.GetAttribLocation(Shape.shaderProgramHandle, "color");
            int a_texcoord = GL.GetAttribLocation(Shape.shaderProgramHandle, "texcoord");



            Console.WriteLine("Buffering Verticies..");

            GL.GenBuffers(1, out vbo_interleaved3);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_interleaved3);
            GL.BufferData<Vertex>(BufferTarget.ArrayBuffer, (IntPtr)(_interleaved3.Length * Vertex.SizeInBytes), _interleaved3, BufferUsageHint.StaticDraw);

            Console.WriteLine("[done]");


            // STRIDE each float is 4 bytes
            // [1 1] [1 1 1 1]  [1 1 1]   [1 1 1]
            //     8        24  28    36  40   48

            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_interleaved3); // InterleavedArrayFormat.T2fC4fN3fV3f

            if (a_position != -1)
            {
                GL.EnableVertexAttribArray(a_position); // vertex position
                GL.VertexAttribPointer(a_position, 3, VertexAttribPointerType.Float, false, Vertex.Stride, (IntPtr)0);
            }

            if (a_normal != -1)
            {
                GL.EnableVertexAttribArray(a_normal); // vertex normal
                GL.VertexAttribPointer(a_normal, 3, VertexAttribPointerType.Float, false, Vertex.Stride, (IntPtr)(Vector3.SizeInBytes));
            }

            if (a_color != -1)
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

            NumVerticies = _interleaved3.Length;

            return vbo_interleaved3;
        }

        public static void Interleave(out int vbo_interleaved3, out int NumVerticies,
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

            //TODO: figure out how to render type 4

            // BUFFER GEOMETRY
            BufferShaderGeometry(verticies3, out vbo_interleaved3, out NumVerticies);

            Console.WriteLine("Expanded to {0}", NumVerticies);
        }

        public static int ApplyTestShader()
        {
            string vert = @"
#version 400

// 0.4, 0.4, 0.4
out lowp vec2 uv;
void main() 
{
    gl_Position = gl_ModelViewProjectionMatrix * gl_Vertex;
    uv = gl_MultiTexCoord0.xy;
}
";
            string frag = @"
#version 400
in vec2 uv;
const vec4 threshold = vec4(0.7, 0.7, 0.7, 1.0); // transparency threshold
uniform sampler2D _MainTex;
void main() 
{
    vec4 c = texture2D(_MainTex, uv);
    //vec4 alp = texture2D(_AlphaMap, c.rg);

    //c = c * alp;

    // color replace function
    if(c.r > 0 && c.r < 0.2
    && c.g > 0 && c.g < 0.2 
    && c.b > 0 && c.b < 0.2
    ){
        //c.r = c.r  + 0.01;
    }

    if(c.r > 0.2 && c.r < 0.6
    && c.g > 0.2 && c.g < 0.6 
    && c.b > 0.2 && c.b < 0.6
    ){
        //c.r = c.r + 0.4;
    }

    if(c.r > 0.6 && c.r < 0.8
    && c.g > 0.6 && c.g < 0.8 
    && c.b > 0.6 && c.b < 0.8
    ){
        //c.r = c.r + 0.2;
    }

    gl_FragColor = c;

    // transparency thresholding
        if (c.r > threshold.x 
        && c.g > threshold.y 
            && c.b > threshold.z) 
            discard;
} 
";
            return ApplyShader(vert, frag);
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

            Console.WriteLine(GL.GetShaderInfoLog(vertexShaderHandle).Trim());
            Console.WriteLine(GL.GetShaderInfoLog(fragmentShaderHandle).Trim());

            GL.AttachShader(shaderProgramHandle, vertexShaderHandle);
            GL.AttachShader(shaderProgramHandle, fragmentShaderHandle);
            GL.LinkProgram(shaderProgramHandle);

            Console.WriteLine(GL.GetProgramInfoLog(shaderProgramHandle).Trim());

            //GL.UseProgram(shaderProgramHandle);

            return shaderProgramHandle;
        }
        public static int ApplyShader(string shaderSource, ShaderType type)
        {
            int shaderProgramHandle = GL.CreateProgram();

            int shaderHandle = GL.CreateShader(type);

            GL.ShaderSource(shaderHandle, shaderSource);

            GL.CompileShader(shaderHandle);

            Console.WriteLine(GL.GetShaderInfoLog(shaderHandle));

            GL.AttachShader(shaderProgramHandle, shaderHandle);
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

        public static string ToString(Vector3 sfVec3f)
        {
            return string.Format("{0} {1} {2}", sfVec3f.X, sfVec3f.Y, sfVec3f.Z);
        }

        public static string ToString(Vector4 sfVec4f)
        {
            return string.Format("{0} {1} {2} {3}", sfVec4f.X, sfVec4f.Y, sfVec4f.Z, sfVec4f.W);
        }

        public static Vector3 SFVec3(string value)
        {
            float[] values = value.Split(' ').Select(s => float.Parse(s)).ToArray();
            return new Vector3(values[0], values[1], values[2]);
        }
        public static Vector3 SFVec3f(string value)
        {
            //Regex regMFInt32 = new Regex("[+-]?\\d+\\.\\d+");
            //MatchCollection mc = regMFInt32.Matches(value);

            //return new Vector3(float.Parse(mc[0].Value), 
            //                   float.Parse(mc[1].Value), 
            //                   float.Parse(mc[2].Value));
            float[] f = Floats(value);

            return new Vector3(f[0],
                   f[1],
                   f[2]);
        }
        public static Vector4 SFVec4f(string value)
        {
            Regex regMFInt32 = new Regex("[+-]?\\d+\\.\\d+");
            MatchCollection mc = regMFInt32.Matches(value);

            return new Vector4(float.Parse(mc[0].Value),
                               float.Parse(mc[1].Value),
                               float.Parse(mc[2].Value),
                               float.Parse(mc[3].Value));
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
