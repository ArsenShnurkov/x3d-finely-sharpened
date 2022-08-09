using System;

namespace X3D.Platform
{
    public class X3DPlatform
    {
        public static PlatformID PlatformID = Environment.OSVersion.Platform;

        public static bool IsWindows
        {
            get
            {
                var isWindows = false;

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
                }

                return isWindows;
            }
        }

        public static bool IsUnix => PlatformID == PlatformID.Unix;

        public static bool IsMacOSX => PlatformID == PlatformID.MacOSX;
    }
}