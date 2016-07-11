using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using X3D.Engine.Shading;

namespace X3D.Parser
{
    public class Helpers
    {

        public static Vector3 ToVec3(OpenTK.Graphics.Color4 color)
        {
            return new Vector3(color.R, color.G, color.B);
        }
        public static Vector4 ToVec4(OpenTK.Graphics.Color4 color)
        {
            return new Vector4(color.R, color.G, color.B, color.A);
        }

        public static string UnescapeXMLValue(string xmlString)
        {
            if (xmlString == null)
                throw new ArgumentNullException("xmlString");

            return xmlString
                .Replace("&#13;", "\n")
                .Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", "\r\n")
                .Replace("&apos;", "'")
                .Replace("&quot;", "\"")
                .Replace("&gt;", ">")
                .Replace("&lt;", "<")
                .Replace("&amp;", "&");
        }

        public static string EscapeXMLValue(string xmlString)
        {

            if (xmlString == null)
                throw new ArgumentNullException("xmlString");
       
            return xmlString
                .Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", "\r\n")
                .Replace("\r\n", "&#13;")
                .Replace("'", "&apos;")
                .Replace("\"", "&quot;")
                .Replace(">", "&gt;")
                .Replace("<", "&lt;")
                .Replace("&", "&amp;");
        }

        public static int ApplyShaderPart(int shaderProgramHandle, ShaderPart part)
        {
            ShaderType type = ShaderType.VertexShader;

            switch (part.Type)
            {
                case shaderPartTypeValues.VERTEX:
                    type = ShaderType.VertexShader;
                    break;
                case shaderPartTypeValues.FRAGMENT:
                    type = ShaderType.FragmentShader;
                    break;
                case shaderPartTypeValues.TESS_CONTROL:
                    type = ShaderType.TessControlShader;
                    break;
                case shaderPartTypeValues.TESS_EVAL:
                    type = ShaderType.TessEvaluationShader;
                    break;
                case shaderPartTypeValues.GEOMETRY:
                    type = ShaderType.GeometryShader;
                    break;
                default:
                    throw new Exception("unknown shader type");
            }

            part.ShaderHandle = GL.CreateShader(type);

            GL.ShaderSource(part.ShaderHandle, part.ShaderSource);
            GL.CompileShader(part.ShaderHandle);

            string err = GL.GetShaderInfoLog(part.ShaderHandle).Trim();
            if (!string.IsNullOrEmpty(err))
                Console.WriteLine(err);

            GL.AttachShader(shaderProgramHandle, part.ShaderHandle);
            Console.WriteLine("ShaderPart {0} [attaching]", part.Type);

            return part.ShaderHandle;
        }

        public static ComposedShader BuildDefaultShader()
        {
            var defaultsh = new ComposedShader();

            defaultsh.IsBuiltIn = true; // specifies this is a built in system shader
            defaultsh.language = "GLSL";
            defaultsh.ShaderParts.Add(new ShaderPart()
            {
                ShaderSource = DefaultShader.vertexShaderSource,
                Type = shaderPartTypeValues.VERTEX
            });

            defaultsh.ShaderParts.Add(new ShaderPart()
            {
                ShaderSource = DefaultShader.fragmentShaderSource,
                Type = shaderPartTypeValues.FRAGMENT
            });

            return defaultsh;
        }

