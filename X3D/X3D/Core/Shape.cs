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
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using X3D.Core;
using X3D.Core.Shading;
using X3D.Core.Shading.DefaultUniforms;
using X3D.Core;

namespace X3D
{
    public partial class Shape : X3DShapeNode
    {
        private bool hasShaders;
        private List<X3DShaderNode> shaders;

        [XmlIgnore]
        public List<ComposedShader> ComposedShaders = new List<ComposedShader>();

        #region Test Shader


        [XmlIgnore]
        public bool texturingEnabled;

        [XmlIgnore]
        public int uniformModelview, uniformProjection;

        [XmlIgnore]
        public ShaderUniformsPNCT uniforms = new ShaderUniformsPNCT();

        [XmlIgnore]
        public ShaderMaterialUniforms Materials = new ShaderMaterialUniforms();

        [XmlIgnore]
        public TessShaderUniforms Uniforms = new TessShaderUniforms();

        private int uniformCameraScale, uniformX3DScale;


        public float TessLevelInner = 137; // 3
        public float TessLevelOuter = 115; // 2
        public Matrix3 NormalMatrix = Matrix3.Identity;

        [XmlIgnore]
        public ComposedShader CurrentShader = null;

        #endregion

        #region Render Methods

        public void IncludeDefaultShader(string vertexShaderSource, string fragmentShaderSource)
        {
            CurrentShader = ShaderCompiler.ApplyShader(vertexShaderSource, fragmentShaderSource);

            IncludeComposedShader(CurrentShader);
        }

        public void IncludeComposedShader(ComposedShader shader)
        {
            shader.Link();
            shader.Use();

            RefreshDefaultUniforms();
            RefreshMaterialUniforms();
            if (shader.IsTessellator)
            {
                RefreshTessUniforms();
            }

            ComposedShaders.Add(shader);
        }

        public void IncludeTesselationShaders(string tessControlShaderSource, string tessEvalShaderSource,
                                              string geometryShaderSource)
        {
            CurrentShader = ShaderCompiler.ApplyShader(DefaultShader.vertexShaderSource, 
                                                       DefaultShader.fragmentShaderSource,
                                                       tessControlShaderSource, 
                                                       tessEvalShaderSource, 
                                                       geometryShaderSource);



            IncludeComposedShader(CurrentShader);


        }

        public void RefreshTessUniforms()
        {
            Uniforms.Modelview = GL.GetUniformLocation(CurrentShader.ShaderHandle, "modelview");
            Uniforms.Projection = GL.GetUniformLocation(CurrentShader.ShaderHandle, "projection");
            Uniforms.NormalMatrix = GL.GetUniformLocation(CurrentShader.ShaderHandle, "normalmatrix");
            Uniforms.LightPosition = GL.GetUniformLocation(CurrentShader.ShaderHandle, "LightPosition");
            Uniforms.AmbientMaterial = GL.GetUniformLocation(CurrentShader.ShaderHandle, "AmbientMaterial");
            Uniforms.DiffuseMaterial = GL.GetUniformLocation(CurrentShader.ShaderHandle, "DiffuseMaterial");
            Uniforms.TessLevelInner = GL.GetUniformLocation(CurrentShader.ShaderHandle, "TessLevelInner");
            Uniforms.TessLevelOuter = GL.GetUniformLocation(CurrentShader.ShaderHandle, "TessLevelOuter");
        }

        public void RefreshDefaultUniforms()
        {
            uniformModelview = GL.GetUniformLocation(CurrentShader.ShaderHandle, "modelview");
            uniformProjection = GL.GetUniformLocation(CurrentShader.ShaderHandle, "projection");
            uniformCameraScale = GL.GetUniformLocation(CurrentShader.ShaderHandle, "camscale");
            uniformX3DScale = GL.GetUniformLocation(CurrentShader.ShaderHandle, "X3DScale");

            uniforms.a_position = GL.GetAttribLocation(CurrentShader.ShaderHandle, "position");
            uniforms.a_normal = GL.GetAttribLocation(CurrentShader.ShaderHandle, "normal");
            uniforms.a_color = GL.GetAttribLocation(CurrentShader.ShaderHandle, "color");
            uniforms.a_texcoord = GL.GetAttribLocation(CurrentShader.ShaderHandle, "texcoord");
            uniforms.a_coloringEnabled = GL.GetUniformLocation(CurrentShader.ShaderHandle, "coloringEnabled");
            uniforms.a_texturingEnabled = GL.GetUniformLocation(CurrentShader.ShaderHandle, "texturingEnabled");
            uniforms.sampler = GL.GetUniformLocation(CurrentShader.ShaderHandle, "_MainTex");
        }

