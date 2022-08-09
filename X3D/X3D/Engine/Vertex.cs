using System.Runtime.InteropServices;
using OpenTK;

namespace X3D.Core
{
    /// <summary>
    ///     Vertex structure for Interleaved Geometry and Meshing.
    ///     Mimics InterleavedArrayFormat.T2fC4fN3fV3f
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Vertex
    {
        /// <summary>
        ///     Required
        /// </summary>
        public Vector3 Position;

        public Vector3 Normal;

        public Vector4 Color;

        public Vector2 TexCoord;

        public static Vertex Zero
        {
            get
            {
                var v = new Vertex();
                v.Position = Vector3.Zero;
                v.TexCoord = Vector2.Zero;
                v.Normal = Vector3.Zero;
                v.Color = new Vector4(0.0f, 0, 0, 1.0f);
                return v;
            }
        }

        #region Constructors

        public Vertex(Vector3 position)
        {
            Position = position;
            TexCoord = Vector2.Zero;
            Normal = Vector3.Zero;
            Color = new Vector4(0.0f, 0, 0, 1.0f);
        }

        public Vertex(Vector3 position, Vector2 texCoord)
        {
            Position = position;
            TexCoord = texCoord;
            Normal = Vector3.Zero;
            Color = new Vector4(0.0f, 0, 0, 1.0f);
        }

        public Vertex(Vector3 position, Vector2 texCoord, Vector3 norm)
        {
            Position = position;
            TexCoord = texCoord;
            Normal = norm;
            Color = new Vector4(0.0f, 0, 0, 1.0f);
        }

        public Vertex(Vector3 position, Vector2 texCoord, Vector3 norm, Vector4 color)
        {
            Position = position;
            TexCoord = texCoord;
            Normal = norm;
            Color = color;
        }

        #endregion

        public static readonly int SizeInBytes =
            Vector2.SizeInBytes + Vector4.SizeInBytes + Vector3.SizeInBytes + Vector3.SizeInBytes;

        public static readonly int Stride = Marshal.SizeOf(default(Vertex));
    }
}