#define EXPERIMENTAL
#define MESHING
//#define WEBGL // If defined, we will be compiled for WebGL support
//#define NO_VBOS	// If Defined, VBOs Will Be Forced Off
#define ASSERT_VBOS
//#define NO_TEXTURING

namespace X3D
{
    public sealed class _Build
    {

#if EXPERIMENTAL
        public static bool EXPERIMENTAL_VERSION = true;
#else
        public static bool EXPERIMENTAL_VERSION=false;
#endif
#if NO_VBOS
        public static bool NO_VBOS=true;
        public static bool VBOSupported=false;
#else
#if ASSERT_VBOS
        public static bool NO_VBOS = false;
        public static bool VBOSupported = true;
#else
        public static bool NO_VBOS=false;
        public static bool VBOSupported;
#endif
#endif

#if WEBGL
        public static bool WEBGLSupported=true;
#else
        public static bool WEBGLSupported = false;
#endif

#if MESHING
        public static bool MESHING = true;
#else
        public static bool MESHING=false;
#endif
#if NO_TEXTURING
        public static bool NO_TEXTURING=true;
#else
        public static bool NO_TEXTURING = false;
#endif

    }
}