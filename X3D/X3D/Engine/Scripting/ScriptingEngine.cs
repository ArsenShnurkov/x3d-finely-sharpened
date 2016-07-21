using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using V8.Net;

namespace X3D.Engine
{
    public class ScriptingEngine
    {
        public const string SOURCE_NAME = "X3D 3.3";
        private V8Engine v8;
        public static ScriptingEngine CurrentContext = null;


        private ScriptingEngine() { }

        public static ScriptingEngine CreateFromDocument(SceneGraphNode document)
        {
            ScriptingEngine engine;

            engine = new ScriptingEngine();

            engine.StartV8(document);

            CurrentContext = engine;

            return engine;
        }

        public static ScriptingEngine Initilize(SceneManager manager)
        {
            SceneGraphNode document;
            ScriptingEngine engine;

            engine = new ScriptingEngine();

            if (manager.SceneGraph != null)
            {
                document = manager.SceneGraph.GetRoot();

                engine.StartV8(document);

            }

            CurrentContext = engine;

            return engine;
        }

        private void StartV8(SceneGraphNode document)
        {
            v8 = new V8Engine();
            v8.RegisterType(typeof(X3DConsole), null, true, ScriptMemberSecurity.Locked);
            HookTypeSystem();
            v8.GlobalObject.SetProperty("console", X3DConsole.Current);
            v8.GlobalObject.SetProperty("window", X3DWindow.Current);
            v8.GlobalObject.SetProperty("document", document);
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
        }

        public string Execute(string script)
        {
            string result;

            using (Handle handle = v8.Execute(script, SOURCE_NAME, false))
            {
                result = handle.AsString;
            }

            return result;
        }
    }
}
