using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace X3D.Core.Shading.DefaultUniforms
{
    public class ShaderUniformsPNCT
    {
        public int a_position;
        public int a_normal;
        public int a_color;
        public int a_texcoord;
        public int a_coloringEnabled;
        public int a_texturingEnabled;
    }

    public class ShaderMaterialUniforms
    {
        //X3D Material Node
        public int ambientIntensity;
        public int diffuseColor;
        public int emissiveColor;
        public int shininess;
        public int specularColor;
        public int transparency;
    }

    public class TessShaderUniforms
    {
        public int Projection;
        public int Modelview;
        public int NormalMatrix;
        public int LightPosition;
        public int AmbientMaterial;
        public int DiffuseMaterial;
        public int TessLevelInner;
        public int TessLevelOuter;
    }
}
