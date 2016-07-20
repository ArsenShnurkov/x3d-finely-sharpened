using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using X3D.Engine;

namespace X3D
{
    public partial class NavigationInfo
    {
        public static NavigationType NavigationType = NavigationType.Examine;

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
        }
    }
}
