using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using X3D.Parser;

namespace X3D
{
    /// <summary>
    /// http://www.web3d.org/documents/specifications/19775-1/V3.3/Part01/components/utils.html#IntegerSequencer
    /// </summary>
    public partial class IntegerSequencer
    {
        #region Private Fields

        private float _set_fraction;
        private int _value_changed;
        private float[] _keys;
        private int[] _keyValues;
        private Dictionary<float, int> map = new Dictionary<float, int>();

        #endregion

        #region Public Fields

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

                float percent = value * 100;

                if (map.ContainsKey(percent))
                {
                    this.value_changed = map[percent];
                }
            }
        }

        [XmlIgnore]
        public int value_changed
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

        #region Rendering Methods

        public override void Load()
        {
            base.Load();

            int i;

            _keys = X3DTypeConverters.Floats(this.key);
            _keyValues = X3DTypeConverters.Integers(this.keyValue);

            // OPTIMISE
            for(i = 0; i < _keys.Length; i++)
            {
                if(i >= _keyValues.Length)
                {
                    map[_keys[i]] = 0;
                }
                else
                {
                    map[_keys[i]] = _keyValues[i];
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
