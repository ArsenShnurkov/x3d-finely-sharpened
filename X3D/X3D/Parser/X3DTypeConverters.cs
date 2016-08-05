using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using X3D.Core.Shading;

namespace X3D.Parser
{
    using MFString = List<string>;

    /// <summary>
    /// A Utility comprising a collection of handy X3D type converters.
    /// Use these converters where XML Serialisation fails and parts of X3D Model need to incorporate additional parse steps.
    /// </summary>
    public class X3DTypeConverters
    {

        public const string DATA_TEXT_PLAIN = "data:text/plain";
        public const string URN_WEB3D_MEDIA = "urn:web3d:media:"; // http://www.web3d.org/WorkingGroups/media/
        public const string URN = "urn:";

        private static Regex regMFString = new Regex(@"(?:[\""][^\""]+\"")|(?:['][^']+['])", RegexOptions.Compiled);

        private static Regex regMFStringSoubleQuotes = new Regex("([\"][^\"]+[\"]+\\s?[\"][^\"]+[\"]+)?", RegexOptions.Compiled);
        private static Regex regMFStringSingleQuotes = new Regex("", RegexOptions.Compiled);

        
        public static object StringToX3DSimpleTypeInstance(string value, string x3dType, out Type type)
        {
            type = X3DTypeConverters.X3DSimpleTypeToManagedType(x3dType);

            if (type == typeof(int)) return int.Parse(value);
            else if (type == typeof(float)) return float.Parse(value);
            else if (type == typeof(Vector3)) return X3DTypeConverters.SFVec3f(value);
            else if (type == typeof(Vector4)) return X3DTypeConverters.SFVec4f(value);
            else if (type == typeof(Matrix3))
            {
                Console.WriteLine("X3D type '{0}' not implemented", x3dType);
            }
            else if (type == typeof(Matrix4))
            {
                Console.WriteLine("X3D type '{0}' not implemented", x3dType);
            }
            else if (type == typeof(string))
            {
                if(x3dType == "SFInt32")
                {
                    return int.Parse(value);
                }
                else if (x3dType == "SFFloat")
                {
                    return float.Parse(value);
                }
                else if (x3dType == "SFVec2f")
                {
                    return X3DTypeConverters.SFVec2f(value);
                }
                else if (x3dType == "SFVec3f")
                {
                    return X3DTypeConverters.SFVec3f(value);
                }
            }
            else
            {
                Console.WriteLine("X3D type '{0}' not implemented",x3dType);
            }


            return null;
        }

        public static Type X3DSimpleTypeToManagedType(string x3dSimpleType)
        {
            Type t = typeof(string);

            switch (x3dSimpleType.ToUpper())
            {
                case "SFINT":
                    t = typeof(int);
                    break;
                case "SFFLOAT":
                    t = typeof(float);
                    break;
                case "SFVEC3":
                    t = typeof(Vector3);
                    break;
                case "SFVEC4":
                    t = typeof(Vector4);
                    break;
            }
            //TODO: mat3, mat4, ..

            return t;
        }

        public static float[] Vec3ToFloatArray(Vector3[] vectors)
        {
            List<float> floats = new List<float>();

            foreach(Vector3 vector in vectors)
            {
                floats.Add(vector.X);
                floats.Add(vector.Y);
                floats.Add(vector.Z);
            }

            return floats.ToArray();
        }
        public static float[] Vec3ToFloatArray(Vector4[] vectors)
        {
            List<float> floats = new List<float>();

            foreach (Vector4 vector in vectors)
            {
                floats.Add(vector.X);
                floats.Add(vector.Y);
                floats.Add(vector.Z);
                floats.Add(vector.W);
            }

            return floats.ToArray();
        }

        public static MFString MFString(string @string)
        {
            string tmp = removeQuotes(@string);
            if (tmp.StartsWith(DATA_TEXT_PLAIN))
            {
                //TODO: complete implementation of data:uri as seen in https://developer.mozilla.org/en-US/docs/Web/HTTP/data_URIs
                return new List<string> { tmp };
            }

            if(!(@string.TrimStart().StartsWith("\"") || @string.TrimStart().StartsWith("'")))
            {
                @string = "'" + @string;
            }
            if (!(@string.TrimEnd().EndsWith("\"") || @string.TrimEnd().EndsWith("'")))
            {
                @string = @string + "'";
            }

            List<string> lst = new MFString();
            Regex r;
            MatchCollection m;

            // Single and double quote parenthesis extraction
            // Order is important in MFString sequences.
            r = new Regex("(\"([^\"]+)\"|'([^']+)')\\s?",
                //RegexOptions.IgnorePatternWhitespace | 
                RegexOptions.Multiline | RegexOptions.ExplicitCapture);
            m = r.Matches(@string);
            if (m.Count > 0)
            {
                foreach(Match mm in m)
                {
                    for (int i = 0; i < mm.Groups.Count; i++)
                    {
                        String string1 = mm.Groups[i].ToString();

                        lst.Add(removeQuotes(string1));

                        break; // just do first group
                    }

                   
                }
               
            }

            return lst;
        }

        public static bool IsMFString(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return false;
            }

            if (str.Contains("\""))
            {
                str = str.Replace("\"", "");
            }

            if (str.Contains("'"))
            {
                str = str.Replace("'", "");
            }

            str = Regex.Replace(str, "\\s+", " ");

            return str.Split(' ').Length > 1;
        }

        public static string[] GetMFString(string str)
        {
            return MFString(str).ToArray();
        }

        public static string removeQuotes(string mfstring)
        {
            if (mfstring.Length > 0)
            {
                mfstring = mfstring.TrimEnd();
                mfstring = mfstring.TrimStart();

                if (mfstring[0] == '\'' || mfstring[0] == '"')
                {
                    mfstring = mfstring.Remove(0, 1);
                }
                if (mfstring.EndsWith("'") || mfstring.EndsWith("\""))
                {
                    mfstring = mfstring.Remove(mfstring.Length - 1, 1);
                }
            }
            return mfstring;
        }

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

        public static int[] ParseIndicies(string value)
        {
            List<int> indicies = new List<int>();

            Regex regMFInt32 = new Regex("\\S+\\S?");
            var mc = regMFInt32.Matches(value);
            int v;

            foreach (Match m in mc)
            {
                v = int.Parse(m.Value.Replace(",",""));
                //if (v != -1)
                //{
                    indicies.Add(v);
                //}
            }

            return indicies.ToArray();
        }

        public static string ToString(int @int)
        {
            return string.Format("{0}", @int);
        }

        public static string ToString(float @float)
        {
            return string.Format("{0}", @float);
        }

        public static string ToString(Vector3 sfVec3f)
        {
            return string.Format("{0} {1} {2}", sfVec3f.X, sfVec3f.Y, sfVec3f.Z);
        }

        public static string ToString(Vector4 sfVec4f)
        {
            return string.Format("{0} {1} {2} {3}", sfVec4f.X, sfVec4f.Y, sfVec4f.Z, sfVec4f.W);
        }

        public static string ToString(Matrix3 mat3)
        {
            throw new NotImplementedException();
        }

        public static string ToString(Matrix4 mat4)
        {
            throw new NotImplementedException();
        }

        public static Vector3 SFVec3(string value)
        {
            float[] values = value.Split(' ').Select(s => float.Parse(s)).ToArray();
            return new Vector3(values[0], values[1], values[2]);
        }
        public static Vector2 SFVec2f(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return Vector2.Zero;
            }

            //Regex regMFInt32 = new Regex("[+-]?\\d+\\.\\d+");
            //MatchCollection mc = regMFInt32.Matches(value);

            //return new Vector3(float.Parse(mc[0].Value), 
            //                   float.Parse(mc[1].Value), 
            //                   float.Parse(mc[2].Value));
            float[] f = Floats(value);

            return new Vector2(f[0], f[1]);
        }

