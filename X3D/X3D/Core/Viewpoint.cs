using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using X3D.Engine;

namespace X3D
{
    /// <summary>
    /// http://www.web3d.org/documents/specifications/19775-1/V3.3/Part01/components/navigation.html#Viewpoint
    /// </summary>
    public partial class Viewpoint
    {
        /// <summary>
        /// http://www.web3d.org/documents/specifications/19775-1/V3.3/Part01/components/navigation.html#ViewpointList
        /// </summary>
        public static List<Viewpoint> ViewpointList = new List<Viewpoint>();

        public const string VIEWPOINT_DEFAULT_DESCRIPTION = "Origin";
        public static Viewpoint CurrentViewpoint = null;
        public static int CurrentIndex = -1;

        public static Viewpoint InitialViewpoint
        {
            get
            {
                return ViewpointList.FirstOrDefault();
            }
        }

        public static Viewpoint FinalViewpoint
        {
            get
            {
                return ViewpointList.LastOrDefault();
            }
        }


        public static void Apply(RenderingContext rc, Viewpoint viewpoint)
        {
            if (viewpoint == null)
            {
                // No viewpoint assigned in X3D


            }
            else
            {
                rc.TranslateWorldview(viewpoint.Position);
                rc.RotateWorldview(viewpoint.Orientation, viewpoint.CenterOfRotation);
            }


        }

        public static void Initilize(SceneCamera activeCamera, View viewport)
        {
            // Set up the Viewport and projection matrix
            if(InitialViewpoint == null)
            {
                activeCamera.ApplyViewport(viewport.Width, viewport.Height);
            }
            else
            {
                activeCamera.ApplyViewportProjection(InitialViewpoint, viewport);
            }

            CurrentViewpoint = InitialViewpoint;
        }

        #region Rendering Methods
         
        public override void Load()
        {
            base.Load();

            if(CurrentIndex == -1)
            {
                CurrentIndex = 0;
            }

            ViewpointList.Add(this);
        }

        public override void PreRenderOnce(RenderingContext rc)
        {
            base.PreRenderOnce(rc);
        }

        public override void Render(RenderingContext rc)
        {
            base.Render(rc);

            //CurrentViewpoint = this; // for now until ViewpointGroup is implemented

            //rc.Translate(this.Position);
            //rc.Rotate(this.Orientation, this.CenterOfRotation);
            
            //rc.PushMatricies();
        }

        public override void PostRender(RenderingContext rc)
        {
            base.PostRender(rc);

            //rc.PopMatricies();
        }

        #endregion
    }
}
