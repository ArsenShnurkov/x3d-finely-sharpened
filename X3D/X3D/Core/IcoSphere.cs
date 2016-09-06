// TODO: calculate texcoords using spherical equation in shader

using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics.OpenGL4;
using X3D.Core;
using OpenTK.Input;
using X3D.Core.Shading.DefaultUniforms;
using X3D.Core.Shading;
using X3D.Parser;

namespace X3D
{
    /// <summary>
    /// A Sphere with underlying geometry implemented as an Ico-Sphere.
    /// No inbuilt tessellator.
    /// </summary>
    public partial class IcoSphere : X3DGeometryNode
    {
        internal PackedGeometry _pack;

        private Shape parentShape;

        public override void CollectGeometry(
                            RenderingContext rc,
                            out GeometryHandle handle,
                            out BoundingBox bbox,
                            out bool coloring,
                            out bool texturing)
        {
            handle = GeometryHandle.Zero;
            bbox = BoundingBox.Zero;
            coloring = false;
            texturing = false;

            parentShape = GetParent<Shape>();

            _pack = new PackedGeometry();
            _pack.restartIndex = -1;
            _pack._indices = Faces;
            _pack._coords = Verts;
            _pack.bbox = BoundingBox.CalculateBoundingBox(Verts);
            
            _pack.Interleave();

            // BUFFER GEOMETRY
            handle = Buffering.BufferShaderGeometry(_pack);
        }

        #region Rendering Methods

        public override void Render(RenderingContext rc)
        {
            //var scale = new Vector3(0.04f, 0.04f, 0.04f);

            //rc.cam.Scale = scale;
        }
        
        #endregion

        #region Icosahedron Geometry

        int[] Faces = new int[] 
        {
            0, 12, 17, -1,
            0, 13, 12, -1,
            0, 16, 13, -1,
            0, 17, 19, -1,
            0, 19, 16, -1,
            1, 13, 15, -1,
            1, 14, 13, -1,
            1, 15, 22, -1,
            1, 22, 25, -1,
            1, 25, 14, -1,
            2, 12, 14, -1,
            2, 14, 24, -1,
            2, 18, 12, -1,
            2, 24, 27, -1,
            2, 27, 18, -1,
            3, 17, 18, -1,
            3, 18, 26, -1,
            3, 20, 17, -1,
            3, 26, 29, -1,
            3, 29, 20, -1,
            4, 19, 20, -1,
            4, 20, 28, -1,
            4, 21, 19, -1,
            4, 28, 31, -1,
            4, 31, 21, -1,
            5, 15, 16, -1,
            5, 16, 21, -1,
            5, 21, 30, -1,
            5, 23, 15, -1,
            5, 30, 23, -1,
            6, 24, 25, -1,
            6, 25, 32, -1,
            6, 32, 37, -1,
            6, 33, 24, -1,
            6, 37, 33, -1,
            7, 26, 27, -1,
            7, 27, 33, -1,
            7, 33, 39, -1,
            7, 34, 26, -1,
            7, 39, 34, -1,
            8, 28, 29, -1,
            8, 29, 34, -1,
            8, 34, 40, -1,
            8, 35, 28, -1,
            8, 40, 35, -1,
            9, 30, 31, -1,
            9, 31, 35, -1,
            9, 35, 41, -1,
            9, 36, 30, -1,
            9, 41, 36, -1,
            10, 22, 23, -1,
            10, 23, 36, -1,
            10, 32, 22, -1,
            10, 36, 38, -1,
            10, 38, 32, -1,
            11, 37, 38, -1,
            11, 38, 41, -1,
            11, 39, 37, -1,
            11, 40, 39, -1,
            11, 41, 40, -1,
            12, 13, 14, -1,
            12, 18, 17, -1,
            13, 16, 15, -1,
            14, 25, 24, -1,
            15, 23, 22, -1,
            16, 19, 21, -1,
            17, 20, 19, -1,
            18, 27, 26, -1,
            20, 29, 28, -1,
            21, 31, 30, -1,
            22, 32, 25, -1,
            23, 30, 36, -1,
            24, 33, 27, -1,
            26, 34, 29, -1,
            28, 35, 31, -1,
            32, 38, 37, -1,
            33, 37, 39, -1,
            34, 39, 40, -1,
            35, 40, 41, -1,
            36, 41, 38, -1
        };
 
