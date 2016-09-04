using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace X3D.Platform
{
    public class X3DPlatform
    {
        public static PlatformID PlatformID = Environment.OSVersion.Platform;

        public static bool IsWindows
        {
            get
            {
                bool isWindows = false;

                switch (PlatformID)
                {
                    case PlatformID.Win32NT:
                        isWindows = true;
                        break;
                    case PlatformID.Win32S:
                        isWindows = true;
                        break;
                    case PlatformID.Win32Windows:
                        isWindows = true;
                        break;
                    case PlatformID.WinCE:
                        isWindows = true;
                        break;
                    default:
                        break;
                }

                return isWindows;
            }
        }

        public static bool IsUnix
        {
            get
            {
                return PlatformID == PlatformID.Unix;
            }
        }

        public static bool IsMacOSX
        {
            get
            {
                return PlatformID == PlatformID.MacOSX;
            }
        }
    }
}
