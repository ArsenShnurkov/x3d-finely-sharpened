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
        //private Camera ActiveCamera;
        private TestCamera ActiveCamera;
        private bool ispanning, iszooming;
        private float mouseScale = 0.01f;
        private bool mouseDragging = false;

        private float dx = 0, dy = 0;

        /// <param name="window">
        /// A window or display which is used to render the X3D application
        /// </param>
        public X3DApplication(INativeWindow window)
        {
            this.window = window;

            //this.window.KeyPress+=new EventHandler<KeyPressEventArgs>(X3DApplication_KeyPress);
            //this.Keyboard.KeyDown+=new EventHandler<OpenTK.Input.KeyboardKeyEventArgs>(Keyboard_KeyDown);
            this.Keyboard.KeyUp += new EventHandler<OpenTK.Input.KeyboardKeyEventArgs>(Keyboard_KeyUp);


            //ActiveCamera = new Camera(this.window.Width, this.window.Height);
            ActiveCamera = new TestCamera(this.window.Width, this.window.Height);

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
                //if (mouseDragging)
                //{

                //    if (ispanning)
                //    {
                //        ActiveCamera.PanXY(e.XDelta * mouseScale, e.YDelta * mouseScale);
                //    }
                //    else if (iszooming)
                //    {
                //        ActiveCamera.OrbitXY(e.XDelta, e.YDelta);
                //    }
                //}

                // TEST new camera implementation:

                //dx = ((float)e.X) - dx;
                //dy = ((float)e.Y) - dy;
                dx = ((float)e.XDelta);
                dy = ((float)e.YDelta);


                LockMouseCursor();


                Vector3 direction = Vector3.Zero;

                if (Math.Abs(dx) > Math.Abs(dy))
                    direction.X = (dx > 0) ? 0.1f : -0.1f;
                else
                    direction.Y = (dy > 0) ? 0.1f : -0.1f;



                float xAngle = (direction.X);
                float yAngle = (direction.Y);

                //float xAngle = dx * 0.0001f;
                //float yAngle = dy * 0.0001f;

                //xAngle = MathHelpers.ClampCircular(xAngle, 0.0f, MathHelpers.TwoPi);
                //yAngle = MathHelpers.ClampCircular(yAngle, 0.0f, MathHelpers.TwoPi);

                //xAngle = MathHelper.ClampCircular(xAngle, 0.0, TwoPi);
                //yAngle = MathHelper.ClampCircular(yAngle, 0.0, HalfPi);

                //    while (xAngle < 0)
                //      xAngle += TwoPi;
                //    while (xAngle >= TwoPi)
                //      xAngle -= TwoPi;
                //
                //    while (yAngle < -HalfPi)
                //      yAngle = -HalfPi;
                //    while (yAngle > HalfPi)
                //      yAngle = HalfPi;


                ActiveCamera.ApplyYaw(xAngle);
                ActiveCamera.ApplyPitch(yAngle);
                ActiveCamera.ApplyRotation();
            };
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

            // INITILISE SCENE

            GL.Disable(EnableCap.Normalize);
            // shade model upgraded from fixed function to shader side
            //GL.ShadeModel(ShadingModel.Flat);
            //GL.ShadeModel(ShadingModel.Smooth);                // Enable Smooth Shading

            GL.ClearColor(0.0f, 0.0f, 0.0f, 0.0f);           // Black Background
            GL.ClearDepth(1.0f);                 // Depth Buffer Setup
            GL.Enable(EnableCap.DepthTest);                // Enables Depth Testing
            //GL.DepthMask(false);
            GL.DepthFunc(DepthFunction.Lequal);                 // The Type Of Depth Testing To Do
            //GL.Enable(EnableCap.CullFace); // causes bugs if enabled i.e. nehe10 wont render properly
            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);  // Really Nice Perspective Calculations

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
            Console.ForegroundColor = ConsoleColor.Yellow;

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

            //TODO: improve current Camera implementation
            
            ActiveCamera.ApplyDollyTransformations();
            //ActiveCamera.ApplyTransformations(); // TEST new camera implementation

            if (scene != null && scene.SceneGraph.Loaded)
            {
                RenderingContext rc = new RenderingContext();
                rc.Time = e.Time;
                rc.matricies.modelview = ActiveCamera.Matrix;
                rc.matricies.projection = ActiveCamera.Projection;
                rc.cam = ActiveCamera;
                rc.Keyboard = this.Keyboard;

                Renderer.Draw(scene.SceneGraph, rc);
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

        private void LockMouseCursor()
        {
            System.Windows.Forms.Cursor.Position = new System.Drawing.Point(window.Bounds.Left + (window.Bounds.Width / 2),
                    window.Bounds.Top + (window.Bounds.Height / 2));
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
