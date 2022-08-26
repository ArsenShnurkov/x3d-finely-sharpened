// TODO: implement bottomRadius

using System.Collections.Generic;
using OpenTK;
using X3D.Core.Shading;

namespace X3D
{
    public partial class Cone
    {
        /// <summary>
        ///     Defines two cone geometries: one with and without a bottom face.
        ///     Not computed for faster results.
        ///     TODO: need to translate bottom face verticies using bottomRadius.
        /// </summary>
        private void buildConeGeometry()
        {
            Normal normal, normal2;
            Coordinate coordinate, coordinate2;

            ifs1 = new IndexedFaceSet();
            coordinate = new Coordinate();
            normal = new Normal();
            ifs2 = new IndexedFaceSet();
            coordinate2 = new Coordinate();
            normal2 = new Normal();
            ifs1.normalPerVertex = true;
            ifs2.normalPerVertex = true;
            ifs1.normalIndex =
                "0 1 11 -1 0 11 12 -1 0 12 15 -1 0 15 16 -1 0 16 1 -1 1 2 10 -1 1 10 11 -1 1 16 2 -1 2 3 9 -1 2 9 10 -1 2 16 3 -1 3 4 8 -1 3 8 9 -1 3 16 4 -1 4 5 7 -1 4 7 8 -1 4 16 5 -1 5 6 7 -1 5 16 6 -1 6 16 7 -1 7 16 8 -1 8 16 9 -1 9 16 10 -1 10 16 11 -1 11 16 12 -1 12 13 15 -1 12 16 13 -1 13 14 15 -1 13 16 14 -1 14 16 15 -1";
            ifs2.normalIndex =
                "0 15 16 -1 0 16 1 -1 1 16 2 -1 2 16 3 -1 3 16 4 -1 4 16 5 -1 5 16 6 -1 6 16 7 -1 7 16 8 -1 8 16 9 -1 9 16 10 -1 10 16 11 -1 11 16 12 -1 12 16 13 -1 13 16 14 -1 14 16 15 -1";
            ifs1.coordIndex =
                "0 1 11 -1 0 11 12 -1 0 12 15 -1 0 15 16 -1 0 16 1 -1 1 2 10 -1 1 10 11 -1 1 16 2 -1 2 3 9 -1 2 9 10 -1 2 16 3 -1 3 4 8 -1 3 8 9 -1 3 16 4 -1 4 5 7 -1 4 7 8 -1 4 16 5 -1 5 6 7 -1 5 16 6 -1 6 16 7 -1 7 16 8 -1 8 16 9 -1 9 16 10 -1 10 16 11 -1 11 16 12 -1 12 13 15 -1 12 16 13 -1 13 14 15 -1 13 16 14 -1 14 16 15 -1";
            ifs2.coordIndex =
                "0 15 16 -1 0 16 1 -1 1 16 2 -1 2 16 3 -1 3 16 4 -1 4 16 5 -1 5 16 6 -1 6 16 7 -1 7 16 8 -1 8 16 9 -1 9 16 10 -1 10 16 11 -1 11 16 12 -1 12 16 13 -1 13 16 14 -1 14 16 15 -1";
            coordinate.point =
                "1.0 -0.8800123 -0.117623776, 0.9238795 -1.26262 -0.12523744, 0.70710677 -1.5869792 -0.13169198, 0.38268343 -1.8037089 -0.13600476, 6.123234E-17 -1.8798144 -0.13751921, -0.38268343 -1.8037089 -0.13600476, -0.70710677 -1.5869792 -0.13169198, -0.9238795 -1.26262 -0.12523744, -1.0 -0.8800123 -0.117623776, -0.9238795 -0.4974046 -0.11001012, -0.70710677 -0.17304549 -0.103555575, -0.38268343 0.043684363 -0.099242784, -1.8369701E-16 0.119789764 -0.097728334, 0.38268343 0.043684363 -0.099242784, 0.70710677 -0.17304549 -0.103555575, 0.9238795 -0.4974046 -0.11001012, 0.0 -0.9198032 1.8819804";
            coordinate2.point =
                "1.0 -0.8756627 -0.117456675, 0.9238795 -1.2577269 -0.13921875, 0.70710677 -1.5816252 -0.15766774, 0.38268343 -1.7980472 -0.16999497, 6.123234E-17 -1.8740444 -0.17432372, -0.38268343 -1.7980472 -0.16999497, -0.70710677 -1.5816252 -0.15766774, -0.9238795 -1.2577269 -0.13921875, -1.0 -0.8756627 -0.117456675, -0.9238795 -0.49359855 -0.0956946, -0.70710677 -0.16970019 -0.07724561, -0.38268343 0.04672177 -0.064918384, -1.8369701E-16 0.12271906 -0.060589638, 0.38268343 0.04672177 -0.064918384, 0.70710677 -0.16970019 -0.07724561, 0.9238795 -0.49359855 -0.0956946, 0.0 -0.98939675 1.8793068, ";
            normal.vector =
                "0.6391195 0.015301731 -0.76895523, 0.5904694 -0.2292303 -0.77382123, 0.45192572 -0.43653455 -0.7779465, 0.24458045 -0.57505083 -0.7807029, -8.903431E-17 -0.62369126 -0.78167075, -0.3229683 -0.768888 -0.55181766, -0.7054874 -0.70400196 -0.081662655, -0.5904694 -0.2292303 -0.77382123, -0.6391195 0.015301731 -0.76895523, -0.5904694 0.25983375 -0.76408917, -0.45192572 0.46713802 -0.7599639, -0.24458045 0.6056543 -0.7572076, -1.05142775E-16 0.6542947 -0.75623965, 0.3229683 0.79023224 -0.5207921, 0.7054874 0.7066935 -0.05359069, 0.5904694 0.25983375 -0.76408917, -9.4556915E-17 -0.01989544 0.99980205, ";
            normal2.vector =
                "0.8944272 -0.025431713 0.4464899, 0.826343 -0.3671603 0.42702532, 0.6324555 -0.6568638 0.410524, 0.34228247 -0.85043746 0.39949822, -1.4095714E-17 -0.9184115 0.3956265, -0.34228247 -0.85043746 0.39949822, -0.6324555 -0.6568638 0.410524, -0.826343 -0.3671603 0.42702532, -0.8944272 -0.025431713 0.4464899, -0.826343 0.31629685 0.46595448, -0.6324555 0.60600036 0.48245576, -0.34228247 0.799574 0.49348158, -2.6781856E-16 0.8675481 0.49735332, 0.34228247 0.799574 0.49348158, 0.6324555 0.60600036 0.48245576, 0.826343 0.31629685 0.46595448, -3.1518973E-17 -0.05686704 0.99838173, ";
            //ifs1.coordinate = coordinate;
            //ifs2.coordinate = coordinate2;
            //ifs1.normal = normal;
            //ifs2.normal = normal2;
            ifs1.Items.Add(coordinate);
            ifs1.Items.Add(normal);
            ifs2.Items.Add(coordinate2);
            ifs2.Items.Add(normal2);
            ifs1.Children.Add(coordinate);
            ifs1.Children.Add(normal);
            ifs2.Children.Add(coordinate2);
            ifs2.Children.Add(normal2);
            ifs1.Parent = Parent;
            ifs2.Parent = Parent;
        }

