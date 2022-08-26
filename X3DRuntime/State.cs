using System;

namespace X3D.Runtime
{
    public partial class X3DApplication
    {
        public static float playerDirectionMagnitude = 0.1f;
        public static float movementSpeed = 1.0f;

        private int _fps;
        private int draw_time;


        private int fps;
        private bool fx_enable = true;
        private bool points_only;

        private bool rotate_enable = true;
        private DateTime time_at_init;
        private bool wireframe;

        /// <summary>
        /// The current rotation.
        /// </summary>
        //private static float rotation = 0.0f;

        /// <summary>
        ///     The current time in the Virtual World
        ///     The VWT
        /// </summary>
        public static TimeSpan WorldTime { get; set; }


        //Grid gridPointOfReferenceX,gridPointOfReferenceY,gridPointOfReferenceZ;
    }
}