        public static Vector3 SFVec3f(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return Vector3.Zero;
            }

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
            if (string.IsNullOrEmpty(value))
            {
                return Vector4.Zero;
            }
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

        public static int[] Integers(string value)
        {
            return ParseIndicies(value);
        }

        public static float[] Floats(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return new float[] { };
            }

            // 20160728 have to take into account the exponent that is also allowable for really large or really tiny floating point numbers
            // i.e. Some datasets can have floating point values specified like: -2.0440411E-16

            // this was the old regex  ([+-]?[0-9]+[.]?[0-9]?)+
            // (kept for reference) 

            // ([-+]?[0-9]+[.]?[0-9]?[eE]?[-+]?[0-9]?)+
            // [+-]?(?=\d*[.eE])(?=\.?\d)\d*\.?\d*(?:[eE][+-]?\d+)?

            Regex regMFInt32 = new Regex(@"([-+]?[0-9]+[.]?[0-9]?[eE]?[-+]?[0-9]?)+"); // [+-]?\\d+\\.\\d+
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
            if (string.IsNullOrEmpty(value))
            {
                return 0.0f;
            }
            Regex regMFInt32 = new Regex("[+-]?\\d+\\.\\d+");
            MatchCollection mc = regMFInt32.Matches(value);

            return Floats(value).FirstOrDefault();
        }

        public static Vector2[] MFVec2f(string value)
        {
            List<Vector2> coords = new List<Vector2>();
            if (string.IsNullOrEmpty(value))
            {
                return coords.ToArray();
            }

            Regex regMFInt32 = new Regex(value.Contains(',') ? "\\S+\\S?\\s+\\S+\\S?" : "\\S+\\S?\\s+\\S+\\S?\\s+");
            //Regex regMFInt32 = new Regex("[+-]?\\d+\\.\\d?[^,]+");
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
            if (string.IsNullOrEmpty(value))
            {
                return coords.ToArray();
            }

            Regex regMFInt32 = new Regex(value.Contains(',') ? "\\S+\\S?\\s+\\S+\\S?\\s+\\S+\\S?" : "\\S+\\S?\\s+\\S+\\S?\\s+\\S+\\S?\\s+");
            var mc = regMFInt32.Matches(value + " ");

            List<float> lst = new List<float>();
            float[] vec3;

            //int i = 0;
            foreach (Match m in mc)
            {
                vec3 = m.Value.Replace(",", "")
                    .Split(' ')
                    .Where(s => !string.IsNullOrEmpty(s))
                    .Select(s => float.Parse(s)).ToArray();

                //vec3 = Floats(m.Value.Replace(",", "")); // still not parsing exponents correctly

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

            int i = 0;
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
