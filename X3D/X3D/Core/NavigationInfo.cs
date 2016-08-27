using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using X3D.Engine;
using X3D.Parser;

namespace X3D
{
    public partial class NavigationInfo
    {
        public static bool HeadlightEnabled  = true;
        public static NavigationType NavigationType = NavigationType.Walk; // should be Examine but it buggy
        public static Vector3 AvatarSize = Vector3.Zero;

        public override void Load()
        {
            base.Load();
            
            string navType = this.type.ToUpper();
             
            if (navType.Contains("ANY"))
            {
                NavigationType = NavigationType.Examine;
            }
            else if (navType.Contains("EXAMINE"))
            {
                NavigationType = NavigationType.Examine;
            }
            else if (navType.Contains("WALK"))
            {
                NavigationType = NavigationType.Walk;
            }
            else if (navType.Contains("FLY"))
            {
                NavigationType = NavigationType.Fly;
            }

            AvatarSize = X3DTypeConverters.SFVec3f(avatarSize);

            HeadlightEnabled = this.headlight;
        }
    }
}
