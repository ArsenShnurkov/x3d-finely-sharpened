/* 
X3D Material
--------------
 The fields in the Material node determine how light reflects off an object to create colour:

a. The ambientIntensity field specifies how much ambient light from light sources this surface shall reflect. Ambient light is omnidirectional and depends only on the number of light sources, not their positions with respect to the surface. Ambient colour is calculated as ambientIntensity × diffuseColor.
b. The diffuseColor field reflects all X3D light sources depending on the angle of the surface with respect to the light source. The more directly the surface faces the light, the more diffuse light reflects.
c. The emissiveColor field models "glowing" objects. This can be useful for displaying pre-lit models (where the light energy of the room is computed explicitly), or for displaying scientific data.
d. The specularColor and shininess fields determine the specular highlights (e.g., the shiny spots on an apple). When the angle from the light to the surface is close to the angle from the surface to the viewer, the specularColor is added to the diffuse and ambient colour calculations. Lower shininess values produce soft glows, while higher values result in sharper, smaller highlights.
e. The transparency field specifies how "clear" an object is, with 1.0 being completely transparent, and 0.0 completely opaque.
 
 */

using X3D.Parser;

namespace X3D
{
    /// <summary>
    ///     http://www.web3d.org/documents/specifications/19775-1/V3.3/Part01/components/shape.html#Material
    /// </summary>
    public partial class Material
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
        public override void Load()
        {
            base.Load();
        }

        public override void Render(RenderingContext rc)
        {
            base.Render(rc);
        }

        public string ToFriendlyString()
        {
            return string.Format(
                "Material <ambientIntensity=\"{0}\", diffuse=\"{1}\", emissive=\"{2}\", shininess=\"{3}\", specular=\"{4}\", transparency=\"{5}\">",
                ambientIntensity,
                X3DTypeConverters.ToString(_diffuseColor),
                X3DTypeConverters.ToString(_emissiveColor),
                _shininess,
                X3DTypeConverters.ToString(_specularColor),
                _transparency
            );
        }
    }
}