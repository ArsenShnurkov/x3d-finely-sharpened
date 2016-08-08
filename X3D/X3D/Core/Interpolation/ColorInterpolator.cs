using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using System.Xml.Serialization;
using X3D.Parser;

namespace X3D
{
    /// <summary>
    /// http://www.web3d.org/documents/specifications/19775-1/V3.3/Part01/components/interp.html#ColorInterpolator
    /// </summary>
    public partial class ColorInterpolator
    {
        #region Private Fields

        private float _set_fraction;
        private Vector3 _value_changed;
        private Dictionary<float, Vector3> map = new Dictionary<float, Vector3>();

        #endregion

        #region Public Fields


        public Vector3[] KeyValues { get; set; }

        #endregion

        #region Public Properties

        [XmlIgnore]
        public float set_fraction
        {
            private get
            {
                return this._set_fraction;
            }
            set
            {
                this._set_fraction = value;

                // A linear interpolation using the value of set_fraction as input (interpolation in HSV space)

                this.value_changed = LerpColor(value);
            }
        }

        [XmlIgnore]
        public Vector3 value_changed
        {
            get
            {
                return this._value_changed;
            }
            private set
            {
                this._value_changed = value;
            }
        }

        #endregion

        #region Public Methods

        public Vector3 LerpColor(float ratio)
        {
            Vector3 result;

            result = base.Lerp<Vector3>(this.KeyValues, ratio, (Vector3 from, Vector3 to, float r) =>
            {
                //return MathHelpers.Lerp(from, to, ratio);

                //TODO: work in HSV space

                Vector3 hsv_result;
                Vector3 hsv_from,
                        hsv_to;



                // HSV SPACE
                hsv_from = MathHelpers.ToHSV(from);
                hsv_to = MathHelpers.ToHSV(to);

                //var test1 = MathHelpers.ToHSV(new Vector3(1, 1, 1));

                // LERP
                hsv_result = MathHelpers.Lerp(hsv_from, hsv_to, r);

                // RGB SPACE
                return MathHelpers.FromHSV(hsv_result); // convert back to RGB space
            });

            return result;
        }

        #endregion

        #region Rendering Methods

        public override void Load()
        {
            base.Load();

            int i;

            // The keyValue field and value_changed events are defined in RGB colour space. 
            KeyValues = X3DTypeConverters.MFVec3f(this.keyValue);

            if(KeyValues.Length > Keys.Length)
            {
                Console.WriteLine("[Warning] ColorInterpolator : The number of colours in the keyValue field should be equal to the number of key frames in the key field");
            }

            // OPTIMISE
            for (i = 0; i < Keys.Length; i++)
            {
                if (i >= KeyValues.Length)
                {
                    map[Keys[i]] = Vector3.Zero;
                }
                else
                {
                    map[Keys[i]] = KeyValues[i];
                }
            }
        }

        public override void PreRenderOnce(RenderingContext rc)
        {
            base.PreRenderOnce(rc);

        }

        public override void Render(RenderingContext rc)
        {
            base.Render(rc);


        }

        #endregion
    }
}
