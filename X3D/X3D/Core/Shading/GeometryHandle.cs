namespace X3D.Core.Shading
{
    public class GeometryHandle
    {
        /// <summary>
        ///     The bounding box enclosing the geometry of the shape.
        ///     (A product of both the Triangle and Quad buffers)
        /// </summary>
        public BoundingBox bbox;

        /// <summary>
        ///     If there were any color coordinates interleaved.
        /// </summary>
        public bool Coloring;

        /// <summary>
        ///     The number of verticies in the Triangle verticies buffer.
        /// </summary>
        public int NumVerticies3;

        /// <summary>
        ///     The number of verticies in the Quad verticies buffer.
        /// </summary>
        public int NumVerticies4;

        /// <summary>
        ///     If there were any texture coordinates interleaved.
        /// </summary>
        public bool Texturing;

        /// <summary>
        ///     The OpenGL handle to the Triangle verticies buffer.
        /// </summary>
        public int vbo3;

        /// <summary>
        ///     The OpenGL handle to the Quad verticies buffer.
        /// </summary>
        public int vbo4;

        /// <summary>
        ///     If there was any geometry buffered.
        /// </summary>
        public bool HasGeometry => (vbo3 > -1 && NumVerticies3 > 0) || (vbo4 > -1 && NumVerticies4 > 0);

        public static GeometryHandle Zero
        {
            get
            {
                GeometryHandle zero;

                zero = new GeometryHandle();
                zero.vbo3 = -1;
                zero.vbo4 = -1;
                zero.NumVerticies3 = 0;
                zero.NumVerticies4 = 0;
                zero.bbox = new BoundingBox();
                zero.Coloring = false;
                zero.Texturing = false;

                return zero;
            }
        }
    }
}