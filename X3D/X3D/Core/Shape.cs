//TODO: buffer all geometric properties
//TODO: implement ADS shading using materials instantiated from Shape Appearance. 
//      Hardcode ADS Shader model if no shader is specified.
//TODO: implement and instantiate Shader model. implement test scene graph with phong shader.
//TODO: dynamically select shader program for each Shape instance.

//TODO: what if these children have USE lookup requirements?

//TODO: dont unpack indicies or transform them if it is not required. we want to save both time and space if at all possible.
// todo implement ifs geometry shader. ensure colors, texcoords,normals,and verticies render. test using primativiēs.
// todo implement creaseAngle: flat and smooth shading
// todo implememt phong shading. doēs x3d specify this?
// todo implememt optimisations; minimal unpacking/transformation of geometry, go direct to webgl datastructs.
// todo implememt node instancing 
// todo implement dynamic buffers, dynamic attributes, node disposal/scene cleanup, 
// todo īmplememt SAI, scene graph debugger, and UI
// todo implememt ccw, solid, concave tesselator, dynamic polygon types/dynamic faceset capability
// todo: webgl currently lacks support for primativeRestartIndex(), but even then if webgl did support this, i dont think that a restart index can be a signed integer. (-1)

using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using X3D.Engine.Shading;
using X3D.Engine.Shading.DefaultUniforms;
using X3D.Parser;

namespace X3D
{
    public partial class Shape : X3DShapeNode
    {
        //private bool isComposedGeometry;
        private bool hasShaders;
        private List<X3DShaderNode> shaders;

        private List<ComposedShader> ComposedShaders = new List<ComposedShader>();

        #region Test Shader


        [XmlIgnore]
        public bool texturingEnabled;

        //int testShader;
        public int shaderProgramHandle;
        public int uniformModelview, uniformProjection;
        public ShaderUniformsPNCT uniforms = new ShaderUniformsPNCT();
        public ShaderMaterialUniforms Materials = new ShaderMaterialUniforms();
        private int uniformCameraScale, uniformX3DScale;

        double fade_time; // for testing


        #endregion

        #region Render Methods

        public void IncludeComposedShader(ComposedShader shader)
        {
            ComposedShaders.Add(shader);
        }

        public void IncludeTesselationShaders(string tessControlShaderSource, string tessEvalShaderSource, 
                                              string geometryShaderSource)
        {
            shaderProgramHandle = Helpers.ApplyShader(DefaultShader.vertexShaderSource, DefaultShader.fragmentShaderSource, 
                tessControlShaderSource, tessEvalShaderSource, geometryShaderSource);

            uniformModelview = GL.GetUniformLocation(shaderProgramHandle, "modelview");
            uniformProjection = GL.GetUniformLocation(shaderProgramHandle, "projection");
            uniformCameraScale = GL.GetUniformLocation(shaderProgramHandle, "camscale");
            uniformX3DScale = GL.GetUniformLocation(shaderProgramHandle, "X3DScale");

            uniforms.a_coloringEnabled = GL.GetUniformLocation(shaderProgramHandle, "coloringEnabled");
            uniforms.a_texturingEnabled = GL.GetUniformLocation(shaderProgramHandle, "texturingEnabled");

            RefreshMaterialUniforms();
        }

        public void RefreshMaterialUniforms()
        {
            Materials.ambientIntensity = GL.GetUniformLocation(shaderProgramHandle, "ambientIntensity");
            Materials.diffuseColor = GL.GetUniformLocation(shaderProgramHandle, "diffuseColor");
            Materials.emissiveColor = GL.GetUniformLocation(shaderProgramHandle, "emissiveColor");
            Materials.shininess = GL.GetUniformLocation(shaderProgramHandle, "shininess");
            Materials.specularColor = GL.GetUniformLocation(shaderProgramHandle, "specularColor");
            Materials.transparency = GL.GetUniformLocation(shaderProgramHandle, "transparency");
        }

