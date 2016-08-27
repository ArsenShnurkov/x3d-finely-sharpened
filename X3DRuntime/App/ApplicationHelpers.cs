using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace X3D.Runtime
{
    public partial class X3DApplication
    {

        private INativeWindow window;

        public OpenTK.Input.KeyboardDevice Keyboard
        {
            get
            {
                return window.InputDriver.Keyboard[0];
            }
        }

        public OpenTK.Input.MouseDevice Mouse
        {
            get
            {
                return window.InputDriver.Mouse[0];
            }
        }

        public OpenTK.Input.JoystickDevice Joystick
        {
            get
            {
                return window.InputDriver.Joysticks[0];
            }
        }

        private void Renderer_RenderingNotificationEventHandler(string message, ConsoleColor textColor)
        {
            //ConsoleColorApp.SetScreenColors(textColor,Color.Black);
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = textColor;

            Console.BufferWidth = 256;
            Console.BufferHeight = 4096;
            Console.WriteLine(message);
        }
    }
}
