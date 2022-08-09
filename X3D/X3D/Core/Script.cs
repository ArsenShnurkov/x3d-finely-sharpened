using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Input;
using X3D.Engine;

namespace X3D
{
    /// <summary>
    ///     http://www.web3d.org/documents/specifications/19775-1/V3.3/Part01/components/scripting.html
    /// </summary>
    public partial class Script
    {
        public static bool ScriptingEnabled = false;
        public static bool EngineEnabled = true;

        private static bool documentEventsBound = false;
        private List<field> _fields;

        private bool compiled = false;

        //private bool executed = false;
        private int headScriptIndex = -1;
        private bool isHeadScript = false;
        private int numHeadScripts = 0;

        private head parentHead = null;

        #region Rendering Methods

        public override void Load()
        {
            base.Load();

            parentHead = GetParent<head>();
            isHeadScript = parentHead != null;

            numHeadScripts = parentHead == null ? 0 : parentHead.Children.Count(n => n.GetType() == typeof(Script));

            if (isHeadScript && !compiled)
                headScriptIndex++;

            if (!string.IsNullOrEmpty(url))
            {
                object resource;

                if (SceneManager.Fetch(this.url, out resource))
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

            _fields = ChildrenWithAppliedReferences
                .Where(n => (typeof(field).IsInstanceOfType(n)))
                .Select(n => (field)n)
                .ToList();

            if (!string.IsNullOrEmpty(this.ScriptSource))
            {
                var engine = ScriptingEngine.CurrentContext;

                // COMPILE

                if (engine != null && !compiled)
                {
                    engine.CompileAndExecute(this.ScriptSource);

                    if (headScriptIndex == 0)
                    {
                        engine.OnFirstHeadScript();
                    }

                    if ((headScriptIndex == numHeadScripts - 1))
                    {
                        engine.OnHeadScriptsLoaded();
                    }

                    if (!documentEventsBound)
                    {
                        // Bind events, then when they occur, call associated dom javascript events

                        // KEYBINDINGS


                        rc.Keyboard.KeyDown += (object sender, KeyboardKeyEventArgs e) =>
                        {
                            int charCode = (int)e.ScanCode;
                            int keyCode;

                            if (Consts.ScanCodeToKeyCodeMap.ContainsKey(charCode))
                            {
                                // Convert scan code to javascript keyCode
                                keyCode = Consts.ScanCodeToKeyCodeMap[charCode];

                                // Send both scan code an javascript keyCode
                                engine.OnKeyDown(keyCode, charCode);
                            }
                            else
                            {
                                Console.WriteLine("*** key {0} not bound *** ", charCode);
                            }
                        };

                        // MOUSE POINTER BINDINGS
                        // TODO

                        documentEventsBound = true;
                    }

                    compiled = true;
                }
            }
        }

        public override void Render(RenderingContext rc)
        {
            base.Render(rc);

            if (compiled && !string.IsNullOrEmpty(this.ScriptSource))
            {
                var engine = ScriptingEngine.CurrentContext;

                // EXECUTE

                if (engine != null)
                {
                    engine.UpdateKeyboardState(rc.Keyboard);

                    //engine.Execute(this.ScriptSource); // not efficient, see OnRenderFrame

                    engine.OnRenderFrame(rc);
                }

                //executed = true;
            }
        }

        #endregion
    }
}