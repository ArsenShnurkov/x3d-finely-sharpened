using System;
using System.Runtime.InteropServices;
using OpenTK;

namespace X3D
{
    /* 
Material : X3DMaterialNode { 
SFFloat [in,out] ambientIntensity 0.2         [0,1]
SFColor [in,out] diffuseColor     0.8 0.8 0.8 [0,1]
SFColor [in,out] emissiveColor    0 0 0       [0,1]
SFNode  [in,out] metadata         NULL        [X3DMetadataObject]
SFFloat [in,out] shininess        0.2         [0,1]
SFColor [in,out] specularColor    0 0 0       [0,1]
SFFloat [in,out] transparency     0           [0,1]
}
*/

    [Serializable]
    //[StructLayout(LayoutKind.Explicit)] // use LayoutKind.Explicit if shader not using std140 layout
    [StructLayout(LayoutKind.Sequential)]
    public struct ShaderMaterial
    {
        /* 
	vec4 diffuseColor;
	vec4 emissiveColor;
	vec4 specularColor;
	float ambientIntensity;
	float shininess;
	float transparency;
             */

        //public Vector4 test;
        //public Vector4 test2;

        public Vector4 diffuseColor;
        public Vector4 emissiveColor;
        public Vector4 specularColor;

        public float ambientIntensity;
        public float shininess;
        public float transparency;

        public static ShaderMaterial FromX3DMaterial(Material material)
        {
            var m = new ShaderMaterial
            {
                diffuseColor = new Vector4(material._diffuseColor, 1.0f),
                emissiveColor = new Vector4(material._emissiveColor, 1.0f),
                specularColor = new Vector4(material._specularColor, 1.0f),

                ambientIntensity = material.ambientIntensity,
                shininess = material.shininess,
                transparency = material.transparency

                //test = new Vector4(0, 1, 0, 1),
                //test2 = new Vector4(1, 1, 0, 1)
            };

            return m;
        }

        // Three different ways to do same thing
        public static readonly int SizeInBytes = Vector4.SizeInBytes + Vector4.SizeInBytes;

        public static readonly int Stride = Marshal.SizeOf(default(ShaderMaterial));

        public static readonly int Size = BlittableValueType<ShaderMaterial>.Stride;
    }
}