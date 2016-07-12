using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using X3D.Engine;
using System.IO;
using System.Xml.Serialization;
using X3D.Core;
using X3D.Parser;

namespace X3D
{
    public partial class ShaderPart
    {
        private ComposedShader parentShader;
        private Shape parentShape;

        [XmlIgnore]
        public int ShaderHandle { get; set; }

        [XmlIgnore]
        public shaderPartTypeValues Type
        {
            get
            {
                return this.type;
            }
            set
            {
                this.type = value;
            }
        }

        [XmlIgnore]
        public string[] Urls
        {
            get
            {
                return this.MFString;
            }
        }

        [XmlIgnore]
        public string ShaderSource;

        #region Rendering Methods

        public override void Load()
        {
            base.Load();

            parentShape = GetParent<Shape>();
            parentShader = GetParent<ComposedShader>();

            string file;
            string[] mf_urls;

            if (Urls != null)
            {
                file = Urls.FirstOrDefault();
                
                if (!string.IsNullOrEmpty(file))
                {
                    _url = _url.TrimStart();

                    if (_url.StartsWith(SceneManager.DATA_TEXT_PLAIN))
                    {
                        ShaderSource = file;

                        LinkShaderSource(ShaderSource);
                    }
                    else
                    {
                        file = file.Replace("\"", "");
                        file = SceneManager.CurrentLocation + "\\" + file;

                        if (SceneManager.IsMFString(file))
                        {
                            object resource;

                            mf_urls = SceneManager.GetMFString(file);

                            foreach (string url in mf_urls)
                            {
                                if (SceneManager.FetchSingle(url, out resource))
                                {
                                    Stream s;

                                    s = (Stream)resource;

                                    StreamReader sr = new StreamReader(s);
                                    ShaderSource = sr.ReadToEnd();

                                    s.Close();

                                    break;
                                }
                            }
                        }
                        else
                        {
                            ShaderSource = File.ReadAllText(file);

                            LinkShaderSource(ShaderSource);
                        }
                    }


                }
            }
        }

        public void LinkShaderSource(string source)
        {
            ShaderSource = X3DTypeConverters.UnescapeXMLValue(source).Trim();

            if (parentShader != null && parentShape != null)
            {
                parentShader.ShaderParts.Add(this);
            }
        }

        public override void Render(RenderingContext rc)
        {
            base.Render(rc);


        }

        #endregion
    }
}
