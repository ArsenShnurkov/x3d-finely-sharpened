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

        #region Test Shader


        
        //int testShader;
        public int shaderProgramHandle;
        public int uniformModelview, uniformProjection;
        public ShaderUniformsPNCT uniforms = new ShaderUniformsPNCT();
        private int uniformCameraScale, uniformX3DScale;

        double fade_time; // for testing


        #endregion

        #region Render Methods

        public void IncludeTesselationShaders(string tessControlShaderSource, string tessEvalShaderSource, 
                                              string geometryShaderSource)
        {
            shaderProgramHandle = Helpers.ApplyShader(DefaultShader.vertexShaderSource, DefaultShader.fragmentShaderSource, 
                tessControlShaderSource, tessEvalShaderSource, geometryShaderSource);

            uniformModelview = GL.GetUniformLocation(shaderProgramHandle, "modelview");
            uniformProjection = GL.GetUniformLocation(shaderProgramHandle, "projection");
            uniformCameraScale = GL.GetUniformLocation(shaderProgramHandle, "camscale");
            uniformX3DScale = GL.GetUniformLocation(shaderProgramHandle, "X3DScale");
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
        }

        public override void PreRender()
        {
            base.PreRender();

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
            
        }

        public override void PostRender(RenderingContext rc)
        {
            base.PostRender(rc);

            GL.UseProgram(0);
        }

        #endregion
    }
}
