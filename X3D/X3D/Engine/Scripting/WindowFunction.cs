﻿using System.Collections.Generic;
using System.Timers;
using V8.Net;

namespace X3D.Engine
{
    /// <summary>
    ///     Window used by Scripting component
    ///     (via V8.Net Engine)
    /// </summary>
    public class WindowFunction : V8Function
    {
        private static List<Timer> timers = new List<Timer>();

        public static View screen { get; set; }

        public override InternalHandle Initialize(bool isConstructCall, params InternalHandle[] args)
        {
            SetProperty("screen", screen);
            SetProperty("setInterval", Engine.CreateFunctionTemplate().GetFunctionObject(this.setInterval));
            SetProperty("clearInterval", Engine.CreateFunctionTemplate().GetFunctionObject(this.clearInterval));

            Engine.DynamicGlobalObject.setInterval =
                Engine.CreateFunctionTemplate().GetFunctionObject(this.setInterval);
            Engine.DynamicGlobalObject.clearInterval =
                Engine.CreateFunctionTemplate().GetFunctionObject(this.clearInterval);

            return base.Initialize(isConstructCall, args);
        }

        internal static void SetView(View view)
        {
            screen = view;
        }

        #region setInterval

        public InternalHandle setInterval(V8Engine engine,
            bool isConstructCall,
            InternalHandle _this,
            params InternalHandle[] args)
        {
            if (isConstructCall)
            {
                return _this;
            }
            else
            {
                if (args != null && args.Length == 2)
                {
                    InternalHandle timerCallback = args[0];
                    InternalHandle milliseconds = args[1];

                    if (milliseconds != null && milliseconds.IsInt32 && timerCallback != null &&
                        timerCallback.IsFunction)
                    {
                        int ms = milliseconds.AsInt32;
                        string sourceFragment = timerCallback.Value.ToString();

                        Timer tmr = new Timer();
                        tmr.Interval = milliseconds.AsInt32;

                        tmr.Elapsed += (object sender, ElapsedEventArgs e) =>
                        {
                            if (!engine.IsDisposed)
                            {
                                try
                                {
                                    engine.Execute("____$=" + sourceFragment + ";____$();", ScriptingEngine.SOURCE_NAME,
                                        false);
                                }
                                catch
                                {
                                }
                            }
                            else
                            {
                                tmr.Stop();
                            }
                        };

                        tmr.Start();

                        timers.Add(tmr);

                        Handle timerHandle = engine.CreateValue(timers.Count - 1);

                        return timerHandle;
                    }
                }
            }

            return InternalHandle.Empty;
        }

        #endregion

        #region clearInterval

        public InternalHandle clearInterval(V8Engine engine,
            bool isConstructCall,
            InternalHandle _this,
            params InternalHandle[] args)
        {
            if (isConstructCall)
            {
                return _this;
            }
            else
            {
                if (args != null && args.Length == 1)
                {
                    InternalHandle timerId = args[0];

                    if (timerId != null && timerId.IsInt32)
                    {
                        Timer tmr = timers[timerId.AsInt32];

                        tmr.Stop();
                    }
                }
            }

            return InternalHandle.Empty;
        }

        #endregion
    }
}