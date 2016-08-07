using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics.OpenGL;
using X3D;
using X3D.Engine;

namespace x3druntime.ui.opentk
{

    public partial class X3DApplication
    {



        private void Keyboard_KeyUp(object sender, OpenTK.Input.KeyboardKeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.F12:
                    showGraphDebugger = !showGraphDebugger;

                    if (showGraphDebugger)
                    {
                        X3DGraphDebugger.Display(scene.SceneGraph);
                    }
                    else
                    {
                        X3DGraphDebugger.Hide();
                    }

                    break;
                case Key.F4: // halt rotate
                    rotate_enable = !rotate_enable;
                    break;
                case Key.F3: // halt special effects
                    fx_enable = !fx_enable;
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
                        lockMouseCursor = true;
                    }
                    else
                    {
                        window.WindowState = WindowState.Normal;
                        lockMouseCursor = false;
                    }

                    isFullscreen = !isFullscreen;

                    ToggleCursor();

                    break;
                case Key.O:

                    NavigationInfo.HeadlightEnabled = !NavigationInfo.HeadlightEnabled;

                    break;
                case Key.V:
                    // View all viewpoints
                    string text = "";
                    foreach (Viewpoint v in Viewpoint.ViewpointList) { text += "[" + v.description + "] "; }

                    // Integrate this into HUD
                    System.Windows.Forms.MessageBox.Show("Viewpoints: " + text.TrimEnd());
                    break;

                #region Viewpoint Key Bindings

                case Key.Home:
                    // Goto Initial Viewpoint
                    Viewpoint.CurrentIndex = (Viewpoint.InitialViewpoint == null ? -1 : 0);
                    Viewpoint.CurrentViewpoint = Viewpoint.InitialViewpoint;
                    break;

                case Key.PageDown:
                    // Goto Next Viewpoint
                    if (Viewpoint.ViewpointList.Count > 0)
                    {
                        if (Viewpoint.CurrentIndex + 1 == Viewpoint.ViewpointList.Count)
                        {
                            Viewpoint.CurrentIndex = 0;
                        }
                        else
                        {
                            Viewpoint.CurrentIndex++;
                        }
                        Viewpoint.CurrentViewpoint = Viewpoint.ViewpointList[Viewpoint.CurrentIndex];
                    }
                    break;

                case Key.PageUp:
                    // Goto Previous viewpoint
                    if (Viewpoint.ViewpointList.Count > 0)
                    {
                        if (Viewpoint.CurrentIndex - 1 < 0)
                        {
                            Viewpoint.CurrentIndex = Viewpoint.ViewpointList.Count - 1;
                        }
                        else
                        {
                            Viewpoint.CurrentIndex--;
                        }
                        Viewpoint.CurrentViewpoint = Viewpoint.ViewpointList[Viewpoint.CurrentIndex];
                    }
                    break;

