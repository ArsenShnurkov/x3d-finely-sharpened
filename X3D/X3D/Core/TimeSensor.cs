using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace X3D
{
    /// <summary>
    /// http://www.web3d.org/documents/specifications/19775-1/V3.3/Part01/components/time.html#TimeSensor
    /// </summary>
    public partial class TimeSensor
    {
        [XmlIgnore]
        public bool Enabled { get; set; }

        [XmlIgnore]
        public float fraction_changed { get; private set; }

        private double worldTime = 0;

        #region Rendering Methods

        public override void Load()
        {
            base.Load();

            Enabled = true;
        }

        public override void PreRenderOnce(RenderingContext rc)
        {
            base.PreRenderOnce(rc);

            this.startTime = rc.Time;
        }

        public override void Render(RenderingContext rc)
        {
            base.Render(rc);

            if (!Enabled) return;

            double time;
            double delta;
            decimal f;

            worldTime += rc.Time;

            time = worldTime;

            delta = (time - startTime) / (double)cycleInterval;

            f = (decimal)MathHelpers.RoundFractional(MathHelpers.FractionalPart(delta), cycleInterval);

            if (f == 0.0m && time > startTime)
            {
                fraction_changed = 1.0f;

                if (!this.loop)
                {
                    Enabled = false;
                }
            }
            else
            {
                fraction_changed = (float)f;
            }
        }

        #endregion
    }
}
