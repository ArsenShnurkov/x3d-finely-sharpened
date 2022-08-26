using System;
using System.Drawing;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using X3D.ConstructionSet;
using X3D.Engine;
using g = OpenTK.Graphics;
using View = X3D.Engine.View;

/* Some important things:
 * --
 * The debug console is a specific feature of the X3D Browser
 * which can be used to display trace/debugging messages 
 * and view validation problems with the input according to the X3D encoding (XML,x3db,x3dv).
 * The code that toggles the display of the console may need to be made platform independent. 
 */

namespace X3D.Runtime
{
    public partial class X3DApplication : IDisposable
    {
        #region Public Constructors

        /// <param name="window">
        ///     A window or display which is used to render the X3D application
        /// </param>
        public X3DApplication(INativeWindow window, SceneGraph cachedGraph = null)
        {
            this.window = window;
            this._cachedGraph = cachedGraph;

            // Set up a Construction Set for the current instance
            SceneManager.ConstructionSet = X3DConsructionSet.GetConstructionSetProvider();


            //this.window.KeyPress+=new EventHandler<KeyPressEventArgs>(X3DApplication_KeyPress);
            //this.Keyboard.KeyDown+=new EventHandler<OpenTK.Input.KeyboardKeyEventArgs>(Keyboard_KeyDown);
            this.Keyboard.KeyUp += new EventHandler<KeyboardKeyEventArgs>(Keyboard_KeyUp);

            this.window.FocusedChanged += (object sender, EventArgs e) =>
            {
                if (this.window.WindowState == WindowState.Fullscreen)
                {
                    window.WindowState = WindowState.Normal;
                    lockMouseCursor = false;

                    isFullscreen = !isFullscreen;

                    ToggleCursor();
                }
            };

            ActiveCamera = new SceneCamera(this.window.Width, this.window.Height);

            if (NavigationInfo.NavigationType == NavigationType.Examine)
            {
                this.Mouse.WheelChanged += Mouse_WheelChanged;
                this.window.MouseLeave += Window_MouseLeave;
                this.Mouse.ButtonDown += (object sender, MouseButtonEventArgs e) =>
                {
                    if (e.IsPressed && e.Button == MouseButton.Left)
                    {
                        iszooming = true;
                        ispanning = false;
                    }
                    else if (e.IsPressed && e.Button == MouseButton.Right)
                    {
                        ispanning = true;
                        iszooming = false;
                    }

                    mouseDragging = true;
                };
                this.Mouse.ButtonUp += (object sender, MouseButtonEventArgs e) => { mouseDragging = false; };
            }
        }

        #endregion

        #region Public Static Properties

        public static SceneGraph SceneGraph
        {
            get { return scene != null ? scene.SceneGraph : null; }
        }

        #endregion

        #region Public Properties

        public string BaseURL { get; set; }
        public string BaseMIME { get; set; }

        #endregion

        #region Private Fields

        private SceneCamera ActiveCamera;
        private bool ispanning, iszooming;
        private float mouseScale = 0.01f;
        private bool mouseDragging = false;

        private bool? lockMouseCursor = true;

        //private float dx = 0, dy = 0;
        private Vector2 mouseDelta = Vector2.Zero;
        private Crosshair _crosshair;
        private bool showCrosshair = true;
        private bool fastFlySpeed = false;
        private bool slowFlySpeed = false;
        private bool isFullscreen = false;
        private bool showGraphDebugger = false;
        private bool reloading = false;

        private SceneGraph _cachedGraph;

        #endregion

        #region Private Static Fields

        private static SceneManager scene;
        private static Vector4 black = new Vector4(0.0f, 0.0f, 0.0f, 1.0f); // Black
        private static Vector4 white = new Vector4(1.0f, 1.0f, 1.0f, 1.0f); // White
        private static Vector4 ClearColor = black;

        #endregion

        #region Rendering Methods

        public void Reload()
        {
            reloading = true;

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.ClearColor(ClearColor.X, ClearColor.Y, ClearColor.Z, ClearColor.W);

            scene.Dispose();

            Init(this.BaseURL, this.BaseMIME);

            ActiveCamera.Reset();

            reloading = false;
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

            //ShowSupportMatrix();

#if GAME_INIT_MODE && FULLSCREEN
            window.WindowState = WindowState.Minimized;
            ConsoleVisibility = true;
#endif

            // INITILISE SCENE

            GL.Disable(EnableCap.Normalize);
            GL.ClearColor(ClearColor.X, ClearColor.Y, ClearColor.Z, ClearColor.W);
            GL.ClearDepth(1.0f); // Depth Buffer Setup
            GL.Enable(EnableCap.DepthTest); // Enables Depth Testing
            GL.DepthFunc(DepthFunction.Lequal); // The Type Of Depth Testing To Do
            //GL.Enable(EnableCap.CullFace); // causes bugs if enabled i.e. nehe10 wont render properly
            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest); // Really Nice Perspective Calculations

