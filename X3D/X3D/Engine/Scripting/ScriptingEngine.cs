// V8.Net, the Google V8 Wrapper for .NET on codeplex: https://v8dotnet.codeplex.com
//
// If using the windows platform and there are any issues referencing this dependancy 
// then try going into the context menu for each *.dll and unblock each on in the properties dialog
//
// If you are not using the windows platform, then another wrapper might be required to work in mono
// Portability will be under investigation.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using OpenTK.Input;
using V8.Net;
using X3D.Platform;
// Google V8 Engine via V8.Net wrapper

namespace X3D.Engine
{
    public delegate void ScriptingInitilizeDelegate(ScriptingEngine engine);

    public delegate void ScriptingShutdownDelegate(ScriptingEngine engine);

    public class ScriptingEngine : IDisposable
    {
        public const string SOURCE_NAME = "X3D 3.3";
        private static V8Engine v8;
        public static ScriptingEngine CurrentContext;
        private bool isDisposing;

        private ScriptingEngine()
        {
        }

        public event ScriptingInitilizeDelegate InitilizeEventHandler;
        public event ScriptingInitilizeDelegate ShutdownEventHandler;

        #region Public Static Methods

        public static ScriptingEngine CreateFromDocument(SceneGraphNode document)
        {
            ScriptingEngine engine;

            engine = new ScriptingEngine();

            engine.StartV8(document);

            CurrentContext = engine;

            return engine;
        }

