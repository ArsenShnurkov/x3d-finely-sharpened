//#define FORCE_HIGH_FPS

//TODO: going to run into problems with shader later, need to convert code from fixed function pipeline

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.ComponentModel;
using System.Threading;
using OpenTK.Input;
using System.Drawing;
using OpenTK.Graphics;
using X3D;
using X3D.Engine;

namespace GraphDebugger.OpenGL
{
    public partial class GraphView : GameWindow
    {
        #region Public Static Methods

        public static GraphView CreateView(SceneGraph graph, out AutoResetEvent closure, out BackgroundWorker worker)
        {
            BackgroundWorker bw; // Have to use the BackgroundWorker to stop COM Interop flop x_x
            AutoResetEvent closureEvent;
            GraphView view = null;
            
            closureEvent = new AutoResetEvent(false);
            bw = new BackgroundWorker();

            bw.WorkerReportsProgress = true;
            bw.WorkerSupportsCancellation = true;
            bw.DoWork += new DoWorkEventHandler((object sender, DoWorkEventArgs e) =>
            {
                view = new GraphView();
                view.Title = "Debugging Scene Graph";
                view.Graph = graph;
#if FORCE_HIGH_FPS
                view.Run();
#else
                view.Run(60);
#endif
            });

            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler((object sender, RunWorkerCompletedEventArgs e) =>
            {
                closureEvent.Set();
            });

            bw.RunWorkerAsync();


            closure = closureEvent;
            worker = bw;

            return view;
        }

        #endregion

        #region Public Properties

        public SceneGraph Graph { get; set; }

        #endregion

        #region Private fields

        private const int RES_W = 800;
        private const int RES_H = 600;

        private MatrixCollection gfx_model;
        private ShaderProgram shader;

        private Vector3 cam_position = new Vector3(0, 0, -1.0f);
        private float zpos1 = -3f;
        private float zpos2 = -4f;
        private List<TNode> all_nodes;
        private List<Vector3> relationship_lines;

        private GfxGraph _graph;
        private GfxTextNode2D lblTitle;

        private static string @base = AppDomain.CurrentDomain.BaseDirectory + "..\\..\\Shaders\\";

        #endregion

        #region Constructors

        public GraphView(): base(RES_W, RES_H, new OpenTK.Graphics.GraphicsMode(32, 16, 0, 4))
        {
#if FORCE_HIGH_FPS
            this.VSync = VSyncMode.Off;
#else
            this.VSync = VSyncMode.On;
#endif
        }

        #endregion

        #region Public Methods

        public void RebuildLayout()
        {
            //todo: implement progressive rendering of the data in the provider

            Tree t;

            // PASS 1
            t = Tree.CreateTreeFromSceneGraph(Graph);

            all_nodes = new List<TNode>();

            // PASS 2
            ProgressiveLoadTree(t);

            // PASS 3
            //all_nodes = t.ToList();

            //relationship_lines = new List<Vector3>();

            //foreach (TNode parent in all_nodes)
            //{
            //    foreach (TNode child in parent.Children)
            //    {
            //        relationship_lines.Add(parent.Point);
            //        relationship_lines.Add(child.Point);
            //    }
            //}
        }

        public void ProgressiveLoadTree(Tree t)
        {
            DateTime before = DateTime.Now;
            Console.WriteLine("loading tree {0} nodes...", t.Count);

            Vector3 root_pos = Vector3.Zero;
            _graph = new GfxGraph();
            _graph.NodeForeColor = System.Drawing.Color.Yellow;
            _graph.NodeBackColor = System.Drawing.Color.Transparent;
            _graph.EdgeColor = System.Drawing.Color.Green;
            _graph.RootVisible = true;
            _graph.Init(shader);
            _graph.gfx_model.Parent = gfx_model;

            _graph.Load(t, root_pos, (TNode node) =>
            {
                // DRAW
                Vector3 p = node.Point;

                all_nodes.Add(node);

                if (node.Data == null) return;

                SceneGraphNode sgn = (SceneGraphNode)node.Data;

                float ratio = 1.0f - ((float)sgn.Children.Count / (float)t.Count);

                string @string;
                Font font;
                System.Drawing.Color forecolor;

                @string = sgn.ToString().Replace("X3D.","");

                if(sgn.Parent == null)
                {
                    font = new Font("Times New Roman", 40.0f);
                    forecolor = System.Drawing.Color.White;
                }
                else
                {
                    int font_size = 20 + (int)(22f * ratio);
                    font = new Font("Times New Roman", font_size);
                    forecolor = _graph.NodeForeColor;
                }

                _graph.AddTextNode((float)(p.X), (float)(p.Y), @string, font, forecolor, _graph.NodeBackColor);
            });

            var dt = DateTime.Now.Subtract(before);
            Console.WriteLine("took " + dt.Seconds.ToString() + "." + (dt.Milliseconds).ToString() + " seconds ");

            _graph.Position.Z = zpos2;

            //lblTitle = new GfxTextNode2D();
            //lblTitle.ForeColor = System.Drawing.Color.White;
            //lblTitle.BackColor = System.Drawing.Color.Transparent;
            //lblTitle.Font = new Font("Times New Roman", 40.0f);
            //lblTitle.Text = string.Format("{0}", "X3D");
            //lblTitle.Position = root_pos; //Vector3.xyz(-0.9, 0.82, zpos1); 
            //lblTitle.Position.Z = zpos1;
            //lblTitle.gfx_model.Parent = gfx_model;

            zpos2 -= 10;
            zpos1 -= 10;
        }

