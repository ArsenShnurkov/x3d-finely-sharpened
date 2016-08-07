using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Graphics.OpenGL;
using OpenTK;
using System.Drawing;
using OpenTK.Graphics;

namespace GraphDebugger.OpenGL
{
    public class GfxGraph : RenderableVertexBufferObjects
    {
        
        #region Variables

        public Color NodeForeColor { get; set; }
        public Color NodeBackColor { get; set; }
        public Color EdgeColor { get; set; }

        public bool RootVisible { get; set; }
        public Vector3 Position = new Vector3(0.0f, 0.0f, 1.0f);
        public Vector3 Scale = new Vector3(0.7f, 0.7f, 0.7f);
        public MatrixCollection gfx_model;

        private List<GfxTextNode2D> textObjects;
        public Tree t;
        public List<Vector3> edges = new List<Vector3>();

        //private int vbo_position,
        //            vbo_color;
        //private int attribute_vcol,
        //            attribute_vpos;

        float edge_length = 1.0f; // 0.3
        float branch_radius = 1.0f;

        TraversalType render_type = TraversalType.DepthFirst;
        TraversalType layout_type = TraversalType.DepthFirst;

        #endregion

        public GfxGraph() 
        {
            RootVisible = true;
        }

        public List<TNode> ToList()
        {
            var lst = new List<TNode>();
            t.TraversePreorder((TNode node) =>
            {
                lst.Add(node);
            }, TraversalType.DepthFirst);
            return lst;
        }

        public void EnableVertexAttribArrays()
        {
            //GL.EnableVertexAttribArray(attribute_vpos);
            //.GL.EnableVertexAttribArray(attribute_vcol);
        }

        public void DisableVertexAttribArrays()
        {
            //GL.DisableVertexAttribArray(attribute_vpos);
            //GL.DisableVertexAttribArray(attribute_vcol);
        }

        public void Init(ShaderProgram shader)
        {
            Console.WriteLine("Initilising graph");
            gfx_model = MatrixCollection.CreateInitialMatricies();
            textObjects = new List<GfxTextNode2D>();

            //attribute_vpos = shader.GetAttribLocation("vPosition");
            //attribute_vcol = shader.GetAttribLocation("vColor");

            //GL.GenBuffers(1, out vbo_position);
            //GL.GenBuffers(1, out vbo_color);

            //GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_position);
            //GL.BufferData<Vector3>(BufferTarget.ArrayBuffer, (IntPtr)(vertdata.Length * Vector3.SizeInBytes), vertdata, BufferUsageHint.StaticDraw);
            //GL.VertexAttribPointer(attribute_vpos, 3, VertexAttribPointerType.Float, false, 0, 0);

            //GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_color);
            //GL.BufferData<Vector3>(BufferTarget.ArrayBuffer, (IntPtr)(coldata.Length * Vector3.SizeInBytes), coldata, BufferUsageHint.StaticDraw);
            //GL.VertexAttribPointer(attribute_vcol, 3, VertexAttribPointerType.Float, true, 0, 0);

            //// Validate that the buffer is the correct size
            //int bufferSize;
            //GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out bufferSize);
            //if (coldata.Length * Vector3.SizeInBytes != bufferSize)
            //    throw new ApplicationException("Vertex array not uploaded correctly");

            //GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        public void Load(Tree t, Vector3 rootPosition, Tree.VisitNodeFunct setupNode)
        {
            Vector2 radius;

            t.Root.Point = rootPosition;

            edges = new List<Vector3>();
            radius = new Vector2(branch_radius, branch_radius);

            t.LayoutRadial2D(radius, edge_length, layout_type, false);
            t.TraversePreorder((TNode node) =>
            {
                if (node.Parent != null)
                {
                    edges.Add(node.Point);
                    edges.Add(node.Parent.Point);
                }

                if (!(node.Parent == null &&  RootVisible == false))
                    setupNode(node);

            }, render_type);
            this.t = t;
        }

        public void Render()
        {
            //EnableVertexAttribArrays();
            //GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_position);
            //GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_color);
            //GL.DrawArrays(BeginMode.Triangles, 0, 3);
            //DisableVertexAttribArrays();

            GL.PushMatrix();

            gfx_model.ApplyLocalTransformations(this.Position, this.Scale);
            gfx_model.SetMatrixUniforms();

            
            foreach (var label in textObjects)
                label.Render();

            GL.PopMatrix();


            GL.PushMatrix();
            gfx_model.ApplyLocalTransformations(this.Position, this.Scale);
            gfx_model.SetMatrixUniforms();
            
            Draw.Lines(edges, EdgeColor);

            GL.PopMatrix();
        }

        public void AddTextNode(float x, float y, string @string, Font font, Color forecolor, Color backcolor)
        {
            var tx = new GfxTextNode2D();
            tx.gfx_model.Parent = gfx_model;
            tx.ForeColor = forecolor;
            tx.BackColor = backcolor;
            tx.Font = font;
            tx.Text = @string;
            tx.Position = new Vector3(x, y, 0.0f);
            textObjects.Add(tx);
        }

        //private Vector3[] vertdata = new Vector3[] { 
        //    new Vector3(-0.8f, -0.8f, 0f),
        //    new Vector3( 0.8f, -0.8f, 0f),
        //    new Vector3( 0f,  0.8f, 0f)
        //};
        //private Vector3[] coldata = new Vector3[] { 
        //    new Vector3(1f, 0f, 0f),
        //    new Vector3( 0f, 0f, 1f),
        //    new Vector3( 0f,  1f, 0f)
        //};
    }
}
