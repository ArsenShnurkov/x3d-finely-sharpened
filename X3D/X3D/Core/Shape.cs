﻿using OpenTK;
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
            //RefreshMaterialUniforms();
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
            if (CurrentShader.HasErrors) return;

            Uniforms.Modelview = GL.GetUniformLocation(CurrentShader.ShaderHandle, "modelview");
            Uniforms.Projection = GL.GetUniformLocation(CurrentShader.ShaderHandle, "projection");
            Uniforms.NormalMatrix = GL.GetUniformLocation(CurrentShader.ShaderHandle, "normalmatrix");
            Uniforms.LightPosition = GL.GetUniformLocation(CurrentShader.ShaderHandle, "LightPosition");
            Uniforms.AmbientMaterial = GL.GetUniformLocation(CurrentShader.ShaderHandle, "AmbientMaterial");
            Uniforms.DiffuseMaterial = GL.GetUniformLocation(CurrentShader.ShaderHandle, "DiffuseMaterial");
            //Uniforms.TessLevelInner = GL.GetUniformLocation(CurrentShader.ShaderHandle, "TessLevelInner");
            //Uniforms.TessLevelOuter = GL.GetUniformLocation(CurrentShader.ShaderHandle, "TessLevelOuter");
        }

        public void RefreshDefaultUniforms()
        {
            if (CurrentShader.HasErrors) return;

            //uniforms.a_position = GL.GetAttribLocation(CurrentShader.ShaderHandle, "position");
            //uniforms.a_normal = GL.GetAttribLocation(CurrentShader.ShaderHandle, "normal");
            //uniforms.a_color = GL.GetAttribLocation(CurrentShader.ShaderHandle, "color");
            //uniforms.a_texcoord = GL.GetAttribLocation(CurrentShader.ShaderHandle, "texcoord");

            uniforms.a_coloringEnabled = GL.GetUniformLocation(CurrentShader.ShaderHandle, "coloringEnabled");
            uniforms.a_texturingEnabled = GL.GetUniformLocation(CurrentShader.ShaderHandle, "texturingEnabled");
            uniforms.sampler = GL.GetUniformLocation(CurrentShader.ShaderHandle, "_MainTex");
        }

        //public void RefreshMaterialUniforms()
        //{
        //    if (CurrentShader.HasErrors) return;
        //
        //    Materials.ambientIntensity = GL.GetUniformLocation(CurrentShader.ShaderHandle, "ambientIntensity");
        //    Materials.diffuseColor = GL.GetUniformLocation(CurrentShader.ShaderHandle, "diffuseColor");
        //    Materials.emissiveColor = GL.GetUniformLocation(CurrentShader.ShaderHandle, "emissiveColor");
        //    Materials.shininess = GL.GetUniformLocation(CurrentShader.ShaderHandle, "shininess");
        //    Materials.specularColor = GL.GetUniformLocation(CurrentShader.ShaderHandle, "specularColor");
        //    Materials.transparency = GL.GetUniformLocation(CurrentShader.ShaderHandle, "transparency");
        //}

        //public void SetSampler(int sampler)
        //{
        //    GL.Uniform1(uniforms.sampler, sampler);
        //}

        public override void Load()
        {
            base.Load();

            var @default = ShaderCompiler.BuildDefaultShader();
            @default.Link();
            @default.Use();
            CurrentShader = @default;
            IncludeComposedShader(@default);

            RefreshDefaultUniforms();
            //RefreshMaterialUniforms();
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
                //RefreshMaterialUniforms();

                if (this.CurrentShader.IsTessellator)
                    RefreshTessUniforms();

                /* 
glm::mat4 View = glm::lookAt(
    glm::vec3(4,3,3), // Camera is at (4,3,3), in World Space
    glm::vec3(0,0,0), // and looks at the origin
    glm::vec3(0,1,0)  // Head is up (set to 0,-1,0 to look upside-down)
    );
  
// Model matrix : an identity matrix (model will be at the origin)
glm::mat4 Model = glm::mat4(1.0f);
// Our ModelViewProjection : multiplication of our 3 matrices
glm::mat4 mvp = Projection * View * Model; // Remember, matrix multiplication is the other way around
                 */

                Matrix4 view = Matrix4.LookAt(new Vector3(4, 3, 3),  // Camera is at (4,3,3), in World Space
                    new Vector3(0, 0, 0),  // and looks at the origin
                    new Vector3(0, 1, 0) // Head is up (set to 0,-1,0 to look upside-down)
                );

                Matrix4 model = rc.matricies.modelview;
                //Matrix4 MVP = rc.matricies.projection * view * model; // this is the model-view-projection matrix
                Matrix4 transl = Matrix4.CreateTranslation(0, 0, 3);

                Matrix4 cameraTransl = Matrix4.CreateTranslation(rc.cam.Position);

                //model = Matrix4.Identity;

                Quaternion q = rc.cam.Orientation;
                //q.Conjugate();

                Matrix4 cameraRot;

                cameraRot = Matrix4.CreateFromQuaternion(q);
                // cameraRot = MathHelpers.CreateRotation(ref q);

                Matrix4 CMR = (cameraTransl * model) * cameraRot //* transl 
                                                                 //* view   
                                                                 //model

                    ; // this is the camera-model-camrotate matrix


                CurrentShader.SetFieldValue("modelview", ref CMR); //GL.UniformMatrix4(uniformModelview, false, ref rc.matricies.modelview);
                CurrentShader.SetFieldValue("projection", ref rc.matricies.projection);
                CurrentShader.SetFieldValue("camscale", rc.cam.Scale.X); //GL.Uniform1(uniformCameraScale, rc.cam.Scale.X);
                CurrentShader.SetFieldValue("X3DScale", rc.matricies.Scale); //GL.Uniform3(uniformX3DScale, rc.matricies.Scale);
                CurrentShader.SetFieldValue("coloringEnabled", 0); //GL.Uniform1(uniforms.a_coloringEnabled, 0);
                CurrentShader.SetFieldValue("texturingEnabled", this.texturingEnabled ? 1 : 0); //GL.Uniform1(uniforms.a_texturingEnabled, this.texturingEnabled ? 1 : 0);

                if (CurrentShader.IsBuiltIn == false)
                {
                    CurrentShader.ApplyFieldsAsUniforms(rc);
                }

            }


        }

        public override void PostRender(RenderingContext rc)
        {
            base.PostRender(rc);

            CurrentShader.Deactivate();
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        #endregion
    }
}
