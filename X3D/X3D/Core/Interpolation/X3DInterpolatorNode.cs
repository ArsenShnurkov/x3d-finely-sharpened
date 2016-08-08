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
    /// http://www.web3d.org/documents/specifications/19775-1/V3.3/Part01/components/interp.html#X3DInterpolatorNode
    /// </summary>
    public partial class X3DInterpolatorNode
    {
        public float[] Keys { get; set; }

        public override void Load()
        {
            base.Load();

            Keys = X3DTypeConverters.Floats(this.key);
        }

        /// <summary>
        /// Computes Linear Interpolation between two points 'from' 'to' using the current time, keys, and key values.
        /// Informal: For the current time, interpolate key values and find the resultant interpolation.  
        /// </summary>
        /// <param name="frameTime">
        /// Typically a time value. Used as a selector to find the right key values for the specified time.  
        /// </param>
        /// <returns>
        /// Returns the resultant interpolated value that lies within the specified range.
        /// The interpolated value should be between the specified 'from' and 'to' key values.
        /// </returns>
        protected T Lerp<T>(T[] keyValues, float frameTime, Func<T,T, float, T> interpolatorFunct)
        {
            T result;
            T from;
            T to;
            float ratio;
            bool applied;
            int i;

            applied = false;
            result = default(T);

            if (frameTime <= this.Keys[0])
            {
                result = keyValues[0];
            }
            else if (frameTime >= this.Keys[this.Keys.Length - 1])
            {
                result = keyValues[this.Keys.Length - 1];
            }
            else
            {
                for (i = 0; !applied && (i < this.Keys.Length - 1); ++i)
                {
                    if ((this.Keys[i] < frameTime) && (frameTime <= this.Keys[i + 1]))
                    {
                        ratio = (frameTime - this.Keys[i]) / (this.Keys[i + 1] - this.Keys[i]);

                        from = keyValues[i];
                        to = keyValues[i + 1];

                        //result = MathHelpers.Lerp(from, to, ratio);
                        result = interpolatorFunct(from, to, ratio); 

                        applied = true;
                    }
                }

                if (!applied)
                {
                    result = keyValues[0];
                }
            }

            return result;
        }
    }
}