        #endregion

        #region Rendering Methods

        protected override void OnLoad(EventArgs e)
        {
            // SCENE
            string info = "[Vendor] "+GL.GetString(StringName.Vendor) + " "
                + "[Version] "+GL.GetString(StringName.Version) + "\n"
                + "[Renderer] "+GL.GetString(StringName.Renderer) + "\n"
                + "[ShadingLanguageVersion] " + GL.GetString(StringName.ShadingLanguageVersion) + "\n"
                + "[Extensions] " + GL.GetString(StringName.Extensions)
                ;
            
            Console.WriteLine(info);

            GL.ClearDepth(1.0f);                 // Depth Buffer Setup
            GL.Enable(EnableCap.DepthTest);      // Enables Depth Testing
            GL.ClearColor(OpenTK.Graphics.Color4.Black);
            GL.PointSize(5f);

            /* load shaders */
            var shaders = new List<Shader>();
            shaders.Add(new VertexShader { FileName = System.IO.Path.GetFullPath(@base + "SceneGraphVertex.shader") });
            shaders.Add(new FragmentShader { FileName = System.IO.Path.GetFullPath(@base + "SceneGraphFragment.shader") });
            shader = ShaderProgram.CreateLoadAndLinkProgram(shaders);

            /* bind shader variables, and bind buffers */
            gfx_model = MatrixCollection.CreateInitialMatricies(window: this);
            gfx_model.GetMatrixUniforms(shader);

            RebuildLayout();

            base.OnLoad(e);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Viewport(0, 0, this.Width, this.Height);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Enable(EnableCap.DepthTest);
            GL.DepthMask(true);
            GL.DepthFunc(DepthFunction.Lequal);

            gfx_model.ApplyLocalTransformations(cam_position, Vector3.One);
            gfx_model.SetMatrixUniforms();

            //txt.Render();
            //shader.Use();


            //lblTitle.Render();

            _graph.Render();


            //GL.PushMatrix();
            //Draw.Lines(relationship_lines, System.Drawing.Color.Red);
            //GL.PopMatrix();


            //shader.UseZero();

            GL.Flush();
            
            SwapBuffers();

            base.OnRenderFrame(e);
        }

        protected override void OnResize(EventArgs e)
        {            
            GL.Viewport(this.ClientRectangle);

            gfx_model.UpdateProjection(window: this);

            base.OnResize(e);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            bind_keys();
        }

        #endregion

        #region Private Methods

        private void bind_keys()
        {
            float real_slow = 0.11f;
            float slow = 0.07f;
            float fast = 0.5f;

            if(Keyboard[Key.W])
            {
                cam_position.Y -= slow;
            }
            if (Keyboard[Key.S])
            {
                cam_position.Y += slow;
            }
            if (Keyboard[Key.A])
            {
                cam_position.X += slow;
            }
            if (Keyboard[Key.D])
            {
                cam_position.X -= slow;
            }
            if (Keyboard[Key.R])
            {
                cam_position.Z -= slow;
            }
            if (Keyboard[Key.F])
            {
                cam_position.Z += slow;
            }

            if (Keyboard[Key.Up])
            {
                cam_position.Y -= fast;
            }
            if (Keyboard[Key.Down])
            {
                cam_position.Y += fast;
            }
            if (Keyboard[Key.Left])
            {
                cam_position.X += slow * 4;
            }
            if (Keyboard[Key.Right])
            {
                cam_position.X -= slow * 4;
            }
            if (Keyboard[Key.T])
            {
                cam_position.Z -= fast;
            }
            if (Keyboard[Key.G])
            {
                cam_position.Z += fast;
            }
            if (Keyboard[Key.Y])
            {
                cam_position.Z -= real_slow;
            }
            if (Keyboard[Key.H])
            {
                cam_position.Z += real_slow;
            }
        }

        #endregion
    }
}