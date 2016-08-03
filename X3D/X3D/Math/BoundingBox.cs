using OpenTK;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using OpenTK.Graphics.OpenGL4;
using X3D.Core;
using X3D.Core.Shading;
using X3D.Parser;

namespace X3D
{
    using static Box;

    public class BoundingBox
    {
        public float Width, Height, Depth;

        public Vector3 Position = Vector3.Zero;
        public Vector3 Center = Vector3.Zero;

        internal static BoxGeometry _geo;
        private ComposedShader bboxShader;
        private GeometryHandle _handle;
        private PackedGeometry _pack;

        private bool renderingEnabled = false;

        #region Constructors

        public BoundingBox()
        {
            this.Width = 0f;
            this.Height = 0f;
            this.Depth = 0f;
        }

        public BoundingBox(float w, float h, float d)
        {
            this.Width = w;
            this.Height = h;
            this.Depth = d;
        }

        #endregion

        #region Public Methods

        private Matrix4 calculateModelview(SceneGraphNode transform_context, RenderingContext rc)
        {
            Matrix4 MVP;
            Matrix4 model; // applied transformation hierarchy
            Matrix4 cameraRot;

            MVP = Matrix4.Identity;


            List<Transform> transformationHierarchy = transform_context.AscendantByType<Transform>().Select(t => (Transform)t).ToList();
            Matrix4 modelview = Matrix4.Identity * rc.matricies.worldview;
            Vector3 x3dScale = new Vector3(0.06f, 0.06f, 0.06f); 
            
            Quaternion modelrotation = Quaternion.Identity;
            Matrix4 modelLocalRotation = Matrix4.Identity;
            Matrix4 localTranslations = Matrix4.Identity;

            foreach (Transform transform in transformationHierarchy)
            {
                localTranslations *= Matrix4.CreateTranslation(transform.Translation * x3dScale);
                modelview *= localTranslations;
            }

            
            Center = (new Vector3(Width / 2f, Height / 2f, Depth / 2f)) * x3dScale;
            var top = (new Vector3(Width - (Width / 2), Height, Depth)) * x3dScale;
            Position = localTranslations.ExtractTranslation() - top;

            modelview = rc.matricies.worldview * Matrix4.CreateTranslation(Position);

            model = modelview;

            Matrix4 cameraTransl = Matrix4.CreateTranslation(rc.cam.Position);

            Quaternion q = rc.cam.Orientation;

            

            cameraRot = Matrix4.CreateFromQuaternion(q); // cameraRot = MathHelpers.CreateRotation(ref q);


            MVP = ((modelLocalRotation * model) * cameraTransl) * cameraRot // position and orient the Shape relative to the world and camera

                ; // this is the MVP matrix

            return MVP;
        }

        public void Render(SceneGraphNode transform_context, RenderingContext rc)
        {
            if (!renderingEnabled) return;

            var shader = bboxShader;

            Matrix4 MVP;

            MVP = calculateModelview(transform_context, rc);

            GL.UseProgram(shader.ShaderHandle);
            //rc.matricies.Scale = new Vector3(this.Width, this.Height, this.Depth);
            shader.SetFieldValue("size", new Vector3(this.Width , this.Height, this.Depth) * 0.02f);
            shader.SetFieldValue("scale", Vector3.One);
            shader.SetFieldValue("modelview", ref MVP); 
            shader.SetFieldValue("projection", ref rc.matricies.projection);
            shader.SetFieldValue("camscale", 1.0f);
            shader.SetFieldValue("X3DScale", Vector3.One);
            shader.SetFieldValue("coloringEnabled", 1);
            shader.SetFieldValue("texturingEnabled", 0);
            shader.SetFieldValue("lightingEnabled", 0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _handle.vbo3);
            Buffering.ApplyBufferPointers(bboxShader);
            GL.DrawArrays(PrimitiveType.LineLoop, 0, _handle.NumVerticies3);

            PostRender();
        }

        public void PostRender()
        {
            GL.UseProgram(0);
        }

        #endregion

        #region Private Methods

        public void EnableRendering()
        {
            Vector3[] lineColors;
            int[] lineIndicies;
            Vector3 green = new Vector3(0, 1, 0);

            bboxShader = ShaderCompiler.BuildDefaultShader();
            bboxShader.Link();

            lineColors = null;
            lineIndicies = null;

            lines(green, 12, out lineColors, out lineIndicies);

            _geo = new BoxGeometry();
            _pack = new PackedGeometry();
            _pack.Coloring = true;
            _pack._indices = _geo.Indices;
            _pack._coords = _geo.Vertices;
            _pack.color = X3DTypeConverters.Vec3ToFloatArray(lineColors);
            _pack._colorIndicies = lineIndicies;
            _pack.restartIndex = -1;
            _pack.Interleave();
            _handle = _pack.CreateHandle();

            renderingEnabled = _handle.HasGeometry;
        }

        private void lines(Vector3 color, int lines, out Vector3[] colors, out int[] colorIndicies)
        {
            List<int> indicies = new List<int>();
            List<Vector3> cols = new List<Vector3>();
            int numVerticies = lines * 3;

            cols.Add(color);

            for (int i = 0; i < numVerticies * 2; i++)
            {
                if (i > 0 && i % 3 == 0)
                {
                    indicies.Add(-1);
                }

                indicies.Add(0);
            }

            colorIndicies = indicies.ToArray();
            colors = cols.ToArray();
        }