        public void RefreshMaterialUniforms()
        {
            Materials.ambientIntensity = GL.GetUniformLocation(CurrentShader.ShaderHandle, "ambientIntensity");
            Materials.diffuseColor = GL.GetUniformLocation(CurrentShader.ShaderHandle, "diffuseColor");
            Materials.emissiveColor = GL.GetUniformLocation(CurrentShader.ShaderHandle, "emissiveColor");
            Materials.shininess = GL.GetUniformLocation(CurrentShader.ShaderHandle, "shininess");
            Materials.specularColor = GL.GetUniformLocation(CurrentShader.ShaderHandle, "specularColor");
            Materials.transparency = GL.GetUniformLocation(CurrentShader.ShaderHandle, "transparency");
        }

        public void SetSampler(int sampler)
        {
            GL.Uniform1(uniforms.sampler, sampler);
        }

        public override void Load()
        {
            base.Load();

            var @default = ShaderCompiler.BuildDefaultShader();
            @default.Link();
            @default.Use();
            CurrentShader = @default;
            IncludeComposedShader(@default);

            RefreshDefaultUniforms();
            RefreshMaterialUniforms();
        }

        public override void PreRender()
        {
            base.PreRender();

            texturingEnabled = GL.IsEnabled(EnableCap.Texture2D);

            shaders = this.DecendantsByType(typeof(X3DShaderNode)).Select(n => (X3DShaderNode)n).ToList();
            hasShaders = shaders.Any();
        }

        public override void Render(RenderingContext rc)
        {
            base.Render(rc);

            NormalMatrix = new Matrix3(rc.matricies.modelview); // NormalMatrix = M4GetUpper3x3(ModelviewMatrix);

            var linkedShaders = ComposedShaders.Last(s => s.Linked);

            if (linkedShaders != null)
            {
                CurrentShader = linkedShaders;
                this.CurrentShader.Use();

                RefreshDefaultUniforms();
                RefreshMaterialUniforms();

                if (this.CurrentShader.IsTessellator)
                    RefreshTessUniforms();

                CurrentShader.SetFieldValue("modelview", ref rc.matricies.modelview); //GL.UniformMatrix4(uniformModelview, false, ref rc.matricies.modelview);
                CurrentShader.SetFieldValue("projection", ref rc.matricies.projection); //GL.UniformMatrix4(uniformProjection, false, ref rc.matricies.projection);
                CurrentShader.SetFieldValue("camscale", rc.cam.Scale.X); //GL.Uniform1(uniformCameraScale, rc.cam.Scale.X);
                CurrentShader.SetFieldValue("X3DScale", rc.matricies.Scale); //GL.Uniform3(uniformX3DScale, rc.matricies.Scale);
                CurrentShader.SetFieldValue("coloringEnabled", 0); //GL.Uniform1(uniforms.a_coloringEnabled, 0);
                CurrentShader.SetFieldValue("texturingEnabled", this.texturingEnabled ? 1 : 0); //GL.Uniform1(uniforms.a_texturingEnabled, this.texturingEnabled ? 1 : 0);

                if (CurrentShader.IsBuiltIn == false)
                {
                    // TODO: later put this logic inside X3D shader examples /sphere-with-tessellation.x3d
                    
                    // The ability to incorporate and vary the amount of tesselation should be a X3D scriptable feature
                    if (rc.Keyboard[Key.L]) TessLevelInner++;
                    if (rc.Keyboard[Key.J]) TessLevelInner = TessLevelInner > 1 ? TessLevelInner - 1 : 1;
                    if (rc.Keyboard[Key.I]) TessLevelOuter++;
                    if (rc.Keyboard[Key.K]) TessLevelOuter = TessLevelOuter > 1 ? TessLevelOuter - 1 : 1;
                }
            }


        }

        public override void PostRender(RenderingContext rc)
        {
            base.PostRender(rc);

            CurrentShader.Deactivate();
        }

        #endregion
    }
}