        public static ScriptingEngine CreateFromManager(SceneManager manager)
        {
            SceneGraphNode document;
            ScriptingEngine engine;

            engine = new ScriptingEngine();

            if (manager.SceneGraph != null)
            {
                document = manager.SceneGraph.GetRoot();

                if (Script.EngineEnabled)
                    engine.StartV8(document);
            }

            CurrentContext = engine;

            return engine;
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Called when the scene is unloading.
        /// </summary>
        public void OnShutdown()
        {
            if (ShutdownEventHandler != null) ShutdownEventHandler(this);
        }

        /// <summary>
        ///     Called on Initilization of Head scripts.
        /// </summary>
        public void OnInitilize()
        {
            if (InitilizeEventHandler != null) InitilizeEventHandler(this);
        }

        public void CompileAndExecute(string script)
        {
            if (isDisposing) return;

            const string ES_REF = "ecmascript:";
            const string JS_REF = "javascript:";

            script = script.TrimStart();

            var start = script.Substring(0, ES_REF.Length).ToLower();

            script = start.StartsWith(ES_REF) ? script.Remove(0, ES_REF.Length) : script;
            script = start.StartsWith(JS_REF) ? script.Remove(0, JS_REF.Length) : script;

            using (var handle = v8.Compile(script, SOURCE_NAME))
            {
                if (!handle.IsError)
                    try
                    {
                        Handle result = v8.Execute(handle, true);
                        //handle.Dispose();
                    }
                    catch (Exception ex)
                    {
                        var tmp = Console.ForegroundColor;
                        Console.ForegroundColor = ConsoleColor.Red;

                        Console.WriteLine("(SCRIPT) {0}", ex.Message);

                        Console.ForegroundColor = tmp;
                    }
            }
        }

        public string Execute(string script)
        {
            if (isDisposing) return null;

            string result;

            using (Handle handle = v8.Execute(script, SOURCE_NAME))
            {
                result = handle.InternalHandle;
            }

            return result;
        }

        public void Dispose()
        {
            if (!isDisposing && !v8.IsDisposed)
            {
                isDisposing = true;

                ExecuteGlobalScriptFunction("shutdown");

                // SHUTDOWN all scripts
                GC.WaitForPendingFinalizers();
                GC.Collect();
                v8.ForceV8GarbageCollection();
                v8.Dispose();

                // Notify engine that we have terminated
                OnShutdown();
            }
        }

        #endregion

        #region Internal Methods

        public void UpdateKeyboardState(KeyboardDevice currentKeyboard)
        {
            if (v8.IsDisposed) return;

            // Copy keyboard state 
            //TODO: copy state quicker. Should be able to transfer over in O(1)
            var keyboard = new InternalHandle[(int)Key.LastKey];
            for (var i = 0; i < keyboard.Length; i++)
            {
                var k = currentKeyboard[(Key)i] ? 1 : 0;
                keyboard[i] = v8.CreateValue(k);
            }

            v8.GlobalObject.SetProperty("Keyboard", v8.CreateArray(keyboard));
        }

        internal void OnKeyDown(int keyCode, int charCode)
        {
            if (v8.IsDisposed) return;

            using (Handle functHandle = v8.Execute("document.onkeydown", SOURCE_NAME))
            {
                if (functHandle.InternalHandle.ValueType != JSValueType.CompilerError &&
                    functHandle.InternalHandle.IsFunction)
                {
                    var obj = v8.CreateObject();

                    obj.SetProperty("keyCode", keyCode);
                    obj.SetProperty("charCode", charCode);

                    functHandle.InternalHandle.StaticCall(obj);
                }
            }
        }

        internal void OnFirstHeadScript()
        {
            // First head script loaded
        }

        internal void OnHeadScriptsLoaded()
        {
            ExecuteGlobalScriptFunction("initilize");

            // Notify engine that we have initilized
            OnInitilize();
        }

        internal void ExecuteGlobalScriptFunction(string name)
        {
            if (v8.IsDisposed) return;

            using (Handle functHandle = v8.Execute(name, SOURCE_NAME))
            {
                if (functHandle.InternalHandle.ValueType != JSValueType.CompilerError &&
                    functHandle.InternalHandle.IsFunction) functHandle.InternalHandle.StaticCall();
            }
        }

        internal void OnRenderFrame(RenderingContext rc)
        {
            if (v8.IsDisposed) return;

            using (Handle functHandle = v8.Execute("onRenderFrame", SOURCE_NAME))
            {
                if (functHandle.InternalHandle.ValueType != JSValueType.CompilerError &&
                    functHandle.InternalHandle.IsFunction)
                {
                    var obj = v8.CreateObject();

                    obj.SetProperty("time", rc.Time);

                    functHandle.InternalHandle.StaticCall(obj);
                }
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Check v8 engine dependencies and fix any problems at runtime.
        /// </summary>
        private void deps()
        {
            //TODO: wait for v8.net.mono to mature

            // ensure v8.net dependencies are in bin folder
            var v8_net_proxy_interface64 =
                string.Format(@"x64{0}V8.Net.Proxy.Interface.x64.dll", Path.DirectorySeparatorChar);
            var v8_net_proxy64 = string.Format(@"x64{0}V8_Net_Proxy_x64.dll", Path.DirectorySeparatorChar);
            var v8_net_proxy_interface32 =
                string.Format(@"x86{0}V8.Net.Proxy.Interface.x86.dll", Path.DirectorySeparatorChar);
            var v8_net_proxy32 = string.Format(@"x86{0}V8_Net_Proxy_x86.dll", Path.DirectorySeparatorChar);

            var is64Bit = Environment.Is64BitProcess;

            var lib_proxy_interface = is64Bit ? v8_net_proxy_interface64 : v8_net_proxy_interface32;
            var lib_proxy = is64Bit ? v8_net_proxy64 : v8_net_proxy32;


            var hasInterface = File.Exists(lib_proxy_interface);
            var hasProxy = File.Exists(lib_proxy);

            if (X3DPlatform.IsWindows)
            {
                unblockDll("V8.Net.dll");
                unblockDll("V8.Net.SharedTypes.dll");
                unblockDll("x64\\V8.Net.Proxy.Interface.x64.dll");
                unblockDll("x64\\V8_Net_Proxy_x64.dll");
                unblockDll("x64\\icudt.dll");
                unblockDll("x86\\V8.Net.Proxy.Interface.x86.dll");
                unblockDll("x86\\V8_Net_Proxy_x86.dll");
                unblockDll("x86\\icudt.dll");
            }


            if (hasInterface && hasProxy)
                Console.WriteLine("V8 engine dependencies seem fine, they are {0}", is64Bit ? "x64" : "x86");
        }

        private void StartV8(SceneGraphNode root)
        {
            // ensure v8.net dependencies are fine for instantiation of V8Engine
            deps();

            v8 = new V8Engine();
            v8.RegisterType(typeof(X3DConsole), null, true, ScriptMemberSecurity.Locked);

            // Scene Access Interface
            // See: http://www.web3d.org/documents/specifications/19775-2/V3.3/Part02/servRef.html

            HookTypeSystem();

            v8.GlobalObject.SetProperty("console", X3DConsole.Current);
            v8.GlobalObject.SetProperty("document", root);

            v8.DynamicGlobalObject.window = v8.CreateFunctionTemplate("window").GetFunctionObject<WindowFunction>();
            v8.DynamicGlobalObject.browser = v8.CreateFunctionTemplate("browser").GetFunctionObject<BrowserFunction>();


            MapKeyValues();

            Console.WriteLine("X3D Scripting [enabled]");
        }

        private void MapKeyValues()
        {
            var type = typeof(Key);
            var keyNames = Enum.GetNames(type);
            var values = Enum.GetValues(type);
            var key = v8.CreateObject();

            for (var i = 0; i < keyNames.Length; i++)
            {
                var value = v8.CreateValue(values.GetValue(i));

                key.SetProperty(keyNames[i], value);
            }

            v8.GlobalObject.SetProperty("Key", key);
        }

        private void HookTypeSystem()
        {
            var asm = Assembly.GetAssembly(typeof(X3D));

            var x3dTypes = new List<Type>(asm.GetTypes())
                .Where(t => t.IsSubclassOf(typeof(SceneGraphNode)))
                .OrderBy(t => t.FullName).ToArray();

            // REGISTER X3D TYPE SYSTEM
            foreach (var x3dType in x3dTypes) v8.RegisterType(x3dType, null, true, ScriptMemberSecurity.Locked);

            //TODO: hook X3D simple types such as SFVec3, MFString, etc. 
            // Note that for some types there is a translation SFVec3 is interchanged to OpenTK.Vector3 internally
        }

        #endregion

        #region WINDOWS PLATFORM

        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DeleteFile(string name);

        private void unblockDll(string file)
        {
            DeleteFile(file + ":Zone.Identifier");
        }

        #endregion
    }
}