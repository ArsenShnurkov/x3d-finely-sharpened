using System;
using OpenTK;
using OpenTK.Input;

namespace X3D.Runtime
{
    public partial class X3DApplication
    {
        private readonly INativeWindow window;

        public KeyboardDevice Keyboard => window.InputDriver.Keyboard[0];

        public MouseDevice Mouse => window.InputDriver.Mouse[0];

        public JoystickDevice Joystick => window.InputDriver.Joysticks[0];

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