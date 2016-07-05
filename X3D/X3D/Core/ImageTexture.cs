using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using X3D.Parser;
using X3D.Engine;

namespace X3D
{
    public partial class ImageTexture
    {
        private GLTexture _texture = null;

        public override void Load()
        {
            base.Load();

            string _url = url.FirstOrDefault();

            if (!string.IsNullOrEmpty(_url))
            {
                _url = _url.Replace("\"", "");
                _url = SceneManager.CurrentLocation + "\\" + _url;

                _texture = GLTexture.LoadTexture(_url);
            }
        }

        public override void Render(RenderingContext rc)
        {
            base.Render(rc);

            if (_texture != null)
            {
                _texture.Bind();
            }
        }
    }
}
