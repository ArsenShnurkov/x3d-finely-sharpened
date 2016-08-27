using System;

using OpenTK;
using OpenTK.Graphics;
using X3D.Engine;

namespace X3D.Runtime
{
    public class X3DBrowser : GameWindow, IDisposable
    {

        public X3DBrowser(VSyncMode VSync, string url, Resolution res, GraphicsMode mode) : base(res.Width, res.Height, mode)
        {
            this.VSync = VSync;
            this.URL = url;

            app = new X3DApplication(this);
        }

        public X3DBrowser(VSyncMode VSync, SceneGraph graph, Resolution res, GraphicsMode mode) : base(res.Width, res.Height, mode)
        {
            this.VSync = VSync;
            this.URL = string.Empty;
            this.Graph = graph;

            app = new X3DApplication(this, graph);
        }

        private X3DApplication app;

        public string URL { get; set; }

        public SceneGraph Graph { get; set; }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            app.Render(e);
            SwapBuffers();
        }

        protected override void OnResize(EventArgs e)
        {
            app.Resize();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            app.FrameUpdated(e);
        }

        protected override void OnLoad(EventArgs e)
        {
            // http://www.web3d.org/x3d/content/examples/HelloWorld.x3d

            app.BaseURL = URL;
            app.BaseMIME = "model/x3d+xml";
            app.Init(URL, app.BaseMIME);
        }

        public new void Dispose()
        {
            //base.Dispose();
            this.app.Dispose();
        }
    }
}