        #endregion

        #region Public Static Methods

        /// <summary>
        /// Calculates the Maximum bounding box size 
        /// </summary>
        public static BoundingBox Max(BoundingBox box1, BoundingBox box2)
        {
            BoundingBox result;

            result = new BoundingBox()
            {
                Width = Math.Max(box1.Width, box2.Width),
                Height = Math.Max(box1.Height, box2.Height),
                Depth = Math.Max(box1.Depth, box2.Depth)
            };

            return result;
        }

        public static BoundingBox CalcBoundingBox(ElevationGrid elevationGrid)
        {
            BoundingBox bbox;
            int xDim, zDim;

            xDim = elevationGrid._xDimension - 1;
            zDim = elevationGrid._zDimension - 1;

            bbox = new BoundingBox();

            bbox.Width = xDim * elevationGrid._xSpacing;
            bbox.Depth = zDim * elevationGrid._zSpacing;
            bbox.Height = elevationGrid.MaxHeight - elevationGrid.MinHeight;

            return bbox;
        }

        /// <summary>
        /// Calculates a bounding box of a String given the specified Font type.
        /// </summary>
        public static BoundingBox CalculateBoundingBox(String text, Font font)
        {
            Bitmap bmp = new Bitmap(1, 1);
            SizeF size;

            using (Graphics g2D = Graphics.FromImage(bmp))
            {
                size = g2D.MeasureString(text, font);
            }

            return new BoundingBox()
            {
                Width = (int)(size.Width),
                Height = (int)(size.Height),
                Depth = 0
            };
        }

        public static BoundingBox CalculateBoundingBox(PackedGeometry pack)
        {
            BoundingBox result;
            List<Vertex> interleaved;

            interleaved = new List<Vertex>();
            interleaved.AddRange(pack.interleaved3);
            interleaved.AddRange(pack.interleaved4);
            
            result = CalculateBoundingBox(interleaved);

            return result;
        }

        public static BoundingBox CalculateBoundingBox(IEnumerable<Vector3> vectors)
        {
            Vector3 max;
            Vector3 min;

            max = Vector3.Zero;
            min = Vector3.Zero;

            foreach (Vector3 vector in vectors)
            {
                if (vector.X > max.X) max.X = vector.X;
                if (vector.Y > max.Y) max.Y = vector.Y;
                if (vector.Z > max.Z) max.Z = vector.Z;

                if (vector.X < min.X) min.X = vector.X;
                if (vector.Y < min.Y) min.Y = vector.Y;
                if (vector.Z < min.Z) min.Z = vector.Z;
            }

            return new BoundingBox()
            {
                Width = max.X,
                Height = max.Y,
                Depth = max.Z
            };
        }

        public static void CalculateBoundingBox(IEnumerable<Vector3> vectors, out Vector3 max, out Vector3 min)
        {
            max = Vector3.Zero;
            min = Vector3.Zero;

            foreach (Vector3 vector in vectors)
            {
                if (vector.X > max.X) max.X = vector.X;
                if (vector.Y > max.Y) max.Y = vector.Y;
                if (vector.Z > max.Z) max.Z = vector.Z;

                if (vector.X < min.X) min.X = vector.X;
                if (vector.Y < min.Y) min.Y = vector.Y;
                if (vector.Z < min.Z) min.Z = vector.Z;
            }
        }

        public static BoundingBox CalculateBoundingBox(IEnumerable<Vertex> vectors)
        {
            Vector3 vector;
            Vector3 max;
            Vector3 min;

            max = Vector3.Zero;
            min = Vector3.Zero;

            foreach (Vertex verticy in vectors)
            {
                vector = verticy.Position;

                if (vector.X > max.X) max.X = vector.X;
                if (vector.Y > max.Y) max.Y = vector.Y;
                if (vector.Z > max.Z) max.Z = vector.Z;

                if (vector.X < min.X) min.X = vector.X;
                if (vector.Y < min.Y) min.Y = vector.Y;
                if (vector.Z < min.Z) min.Z = vector.Z;
            }

            return new BoundingBox()
            {
                Width = max.X,
                Height = max.Y,
                Depth = max.Z
            };
        }

        public static void CalculateBoundingBox(IEnumerable<Vertex> vectors, out Vector3 max, out Vector3 min)
        {
            Vector3 vector;

            max = Vector3.Zero;
            min = Vector3.Zero;
           
            foreach (Vertex verticy in vectors)
            {
                vector = verticy.Position;

                if (vector.X > max.X) max.X = vector.X;
                if (vector.Y > max.Y) max.Y = vector.Y;
                if (vector.Z > max.Z) max.Z = vector.Z;

                if (vector.X < min.X) min.X = vector.X;
                if (vector.Y < min.Y) min.Y = vector.Y;
                if (vector.Z < min.Z) min.Z = vector.Z;
            }
        }

        public static BoundingBox Zero
        {
            get
            {
                return new BoundingBox(0, 0, 0);
            }
        }

        #endregion

    }
}
