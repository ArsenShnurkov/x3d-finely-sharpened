﻿using System;
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
        public float fraction_changed { get; private set; }

        #region Rendering Methods

        public override void Load()
        {
            base.Load();
        }

        public override void PreRenderOnce(RenderingContext rc)
        {
            base.PreRenderOnce(rc);

            this.startTime = rc.Time;
        }

        double worldTime = 0;

        public override void Render(RenderingContext rc)
        {
            base.Render(rc);

            double time;
            double delta;
            decimal f;

            // make use of the 
            worldTime += rc.Time;

            time = worldTime;

            delta = (time - startTime) / (double)cycleInterval;

            f = (decimal)RoundFractional(fractionalPart(delta), cycleInterval);

            if (f == 0.0m && time > startTime)
            {
                fraction_changed = 1.0f;
            }
            else
            {
                fraction_changed = (float)f;
            }
        }

        private double fractionalPart(double n)
        {
            double fraction;

            fraction = n - Math.Floor(n);

            return fraction;
        }

        private double RoundFractional(double n, double interval)
        {
            return Math.Round(n * interval, 0) / interval;
        }

        #endregion
    }
}