        /// <summary>
        /// Relocate this elsewhere
        /// </summary>
        public static ComposedShader ApplyShader(string vertexShaderSource, string fragmentShaderSource, 
            string tessControlSource = "", string tessEvalSource = "", string geometryShaderSource = "")
        {
            ComposedShader shader = new ComposedShader();

            shader.IsBuiltIn = true; // specifies this is a built in system shader
            shader.language = "GLSL";
            shader.ShaderParts.Add(new ShaderPart()
            {
                ShaderSource = vertexShaderSource,
                Type = shaderPartTypeValues.VERTEX
            });

            shader.ShaderParts.Add(new ShaderPart()
            {
                ShaderSource = tessControlSource,
                Type = shaderPartTypeValues.TESS_CONTROL
            });

            shader.ShaderParts.Add(new ShaderPart()
            {
                ShaderSource = tessEvalSource,
                Type = shaderPartTypeValues.TESS_EVAL
            });

            if (!string.IsNullOrEmpty(geometryShaderSource))
            {
                shader.ShaderParts.Add(new ShaderPart()
                {
                    ShaderSource = geometryShaderSource,
                    Type = shaderPartTypeValues.GEOMETRY
                });
            }


            shader.ShaderParts.Add(new ShaderPart()
            {
                ShaderSource = fragmentShaderSource,
                Type = shaderPartTypeValues.FRAGMENT
            });

            return shader;

            //int shaderProgramHandle = GL.CreateProgram();

            //int vertexShaderHandle = GL.CreateShader(ShaderType.VertexShader);
            //int fragmentShaderHandle = GL.CreateShader(ShaderType.FragmentShader);
            //int tessControlShaderHandle = -1, tessEvalShaderHandle = -1, geometryShaderHandle = -1;

            //GL.ShaderSource(vertexShaderHandle, vertexShaderSource);
            //GL.ShaderSource(fragmentShaderHandle, fragmentShaderSource);
            //GL.CompileShader(vertexShaderHandle);
            //GL.CompileShader(fragmentShaderHandle);

            //Console.WriteLine(GL.GetShaderInfoLog(vertexShaderHandle).Trim());
            //Console.WriteLine(GL.GetShaderInfoLog(fragmentShaderHandle).Trim());

            //GL.AttachShader(shaderProgramHandle, vertexShaderHandle);


            //if (!string.IsNullOrEmpty(tessControlSource))
            //{
            //    tessControlShaderHandle = GL.CreateShader(ShaderType.TessControlShader);
            //    GL.ShaderSource(tessControlShaderHandle, tessControlSource);
            //    GL.CompileShader(tessControlShaderHandle);
            //    Console.WriteLine(GL.GetShaderInfoLog(tessControlShaderHandle).Trim());

            //    GL.AttachShader(shaderProgramHandle, tessControlShaderHandle);
            //}
            //if (!string.IsNullOrEmpty(tessEvalSource))
            //{
            //    tessEvalShaderHandle = GL.CreateShader(ShaderType.TessEvaluationShader);
            //    GL.ShaderSource(tessEvalShaderHandle, tessEvalSource);
            //    GL.CompileShader(tessEvalShaderHandle);
            //    Console.WriteLine(GL.GetShaderInfoLog(tessEvalShaderHandle).Trim());

            //    GL.AttachShader(shaderProgramHandle, tessEvalShaderHandle);
            //}
            //if (!string.IsNullOrEmpty(geometryShaderSource))
            //{
            //    geometryShaderHandle = GL.CreateShader(ShaderType.GeometryShader);
            //    GL.ShaderSource(geometryShaderHandle, geometryShaderSource);
            //    GL.CompileShader(geometryShaderHandle);
            //    Console.WriteLine(GL.GetShaderInfoLog(geometryShaderHandle).Trim());

            //    GL.AttachShader(shaderProgramHandle, geometryShaderHandle);
            //}

            //GL.AttachShader(shaderProgramHandle, fragmentShaderHandle);

            //GL.LinkProgram(shaderProgramHandle);

            //Console.WriteLine(GL.GetProgramInfoLog(shaderProgramHandle).Trim());

            ////GL.UseProgram(shaderProgramHandle);

            //if(GL.GetError() != ErrorCode.NoError)
            //{
            //    throw new Exception("Error Linking Shader Program");
            //}

            //return shaderProgramHandle;
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
            //Regex regMFInt32 = new Regex("[+-]?\\d+\\.\\d+");
            //MatchCollection mc = regMFInt32.Matches(value);
            float[] f = Floats(value);

            return new Vector4(f[0],
                   f[1],
                   f[2],
                   f[3]);

            //return new Vector4(float.Parse(mc[0].Value),
            //                   float.Parse(mc[1].Value),
            //                   float.Parse(mc[2].Value),
            //                   float.Parse(mc[3].Value));
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
