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
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using X3D.Parser;

namespace X3D
{
    public partial class Shape : X3DShapeNode
    {
        //private bool isComposedGeometry;
        private bool hasShaders;
        private List<X3DShaderNode> shaders;

        #region Test Shader

        string vertexShaderSource = @"
#version 400
layout(location = 0) in vec3 position;
layout(location = 1) in vec3 normal;
layout(location = 2) in vec4 color;
layout(location = 3) in vec2 texcoord;

uniform mat4 modelview;
uniform mat4 projection;
uniform float camscale;
uniform vec3 size;
uniform vec3 scale;

varying vec3 lightVec; 
varying vec3 eyeVec; 
varying vec3 normalVec;

out vec4 vColor;
out lowp vec2 uv;

void main()
{
    mat4 model = projection * modelview;

	gl_Position = model * vec4(camscale * scale * size * position, 1.0);
    vColor = color;

	//gl_TexCoord[0] = gl_MultiTexCoord0; 
	normalVec = normalize(normal); // gl_Normal

	vec4 eyePos = gl_ModelViewMatrixInverse * vec4(0., 0., 0., 1.); 
	eyeVec = normalize(eyePos.xyz - position.xyz);

	vec4 lightPos = modelview * vec4(1.0, 0.0, 0.0, 1.0); // gl_ModelViewMatrixInverse  gl_LightSource[0].position.xyz
	lightVec = normalize(lightPos.xyz - position.xyz);

    uv = texcoord;
}";

        string fragmentShaderSource = @"
#version 400
 
varying vec3 lightVec; 
varying vec3 eyeVec; 
varying vec3 normalVec;

in vec2 uv;
in vec4 vColor;
out vec4 FragColor;

uniform sampler2D _MainTex;
uniform vec3 specular = vec3(.7, .7, .7); 
uniform float ambient = 0.2;

void main()
{
    vec4 texture_color = texture2D(_MainTex, uv);

    // PHONG SHADING TEST
	vec3 texCol = vec3(0.1, 0.1, 0.1); 
	vec3 halfVec = normalize( eyeVec + lightVec );
	float ndotl = max( dot( lightVec, normalVec ), 0.0 ); 
	float ndoth = (ndotl > 0.0) ? pow(max( dot( halfVec, normalVec ), 0.0 ), 128.) : 0.0;  
	vec3 color = 0.2 * ambient + ndotl * texCol + ndoth * specular;

    //FragColor = vec4(color, 1.0);	
    //FragColor = vec4(color, 1.0) +  vColor / 2;
    

	//FragColor = vec4(0.5, 0.8, 1.0, 1.0);
    //FragColor = vColor;

    FragColor = texture_color;
}

";

        public static int shaderProgramHandle;
        public static int uniformModelview, uniformProjection;

        private int uniformCameraScale;
        private static float variableScale;

        public Vector3 ShapeScale = new Vector3(1,1,1);

        double fade_time;


        #endregion

        #region Render Methods

        public override void Load()
        {
            base.Load();

            // load assets

            shaderProgramHandle = Helpers.ApplyShader(vertexShaderSource, fragmentShaderSource);

            uniformModelview = GL.GetUniformLocation(shaderProgramHandle, "modelview");
            uniformProjection = GL.GetUniformLocation(shaderProgramHandle, "projection");
            uniformCameraScale = GL.GetUniformLocation(shaderProgramHandle, "camscale");
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

            fade_time = (fade_time >= Math.PI) ? 0.0 : fade_time + rc.e.Time; // fade in/out

            variableScale = (float)(Math.Sin(fade_time));

            GL.UseProgram(shaderProgramHandle);
            GL.UniformMatrix4(uniformModelview, false, ref rc.modelview);
            GL.UniformMatrix4(uniformProjection, false, ref rc.projection);
            GL.Uniform1(uniformCameraScale, rc.cam.Scale.X);

            rc.modelview = Matrix4.Identity;
        }

        public override void PostRender()
        {
            base.PostRender();
            GL.UseProgram(0);
        }

        #endregion
    }
}
