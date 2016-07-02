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

        /// <param name="window">
        /// A window or display which is used to render the X3D application
        /// </param>
        public X3DApplication(INativeWindow window)
        {
            this.window = window;

            //this.window.KeyPress+=new EventHandler<KeyPressEventArgs>(X3DApplication_KeyPress);
            //this.Keyboard.KeyDown+=new EventHandler<OpenTK.Input.KeyboardKeyEventArgs>(Keyboard_KeyDown);
            this.Keyboard.KeyUp += new EventHandler<OpenTK.Input.KeyboardKeyEventArgs>(Keyboard_KeyUp);
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
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Enable(EnableCap.DepthTest);
            GL.DepthMask(true);
            GL.DepthFunc(DepthFunction.Lequal);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref modelview);

#if NAVIGATION_MOUSE
            //TODO: Improve this: think of mouse navigation as a vector pointing from the origin inside a sphere
            GL.Rotate(Mouse.X,Vector3.UnitY);
            GL.Rotate(Mouse.Y,Vector3.UnitX);
#endif
#if NAVIGATION_KEYBOARD
            //This is shit. Improve this
            // player should have 6DOF, and movement should be relative to the player's current location
            GL.Rotate(this.lookupdown, 1.0f, 0.0f, 0.0f);
            GL.Rotate(360.0f - this.yrot, 0.0f, 1.0f, 0.0f);

            //GL.Rotate(0,0.0f,0.0f,lookleftright);
            //GL.Rotate(360.0f-lookleftright,Vector3d.UnitX);
            //GL.Rotate(360.0f-lookleftright,Vector3d.UnitZ);
            //GL.Rotate(360.0f-lookleftright,Vector3d.UnitY); // left|right

            GL.Translate(-this.xpos, -this.walkbias - 0.25f + this.ypos, -this.zpos);
#endif
            if (scene != null && scene.SceneGraph.Loaded)
            {
                GL.PushMatrix();
                scene.Draw(e);
                GL.PopMatrix();
            }
            else
            {
                Console.WriteLine("null scene draw");
            }
#if ROTATE_SCENE_ANNOYINGLY
            if(rotate_enable) {
                rotation+=3.0f;
            }
#endif

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
            Matrix4 projection;

            GL.Viewport(this.window.ClientRectangle);

            //projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver4,Width/(float)Height, 0.00001f, 500.0f);
            projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver4, window.Width / (float)window.Height, 1.0f, 500.0f);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref projection);
        }

        public void FrameUpdated(FrameEventArgs e)
        {
            UserInput_ScanKeyboard(e);
            //fps=GetFps(e.Time);
        }



    }
}
