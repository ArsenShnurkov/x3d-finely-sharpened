using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using OpenTK.Graphics.OpenGL4;
using X3D.Core;
using X3D.Core;
using OpenTK;
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

        public void setFieldValue(string name, object value)
        {
            field field = (field)this.Children
                .FirstOrDefault(n => n.GetType() == typeof(field) 
                && n.getAttribute("name").ToString() == name);

            Type type;
            object convValue;
            Type conv;

            try
            {
                type = X3DTypeConverters.X3DSimpleTypeToManagedType(field.type);
                convValue = Convert.ChangeType(value, type);
                conv = convValue.GetType();

                if (conv == typeof(int)) UpdateField(name, X3DTypeConverters.ToString((int)convValue));
                if (conv == typeof(float)) UpdateField(name, X3DTypeConverters.ToString((float)convValue));
                if (conv == typeof(Vector3)) UpdateField(name, X3DTypeConverters.ToString((Vector3)convValue));
                if (conv == typeof(Vector4)) UpdateField(name, X3DTypeConverters.ToString((Vector4)convValue));
                if (conv == typeof(Matrix3))
                {
                    UpdateField(name, X3DTypeConverters.ToString((Matrix3)convValue));
                }
                if (conv == typeof(Matrix4))
                {
                    UpdateField(name, X3DTypeConverters.ToString((Matrix4)convValue));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("error");
            }

        }

        public void SetFieldValueSTR(string name, string value, string x3dType)
        {
            object v;
            Type type;

            v = X3DTypeConverters.StringToX3DSimpleTypeInstance(value, x3dType, out type);

            if (type == typeof(int)) SetFieldValue(name, (int)v);
            if (type == typeof(float)) SetFieldValue(name, (float)v);
            if (type == typeof(Vector3)) SetFieldValue(name, (Vector3)v);
            if (type == typeof(Vector4)) SetFieldValue(name, (Vector4)v);
            if (type == typeof(Matrix3))
            {
                Matrix3 m = (Matrix3)v;
                SetFieldValue(name, ref m);
            }
            if (type == typeof(Matrix4))
            {
                Matrix4 m = (Matrix4)v;
                SetFieldValue(name, ref m);
            }
        }

        public void SetFieldValue(string name, int value)
        {
            GL.Uniform1(GL.GetUniformLocation(this.ShaderHandle, name), value);

            //UpdateField(name, X3DTypeConverters.ToString(value));
        }

        public void SetFieldValue(string name, float value)
        {
            var loc = GL.GetUniformLocation(this.ShaderHandle, name);
            GL.Uniform1(loc, value);

            //UpdateField(name, X3DTypeConverters.ToString(value));
        }

        public void SetFieldValue(string name, Vector3 value)
        {
            GL.Uniform3(GL.GetUniformLocation(this.ShaderHandle, name), ref value);

            //UpdateField(name, X3DTypeConverters.ToString(value));
        }

        public void SetFieldValue(string name, Vector4 value)
        {
            GL.Uniform4(GL.GetUniformLocation(this.ShaderHandle, name), ref value);

            //UpdateField(name, X3DTypeConverters.ToString(value));
        }

        public void SetFieldValue(string name, ref Matrix3 value)
        {
            GL.UniformMatrix3(GL.GetUniformLocation(this.ShaderHandle, name), false, ref value);

            //TODO: convert matrix back to string and update field
            //UpdateField(name, X3DTypeConverters.ToString(value));
        }

        public void SetFieldValue(string name, ref Matrix4 value)
        {
            GL.UniformMatrix4(GL.GetUniformLocation(this.ShaderHandle, name), false, ref value);

            //TODO: convert matrix back to string and update field
            //UpdateField(name, X3DTypeConverters.ToString(value));
        }

        private void UpdateField(string name, string value)
        {
            List<field> fields = this.Children
                .Where(n => n.GetType() == typeof(field) && n.getAttribute("name").ToString() == name)
                .Select(n=> (field)n).ToList();

            foreach(field f in fields)
            {
                f.value = value;
            }
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

        public void ApplyFieldsAsUniforms(RenderingContext rc)
        {
            field[] fields = this.Children.Where(n => n.GetType() == typeof(field)).Select(n => (field)n) .ToArray();

            foreach(field f in fields)
            {
                string access = f.accessType.ToLower();

                if (access == "inputonly" || access == "inputoutput")
                {
                    SetFieldValueSTR(f.name, f.value, f.type);

                }
            }

        }
    }
}