                case Key.End:
                    // Goto Final Viewpoint
                    Viewpoint.CurrentIndex = Viewpoint.ViewpointList.Count - 1;
                    Viewpoint.CurrentViewpoint = Viewpoint.FinalViewpoint;
                    break;
                    #endregion
            }
        }
        
        private void ApplyKeyBindings(FrameEventArgs e)
        {
            Vector3 direction = Vector3.Zero;
            bool rotated = false;

            slowFlySpeed = Keyboard[Key.AltLeft];
            fastFlySpeed = Keyboard[Key.ShiftLeft];
            movementSpeed = fastFlySpeed ? 10.0f : 1.0f;
            movementSpeed = slowFlySpeed ? 0.01f : movementSpeed;



            // Calibrator (for translation debugging)
            if (Keyboard[Key.Number1])
            {
                ActiveCamera.calibTrans.X += ActiveCamera.calibSpeed.X;
            }
            if (Keyboard[Key.Number2])
            {
                ActiveCamera.calibTrans.X -= ActiveCamera.calibSpeed.X;
            }
            if (Keyboard[Key.Number3])
            {
                ActiveCamera.calibTrans.Y += ActiveCamera.calibSpeed.Y;
            }
            if (Keyboard[Key.Number4])
            {
                ActiveCamera.calibTrans.Y -= ActiveCamera.calibSpeed.Y;
            }
            if (Keyboard[Key.Number5])
            {
                ActiveCamera.calibTrans.Z += ActiveCamera.calibSpeed.Z;
            }
            if (Keyboard[Key.Number6])
            {
                ActiveCamera.calibTrans.Z -= ActiveCamera.calibSpeed.Z;
            }

            // Calibrator (for orientation debugging)
            if (Keyboard[Key.Number6])
            {
                ActiveCamera.calibOrient.X += ActiveCamera.calibSpeed.X;
            }
            if (Keyboard[Key.Number7])
            {
                ActiveCamera.calibOrient.X -= ActiveCamera.calibSpeed.X;
            }
            if (Keyboard[Key.Number8])
            {
                ActiveCamera.calibOrient.Y += ActiveCamera.calibSpeed.Y;
            }
            if (Keyboard[Key.Number9])
            {
                ActiveCamera.calibOrient.Y -= ActiveCamera.calibSpeed.Y;
            }
            if (Keyboard[Key.Minus])
            {
                ActiveCamera.calibOrient.Z += ActiveCamera.calibSpeed.Z;
            }
            if (Keyboard[Key.Plus])
            {
                ActiveCamera.calibOrient.Z -= ActiveCamera.calibSpeed.Z;
            }



            if (Keyboard[Key.Escape] || Keyboard[Key.Q])
            {
                // QUIT APPLICATION
                if (window.WindowState == WindowState.Fullscreen)
                {
                    window.WindowState = WindowState.Normal;
                }

                X3DProgram.Quit();
            }
            if (Keyboard[Key.P])
            {
                // LOAD NEW SCENE
                if (window.WindowState == WindowState.Fullscreen)
                {
                    window.WindowState = WindowState.Normal;
                }

                X3DProgram.Restart();
            }

            if (Keyboard[Key.R])
            {
                // RESET CAMERA POSITION+ORIENTATION
                ActiveCamera.Reset();
            }

            if (NavigationInfo.NavigationType != NavigationType.Examine)
            {
                if (Keyboard[Key.T])
                {
                    ActiveCamera.Fly(playerDirectionMagnitude * movementSpeed);
                }
                if (Keyboard[Key.G])
                {
                    ActiveCamera.Fly(-playerDirectionMagnitude * movementSpeed);
                }

                if (Keyboard[Key.W])
                {
                    ActiveCamera.Walk(playerDirectionMagnitude * movementSpeed);
                    //direction += ActiveCamera.Direction * playerDirectionMagnitude;
                }
                if (Keyboard[Key.S])
                {
                    ActiveCamera.Walk(-playerDirectionMagnitude * movementSpeed);
                    //direction -= ActiveCamera.Direction * playerDirectionMagnitude;
                }
                if (Keyboard[Key.A])
                {
                    ActiveCamera.Strafe(playerDirectionMagnitude * movementSpeed);
                    //ActiveCamera.Right = ActiveCamera.Up.Cross(ActiveCamera.Direction);
                    //direction += ActiveCamera.Right * playerDirectionMagnitude;
                }
                if (Keyboard[Key.D])
                {
                    ActiveCamera.Strafe(-playerDirectionMagnitude * movementSpeed);
                    //ActiveCamera.Right = ActiveCamera.Up.Cross(ActiveCamera.Direction);
                    //direction -= ActiveCamera.Right * playerDirectionMagnitude;
                }

                #region G.3 Emulate pointing device Key Bindings

                if (Keyboard[Key.Left])
                {
                    //ActiveCamera.Horizon();
                    //ActiveCamera.Yaw(-10.0f * 0.007f);
                    ActiveCamera.ApplyYaw(-playerDirectionMagnitude * 0.3f);
                    //ActiveCamera.ApplyRotation();

                    //this.heading += 1.0f;
                    //this.yrot = this.heading;

                    rotated = true;
                }
                if (Keyboard[Key.Right])
                {
                    //ActiveCamera.Horizon();
                    //ActiveCamera.Yaw(10.0f * 0.007f);
                    ActiveCamera.ApplyYaw(playerDirectionMagnitude * 0.3f);
                    //ActiveCamera.ApplyRotation();
                    //this.heading -= 1.0f;
                    //this.yrot = this.heading;

                    rotated = true;
                }
                if (Keyboard[Key.Up])
                {
                    //ActiveCamera.Pitch(10.0f * 0.007f);
                    ActiveCamera.ApplyPitch(-playerDirectionMagnitude * 0.3f);
                    //ActiveCamera.ApplyRotation();

                    //this.xpos += (float)Math.Sin(this.heading * Math.PI / 180.0) * 0.05f;
                    //this.zpos += (float)Math.Cos(this.heading * Math.PI / 180.0) * 0.05f;
                    //if (this.walkbiasangle >= 359.0f)
                    //    this.walkbiasangle = 0.0f;
                    //else
                    //    this.walkbiasangle -= 5.0f;
                    //this.walkbias = (float)Math.Sin(this.walkbiasangle * Math.PI / 180.0) / 10.0f;

                    rotated = true;
                }
                if (Keyboard[Key.Down])
                {
                    //ActiveCamera.Pitch(-10.0f * 0.007f);
                    ActiveCamera.ApplyPitch(playerDirectionMagnitude * 0.3f);
                    //ActiveCamera.ApplyRotation();

                    //this.xpos -= (float)Math.Sin(this.heading * Math.PI / 180.0) * 0.05f;
                    //this.zpos -= (float)Math.Cos(this.heading * Math.PI / 180.0) * 0.05f;
                    //if (this.walkbiasangle >= 359.0f)
                    //    this.walkbiasangle = 0.0f;
                    //else
                    //    this.walkbiasangle += 10.0f;
                    //this.walkbias = (float)Math.Sin(this.walkbiasangle * Math.PI / 180.0) / 10.0f;

                    rotated = true;
                }

                if (Keyboard[Key.Number0])
                {
                    ActiveCamera.ApplyRoll(-0.1f);
                    rotated = true;
                }
                if (Keyboard[Key.Number9])
                {
                    ActiveCamera.ApplyRoll(0.1f);
                    rotated = true;
                }

                #endregion
            }

            if (rotated)
            {
                ActiveCamera.ApplyRotation();
            }
        }
    }
}