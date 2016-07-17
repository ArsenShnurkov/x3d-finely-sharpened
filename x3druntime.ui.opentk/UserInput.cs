using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics.OpenGL;

namespace x3druntime.ui.opentk
{

    public partial class X3DApplication
    {

        private void Keyboard_KeyUp(object sender, OpenTK.Input.KeyboardKeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.F4: // halt rotate
                    rotate_enable = !rotate_enable;
                    break;
                case Key.F3: // halt special effects
                    fx_enable = !fx_enable;
                    break;
                case Key.F5:
                    //if(dmIndex-1>=0) {
                    //    this.dmIndex--;
                    //    currentDrawingMethod=drawingMethods[dmIndex];
                    //}
                    //else {
                    //    dmIndex=drawingMethods.Length-1;
                    //    currentDrawingMethod=drawingMethods[dmIndex];
                    //}
                    break;
                case Key.F6:
                    //if(dmIndex+1<drawingMethods.Length-1) {
                    //    this.dmIndex++;
                    //    currentDrawingMethod=drawingMethods[dmIndex];
                    //}
                    //else {
                    //    dmIndex=0;
                    //    currentDrawingMethod=drawingMethods[dmIndex];
                    //}
                    break;
                case Key.F1:
                    wireframe = !wireframe;
                    if (wireframe)
                    {
                        GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
                    }
                    else
                    {
                        if (points_only)
                        {
                            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Point);
                        }
                        else
                        {
                            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
                        }
                    }
                    break;
                case Key.F2:
                    points_only = !points_only;
                    if (points_only)
                    {
                        GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Point);
                    }
                    else
                    {
                        if (wireframe)
                        {
                            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
                        }
                        else
                        {
                            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
                        }
                    }
                    break;
                case Key.F:
                    if (window.WindowState == OpenTK.WindowState.Normal)
                    {
                        window.WindowState = WindowState.Fullscreen;
                    }
                    else
                    {
                        window.WindowState = WindowState.Normal;
                    }
                    break;
            }
        }
        
        private void UserInput_ScanKeyboard(FrameEventArgs e)
        {
            Vector3 direction = Vector3.Zero;

            if (Keyboard[Key.W])
            {
                ActiveCamera.Walk(playerDirectionMagnitude);
                //direction += ActiveCamera.Direction * playerDirectionMagnitude;
            }
            if (Keyboard[Key.S])
            {
                ActiveCamera.Walk(-playerDirectionMagnitude);
                //direction -= ActiveCamera.Direction * playerDirectionMagnitude;
            }
            if (Keyboard[Key.A])
            {
                ActiveCamera.Strafe(playerDirectionMagnitude);
                //ActiveCamera.Right = ActiveCamera.Up.Cross(ActiveCamera.Direction);
                //direction += ActiveCamera.Right * playerDirectionMagnitude;
            }
            if (Keyboard[Key.D])
            {
                ActiveCamera.Strafe(-playerDirectionMagnitude);
                //ActiveCamera.Right = ActiveCamera.Up.Cross(ActiveCamera.Direction);
                //direction -= ActiveCamera.Right * playerDirectionMagnitude;
            }

            if (Keyboard[Key.Escape])
            {
                //Exit();
                System.Windows.Forms.Application.Exit();
            }
            if (Keyboard[Key.PageUp])
            {// On page up, move out
                this.z -= 0.02f;
                this.lookupdown -= 1.0f;
            }
            if (Keyboard[Key.PageDown])
            {// On page down, move in
                this.z += 0.02f;
                this.lookupdown += 1.0f;
            }
            if (Keyboard[Key.W])
            {
                this.xpos += (float)Math.Sin(this.heading * Math.PI / 180.0) * 1.05f;
                this.zpos += (float)Math.Cos(this.heading * Math.PI / 180.0) * 1.05f;
                if (this.walkbiasangle >= 359.0f)
                    this.walkbiasangle = 0.0f;
                else
                    this.walkbiasangle -= 10.0f;
                this.walkbias = (float)Math.Sin(this.walkbiasangle * Math.PI / 180.0) / 20.0f;
            }
            if (Keyboard[Key.S])
            {
                this.xpos -= (float)Math.Sin(this.heading * Math.PI / 180.0) * 1.05f;
                this.zpos -= (float)Math.Cos(this.heading * Math.PI / 180.0) * 1.05f;
                if (this.walkbiasangle >= 359.0f)
                    this.walkbiasangle = 0.0f;
                else
                    this.walkbiasangle += 10.0f;
                this.walkbias = (float)Math.Sin(this.walkbiasangle * Math.PI / 180.0) / 20.0f;
            }
            if (Keyboard[Key.A])
            {
                this.heading += 10.0f;
                this.yrot = this.heading;
            }
            if (Keyboard[Key.D])
            {
                this.heading -= 10.0f;
                this.yrot = this.heading;
            }
            if (Keyboard[Key.Left])
            {
                ActiveCamera.ApplyYaw(-10.0f * 0.007f);
                ActiveCamera.ApplyRotation();

                //this.heading += 1.0f;
                //this.yrot = this.heading;
            }
            if (Keyboard[Key.Right])
            {
                ActiveCamera.ApplyYaw(10.0f * 0.007f);
                ActiveCamera.ApplyRotation();
                //this.heading -= 1.0f;
                //this.yrot = this.heading;
            }
            if (Keyboard[Key.Up])
            {
                this.xpos += (float)Math.Sin(this.heading * Math.PI / 180.0) * 0.05f;
                this.zpos += (float)Math.Cos(this.heading * Math.PI / 180.0) * 0.05f;
                if (this.walkbiasangle >= 359.0f)
                    this.walkbiasangle = 0.0f;
                else
                    this.walkbiasangle -= 5.0f;
                this.walkbias = (float)Math.Sin(this.walkbiasangle * Math.PI / 180.0) / 10.0f;
            }
            if (Keyboard[Key.Down])
            {
                this.xpos -= (float)Math.Sin(this.heading * Math.PI / 180.0) * 0.05f;
                this.zpos -= (float)Math.Cos(this.heading * Math.PI / 180.0) * 0.05f;
                if (this.walkbiasangle >= 359.0f)
                    this.walkbiasangle = 0.0f;
                else
                    this.walkbiasangle += 10.0f;
                this.walkbias = (float)Math.Sin(this.walkbiasangle * Math.PI / 180.0) / 10.0f;
            }
            if (Keyboard[Key.C])
            {

                this.lookleftright -= 1.0f;
                //this.lookupdown-=1.0f;
            }
            if (Keyboard[Key.V])
            {

                this.lookleftright += 1.0f;
                //this.lookupdown+=1.0f;
            }
            if (Keyboard[Key.E])
            {

            }
            if (Keyboard[Key.F])
            {

            }
            if (Keyboard[Key.R])
            {

            }
            if (Keyboard[Key.Space])
            {

            }
            if (Keyboard[Key.T])
            {
                //this.tmr_enabled=!this.tmr_enabled;
                //if(this.tmr_enabled) {
                //    timer_init();
                //}
                //else {
                //    tmrDMCycle.Dispose();
                //    tmrDMCycle=null;
                //}
            }
            if (Keyboard[Key.I])
            {
                this.ypos += 0.2f;
            }
            if (Keyboard[Key.K])
            {
                this.ypos -= 0.2f;
            }

            ActiveCamera.move(direction, e.Time);
        }
    }
}