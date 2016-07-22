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
        private int headScriptIndex = -1;
        private int numHeadScripts = 0;

        #region Rendering Methods

        public override void Load()
        {
            base.Load();

            parentHead = GetParent<head>();
            executeOnce = parentHead != null;

            numHeadScripts = parentHead == null ? 0 : parentHead.Children.Count(n => n.GetType() == typeof(Script));

            if (executeOnce && !executed)
                headScriptIndex++;

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
                var engine = ScriptingEngine.CurrentContext;
                
                // COMPILE

                if (engine != null && executeOnce && !executed)
                {
                    engine.CompileAndExecute(this.ScriptSource);

                    executed = true;

                    if (headScriptIndex == 0)
                    {
                        engine.OnFirstHeadScript();
                    }
                    if ((headScriptIndex == numHeadScripts - 1))
                    {
                        engine.OnHeadScriptsLoaded();
                    }
                }
            }
        }

        public override void Render(RenderingContext rc)
        {
            base.Render(rc);

            if (!executeOnce && !string.IsNullOrEmpty(this.ScriptSource))
            {
                var engine = ScriptingEngine.CurrentContext;

                // EXECUTE

                if (engine != null)
                {
                    engine.Execute(this.ScriptSource);
                }

                executed = true;
            }
        }

        #endregion
    }
}
