using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace X3D.Parser // TODO: put in X3D.Engine namespace
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Vertex
    { // mimic InterleavedArrayFormat.T2fC4fN3fV3f
      /// <summary>
      /// Required
      /// </summary>
        public Vector3 Position;
        public Vector3 Normal;
        //public Vector4 Color;
        public Vector2 TexCoord;

        public static Vertex Zero
        {
            get
            {
                Vertex v = new Vertex();
                v.TexCoord = Vector2.Zero;
                v.Normal = Vector3.Zero;
                return v;
            }
        }

        #region Constructors

        public Vertex(Vector3 position)
        {
            this.Position = position;
            this.TexCoord = Vector2.Zero;
            this.Normal = Vector3.Zero;
        }

        public Vertex(Vector3 position, Vector2 texCoord)
        {
            this.Position = position;
            this.TexCoord = texCoord;
            this.Normal = Vector3.Zero;
        }

        public Vertex(Vector3 position, Vector2 texCoord, Vector3 norm)
        {
            this.Position = position;
            this.TexCoord = texCoord;
            this.Normal = norm;
        }

        #endregion

        public static readonly int SizeInBytes = Vector2.SizeInBytes + Vector4.SizeInBytes + Vector3.SizeInBytes + Vector3.SizeInBytes;

        public static readonly int Stride = Marshal.SizeOf(default(Vertex));
    }
}
