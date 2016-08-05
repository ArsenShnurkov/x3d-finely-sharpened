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

    //TODO: bounding volume hierarchy https://en.wikipedia.org/wiki/Bounding_volume_hierarchy

    public class BoundingBox
    {
        public float Width, Height, Depth;

        public Vector3 Maximum;
        public Vector3 Minimum;


        public Vector3 Position = Vector3.Zero;
        public Vector3 Center = Vector3.Zero;
        public Vector3 LineColor = yellow;
        private static Vector3 red = new Vector3(1, 0, 0);
        private static Vector3 green = new Vector3(0, 1, 0);
        private static Vector3 yellow = new Vector3(1, 1, 0);
        private static Vector3 blue = new Vector3(0, 0, 1);
        private static Vector3 cyan = new Vector3(0, 1, 1);
        private static Vector3 orange = new Vector3(1, 0.5f, 0);

        internal static BoxGeometry _geo;
        private ComposedShader bboxShader;
        private GeometryHandle _handle;
        private PackedGeometry _pack;

        private GeometryHandle _handlepb; // for testing of boundaries

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

        /// <summary>
        /// Computes the bounding volume surrounding the specified Group, 
        /// that is the bounding volume of all decendants within the Group.
        /// </summary>
        public static BoundingBox CalculateBoundingVolume(X3DGroupingNode group)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Computes the bounding volume surrounding the specified two shapes
        /// </summary>
        public static BoundingBox CalculateBoundingVolume(BoundingBox left, BoundingBox right)
        {
            throw new NotImplementedException();
        }

        private Matrix4 calculateModelview(
            Shape transform_context, 
            RenderingContext rc
        )
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
            Vector3 transAccum = Vector3.Zero;

            foreach (Transform transform in transformationHierarchy)
            {
                transAccum += transform.Translation;

                //localTranslations *= Matrix4.CreateTranslation(transform.Translation * x3dScale);
                localTranslations = ApplyX3DTransform(Vector3.Zero, 
                                                      Vector3.Zero, 
                                                      Vector3.One, 
                                                      Vector3.Zero, 
                                                      transform.Translation * x3dScale, 
                                                      localTranslations);
                modelview *= localTranslations;
            }

            Vector3 p = Vector3.Zero;
            //Vector3 p = transform_context.GetPosition(rc);
            //Vector3 p2 = localTranslations.ExtractTranslation();
            //Vector3 origin = Vector3.Zero;
            //float dist = Vector3.Dot(origin, p2);
            //var p = p2 - new Vector3(dist);

            Center = (new Vector3(((Width - p.X) / 2f), ( (Height-p.Y) / 2f), ((Depth - p.Z) / 2f) ) * x3dScale);
            var Center2 = (new Vector3(((Width) / 2f), ((Height) / 2f), ((Depth) / 2f)) * x3dScale);
            var top = (new Vector3((Width - p.X), (Height - p.Y) , (Depth - p.Z))) * x3dScale;
            var left = Center + ((new Vector3(0, 0, Depth) * x3dScale));
            //var top = (new Vector3(Width - (Width / 2), Height, Depth)) * x3dScale;

            //localTranslations = Matrix4.Identity;

            Position = (localTranslations.ExtractTranslation());

            //Quaternion qlocal = QuaternionExtensions.EulerToQuat(0, MathHelpers.PI, 0);
            //modelLocalRotation = Matrix4.CreateFromQuaternion(qlocal);

            //modelview = Matrix4.CreateTranslation(Position);

            float distZ = Math.Abs(Maximum.Z) - Math.Abs(Minimum.Z);

            modelview = ApplyX3DTransform(
                Vector3.Zero,
                rc.cam.calibOrient,
                Vector3.One,
                Vector3.Zero,
                new Vector3((((Minimum.X + (Maximum.X/2.0f)) / 2.0f) * x3dScale.X) ,
                ((((Maximum.Y) - (((Maximum.Y - Minimum.Y)/ 2.0f)) + (Maximum.Y / 2.0f)) / 2.0f) * x3dScale.Y) , // y-axis buggy
                ((((Maximum.Z) - (((Maximum.Z - Minimum.Z) / 2.0f)) + (Maximum.Z / 2.0f)) / 2.0f) * x3dScale.Z)), // z-axis buggy
                //(((    (((  ((((Maximum.Z)  )  ) )) / 2.0f  )  ) + Math.Abs(Depth - Maximum.Z)) * x3dScale.Z))), // z-axis buggy
                Matrix4.Identity);

            model = modelview;

            Matrix4 cameraTransl = Matrix4.CreateTranslation(rc.cam.Position);

            
            cameraRot = Matrix4.CreateFromQuaternion(rc.cam.Orientation); // cameraRot = MathHelpers.CreateRotation(ref q);


            MVP = ((model) * cameraTransl) * cameraRot ; 



            //Matrix4 translation = Matrix4.CreateTranslation(left);
            //Matrix4 scale = Matrix4.CreateScale(1.0f, 1.0f, 1.0f);
            //Matrix4 rotationX = Matrix4.CreateRotationX(0.0f); // MathHelpers.PI
            //Matrix4 rotationY = Matrix4.CreateRotationY(0.0f);
            //Matrix4 rotationZ = Matrix4.CreateRotationZ(0.0f);
            //Matrix4 _model = scale * translation * (rotationX * rotationY * rotationZ);
            //MVP = (_model * cameraTransl )* cameraRot;

            return MVP;
        }

        public static Matrix4 ApplyX3DTransform(
            Vector3 centerOffset, 
            Vector3 rotation, 
            Vector3 scale, 
            Vector3 scaleOrientation, 
            Vector3 translation,
            Matrix4? ParentTransform = null
        )
        {
            Matrix4 PDerived;
            Matrix4 R;
            Matrix4 SR;
            Quaternion qR;
            Quaternion qSR;
            Matrix4 S;
            Matrix4 C;
            Matrix4 T;

            if (ParentTransform.HasValue == false) ParentTransform = Matrix4.Identity;

            qR = QuaternionExtensions.QuaternionFromEulerAnglesRad(rotation);
            qSR = QuaternionExtensions.QuaternionFromEulerAnglesRad(scaleOrientation);
            T = Matrix4.CreateTranslation(translation);
            C = Matrix4.CreateTranslation(centerOffset);
            R = Matrix4.CreateFromQuaternion(qR);
            SR = Matrix4.CreateFromQuaternion(qSR);
            S = Matrix4.CreateScale(scale);

            PDerived = T 
                * C 
                * R 
                //* SR 
                * S 
                //* SR.Inverted() 
                * C.Inverted() 
                * ParentTransform.Value.Inverted();

            return PDerived;
        }

        public void Render(Shape transform_context, RenderingContext rc)
        {
            if (!renderingEnabled) return;

            var shader = bboxShader;

            Matrix4 modelview;
            Vector3 x3dScale = new Vector3(0.06f, 0.06f, 0.06f);

            modelview = calculateModelview(transform_context, rc);

            const float bbscale = 0.0329999961f; // (rc.cam.calibTrans.X* 0.1f)

            GL.UseProgram(shader.ShaderHandle);
            //rc.matricies.Scale = new Vector3(this.Width, this.Height, this.Depth);
            shader.SetFieldValue("size", new Vector3(this.Width , this.Height, this.Depth) * bbscale);
            shader.SetFieldValue("scale", Vector3.One);
            shader.SetFieldValue("modelview", ref modelview); 
            shader.SetFieldValue("projection", ref rc.matricies.projection);
            shader.SetFieldValue("camscale", 1.0f);
            shader.SetFieldValue("X3DScale", Vector3.One);
            shader.SetFieldValue("coloringEnabled", 1);
            shader.SetFieldValue("texturingEnabled", 0);
            shader.SetFieldValue("lightingEnabled", 0);

            // BOUNDARY LINES
            GL.BindBuffer(BufferTarget.ArrayBuffer, _handle.vbo3);
            Buffering.ApplyBufferPointers(bboxShader);
            GL.DrawArrays(PrimitiveType.LineLoop, 0, _handle.NumVerticies3);


            // BOUNDARY POINTS
            GL.BindBuffer(BufferTarget.ArrayBuffer, _handle.vbo3);
            Buffering.ApplyBufferPointers(bboxShader);
            GL.DrawArrays(PrimitiveType.Points, 0, _handle.NumVerticies3);

            // MAX-MIN POINTS
            //GL.PointSize(8.0f);
            //GL.BindBuffer(BufferTarget.ArrayBuffer, _handlepb.vbo3);
            //Buffering.ApplyBufferPointers(bboxShader);
            //GL.DrawArrays(PrimitiveType.Points, 0, _handlepb.NumVerticies3);

            PostRender();
        }

        public void InitBoundaryPoints(Vector3 origin)
        {
            int[] colIndicies;
            Vector3[] verticies;
            Vector3[] pointColors;
            int[] indicies;

            boundaries(origin, new Vector3(1, 0, 0), out pointColors, out colIndicies, out indicies, out verticies);

            var _pb = new PackedGeometry();
            _pb.Coloring = true;
            _pb._indices = indicies;
            _pb._coords = verticies;
            _pb.color = X3DTypeConverters.Vec3ToFloatArray(pointColors);
            _pb._colorIndicies = colIndicies;
            _pb.restartIndex = -1;
            _pb.Interleave();
            _handlepb = _pb.CreateHandle();
        }

        public void PostRender()
        {
            GL.UseProgram(0);
        }

        #endregion

        #region Private Methods

        public void EnableRendering(Vector3 origin)
        {
            Vector3[] lineColors;
            int[] lineIndicies;

            bboxShader = ShaderCompiler.BuildDefaultShader();
            bboxShader.Link();

            lineColors = null;
            lineIndicies = null;

            lines(LineColor, 12, out lineColors, out lineIndicies);

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

            InitBoundaryPoints(origin);
        }

        private void boundaries(
            Vector3 origin,
            Vector3 color,  
            out Vector3[] colors, 
            out int[] colorIndicies, 
            out int[] coordIndicies, 
            out Vector3[] verticies)
        {
            List<int> indicies = new List<int>();
            List<int> colIndicies = new List<int>();
            List<Vector3> vectors = new List<Vector3>();
            List<Vector3> cols = new List<Vector3>();

            Vector3 x3dScale = new Vector3(0.06f, 0.06f, 0.06f);

            

            cols.Add(red);
            cols.Add(green);
            cols.Add(orange);
            cols.Add(blue);
            cols.Add(yellow);
            cols.Add(cyan);

            //vectors.Add(new Vector3(Maximum.X, origin.Y, origin.Z) * x3dScale);
            //vectors.Add(new Vector3(origin.X, Maximum.Y, origin.Z) * x3dScale);
            //vectors.Add(new Vector3(origin.X, origin.Y, Maximum.Z) * x3dScale);
            //vectors.Add(new Vector3(Minimum.X, origin.Y, origin.Z) * x3dScale);
            //vectors.Add(new Vector3(origin.X, Minimum.Y, origin.Z) * x3dScale);
            //vectors.Add(new Vector3(origin.X, origin.Y, Minimum.Z) * x3dScale);

            indicies.Add(0);
            indicies.Add(1);
            indicies.Add(2);
            indicies.Add(-1);
            indicies.Add(3);
            indicies.Add(4);
            indicies.Add(5);

            colIndicies.Add(0);
            colIndicies.Add(1);
            colIndicies.Add(2);
            colIndicies.Add(-1);
            colIndicies.Add(3);
            colIndicies.Add(4);
            colIndicies.Add(5);

            Vector3 left = new Vector3(0.0f, 0.0f, (Math.Abs(Minimum.Z) + Math.Abs(Depth))) + origin;

            vectors.Add((left) );
            vectors.Add((left));
            vectors.Add((left) );
            vectors.Add((left) );
            vectors.Add((left) );
            vectors.Add((left));

            verticies = vectors.ToArray();
            colorIndicies = colIndicies.ToArray();
            colors = cols.ToArray();
            coordIndicies = indicies.ToArray();
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
