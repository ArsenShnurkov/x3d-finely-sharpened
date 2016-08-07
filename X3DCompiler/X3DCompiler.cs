using ILRepacking;
using Microsoft.CSharp;
using OpenTK;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;
using X3D.Engine;
using X3D.Parser;

namespace X3D
{
    public enum PlatformTarget
    {
        Windows
    }

    /// <summary>
    /// The X3D Compiler for the x3d-finely-sharpened project.
    /// Compiles X3D scenes into platform specific executables.
    /// </summary>
    public class X3DCompiler
    {
        /// <summary>
        /// Compiles a scene graph into a windows platform .NET executable.
        /// </summary>
        /// <param name="graph">
        /// The specified scene graph to compile source from.
        /// </param>
        /// <returns>
        /// A Stream comprising the compilation output 
        /// </returns>
        public static Stream Compile(SceneGraph graph, PlatformTarget platform = PlatformTarget.Windows)
        {
            Stream s;
            MemoryStream ms;
            ILRepack repack;
            RepackOptions options;
            string source;
            bool compiled;
            string tmpExeFile;
            string tmpExeFileRepacked;
            byte[] data;

            source = CompileAsObjects(graph);

            compiled = CompileCSharp(source, out tmpExeFile);

            tmpExeFileRepacked = Path.GetTempFileName()+".exe";

            options = new RepackOptions()
            {
                TargetKind = ILRepack.Kind.Exe,
                Parallel = true,
                Internalize = true,
                InputAssemblies = new string[] { tmpExeFile },
                OutputFile = tmpExeFileRepacked
            };
            repack = new ILRepack(options);

            data = File.ReadAllBytes(tmpExeFileRepacked);
            ms = new MemoryStream(data);

            s = ms;

            return s;
        }

        /// <summary>
        /// Builds a CSharp source code representation of a Scene Graph.
        /// </summary>
        /// <param name="graph">
        /// The Specified Scene Graph to compile.
        /// </param>
        /// <returns>
        /// CSharp source code strings.
        /// </returns>
        public static string CompileAsObjects(SceneGraph graph)
        {
            string source;
            Queue<SceneGraphNode> work_items;
            SceneGraphNode n, root;
            string nodeName;
            List<attribute> attributes;
            List<string> lines;
            string line;
            int i;
            //IEnumerable<KeyValuePair<string, string>> attributeItems;
            const string NODE_IDENTIFIER = "n";

            lines = new List<string>();
            root = graph.GetRoot();
            work_items = new Queue<SceneGraphNode>();
            work_items.Enqueue(root);

            source = "";

            while(work_items.Count> 0)
            {
                n = work_items.Dequeue();

                attributes = GetAttributes(n);

                nodeName = n.ToString().Replace("X3D.", "");

                line = string.Format("{0} {2}{1} = new {0}();", nodeName, n._ID, NODE_IDENTIFIER);
                lines.Add(line);

                if(n.Parent != null)
                {
                    line = string.Format("{2}{0}.Parent = {2}{1};", n._ID, n.Parent._ID, NODE_IDENTIFIER);
                    lines.Add(line);
                }

                line = string.Format("{2}{0}.Depth = {1};", n._ID, n.Depth, NODE_IDENTIFIER);
                lines.Add(line);

                //attributeItems = attributes.AllKeys.SelectMany(attributes.GetValues, (k, v) => new KeyValuePair<string,string>(k, v ));

                foreach(attribute attribute in attributes)
                {
                    if (attribute.compiled)
                    {
                        line = string.Format("{4}{3}.{0} = {1}", attribute.name, attribute.value, nodeName, n._ID, NODE_IDENTIFIER);
                    }
                    else
                    {
                        line = string.Format("{4}{3}.{0} = \"{1}\";", attribute.name, attribute.value, nodeName, n._ID, NODE_IDENTIFIER);
                    }
                    
                    lines.Add(line);
                }

                foreach (SceneGraphNode child in n.Children)
                {
                    work_items.Enqueue(child);
                }
            }
            
            line = string.Format("Application = new SceneGraph({1}{0});", root._ID, NODE_IDENTIFIER);
            lines.Add(line);

            for (i=0; i < lines.Count; i++)
            {
                line = lines[i];

                source += line + "\n";
            }

            return source;
        }

        private class attribute
        {
            public string name;
            public string value;
            public bool compiled = false;
        }

        private static List<attribute> GetAttributes(SceneGraphNode node)
        {
            List<attribute> attributes;
            Type type;
            PropertyInfo[] properties;
            FieldInfo[] fields;
            string value;
            object v;
            bool isCompiled;

            type = node.GetType();
            attributes = new List<attribute>();

            properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(prop => !prop.IsDefined(typeof(XmlIgnoreAttribute), false) 
                            || prop.IsDefined(typeof(XmlAttributeAttribute), false))
                .ToArray();

            fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance)
                .Where(field => !field.IsDefined(typeof(XmlIgnoreAttribute), false)).ToArray();

            foreach (PropertyInfo pi in properties)
            {
                v = pi.GetValue(node, null);

                value = serializeValue(v, out isCompiled);

                if (!string.IsNullOrEmpty(value))
                {
                    attributes.Add(new attribute()
                    {
                        name = pi.Name,
                        value = value,
                        compiled = isCompiled
                    });
                }
            }