        public override void Load()
        {
            base.Load();

            // load assets
            //testShader = Helpers.ApplyTestShader();

            shaderProgramHandle = Helpers.ApplyShader(DefaultShader.vertexShaderSource, DefaultShader.fragmentShaderSource);


            uniformModelview = GL.GetUniformLocation(shaderProgramHandle, "modelview");
            uniformProjection = GL.GetUniformLocation(shaderProgramHandle, "projection");
            uniformCameraScale = GL.GetUniformLocation(shaderProgramHandle, "camscale");
            uniformX3DScale = GL.GetUniformLocation(shaderProgramHandle, "X3DScale");

            uniforms.a_position = GL.GetAttribLocation(shaderProgramHandle, "position");
            uniforms.a_normal = GL.GetAttribLocation(shaderProgramHandle, "normal");
            uniforms.a_color = GL.GetAttribLocation(shaderProgramHandle, "color");
            uniforms.a_texcoord = GL.GetAttribLocation(shaderProgramHandle, "texcoord");
            uniforms.a_coloringEnabled = GL.GetUniformLocation(shaderProgramHandle, "coloringEnabled");
            uniforms.a_texturingEnabled = GL.GetUniformLocation(shaderProgramHandle, "texturingEnabled");

            RefreshMaterialUniforms();
        }

        public override void PreRenderOnce(RenderingContext rc)
        {
            base.PreRenderOnce(rc);

            foreach (ComposedShader shader in ComposedShaders)
            {
                Console.WriteLine("ComposedShader {0}", shader.language);

                if (shader.language == "GLSL")
                {
                    int _shaderProgramHandle = GL.CreateProgram();

                    foreach (ShaderPart part in shader.ShaderParts)
                    {
                        Helpers.ApplyShaderPart(_shaderProgramHandle, part);
                    }

                    GL.LinkProgram(_shaderProgramHandle);
                    string err = GL.GetProgramInfoLog(_shaderProgramHandle).Trim();
                    Console.WriteLine(err);
                    Console.WriteLine("ComposedShader [linked]"); //TODO: check for link errors

                    if (GL.GetError() != ErrorCode.NoError)
                    {
                        throw new Exception("Error Linking ComposedShader Shader Program");
                    }
                }
                else
                {
                    Console.WriteLine("ComposedShader language {0} unsupported", shader.language);
                }
            }
        }

        public override void PreRender()
        {
            base.PreRender();

            texturingEnabled = GL.IsEnabled(EnableCap.Texture2D);

            //this.geometry = (X3DGeometryNode)this.Children.FirstOrDefault(c => typeof(X3DGeometryNode).IsInstanceOfType(c));
            this.appearance = (X3DAppearanceNode)this.Children.FirstOrDefault(c => typeof(X3DAppearanceNode).IsInstanceOfType(c));

            //this.isComposedGeometry = typeof(X3DComposedGeometryNode).IsInstanceOfType(this.geometry);

            shaders = this.DecendantsByType(typeof(X3DShaderNode)).Select(n => (X3DShaderNode)n).ToList();
            hasShaders = shaders.Any();
        }

        public override void Render(RenderingContext rc)
        {
            base.Render(rc);

            fade_time = (fade_time >= Math.PI) ? 0.0 : fade_time + rc.Time; // fade in/out

            float variableScale = (float)(Math.Sin(fade_time));

            //GL.UseProgram(testShader);
            GL.UseProgram(shaderProgramHandle);
            GL.UniformMatrix4(uniformModelview, false, ref rc.matricies.modelview);
            GL.UniformMatrix4(uniformProjection, false, ref rc.matricies.projection);
            GL.Uniform1(uniformCameraScale, rc.cam.Scale.X);
            GL.Uniform3(uniformX3DScale, rc.matricies.Scale);
            GL.Uniform1(uniforms.a_coloringEnabled, 0);
            GL.Uniform1(uniforms.a_texturingEnabled, this.texturingEnabled ? 1 : 0);
        }

        public override void PostRender(RenderingContext rc)
        {
            base.PostRender(rc);

            GL.UseProgram(0);
        }

        #endregion
    }
}
