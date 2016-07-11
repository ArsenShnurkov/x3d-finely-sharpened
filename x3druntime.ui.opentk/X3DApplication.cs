//#define FULLSCREEN
//#define NAVIGATION_MOUSE
#define NAVIGATION_KEYBOARD
//#define ROTATE_SCENE_ANNOYINGLY
//#define GAME_INIT_MODE
//#define VSYNC_ACTIVE

using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics.OpenGL;
using g = OpenTK.Graphics;
using System.Runtime.InteropServices;
using X3D.Engine;
using OpenTK.Input;
using X3D;
using System.Reflection;
using System.Linq;

/* Need OpenTK.Compatibility.dll for GLu */

/* Some important things:
 * --
 * The debug console is a specific feature of the X3D Browser
 * which can be used to display trace/debugging messages 
 * and view validation problems with the input according to the X3D encoding (XML,x3db,x3dv).
 * The code that toggles the display of the console may need to be made platform independent. 
 */

namespace x3druntime.ui.opentk
{
    public partial class X3DApplication
    {

        public string BaseURL { get; set; }
        public string BaseMIME { get; set; }

        private static SceneManager scene;
        private Camera ActiveCamera;
        private bool ispanning, iszooming;
        private float mouseScale = 0.01f;
        private bool mouseDragging = false;

        /// <param name="window">
        /// A window or display which is used to render the X3D application
        /// </param>
        public X3DApplication(INativeWindow window)
        {
            this.window = window;

            //this.window.KeyPress+=new EventHandler<KeyPressEventArgs>(X3DApplication_KeyPress);
            //this.Keyboard.KeyDown+=new EventHandler<OpenTK.Input.KeyboardKeyEventArgs>(Keyboard_KeyDown);
            this.Keyboard.KeyUp += new EventHandler<OpenTK.Input.KeyboardKeyEventArgs>(Keyboard_KeyUp);


            ActiveCamera = new Camera(this.window.Width, this.window.Height);

            this.Mouse.WheelChanged += Mouse_WheelChanged;
            this.window.MouseLeave += Window_MouseLeave;
            this.Mouse.ButtonDown += (object sender, MouseButtonEventArgs e) =>
            {
                if (e.IsPressed && e.Button == MouseButton.Left) {
                    ispanning = true;
                    iszooming = false;
                }
                else if (e.IsPressed && e.Button == MouseButton.Right) {
                    iszooming = true;
                    ispanning = false;
                }
                mouseDragging = true;
            };
            this.Mouse.ButtonUp += (object sender, MouseButtonEventArgs e) =>
            {
                mouseDragging = false;
            };
            this.Mouse.Move += (object sender, MouseMoveEventArgs e) =>
            {
                if (mouseDragging)
                {

                    if (ispanning)
                    {
                        ActiveCamera.PanXY(e.XDelta * mouseScale, e.YDelta * mouseScale);
                    }
                    else if (iszooming)
                    {
                        ActiveCamera.OrbitXY(e.XDelta, e.YDelta);
                    }
                }
            };
        }

        public void ShowSupportMatrix()
        {
            Assembly asm = Assembly.GetAssembly(typeof(X3D.Engine.XMLParser));

            Type[] types = (new List<Type>(asm.GetTypes())).Where(t => t.IsSubclassOf(typeof (SceneGraphNode))).ToArray();

            Console.WriteLine("-- Support Matrix (Informal indicator) --");
            int w = 4, h = types.Length / w;


            string[,] grid = new string[h, w];
            int r = 0, c = 0;

            for(int i = 0; i < types.Length && c < h; i++)
            {
                int j = i;
                Type t = types[i];

                grid[c, r] = t.FullName;

                if (c < h)
                {
                    if (i > 0 && (j + 1) % w == 0)
                    {
                        if (c < h) c++;
                        r = 0;
                    }
                    else
                    {
                        r++;
                    }
                }
                else
                {
                    break;
                }
            }

            for(int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    string s = grid[y, x];

                    if (!string.IsNullOrEmpty(s))
                    {
                        Console.SetCursorPosition(x * 25, y + 3);

                        Console.Write((x > 0 ? " " : "") + "{0}", s);
                    }
                }
            }
            Console.Write("\n");
            Console.WriteLine("-- EO Support Matrix --");
        }

