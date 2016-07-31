using System;
using OpenTK;

using X3D;

namespace x3druntime.ui.opentk
{
    public partial class X3DApplication
    {

        public static float playerDirectionMagnitude = 0.1f;
        public static float movementSpeed = 1.0f;

        bool rotate_enable = true;
        bool fx_enable = true;
        bool wireframe = false;
        bool points_only = false;

        /// <summary>
        /// The current rotation.
        /// </summary>
        //private static float rotation = 0.0f;

        /// <summary>
        /// The current time in the Virtual World
        /// The VWT
        /// </summary>
        public static TimeSpan WorldTime
        {
            get;
            set;
        }

        const float piover180 = 0.0174532925f;


        int fps;

        private int _fps = 0;
        private int draw_time;
        private DateTime time_at_init;


        //Grid gridPointOfReferenceX,gridPointOfReferenceY,gridPointOfReferenceZ;
    }
}
