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
        public static bool ScriptingEnabled = false;

        private head parentHead = null;
        private bool executeOnce = false;
        private bool executed = false;

        #region Rendering Methods

        public override void Load()
        {
            base.Load();

            parentHead = GetParent<head>();

            executeOnce = parentHead != null;


            if (!string.IsNullOrEmpty(url))
            {
                object resource;

                if(SceneManager.Fetch(this.url, out resource))
                {
                    this.ScriptSource = (string)resource;

                    ScriptingEnabled = true;
                }
            }
            else if (!string.IsNullOrEmpty(this.ScriptSource))
            {
                ScriptingEnabled = true;
            }
        }

        public override void PreRenderOnce(RenderingContext rc)
        {
            base.PreRenderOnce(rc);

            if (!string.IsNullOrEmpty(this.ScriptSource))
            {
                // COMPILE

                if (ScriptingEngine.CurrentContext != null && executeOnce && !executed)
                {
                    ScriptingEngine.CurrentContext.CompileAndExecute(this.ScriptSource);
                }
            }
        }

        public override void Render(RenderingContext rc)
        {
            base.Render(rc);

            if (!executeOnce && !string.IsNullOrEmpty(this.ScriptSource))
            {
                // EXECUTE

                if(ScriptingEngine.CurrentContext != null)
                {
                    ScriptingEngine.CurrentContext.Execute(this.ScriptSource);
                }

                executed = true;
            }
        }



        #endregion
    }
}
