using System.IO;
using System.Linq;
using System.Xml.Serialization;
using X3D.Engine;
using X3D.Parser;

namespace X3D
{
    public partial class ShaderPart
    {
        private ComposedShader parentShader;
        private Shape parentShape;

        [XmlIgnore] public int ShaderHandle { get; set; }

        [XmlIgnore]
        public shaderPartTypeValues Type
        {
            get => type;
            set => type = value;
        }

        [XmlIgnore] public string[] Urls => MFString;

        //[XmlIgnore]
        //public string ShaderSource;

        #region Rendering Methods

        public override void Load()
        {
            base.Load();

            parentShape = GetParent<Shape>();
            parentShader = GetParent<ComposedShader>();

            string file;
            string[] mf_urls;

            if (!string.IsNullOrEmpty(ShaderSource))
            {
                LinkShaderSource(ShaderSource);
            }
            else if (Urls != null)
            {
                file = Urls.FirstOrDefault();

                if (!string.IsNullOrEmpty(file))
                {
                    _url = _url.TrimStart();

                    if (_url.StartsWith(X3DTypeConverters.DATA_TEXT_PLAIN))
                    {
                        ShaderSource = _url.Remove(0, X3DTypeConverters.DATA_TEXT_PLAIN.Length).TrimStart();

                        LinkShaderSource(ShaderSource);
                    }
                    else
                    {
                        file = file.Replace("\"", "");
                        file = SceneManager.CurrentLocation + "\\" + file;

                        if (X3DTypeConverters.IsMFString(file))
                        {
                            object resource;

                            mf_urls = X3DTypeConverters.GetMFString(file);

                            foreach (var url in mf_urls)
                                if (SceneManager.FetchSingle(url, out resource))
                                {
                                    Stream s;

                                    s = (Stream)resource;

                                    var sr = new StreamReader(s);
                                    ShaderSource = sr.ReadToEnd();

                                    s.Close();

                                    break;
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

            if (parentShader != null && parentShape != null) parentShader.ShaderParts.Add(this);
        }

        public override void Render(RenderingContext rc)
        {
            base.Render(rc);
        }

        #endregion
    }
}