            foreach (FieldInfo fi in fields)
            {
                v = fi.GetValue(node);

                value = serializeValue(v, out isCompiled);

                if (!string.IsNullOrEmpty(value))
                {
                    attributes.Add(new attribute()
                    {
                        name = fi.Name,
                        value = value,
                        compiled = isCompiled
                    });
                }
            }

            return attributes;
        }

        private static string serializeValue(object v, out bool isCompiled)
        {
            string value;

            isCompiled = false;

            if (v != null)
            {
                if (v is Vector3)
                {
                    Vector3 vec = (Vector3)v;

                    value = string.Format("new Vector3({0}f, {1}f, {2}f);", vec.X, vec.Y, vec.Z);

                    isCompiled = true;
                }
                else if (v is Vector4)
                {
                    Vector4 vec = (Vector4)v;

                    value = string.Format("new Vector4({0}f, {1}f, {2}f, {3}f);", vec.X, vec.Y, vec.Z, vec.W);

                    isCompiled = true;
                }
                else if (v is string)
                {
                    value = v.ToString();
                }
                else if (v is float)
                {
                    value = v.ToString() + "f";
                    isCompiled = true;
                }
                else if (v is Single)
                {
                    value = v.ToString() + "f";
                    isCompiled = true;
                }
                else if (v is double)
                {
                    value = v.ToString();
                }
                else if (v is object)
                {
                    value = "";
                }
                else
                {
                    value = "";
                }
            }
            else
            {
                value = "";
            }

            return value;
        }


        /// <summary>
        /// Compiles a Scene Graph into XML.
        /// </summary>
        public static string CompileXML(SceneGraph graph)
        {
            X3DSceneGraphXMLSerializer serializer;

            serializer = new X3DSceneGraphXMLSerializer(graph.GetRoot());

            return serializer.Serialize();
        }

        public static bool CompileCSharp(string inlineSource, out string tmpExeFile)
        {
            CSharpCodeProvider codeProvider;
            string code;

            tmpExeFile = System.IO.Path.GetTempFileName() + ".exe";

            codeProvider = new CSharpCodeProvider();

            code = ProjectTemplate.buildSource(inlineSource);

            if (CompileCode(codeProvider, code, tmpExeFile))
            {
                Console.WriteLine("X3D Application [Compiled]");

                return true;
            }
            else
            {
                Console.WriteLine("X3D Application failed compiling");
            }

            return false;
        }

        public static bool CompileCode
            (
            CodeDomProvider provider,
            String sourceCode,
            String exeFile
            )
        {

            CompilerParameters cp = new CompilerParameters();


            cp.GenerateExecutable = true;
            cp.OutputAssembly = exeFile;

            cp.IncludeDebugInformation = true;

            cp.ReferencedAssemblies.Add("System.dll");
            cp.ReferencedAssemblies.Add(@"D:\projects\X3D\x3d-finely-sharpened\OpenTK\OpenTK.dll");
            cp.ReferencedAssemblies.Add(@"D:\projects\X3D\x3d-finely-sharpened\X3DRuntime\bin\Debug\X3DRuntime.exe");
            cp.ReferencedAssemblies.Add(@"D:\projects\X3D\x3d-finely-sharpened\X3DRuntime\bin\Debug\X3D.dll");


            cp.GenerateInMemory = false; // Save the assembly as a physical file.

            // Set the level at which the compiler 
            // should start displaying warnings.
            cp.WarningLevel = 3;
            cp.TreatWarningsAsErrors = false;

            cp.CompilerOptions = "/optimize";

            cp.TempFiles = new TempFileCollection(".", true);

            if (provider.Supports(GeneratorSupport.EntryPointMethod))
            {
                // Specify the class that contains 
                // the main method of the executable.
                cp.MainClass = "X3D.Program";
            }

            if (Directory.Exists("Resources"))
            {
                if (provider.Supports(GeneratorSupport.Resources))
                {
                    // Set the embedded resource file of the assembly.
                    // This is useful for culture-neutral resources,
                    // or default (fallback) resources.
                    cp.EmbeddedResources.Add("Resources\\Default.resources");

                    // Set the linked resource reference files of the assembly.
                    // These resources are included in separate assembly files,
                    // typically localized for a specific language and culture.
                    cp.LinkedResources.Add("Resources\\nb-no.resources");
                }
            }


            // Invoke compilation.
            CompilerResults cr = provider.CompileAssemblyFromSource(cp, sourceCode.Split('\n'));
            //CompilerResults cr = provider.CompileAssemblyFromFile(cp, sourceFile);

            if (cr.Errors.Count > 0)
            {
                // Display compilation errors.
                Console.WriteLine("Errors building source code into {0}", cr.PathToAssembly);

                foreach (CompilerError ce in cr.Errors)
                {
                    Console.WriteLine("  {0}", ce.ToString());
                    Console.WriteLine();
                }
            }
            else
            {
                Console.WriteLine("Source code built into {0} successfully.", cr.PathToAssembly);
                Console.WriteLine("{0} temporary files created during the compilation.",
                    cp.TempFiles.Count.ToString());

            }

            // Return the results of compilation.
            if (cr.Errors.Count > 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