        public void Init(string url, string mime_type)
        {
            Console.WriteLine("LOAD <" + BaseMIME + "> " + BaseURL);
            this.time_at_init = DateTime.Now;
            Console.Title = AppInfo;
            int[] t = new int[2];
            GL.GetInteger(GetPName.MajorVersion, out t[0]);
            GL.GetInteger(GetPName.MinorVersion, out t[1]);
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("OpenGL Version " + t[0] + "." + t[1]);

            ShowSupportMatrix();

#if GAME_INIT_MODE && FULLSCREEN
            window.WindowState=WindowState.Minimized;
            ConsoleVisibility=true;
#endif

            modelview = Matrix4.LookAt(Vector3.UnitX, Vector3.UnitZ, Vector3.UnitY);

            { //TODO: specify code in this code block somewhere somehow in the loading of individual Scenes

                GL.Disable(EnableCap.Normalize);
                //GL.ShadeModel(ShadingModel.Flat);
                //GL.ShadeModel(ShadingModel.Smooth);                // Enable Smooth Shading

                GL.ClearColor(0.0f, 0.0f, 0.0f, 0.5f);           // Black Background
                GL.ClearDepth(1.0f);                 // Depth Buffer Setup
                GL.Enable(EnableCap.DepthTest);                // Enables Depth Testing
                GL.DepthFunc(DepthFunction.Lequal);                 // The Type Of Depth Testing To Do
                //GL.Enable(EnableCap.CullFace); // causes bugs if enabled i.e. nehe10 wont render properly
                GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);  // Really Nice Perspective Calculations

            }

            DateTime time_before = DateTime.Now;

            if (!string.IsNullOrEmpty(url) && !string.IsNullOrEmpty(mime_type))
            {
                SceneManager.BaseURL = BaseURL;
                SceneManager.BaseMIME = SceneManager.GetMIMEType(BaseMIME);

                scene = SceneManager.fromURL(url, mime_type);
            }
            else
            {
                //Console.WriteLine("null init");
            }

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("loading time: " + DateTime.Now.Subtract(time_before).TotalMilliseconds.ToString() + "ms");
            Console.ForegroundColor = ConsoleColor.Red;

#if GAME_INIT_MODE&&FULLSCREEN
            Console.ForegroundColor=ConsoleColor.DarkGreen;
            Console.WriteLine("Sleeping for 5 secs so you can read me");
            Console.ForegroundColor=ConsoleColor.DarkCyan;
            System.Threading.Thread.Sleep(5000);
            window.WindowState=WindowState.Fullscreen;
#elif FULLSCREEN
            window.WindowState=WindowState.Fullscreen;
#endif

        }

        public void Render(FrameEventArgs e)
        {
            _prev = DateTime.Now;
            GL.Disable(EnableCap.Lighting);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.ClearColor(System.Drawing.Color.White);
            GL.Enable(EnableCap.DepthTest);
            GL.DepthMask(true);
            GL.DepthFunc(DepthFunction.Lequal);

            GL.PointSize(6.0f);

            //TODO: replace with better Camera Class
            ActiveCamera.setupGLRenderMatrix();

            if (scene != null && scene.SceneGraph.Loaded)
            {
                RenderingContext rc = new RenderingContext();
                rc.Time = e.Time;
                rc.matricies.modelview = ActiveCamera.cameraViewMatrix;
                rc.matricies.projection = ActiveCamera.projectionMatrix;
                rc.cam = ActiveCamera;
                rc.Keyboard = this.Keyboard;
                
                scene.Draw(rc);
            }
            else
            {
                X3DProgram.Restart();
            }

            if (e != null)
            {
                UpdateTitle(e);
            }

#if VSYNC_ACTIVE
            System.Threading.Thread.Sleep(1);
#else
#endif
        }

        /// <summary>
        /// When the application is Resized
        /// </summary>
        public void Resize()
        {
            //Matrix4 projection;

            GL.Viewport(this.window.ClientRectangle);


            //projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver4, window.Width / (float)window.Height, 1.0f, 500.0f);
            //GL.MatrixMode(MatrixMode.Projection);
            //GL.LoadMatrix(ref projection);


            //GL.MatrixMode(MatrixMode.Projection);
            //GL.LoadIdentity();
            //GL.Ortho(-10.0 - zoom - panX, 10.0 + zoom - panX, -10.0 - zoom + panY, 10.0 + zoom + panY, -50.0, 50.0);

            ActiveCamera.viewportSize(window.Width, window.Height);
        }


        public void FrameUpdated(FrameEventArgs e)
        {
            UserInput_ScanKeyboard(e);
            //fps=GetFps(e.Time);
        }

        #region test orbital control

        private void Mouse_WheelChanged(object sender, MouseWheelEventArgs e)
        {
            //base.OnMouseWheel(e);
            ActiveCamera.Dolly(e.DeltaPrecise * mouseScale * 10);
        }


        private void Window_MouseLeave(object sender, EventArgs e)
        {

            mouseDragging = false;
        }

        #endregion

    }
}
