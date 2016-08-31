using System;

using OpenTK;
using OpenTK.Graphics;
using X3D.Engine;

namespace X3D.Runtime
{
    public class X3DBrowser : GameWindow, IDisposable
    {
        #region Public Properties

        public SceneGraph Graph { get; set; }
        public string URL { get; set; }

        #endregion

        #region Private Fields

        private X3DApplication app;

        #endregion

        #region Constructors

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

        #endregion

        #region Rendering Methods

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

        #endregion

        #region Destructors

        public new void Dispose()
        {
            //base.Dispose();
            this.app.Dispose();
        }

        #endregion
    }
}