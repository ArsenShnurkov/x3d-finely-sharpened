// V8.Net, the Google V8 Wrapper for .NET on codeplex: https://v8dotnet.codeplex.com
//
// If using the windows platform and there are any issues referencing this dependancy 
// then try going into the context menu for each *.dll and unblock each on in the properties dialog
//
// If you are not using the windows platform, then another wrapper might be required to work in mono
// Portability will be under investigation.

using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

// Google V8 Engine via V8.Net wrapper
using V8.Net;

using X3D.Platform;

namespace X3D.Engine
{
    public delegate void ScriptingInitilizeDelegate(ScriptingEngine engine);
    public delegate void ScriptingShutdownDelegate(ScriptingEngine engine);

    public class ScriptingEngine : IDisposable
    {
        public const string SOURCE_NAME = "X3D 3.3";
        private bool isDisposing = false;
        private static V8Engine v8;
        public static ScriptingEngine CurrentContext = null;

        public event ScriptingInitilizeDelegate InitilizeEventHandler;
        public event ScriptingInitilizeDelegate ShutdownEventHandler;

        private ScriptingEngine() { }

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

                if(Script.EngineEnabled)
                    engine.StartV8(document);
            }
            
            CurrentContext = engine;

            return engine;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Called when the scene is unloading.
        /// </summary>
        public void OnShutdown()
        {
            if (ShutdownEventHandler != null)
            {
                ShutdownEventHandler(this);
            }
        }

        /// <summary>
        /// Called on Initilization of Head scripts.
        /// </summary>
        public void OnInitilize()
        {
            if (InitilizeEventHandler != null)
            {
                InitilizeEventHandler(this);
            }
        }

        public void CompileAndExecute(string script)
        {
            if (isDisposing) return;

            const string ES_REF = "ecmascript:";
            const string JS_REF = "javascript:";

            script = script.TrimStart();

            string start = script.Substring(0, ES_REF.Length).ToLower();

            script = start.StartsWith(ES_REF) ? script.Remove(0, ES_REF.Length) : script;
            script = start.StartsWith(JS_REF) ? script.Remove(0, JS_REF.Length) : script;

            using (InternalHandle handle = v8.Compile(script, SOURCE_NAME, false).AsInternalHandle)
            {
                if (!handle.IsError)
                {
                    try
                    {
                        Handle result = v8.Execute(handle, true);
                        //handle.Dispose();
                    }
                    catch (Exception ex)
                    {
                        ConsoleColor tmp = Console.ForegroundColor;
                        Console.ForegroundColor = ConsoleColor.Red;

                        Console.WriteLine("(SCRIPT) {0}", ex.Message);

                        Console.ForegroundColor = tmp;
                    }
                }
            }
        }

        public string Execute(string script)
        {
            if (isDisposing) return null;

            string result;

            using (Handle handle = v8.Execute(script, SOURCE_NAME, false))
            {
                result = handle.AsString;
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
            InternalHandle[] keyboard = new InternalHandle[(int)Key.LastKey];
            for (int i = 0; i < keyboard.Length; i++)
            {
                int k = currentKeyboard[(Key)i] ? 1 : 0;
                keyboard[i] = v8.CreateValue(k);
            }

            v8.GlobalObject.SetProperty("Keyboard", v8.CreateArray(keyboard));
        }

        internal void OnKeyDown(int keyCode, int charCode)
        {
            if (v8.IsDisposed) return;

            using (Handle functHandle = v8.Execute("document.onkeydown", SOURCE_NAME, false))
            {
                if (functHandle.ValueType != JSValueType.CompilerError && functHandle.IsFunction)
                {
                    InternalHandle obj = v8.CreateObject();

                    obj.SetProperty("keyCode", keyCode);
                    obj.SetProperty("charCode", charCode);

                    functHandle.AsInternalHandle.StaticCall(obj);
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

            using (Handle functHandle = v8.Execute(name, SOURCE_NAME, false))
            {
                if (functHandle.ValueType != JSValueType.CompilerError && functHandle.IsFunction)
                {
                    functHandle.AsInternalHandle.StaticCall();
                }
            }
        }

        internal void OnRenderFrame(RenderingContext rc)
        {
            if (v8.IsDisposed) return;

            using (Handle functHandle = v8.Execute("onRenderFrame", SOURCE_NAME, false))
            {
                if (functHandle.ValueType != JSValueType.CompilerError && functHandle.IsFunction)
                {
                    InternalHandle obj = v8.CreateObject();

                    obj.SetProperty("time", rc.Time);

                    functHandle.AsInternalHandle.StaticCall(obj);
                }
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Check v8 engine dependencies and fix any problems at runtime.
        /// </summary>
        private void deps()
        {
            //TODO: wait for v8.net.mono to mature

            // ensure v8.net dependencies are in bin folder
            string v8_net_proxy_interface64 = string.Format(@"x64{0}V8.Net.Proxy.Interface.x64.dll", System.IO.Path.DirectorySeparatorChar);
            string v8_net_proxy64 = string.Format(@"x64{0}V8_Net_Proxy_x64.dll", System.IO.Path.DirectorySeparatorChar);
            string v8_net_proxy_interface32 = string.Format(@"x86{0}V8.Net.Proxy.Interface.x86.dll", System.IO.Path.DirectorySeparatorChar);
            string v8_net_proxy32 = string.Format(@"x86{0}V8_Net_Proxy_x86.dll", System.IO.Path.DirectorySeparatorChar);

            bool is64Bit = Environment.Is64BitProcess;

            string lib_proxy_interface = is64Bit ? v8_net_proxy_interface64 : v8_net_proxy_interface32;
            string lib_proxy = is64Bit ? v8_net_proxy64 : v8_net_proxy32;

            
            bool hasInterface = System.IO.File.Exists(lib_proxy_interface);
            bool hasProxy = System.IO.File.Exists(lib_proxy);

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
            {
                Console.WriteLine("V8 engine dependencies seem fine, they are {0}", is64Bit ? "x64" : "x86");
            }
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
            Type type = typeof(Key);
            string[] keyNames = Enum.GetNames(type);
            Array values = Enum.GetValues(type);
            InternalHandle key = v8.CreateObject();

            for (int i = 0; i < keyNames.Length; i++)
            {
                InternalHandle value = v8.CreateValue(values.GetValue(i));

                key.SetProperty(keyNames[i], value);
            }

            v8.GlobalObject.SetProperty("Key", key);
        }

        private void HookTypeSystem()
        {
            Assembly asm = Assembly.GetAssembly(typeof(X3D));

            Type[] x3dTypes = (new List<Type>(asm.GetTypes()))
                .Where(t => t.IsSubclassOf(typeof(SceneGraphNode)))
                .OrderBy(t => t.FullName).ToArray();

            // REGISTER X3D TYPE SYSTEM
            foreach(Type x3dType in x3dTypes)
            {
                v8.RegisterType(x3dType, null, true, ScriptMemberSecurity.Locked);
            }

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
