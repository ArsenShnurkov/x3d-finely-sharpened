using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace X3D
{
    public partial class X3D
    {
        public static double X3DEngineVersionNum = 3.3;
        public static x3dVersion X3DEngineVersion = x3dVersion.X3D_3_3;
        public static profileNames X3DEngineProfile = profileNames.Interchange;

        public X3D()
        {
            this._version = x3dVersion.X3D_3_3;
            this._profile = profileNames.Interchange;
        }

        public override void Load()
        {
            base.Load();

            if(!string.IsNullOrEmpty(this.version) && float.Parse(this.version) > 3.3)
            {
                Console.WriteLine("X3D Document requires version {0} which is not available current browser version is X3D v{1}", this.version, X3DEngineVersionNum);
            }
            if (!(profile == profileNames.Core || profile == profileNames.Interchange))
            {
                Console.WriteLine("X3D Document requires profile {0} which is not available current browser supports {1} profile", this.profile, X3DEngineProfile);
            }

        }
    }
}
