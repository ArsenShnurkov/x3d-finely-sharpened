using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using V8.Net;

namespace X3D.Engine
{
    /// <summary>
    /// Window used by Scripting component 
    /// (via V8.Net Engine)
    /// </summary>
    public class X3DWindow
    {
        public static X3DWindow Current;

        internal static List<System.Timers.Timer> timers = new List<System.Timers.Timer>();

        public View screen { get; set; }

        internal static void SetView(View view)
        {
            Current = new X3DWindow();
            Current.screen = view;
        }

        public int setInterval(string sourceFragment, int milliseconds)
        {
            if(!string.IsNullOrEmpty(sourceFragment) && milliseconds > 0)
            {
                System.Timers.Timer tmr = new System.Timers.Timer();
                tmr.Interval = milliseconds;

                tmr.Elapsed += (object sender, System.Timers.ElapsedEventArgs e) =>
                {
                    try
                    {
                        ScriptingEngine.v8.Execute("____$=" + sourceFragment + ";____$();", ScriptingEngine.SOURCE_NAME, false);
                    }
                    catch (Exception ex) { }
                };

                tmr.Start();

                timers.Add(tmr);

                return timers.Count -1;
            }
            return -1;
        }

        public static InternalHandle setInterval(V8Engine engine, bool isConstructCall, InternalHandle _this, params InternalHandle[] args)
        {
            if (isConstructCall)
            {
                return _this;
            }
            else
            {
                if (args.Length == 2)
                {
                    InternalHandle timerCallback = args[0];
                    InternalHandle milliseconds = args[1];

                    if (milliseconds != null && milliseconds.IsInt32 && timerCallback != null && timerCallback.IsFunction)
                    {
                        int ms = milliseconds.AsInt32;
                        string sourceFragment = timerCallback.Value.ToString();

                        System.Timers.Timer tmr = new System.Timers.Timer();
                        tmr.Interval = milliseconds.AsInt32;

                        tmr.Elapsed += (object sender, System.Timers.ElapsedEventArgs e) =>
                        {
                            try
                            {
                                ScriptingEngine.v8.Execute("____$=" + sourceFragment + ";____$();", ScriptingEngine.SOURCE_NAME, false);
                            }
                            catch (Exception ex) { }
                        };

                        tmr.Start();

                        X3DWindow.timers.Add(tmr);

                        return ScriptingEngine.v8.CreateValue(X3DWindow.timers.Count - 1);
                    }
                }
            }

            return InternalHandle.Empty;
        }

        public void clearInterval(int timer)
        {
            if (timer > -1)
            {
                System.Timers.Timer tmr = timers[timer];

                tmr.Stop();
                tmr.Dispose();
                tmr = null;
            }
        }

        public static InternalHandle clearInterval(V8Engine engine, bool isConstructCall, InternalHandle _this, params InternalHandle[] args)
        {
            if (isConstructCall)
            {
                return _this;
            }
            else
            {
                if (args.Length == 1)
                {
                    InternalHandle timerId = args[0];

                    if (timerId != null && timerId.IsInt32)
                    {
                        System.Timers.Timer tmr = X3DWindow.timers[timerId.AsInt32];

                        tmr.Stop();
                    }
                }
            }

            return InternalHandle.Empty;
        }
    }
}
