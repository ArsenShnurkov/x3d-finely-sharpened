using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using OpenTK.Graphics.OpenGL4;
using X3D.Core;
using X3D.Core;
using OpenTK;

namespace X3D
{
    public partial class ComposedShader
    {
        [XmlIgnore]
        public int ShaderHandle;

        [XmlIgnore]
        public List<field> Fields { get; set; }
        [XmlIgnore]
        public List<ShaderPart> ShaderParts = new List<ShaderPart>();

        [XmlIgnore]
        public bool Linked { get; internal set; }

        [XmlIgnore]
        public bool IsTessellator
        {
            get
            {
                var sps = ShaderParts.Any(s => s.type == shaderPartTypeValues.TESS_CONTROL 
                                         || s.type == shaderPartTypeValues.TESS_EVAL
                                         || s.type == shaderPartTypeValues.GEOMETRY
                        );

                return sps;
            }
        }

        /// <summary>
        /// If the shader is a built in system shader
        /// </summary>
        [XmlIgnore]
        public bool IsBuiltIn { get; internal set; }

        public override void Load()
        {
            Fields = new List<field>();
            ShaderParts = new List<ShaderPart>();
            base.Load();
        }

        public override void PostDescendantDeserialization()
        {
            base.PostDescendantDeserialization();

            GetParent<Shape>().IncludeComposedShader(this);
        }
        public ComposedShader Use()
        {
            GL.UseProgram(this.ShaderHandle);
            return this;
        }

        #region Field Setter Helpers

        public void SetFieldValue(string name, int value)
        {
            GL.Uniform1(GL.GetUniformLocation(this.ShaderHandle, name), value);
        }

        public void SetFieldValue(string name, float value)
        {
            GL.Uniform1(GL.GetUniformLocation(this.ShaderHandle, name), value);
        }

        public void SetFieldValue(string name, Vector3 value)
        {
            GL.Uniform3(GL.GetUniformLocation(this.ShaderHandle, name), ref value);
        }

        public void SetFieldValue(string name, Vector4 value)
        {
            GL.Uniform4(GL.GetUniformLocation(this.ShaderHandle, name), ref value);
        }

        public void SetFieldValue(string name, ref Matrix3 value)
        {
            GL.UniformMatrix3(GL.GetUniformLocation(this.ShaderHandle, name), false, ref value);
        }

        public void SetFieldValue(string name, ref Matrix4 value)
        {
            GL.UniformMatrix4(GL.GetUniformLocation(this.ShaderHandle, name), false, ref value);
        }

        #endregion

        public void Deactivate()
        {
            GL.UseProgram(0);
        }

        public void Link()
        {
            Console.WriteLine("ComposedShader {0}", this.language);

            if (this.language == "GLSL")
            {
                this.ShaderHandle = GL.CreateProgram();

                foreach (ShaderPart part in this.ShaderParts)
                {
                    ShaderCompiler.ApplyShaderPart(this.ShaderHandle, part);
                }

                GL.LinkProgram(this.ShaderHandle);
                string err = GL.GetProgramInfoLog(this.ShaderHandle).Trim();

                if(!string.IsNullOrEmpty(err))
                    Console.WriteLine(err);
                

                if (GL.GetError() != ErrorCode.NoError)
                {
                    throw new Exception("Error Linking ComposedShader Shader Program");
                }
                else
                {
                    this.Linked = true;

                    Console.WriteLine("ComposedShader [linked]"); //TODO: check for more link errors
                }
            }
            else
            {
                Console.WriteLine("ComposedShader language {0} unsupported", this.language);
            }
        }

        public void ApplyFieldsAsUniforms()
        {



        }
    }
}
