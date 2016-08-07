using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using OpenTK.Graphics.OpenGL;
using OpenTK;
using System.Drawing;
using OpenTK.Graphics;

namespace GraphDebugger.OpenGL
{
    public class Draw
    {

        public static void Lines(List<Vector3> vectors, Color line_color)
        {
            GL.Begin(BeginMode.Lines);

            for (int i = 0; i < vectors.Count; i += 2)
            {
                var a = vectors[i];
                var b = vectors[i + 1]; 

                GL.Vertex3(a);
                GL.Color4((Color4)line_color);

                GL.Vertex3(b);
                GL.Color4((Color4)line_color);
            }
                

            GL.End();
            GL.Color4(1f, 1f, 1f, 1f);
        }

        public static void Line(Vector3 a, Vector3 b, Color line_color)
        {
            GL.Vertex3(a);
            GL.Color4((Color4)line_color);

            GL.Vertex3(b);
            GL.Color4((Color4)line_color);
        }

    }

    public class MatrixCollection
    {
        public MatrixCollection Parent { get; set; }

        internal Matrix4 projection, 
                        modelview;

        internal static Matrix4 DefaultModelview = Matrix4.Identity ;

        private int uniform_mview, 
                    uniform_pview;

        public void GetMatrixUniforms(ShaderProgram shader)
        {
            //uniform_pview = shader.GetUniformLocation("projection");
            //uniform_mview = shader.GetUniformLocation("modelview");
        }

        public void SetMatrixUniforms()
        {
            if (Parent == null)
            {
                // root transform
                GL.MatrixMode(MatrixMode.Projection);
                GL.LoadMatrix(ref projection);
            }

            //modelview *= projection;

            //GL.UniformMatrix4(uniform_pview, false, ref projection);
            //GL.UniformMatrix4(uniform_mview, false, ref modelview);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref modelview);
        }

        public void ApplyLocalTransformations(Vector3 translation, Vector3 scale)
        {
            modelview = Matrix4.Identity
                * Matrix4.CreateTranslation(translation)
                * Matrix4.Scale(scale);

            if (Parent == null)
            {
                
            }
            else
            {
                modelview *= Parent.modelview;
            }

            

            //modelview *= Matrix4.CreateTranslation(translation); // Matrix4.Mult(ref translation, ref modelview, out modelview);
            //modelview *= Matrix4.Scale(scale); // Matrix4.Mult(ref scalematrix, ref modelview, out modelview);
        }

        public void UpdateProjection(INativeWindow window)
        {
            this.projection = CreatePerspectiveFieldOfView(window);
        }

        public static MatrixCollection CreateInitialMatricies()
        {
            MatrixCollection mc = new MatrixCollection();

            mc.modelview = Matrix4.Identity;
            //mc.modelview = Matrix4.LookAt(Vector3.UnitX, Vector3.UnitZ, Vector3.UnitY);
            mc.projection = Matrix4.Identity;

            return mc;
        }

        public static MatrixCollection CreateInitialMatricies(INativeWindow window)
        {
            MatrixCollection mc = new MatrixCollection();

            mc.modelview = Matrix4.Identity;
            //mc.modelview = Matrix4.LookAt(Vector3.UnitX, Vector3.UnitZ, Vector3.UnitY);
            mc.projection = CreatePerspectiveFieldOfView(window);

            return mc;
        }

        public static Matrix4 CreatePerspectiveFieldOfView(INativeWindow window)
        {
            float aspect = (float)window.Width / (float)window.Height;

            //return Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver4, aspect, 1.0f, 500.0f);
            return Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver4, aspect, 0.00001f, 500.0f);
        }



        public void BeginModel()
        {
            //Matrices.PushMatrix(this);
        }

        public void EndModel()
        {
            //Matrices.PopMatrix();
        }
    }

    public class Matrices
    {
        /// <summary>
        /// scene object transformation hyrachy
        /// </summary>
        public static Stack<MatrixCollection> k = new Stack<MatrixCollection>();

        public static void PushMatrix(MatrixCollection m)
        {
            //GL.PushMatrix();
            k.Push(m);
        }
        public static MatrixCollection PopMatrix()
        {
            //GL.PopMatrix();
            return k.Pop();
        }
        
    }

    public class ShaderProgram
    {
        public int ID { get; set; }

        public int LinkStatus { get; set; }

        public string ProgramInfoLog { get; set; }

        public void Use()
        {
            GL.UseProgram(ID);
        }

        public void UseZero()
        {
            GL.UseProgram(0);
        }

        public int GetAttribLocation(string name)
        {
            int attribute_loc;

            attribute_loc = GL.GetAttribLocation(this.ID, name);

            if (attribute_loc == -1)
            {
                Console.WriteLine("[Error] binding attribute variable " + name);
            }

            return attribute_loc;
        }

        public int GetUniformLocation(string name)
        {
            int uniform_loc;

            uniform_loc = GL.GetUniformLocation(this.ID, name);

            if (uniform_loc == -1)
            {
                Console.WriteLine("[Error] binding uniform variable " + name);
            }

            return uniform_loc;
        }

        public static ShaderProgram CreateLoadAndLinkProgram(IEnumerable<Shader> shaders)
        {
            ShaderProgram pgm;
            int shader_id;
            int link_status;

            pgm = new ShaderProgram();
            pgm.ID = GL.CreateProgram();

            foreach (Shader shader in shaders)
            {
                if (shader.GetType() == typeof(VertexShader))
                {
                    LoadShader(shader.FileName, ShaderType.VertexShader, pgm.ID, out shader_id);
                    shader.ID = shader_id;
                }
                else if (shader.GetType() == typeof(FragmentShader))
                {
                    LoadShader(shader.FileName, ShaderType.FragmentShader, pgm.ID, out shader_id);
                    shader.ID = shader_id;
                }
            }

            GL.LinkProgram(pgm.ID);
            GL.GetProgram(pgm.ID, ProgramParameter.LinkStatus, out link_status);

            pgm.LinkStatus = link_status;
            pgm.ProgramInfoLog = GL.GetProgramInfoLog(pgm.ID);

            Console.WriteLine("[Program] " + pgm.ProgramInfoLog);            

            return pgm;
        }

        public static void LoadShader(String filename, ShaderType type, int program, out int address)
        {
            address = GL.CreateShader(type);

            string code;
            using (StreamReader sr = new StreamReader(filename))
            {
                code = sr.ReadToEnd();
                GL.ShaderSource(address, code);
            }
            GL.CompileShader(address);
            GL.AttachShader(program, address);

            string shaderInfo = GL.GetShaderInfoLog(address).Trim();

            Console.WriteLine("[Shader] " + shaderInfo);
        }
    }

    public abstract class Shader
    {
        public int ID { get; internal set; }
        public string FileName { get; set; }
    }

    public class VertexShader : Shader { }
    public class FragmentShader : Shader { }

    public interface RenderableVertexBufferObjects
    {
        void EnableVertexAttribArrays();
        void DisableVertexAttribArrays();
        void Init(ShaderProgram shader);
        void Render();
    }
}