        #region Rendering Methods

        // Use IndexedFaceSet but dont connect it to the actual scene graph the current Cone is in.
        private IndexedFaceSet ifs1; // with bottom face
        private IndexedFaceSet ifs2; // without bottom face

        public override void CollectGeometry(
            RenderingContext rc,
            out GeometryHandle handle,
            out BoundingBox bbox,
            out bool Coloring,
            out bool Texturing)
        {
            PackedGeometry master;
            PackedGeometry packed1;
            PackedGeometry packed2;
            List<PackedGeometry> packs;

            handle = GeometryHandle.Zero;
            bbox = BoundingBox.Zero;
            Texturing = false;
            Coloring = false;

            packs = new List<PackedGeometry>();

            if (bottom)
            {
                // Cone with bottom face

                packed1 = PackedGeometry.Pack(ifs1);
                packs.Add(packed1);
            }
            else
            {
                // Cone without bottom face

                packed2 = PackedGeometry.Pack(ifs2);
                packs.Add(packed2);
            }

            master = PackedGeometry.InterleavePacks(packs);

            bbox = BoundingBox.CalculateBoundingBox(master);

            // BUFFER GEOMETRY
            handle = Buffering.BufferShaderGeometry(master);
        }

        public override void Load()
        {
            base.Load();

            buildConeGeometry();
        }

        public override void Render(RenderingContext rc)
        {
            base.Render(rc);

            //rc.PushMatricies();

            rc.matricies.Scale =
                new Vector3(2 * bottomRadius, 1f * height,
                    2 * bottomRadius); // Too easy, almost feel like im cheating here.

            //rc.PopMatricies();
        }

        #endregion
    }
}