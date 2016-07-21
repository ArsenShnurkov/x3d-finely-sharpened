using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using X3D.Engine;

namespace X3D
{
    /// <summary>
    /// http://www.web3d.org/documents/specifications/19775-1/V3.3/Part01/components/scripting.html
    /// </summary>
    public partial class Script
    {
        #region Rendering Methods

        public override void Load()
        {
            base.Load();

            if (!string.IsNullOrEmpty(url))
            {
                object resource;

                if(SceneManager.Fetch(this.url, out resource))
                {
                    this.ScriptSource = (string)resource;
                }
            }
        }

        public override void Render(RenderingContext rc)
        {
            base.Render(rc);

            if (!string.IsNullOrEmpty(this.ScriptSource))
            {
                var engine = ScriptingEngine.CurrentContext;

                if(engine != null)
                {
                    var result = engine.Execute(this.ScriptSource);
                }
            }
        }



        #endregion
    }
}
