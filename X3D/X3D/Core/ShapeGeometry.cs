#region --- License ---
/* Copyright (c) 2006, 2007 Stefanos Apostolopoulos
 * See license.txt for license info
 */
#endregion

using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace X3D
{
    public abstract class ShapeGeometry
    {
        private Vector3[] vertices, normals;
        private Vector2[] texcoords;
        private int[] indices;
        private int[] colors;

        private Vbo _vbo;

        public Vector3[] Vertices
        {
            get { return vertices; }
            protected set
            {
                vertices = value;
            }
        }

        public Vector3[] Normals
        {
            get { return normals; }
            protected set
            {
                normals = value;
            }
        }

        public Vector2[] Texcoords
        {
            get { return texcoords; }
            protected set
            {
                texcoords = value;
            }
        }

        public int[] Indices
        {
            get { return indices; }
            protected set
            {
                indices = value;
            }
        }

        public int[] Colors
        {
            get { return colors; }
            protected set
            {
                colors = value;
            }
        }

        public void Load()
        {
            _vbo = Vbo.LoadVBO(this);
        }

        public void Render()
        {
            _vbo.Draw();
        }

        /// <summary>
        /// Converts a System.Drawing.Color to a System.Int32.
        /// </summary>
        /// <param name="c">The System.Drawing.Color to convert.</param>
        /// <returns>A System.Int32 containing the R, G, B, A values of the
        /// given System.Drawing.Color in the Rbga32 format.</returns>
        public static int ColorToRgba32(System.Drawing.Color c)
        {
            return (int)((c.A << 24) | (c.B << 16) | (c.G << 8) | c.R);
        }

        public struct Vbo
        {
            public int VertexBufferID;
            public int ColorBufferID;
            public int TexCoordBufferID;
            public int NormalBufferID;
            public int ElementBufferID;
            public int NumElements;

            /// <summary>
            /// Generate a VertexBuffer for each of Color, Normal, TextureCoordinate, Vertex, and Indices
            /// </summary>
            public static Vbo LoadVBO(ShapeGeometry shape)
            {
                Vbo vbo = new Vbo();

                if (shape.Vertices == null) return vbo;
                if (shape.Indices == null) return vbo;

                int bufferSize;

                // Color Array Buffer
                if (shape.Colors != null)
                {
                    // Generate Array Buffer Id
                    GL.GenBuffers(1, out vbo.ColorBufferID);

                    // Bind current context to Array Buffer ID
                    GL.BindBuffer(BufferTarget.ArrayBuffer, vbo.ColorBufferID);

                    // Send data to buffer
                    GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(shape.Colors.Length * sizeof(int)), shape.Colors, BufferUsageHint.StaticDraw);

                    // Validate that the buffer is the correct size
                    GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out bufferSize);
                    if (shape.Colors.Length * sizeof(int) != bufferSize)
                        throw new ApplicationException("Vertex array not uploaded correctly");

                    // Clear the buffer Binding
                    GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
                }

                // Normal Array Buffer
                if (shape.Normals != null)
                {
                    // Generate Array Buffer Id
                    GL.GenBuffers(1, out vbo.NormalBufferID);

                    // Bind current context to Array Buffer ID
                    GL.BindBuffer(BufferTarget.ArrayBuffer, vbo.NormalBufferID);

                    // Send data to buffer
                    GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(shape.Normals.Length * Vector3.SizeInBytes), shape.Normals, BufferUsageHint.StaticDraw);

                    // Validate that the buffer is the correct size
                    GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out bufferSize);
                    if (shape.Normals.Length * Vector3.SizeInBytes != bufferSize)
                        throw new ApplicationException("Normal array not uploaded correctly");

                    // Clear the buffer Binding
                    GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
                }

                // TexCoord Array Buffer
                if (shape.Texcoords != null)
                {
                    // Generate Array Buffer Id
                    GL.GenBuffers(1, out vbo.TexCoordBufferID);

                    // Bind current context to Array Buffer ID
                    GL.BindBuffer(BufferTarget.ArrayBuffer, vbo.TexCoordBufferID);

                    // Send data to buffer
                    GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(shape.Texcoords.Length * 8), shape.Texcoords, BufferUsageHint.StaticDraw);

                    // Validate that the buffer is the correct size
                    GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out bufferSize);
                    if (shape.Texcoords.Length * 8 != bufferSize)
                        throw new ApplicationException("TexCoord array not uploaded correctly");

                    // Clear the buffer Binding
                    GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
                }

                // Vertex Array Buffer
                {
                    // Generate Array Buffer Id
                    GL.GenBuffers(1, out vbo.VertexBufferID);

                    // Bind current context to Array Buffer ID
                    GL.BindBuffer(BufferTarget.ArrayBuffer, vbo.VertexBufferID);

                    // Send data to buffer
                    GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(shape.Vertices.Length * Vector3.SizeInBytes), shape.Vertices, BufferUsageHint.DynamicDraw);

                    // Validate that the buffer is the correct size
                    GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out bufferSize);
                    if (shape.Vertices.Length * Vector3.SizeInBytes != bufferSize)
                        throw new ApplicationException("Vertex array not uploaded correctly");

                    // Clear the buffer Binding
                    GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
                }

                // Element Array Buffer
                {
                    // Generate Array Buffer Id
                    GL.GenBuffers(1, out vbo.ElementBufferID);

                    // Bind current context to Array Buffer ID
                    GL.BindBuffer(BufferTarget.ElementArrayBuffer, vbo.ElementBufferID);

                    // Send data to buffer
                    GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(shape.Indices.Length * sizeof(int)), shape.Indices, BufferUsageHint.StaticDraw);

                    // Validate that the buffer is the correct size
                    GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out bufferSize);
                    if (shape.Indices.Length * sizeof(int) != bufferSize)
                        throw new ApplicationException("Element array not uploaded correctly");

                    // Clear the buffer Binding
                    GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
                }

                // Store the number of elements for the DrawElements call
                vbo.NumElements = shape.Indices.Length;

                return vbo;
            }

            public void Draw()
            {
                // Push current Array Buffer state so we can restore it later
                GL.PushClientAttrib(ClientAttribMask.ClientVertexArrayBit);

                if (this.VertexBufferID == 0) return;
                if (this.ElementBufferID == 0) return;

                if (GL.IsEnabled(EnableCap.Lighting))
                {
                    // Normal Array Buffer
                    if (this.NormalBufferID != 0)
                    {
                        // Bind to the Array Buffer ID
                        GL.BindBuffer(BufferTarget.ArrayBuffer, this.NormalBufferID);

                        // Set the Pointer to the current bound array describing how the data ia stored
                        GL.NormalPointer(NormalPointerType.Float, Vector3.SizeInBytes, IntPtr.Zero);

                        // Enable the client state so it will use this array buffer pointer
                        GL.EnableClientState(EnableCap.NormalArray);
                    }
                }
                else
                {
                    // Color Array Buffer (Colors not used when lighting is enabled)
                    if (this.ColorBufferID != 0)
                    {
                        // Bind to the Array Buffer ID
                        GL.BindBuffer(BufferTarget.ArrayBuffer, this.ColorBufferID);

                        // Set the Pointer to the current bound array describing how the data ia stored
                        GL.ColorPointer(4, ColorPointerType.UnsignedByte, sizeof(int), IntPtr.Zero);

                        // Enable the client state so it will use this array buffer pointer
                        GL.EnableClientState(EnableCap.ColorArray);
                    }
                }

                // Texture Array Buffer
                if (GL.IsEnabled(EnableCap.Texture2D))
                {
                    if (this.TexCoordBufferID != 0)
                    {
                        // Bind to the Array Buffer ID
                        GL.BindBuffer(BufferTarget.ArrayBuffer, this.TexCoordBufferID);

                        // Set the Pointer to the current bound array describing how the data ia stored
                        GL.TexCoordPointer(2, TexCoordPointerType.Float, 8, IntPtr.Zero);

                        // Enable the client state so it will use this array buffer pointer
                        GL.EnableClientState(EnableCap.TextureCoordArray);
                    }
                }

                // Vertex Array Buffer
                {
                    // Bind to the Array Buffer ID
                    GL.BindBuffer(BufferTarget.ArrayBuffer, this.VertexBufferID);

                    // Set the Pointer to the current bound array describing how the data ia stored
                    GL.VertexPointer(3, VertexPointerType.Float, Vector3.SizeInBytes, IntPtr.Zero);

                    // Enable the client state so it will use this array buffer pointer
                    GL.EnableClientState(EnableCap.VertexArray);
                }

                // Element Array Buffer
                {
                    // Bind to the Array Buffer ID
                    GL.BindBuffer(BufferTarget.ElementArrayBuffer, this.ElementBufferID);

                    // Draw the elements in the element array buffer
                    // Draws up items in the Color, Vertex, TexCoordinate, and Normal Buffers using indices in the ElementArrayBuffer
                    GL.DrawElements(BeginMode.Triangles, this.NumElements, DrawElementsType.UnsignedInt, IntPtr.Zero);

                    // Could also call GL.DrawArrays which would ignore the ElementArrayBuffer and just use primitives
                    // Of course we would have to reorder our data to be in the correct primitive order
                }

                // Restore the state
                GL.PopClientAttrib();
            }
        }
    }

}