        //TODO: scale values correctly
        Vector3[] Verts = new Vector3[] 
        {
            new Vector3(5.571941E-8f, -1.0f, 1.7614703E-8f),
            new Vector3(0.7236071f, -0.4472194f, 0.52572536f),
            new Vector3(-0.2763881f, -0.44721988f, 0.85064936f),
            new Vector3(-0.8944263f, -0.4472156f, 1.7614703E-8f),
            new Vector3(-0.2763881f, -0.44721988f, -0.85064936f),
            new Vector3(0.7236071f, -0.4472194f, -0.52572536f),
            new Vector3(0.27638823f, 0.44721982f, 0.85064936f),
            new Vector3(-0.723607f, 0.44721934f, 0.52572536f),
            new Vector3(-0.723607f, 0.44721934f, -0.52572536f),
            new Vector3(0.27638823f, 0.44721982f, -0.85064936f),
            new Vector3(0.8944264f, 0.44721553f, 1.7614703E-8f),
            new Vector3(5.571941E-8f, 1.0f, 1.7614703E-8f),
            new Vector3(-0.1624555f, -0.85065436f, 0.49999526f),
            new Vector3(0.4253226f, -0.8506541f, 0.3090115f),
            new Vector3(0.26286894f, -0.52573776f, 0.80901146f),
            new Vector3(0.850648f, -0.52573586f, 1.7614703E-8f),
            new Vector3(0.4253226f, -0.8506541f, -0.30901143f),
            new Vector3(-0.5257301f, -0.85065174f, 1.7614703E-8f),
            new Vector3(-0.68818945f, -0.52573633f, 0.49999717f),
            new Vector3(-0.1624555f, -0.85065436f, -0.4999952f),
            new Vector3(-0.68818945f, -0.52573633f, -0.49999687f),
            new Vector3(0.26286894f, -0.52573776f, -0.8090117f),
            new Vector3(0.9510575f, -2.442427E-8f, 0.30901244f),
            new Vector3(0.9510575f, -2.442427E-8f, -0.30901262f),
            new Vector3(5.571941E-8f, -2.442427E-8f, 1.0f),
            new Vector3(0.5877858f, -2.442427E-8f, 0.8090167f),
            new Vector3(-0.95105785f, -2.442427E-8f, 0.30901244f),
            new Vector3(-0.58778566f, -2.442427E-8f, 0.8090167f),
            new Vector3(-0.58778566f, -2.442427E-8f, -0.8090167f),
            new Vector3(-0.95105785f, -2.442427E-8f, -0.30901262f),
            new Vector3(0.5877858f, -2.442427E-8f, -0.8090167f),
            new Vector3(5.571941E-8f, -2.442427E-8f, -1.0f),
            new Vector3(0.68818957f, 0.52573633f, 0.49999717f),
            new Vector3(-0.26286882f, 0.52573776f, 0.80901146f),
            new Vector3(-0.85064787f, 0.52573586f, 1.7614703E-8f),
            new Vector3(-0.26286882f, 0.52573776f, -0.8090117f),
            new Vector3(0.68818957f, 0.52573633f, -0.49999687f),
            new Vector3(0.16245562f, 0.8506546f, 0.49999526f),
            new Vector3(0.5257302f, 0.85065174f, 1.7614703E-8f),
            new Vector3(-0.42532247f, 0.8506541f, 0.3090115f),
            new Vector3(-0.42532247f, 0.8506541f, -0.30901143f),
            new Vector3(0.16245562f, 0.8506546f, -0.4999952f)
        };
        #endregion
    }
}
