using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using V8.Net;

namespace X3D.Engine
{
    /// <summary>
    /// Scene Access Interface
    /// See: http://www.web3d.org/documents/specifications/19775-2/V3.3/Part02/servRef.html
    /// </summary>
    public class BrowserFunction : V8Function
    {

        public static string AppInfo
        {
            get
            {
                Assembly asm;
                AssemblyProductAttribute productName;
                //AssemblyVersionAttribute ver;
                AssemblyDescriptionAttribute desc;

                asm = Assembly.GetAssembly(typeof(BrowserFunction));
                productName = (AssemblyProductAttribute)Attribute.GetCustomAttribute(asm, typeof(AssemblyProductAttribute));
                //ver=(AssemblyVersionAttribute)Attribute.GetCustomAttribute(asm,typeof(AssemblyVersionAttribute));
                desc = (AssemblyDescriptionAttribute)Attribute.GetCustomAttribute(asm, typeof(AssemblyDescriptionAttribute));

                string version = asm.GetName().Version.ToString();
                return productName.Product + " " + version + " \"" + desc.Description + "\"";
            }
        }


        public override ObjectHandle Initialize(bool isConstructCall, params InternalHandle[] args)
        {
            //SetProperty("getName", Engine.CreateFunctionTemplate().GetFunctionObject(this.GetName));
            //SetProperty("getVersion", Engine.CreateFunctionTemplate().GetFunctionObject(this.GetVersion));

            Type fot = typeof(JSFunction);
            JSFunction instance;

            MethodInfo[] methods = this.GetType().GetMethods(BindingFlags.Public 
                | BindingFlags.Instance 
                | BindingFlags.FlattenHierarchy 
                | BindingFlags.DeclaredOnly);

            foreach(MethodInfo method in methods)
            {
                if(method.ReturnType == typeof(InternalHandle))
                {
                    string name = method.Name.First().ToString().ToLower() + method.Name.Substring(1);

                    instance = (JSFunction)Delegate.CreateDelegate(fot, this, method);

                    SetProperty(name, Engine.CreateFunctionTemplate().GetFunctionObject(instance));
                }

            }


            return base.Initialize(isConstructCall, args);
        }

        #region Public Static Methods

        /// <summary>
        /// Name of the X3D Browser
        /// </summary>
        public InternalHandle GetName(V8Engine engine, bool isConstructCall, InternalHandle _this, params InternalHandle[] args)
        {
            return engine.CreateValue(AppInfo);
        }

        public InternalHandle GetVersion(V8Engine engine, bool isConstructCall, InternalHandle _this, params InternalHandle[] args)
        {
            if (isConstructCall)
            {
                return _this;
            }
            else
            {

            }

            return InternalHandle.Empty;
        }

        public InternalHandle GetCurrentSpeed(V8Engine engine, bool isConstructCall, InternalHandle _this, params InternalHandle[] args)
        {
            if (isConstructCall)
            {
                return _this;
            }
            else
            {

            }

            return InternalHandle.Empty;
        }

        public InternalHandle GetCurrentFramerate(V8Engine engine, bool isConstructCall, InternalHandle _this, params InternalHandle[] args)
        {
            if (isConstructCall)
            {
                return _this;
            }
            else
            {

            }

            return InternalHandle.Empty;
        }

        public InternalHandle GetSupportedProfiles(V8Engine engine, bool isConstructCall, InternalHandle _this, params InternalHandle[] args)
        {
            if (isConstructCall)
            {
                return _this;
            }
            else
            {

            }

            return InternalHandle.Empty;
        }

        public InternalHandle GetSupportedComponents(V8Engine engine, bool isConstructCall, InternalHandle _this, params InternalHandle[] args)
        {
            if (isConstructCall)
            {
                return _this;
            }
            else
            {

            }

            return InternalHandle.Empty;
        }

        public InternalHandle CreateScene(V8Engine engine, bool isConstructCall, InternalHandle _this, params InternalHandle[] args)
        {
            if (isConstructCall)
            {
                return _this;
            }
            else
            {

            }

            return InternalHandle.Empty;
        }

        public InternalHandle ReplaceWorld(V8Engine engine, bool isConstructCall, InternalHandle _this, params InternalHandle[] args)
        {
            if (isConstructCall)
            {
                return _this;
            }
            else
            {

            }

            return InternalHandle.Empty;
        }

        public InternalHandle ImportDocument(V8Engine engine, bool isConstructCall, InternalHandle _this, params InternalHandle[] args)
        {
            if (isConstructCall)
            {
                return _this;
            }
            else
            {

            }

            return InternalHandle.Empty;
        }

        public InternalHandle LoadURL(V8Engine engine, bool isConstructCall, InternalHandle _this, params InternalHandle[] args)
        {
            if (isConstructCall)
            {
                return _this;
            }
            else
            {

            }

            return InternalHandle.Empty;
        }

        public InternalHandle SetDescription(V8Engine engine, bool isConstructCall, InternalHandle _this, params InternalHandle[] args)
        {
            if (isConstructCall)
            {
                return _this;
            }
            else
            {

            }

            return InternalHandle.Empty;
        }

        public InternalHandle CreateX3DFromString(V8Engine engine, bool isConstructCall, InternalHandle _this, params InternalHandle[] args)
        {
            if (isConstructCall)
            {
                return _this;
            }
            else
            {

            }

            return InternalHandle.Empty;
        }

        public InternalHandle CreateX3DFromStream(V8Engine engine, bool isConstructCall, InternalHandle _this, params InternalHandle[] args)
        {
            if (isConstructCall)
            {
                return _this;
            }
            else
            {

            }

            return InternalHandle.Empty;
        }

        public InternalHandle CreateX3DFromURL(V8Engine engine, bool isConstructCall, InternalHandle _this, params InternalHandle[] args)
        {
            if (isConstructCall)
            {
                return _this;
            }
            else
            {

            }

            return InternalHandle.Empty;
        }

        public InternalHandle GetRenderingProperties(V8Engine engine, bool isConstructCall, InternalHandle _this, params InternalHandle[] args)
        {
            if (isConstructCall)
            {
                return _this;
            }
            else
            {

            }

            return InternalHandle.Empty;
        }

        public InternalHandle GetBrowserProperties(V8Engine engine, bool isConstructCall, InternalHandle _this, params InternalHandle[] args)
        {
            if (isConstructCall)
            {
                return _this;
            }
            else
            {

            }

            return InternalHandle.Empty;
        }

        public InternalHandle ChangeViewpoint(V8Engine engine, bool isConstructCall, InternalHandle _this, params InternalHandle[] args)
        {
            if (isConstructCall)
            {
                return _this;
            }
            else
            {

            }

            return InternalHandle.Empty;
        }

        /// <summary>
        /// Prints a string message to the browser's console.
        /// </summary>
        public InternalHandle Print(V8Engine engine, bool isConstructCall, InternalHandle _this, params InternalHandle[] args)
        {
            if (args.Length >= 1)
            {
                string message = args[0].AsString;

                Console.WriteLine("(SCRIPT) {0}", message);
            }

            return InternalHandle.Empty;
        }

        public InternalHandle Dispose(V8Engine engine, bool isConstructCall, InternalHandle _this, params InternalHandle[] args)
        {
            if (isConstructCall)
            {
                return _this;
            }
            else
            {

            }

            return InternalHandle.Empty;
        }

        #endregion
    }
}
