using System.Xml.Serialization;

namespace X3D
{
    /// <summary>
    ///     http://www.web3d.org/documents/specifications/19775-1/V3.3/Part01/components/time.html#TimeSensor
    /// </summary>
    public partial class TimeSensor
    {
        private double worldTime;

        [XmlIgnore] public bool Enabled { get; set; }

        [XmlIgnore] public float fraction_changed { get; private set; }

        #region Rendering Methods

        public override void Load()
        {
            base.Load();

            Enabled = true;
        }

        public override void PreRenderOnce(RenderingContext rc)
        {
            base.PreRenderOnce(rc);

            startTime = rc.Time;
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

            delta = (time - startTime) / cycleInterval;

            f = (decimal)MathHelpers.RoundFractional(MathHelpers.FractionalPart(delta), cycleInterval);

            if (f == 0.0m && time > startTime)
            {
                fraction_changed = 1.0f;

                if (!loop) Enabled = false;
            }
            else
            {
                fraction_changed = (float)f;
            }
        }

        #endregion
    }
}