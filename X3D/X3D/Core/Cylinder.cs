using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using X3D.Core;
using X3D.Core.Shading;

namespace X3D
{
    public partial class Cylinder
    {
        private Shape parentShape;

        // Use IndexedFaceSet but dont connect it to the actual scene graph the current Cone is in.
        private IndexedFaceSet ifs1;

        internal PackedGeometry _pack;

        public override void CollectGeometry(
                            RenderingContext rc,
                            out GeometryHandle handle,
                            out BoundingBox bbox,
                            out bool Coloring,
                            out bool Texturing)
        {



            handle = GeometryHandle.Zero;
            bbox = BoundingBox.Zero;
            Texturing = true;
            Coloring = true;

            parentShape = GetParent<Shape>();

            buildCylinderGeometry();

            _pack = PackedGeometry.Pack(ifs1);

            // BUFFER GEOMETRY
            handle = Buffering.BufferShaderGeometry(_pack);
        }

        #region Rendering Methods

        //public override void Load()
        //{
        //    base.Load();

        //    buildCylinderGeometry();
        //}

        //public override void PreRenderOnce(RenderingContext rc)
        //{
        //    base.PreRenderOnce(rc);

        //    parentShape = GetParent<Shape>();

        //    ifs1.PreRenderOnce(rc);
        //}

        //public override void Render(RenderingContext rc)
        //{
        //    base.Render(rc);


        //    rc.PushMatricies();

        //    rc.matricies.Scale = new Vector3(2 * this.radius, (1f * height), 2 * radius);  // Too easy, almost feel like im cheating here.

        //    rc.matricies.Scale *= new Vector3(0.1f, 0.1f, 0.1f);



        //    ifs1.Render(rc);

        //    rc.PopMatricies();
        //}

        #endregion

        private void buildCylinderGeometry()
        {
            Normal normal;
            Coordinate coordinate;

            ifs1 = new IndexedFaceSet();
            coordinate = new Coordinate();
            normal = new Normal();

            ifs1.normalPerVertex = true;
            ifs1.normalIndex = "0 1 17 16 -1 0 15 14 13 12 11 10 9 8 7 6 5 4 3 2 1 -1 0 16 31 15 -1 1 2 18 17 -1 2 3 19 18 -1 3 4 20 19 -1 4 5 21 20 -1 5 6 22 21 -1 6 7 23 22 -1 7 8 24 23 -1 8 9 25 24 -1 9 10 26 25 -1 10 11 27 26 -1 11 12 28 27 -1 12 13 29 28 -1 13 14 30 29 -1 14 15 31 30 -1 16 17 18 19 20 21 22 23 24 25 26 27 28 29 30 31 -1";
            ifs1.coordIndex = "0 1 17 16 -1 0 15 14 13 12 11 10 9 8 7 6 5 4 3 2 1 -1 0 16 31 15 -1 1 2 18 17 -1 2 3 19 18 -1 3 4 20 19 -1 4 5 21 20 -1 5 6 22 21 -1 6 7 23 22 -1 7 8 24 23 -1 8 9 25 24 -1 9 10 26 25 -1 10 11 27 26 -1 11 12 28 27 -1 12 13 29 28 -1 13 14 30 29 -1 14 15 31 30 -1 16 17 18 19 20 21 22 23 24 25 26 27 28 29 30 31 -1";
            coordinate.point = "1.0 1.0 0.0, 0.9238795 1.0 0.38268343, 0.70710677 1.0 0.70710677, 0.38268343 1.0 0.9238795, 6.123234E-17 1.0 1.0, -0.38268343 1.0 0.9238795, -0.70710677 1.0 0.70710677, -0.9238795 1.0 0.38268343, -1.0 1.0 1.2246469E-16, -0.9238795 1.0 -0.38268343, -0.70710677 1.0 -0.70710677, -0.38268343 1.0 -0.9238795, -1.8369701E-16 1.0 -1.0, 0.38268343 1.0 -0.9238795, 0.70710677 1.0 -0.70710677, 0.9238795 1.0 -0.38268343, 1.0 -1.0 0.0, 0.9238795 -1.0 0.38268343, 0.70710677 -1.0 0.70710677, 0.38268343 -1.0 0.9238795, 6.123234E-17 -1.0 1.0, -0.38268343 -1.0 0.9238795, -0.70710677 -1.0 0.70710677, -0.9238795 -1.0 0.38268343, -1.0 -1.0 1.2246469E-16, -0.9238795 -1.0 -0.38268343, -0.70710677 -1.0 -0.70710677, -0.38268343 -1.0 -0.9238795, -1.8369701E-16 -1.0 -1.0, 0.38268343 -1.0 -0.9238795, 0.70710677 -1.0 -0.70710677, 0.9238795 -1.0 -0.38268343,";
            normal.vector = "0.89090914 0.45418155 -2.2690927E-16, 0.82309276 0.45418155 0.34093618, 0.6299679 0.45418155 0.6299679, 0.34093618 0.45418155 0.82309276, 0.0 0.45418155 0.89090914, -0.34093618 0.45418155 0.82309276, -0.6299679 0.45418155 0.6299679, -0.82309276 0.45418155 0.34093618, -0.89090914 0.45418155 1.260607E-17, -0.82309276 0.45418155 -0.34093618, -0.6299679 0.45418155 -0.6299679, -0.34093618 0.45418155 -0.82309276, -1.3866677E-16 0.45418155 -0.89090914, 0.34093618 0.45418155 -0.82309276, 0.6299679 0.45418155 -0.6299679, 0.82309276 0.45418155 -0.34093618, 0.89090914 -0.45418155 -1.8909105E-16, 0.82309276 -0.45418155 0.34093618, 0.6299679 -0.45418155 0.6299679, 0.34093618 -0.45418155 0.82309276, 0.0 -0.45418155 0.89090914, -0.34093618 -0.45418155 0.82309276, -0.6299679 -0.45418155 0.6299679, -0.82309276 -0.45418155 0.34093618, -0.89090914 -0.45418155 5.042428E-17, -0.82309276 -0.45418155 -0.34093618, -0.6299679 -0.45418155 -0.6299679, -0.34093618 -0.45418155 -0.82309276, -1.3866677E-16 -0.45418155 -0.89090914, 0.34093618 -0.45418155 -0.82309276, 0.6299679 -0.45418155 -0.6299679, 0.82309276 -0.45418155 -0.34093618, ";

            //ifs1.coordinate = coordinate;
            //ifs1.normal = normal;
            ifs1.Items.Add(coordinate);
            ifs1.Items.Add(normal);
            ifs1.Children.Add(coordinate);
            ifs1.Children.Add(normal);
            ifs1.Parent = this.Parent;
        }
    }
}