            DateTime time_before = DateTime.Now;

            if (!string.IsNullOrEmpty(url) && !string.IsNullOrEmpty(mime_type))
            {
                View view = View.CreateViewFromWindow(this.window);

                Viewpoint.ViewpointList.Clear();

                SceneManager.BaseURL = BaseURL;
                SceneManager.BaseMIME = SceneManager.GetMIMEType(BaseMIME);

                if (_cachedGraph != null)
                {
                    scene = SceneManager.fromSceneGraph(_cachedGraph);
                }
                else
                {
                    scene = SceneManager.fromURL(url, mime_type);
                }

                X3DGraphDebugger.UpdateSceneGraph(scene.SceneGraph);

                Viewpoint.Initilize(ActiveCamera, view);

                if (showCrosshair)
                {
                    //Set crosshair
                    _crosshair = new Crosshair();
                    _crosshair.Load();
                }
            }

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("loading time: " + DateTime.Now.Subtract(time_before).TotalMilliseconds.ToString() +
                              "ms");
            Console.ForegroundColor = ConsoleColor.Yellow;

#if GAME_INIT_MODE&&FULLSCREEN
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine("Sleeping for 5 secs so you can read me");
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            System.Threading.Thread.Sleep(5000);
            window.WindowState = WindowState.Fullscreen;
#elif FULLSCREEN
            window.WindowState = WindowState.Fullscreen;
#endif
        }

