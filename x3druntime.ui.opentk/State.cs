using System;
using OpenTK;

using X3D;

namespace x3druntime.ui.opentk
{
    public partial class X3DApplication
    {

        public static float playerDirectionMagnitude = 0.1f;

        bool rotate_enable = true;
        bool fx_enable = true;
        bool wireframe = false;
        bool points_only = false;

        /// <summary>
        /// The current rotation.
        /// </summary>
        private static float rotation = 0.0f;

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
        float heading;
        float xpos;
        float ypos;
        float zpos;

        private float yrot;				// Y Rotation
        private float walkbias = 0;
        private float walkbiasangle = 0;
        private float lookupdown = 0.0f;
        private float lookleftright = 0.0f;
        private float z = 0.0f;				// Depth Into The Screen
        int fps;

        private double _time = 0.0, _frames = 0.0;
        private int _fps = 0;
        private double prev_time = 0;
        private int draw_time;
        private DateTime time_at_init;

        Matrix4 modelview;
        Matrix4 textureview;

        //Grid gridPointOfReferenceX,gridPointOfReferenceY,gridPointOfReferenceZ;
    }
}
