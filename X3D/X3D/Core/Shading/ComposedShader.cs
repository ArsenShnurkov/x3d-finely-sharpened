using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using OpenTK.Graphics.OpenGL4;
using X3D.Parser;

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
                    Helpers.ApplyShaderPart(this.ShaderHandle, part);
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