        public void Render(FrameEventArgs e)
        {
            if (reloading) return;

            _prev = DateTime.Now;
            GL.Disable(EnableCap.Lighting);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.ClearColor(ClearColor.X, ClearColor.Y, ClearColor.Z, ClearColor.W);
            GL.Enable(EnableCap.DepthTest);
            GL.DepthMask(true);
            GL.DepthFunc(DepthFunction.Lequal);
            GL.PointSize(6.0f);
            //GL.Enable(EnableCap.Blend);

            ActiveCamera.ApplyTransformations();


            if (scene != null && scene.SceneGraph.Loaded)
            {
                RenderingContext rc = new RenderingContext();
                rc.View = View.CreateViewFromWindow(this.window);
                rc.Time = e.Time;
                rc.matricies.worldview = Matrix4.Identity;
                rc.matricies.projection = ActiveCamera.Projection;

                rc.matricies.modelview = Matrix4.Identity;
                rc.matricies.orientation = Quaternion.Identity;
                rc.cam = ActiveCamera;
                rc.Keyboard = this.Keyboard;

                // Apply the current Viewpoint
                Viewpoint.Apply(rc, Viewpoint.CurrentViewpoint);


                Engine.Runtime.Draw(scene.SceneGraph, rc);

                if (showCrosshair && NavigationInfo.NavigationType != NavigationType.Examine)
                {
                    rc.PushMatricies();
                    this._crosshair.Render(rc);
                    rc.PopMatricies();
                }
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
        ///     When the application is Resized
        /// </summary>
        public void Resize()
        {
            if (Viewpoint.CurrentViewpoint == null)
            {
                // No viewpoint assigned in X3D

                ActiveCamera.ApplyViewport(window.Width, window.Height);
            }
            else
            {
                ActiveCamera.ApplyViewportProjection(Viewpoint.CurrentViewpoint,
                    View.CreateViewFromWindow(this.window));
            }
        }

        public void FrameUpdated(FrameEventArgs e)
        {
            //if (reloading) return;

            ApplyKeyBindings(e);

            TrackMouseCursor(ref mouseDelta, this.window, this.updateCamera, this.LockMouseCursor);
        }

        #endregion

        #region Public Methods

        public static void TrackMouseCursor(ref Vector2 mouseDelta, INativeWindow window, Action updateCamera,
            Action lockMouseCursor)
        {
            Rectangle screen = Screen.PrimaryScreen.WorkingArea;

            if (NavigationInfo.NavigationType != NavigationType.Examine)
            {
                if (window.WindowState == WindowState.Fullscreen)
                {
                    mouseDelta = new Vector2
                    (
                        Cursor.Position.X - (screen.Width / 2.0f),
                        Cursor.Position.Y - (screen.Height / 2.0f)
                    );

                    mouseDelta *= 0.005f;
                }
                else
                {
                    //mouseDelta = new Vector2
                    //(
                    //    window.Bounds.Left + (System.Windows.Forms.Cursor.Position.X - (window.Width / 2.0f) ),
                    //    window.Bounds.Top + (System.Windows.Forms.Cursor.Position.Y - (window.Height / 2.0f))
                    //);
                }

                updateCamera();

                lockMouseCursor();
            }
            else
            {
                if (window.WindowState == WindowState.Fullscreen)
                {
                    mouseDelta = new Vector2
                    (
                        Cursor.Position.X - (screen.Width / 2.0f),
                        Cursor.Position.Y - (screen.Height / 2.0f)
                    );

                    mouseDelta *= 0.005f;

                    updateCamera();

                    lockMouseCursor();
                }
                else
                {
                    mouseDelta = new Vector2
                    (
                        Cursor.Position.X - screen.Width,
                        Cursor.Position.Y - screen.Height
                    );

                    mouseDelta *= 0.0005f;

                    updateCamera();
                }
            }
        }

        public void Dispose()
        {
            // Perform a shutdown of the scene and its resources
            if (scene != null)
            {
                if (scene.ScriptingEngine != null)
                {
                    // Cleanup loaded scripts from scripting engine 
                    scene.ScriptingEngine.Dispose();
                }

                scene.Dispose();
            }
        }

        #endregion

        #region Private Methods

        private void updateCamera()
        {
            if (NavigationInfo.NavigationType == NavigationType.Examine)
            {
                // MOUSE ORBIT/PAN NAVIGATION
                if (mouseDragging)
                {
                    if (ispanning)
                    {
                        ActiveCamera.PanXY(mouseDelta.X * mouseScale, mouseDelta.Y * mouseScale);

                        //ActiveCamera.ScaleXY(mouseDelta.X, mouseDelta.Y);
                    }
                    else if (iszooming)
                    {
                        // orbits using shape's centerOfRotation
                        ActiveCamera.OrbitObjectsXY(mouseDelta.X / 0.5f, -1 * mouseDelta.Y / 0.5f);
                    }
                }
            }

            if (NavigationInfo.NavigationType == NavigationType.Fly ||
                NavigationInfo.NavigationType == NavigationType.Walk)
            {
                // TEST new camera walk/fly implementation:

                Vector3 direction = Vector3.Zero;

                //if (Math.Abs(mouseDelta.X) > Math.Abs(mouseDelta.Y))
                //    direction.X = (dx > 0) ? 0.1f : -0.1f;
                //else
                //    direction.Y = (dy > 0) ? 0.1f : -0.1f;

                direction = new Vector3(mouseDelta);

                float xAngle = (direction.X);
                float yAngle = (direction.Y);

                ActiveCamera.ApplyYaw(xAngle);
                ActiveCamera.ApplyPitch(yAngle);
                ActiveCamera.ApplyRotation();
            }
        }

        private void LockMouseCursor()
        {
            if (NavigationInfo.NavigationType != NavigationType.Examine && lockMouseCursor.HasValue == false)
            {
                var result = MessageBox.Show(
                    "Do you want to allow this application to lock the mouse cursor?\n (Note if you allow the lock, you can quit the application by pressing 'q')",
                    "Lock Mouse Cursor",
                    MessageBoxButtons.YesNo);

                lockMouseCursor = (result == DialogResult.Yes);
            }

            if (lockMouseCursor.HasValue && lockMouseCursor.Value == true)
            {
                //        System.Windows.Forms.Cursor.Position = new System.Drawing.Point(window.Bounds.Left + (window.Bounds.Width / 2),
                //window.Bounds.Top + (window.Bounds.Height / 2));


                if (this.isFullscreen)
                {
                    Rectangle screen = Screen.PrimaryScreen.WorkingArea;

                    Cursor.Position = new Point(screen.Width / 2,
                        screen.Height / 2);
                }
                else
                {
                    //System.Windows.Forms.Cursor.Position = new Point(
                    //    window.Bounds.Left + window.Width / 2,
                    //    window.Bounds.Top + window.Height / 2
                    //    );
                }

                //if (this.window.WindowState == WindowState.Fullscreen)
                //{
                //    SetCursorPos((screen.Width / 2) + (int)dx, (screen.Height / 2) + (int)dy);

                //    //System.Windows.Forms.Cursor.Position = new Point(
                //    //    window.Width / 2,
                //    //     window.Height / 2
                //    //);
                //}
                //else
                //{
                //    System.Windows.Forms.Cursor.Position = new Point(
                //        this.window.Bounds.Left + window.Width / 2,
                //        this.window.Bounds.Top + window.Height / 2
                //    );
                //}
            }
        }

        private void Mouse_WheelChanged(object sender, MouseWheelEventArgs e)
        {
            ActiveCamera.Dolly(e.DeltaPrecise * mouseScale * 10);
        }


        private void Window_MouseLeave(object sender, EventArgs e)
        {
            mouseDragging = false;
        }

        #endregion
    }
}