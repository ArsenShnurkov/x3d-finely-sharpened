using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace X3D
{
    /// <summary>
    /// http://www.web3d.org/files/specifications/19775-1/V3.3/Part01/components/group.html#Switch
    /// </summary>
    public partial class Switch
    {

        public Switch()
        {
            // Could implement switch by disabling passthrough, 
            // however switch would have to ensure its child is completely rendered. 
            // Better to keep rendering logic within the renderer.

            //this.PassthroughAllowed = false; 
            this.debug = true;
        }

        #region Rendering Methods

        public override void SwitchNode(int choice)
        {
            if (debug && Shadow != null && Shadow.Count > 0)
            {
                if (choice >= 0 && choice < Shadow.Count)
                {
                    Console.WriteLine("switching to child {0} {1}", Shadow[choice], Shadow[choice]._id);
                }
                else
                {
                    Console.WriteLine("switching to child {0} {1}", Shadow[choice], Shadow[choice]._id);
                }
            }

            if (Shadow != null && Shadow.Count > 0)
            {
                this.Children.Clear();

                this.Children.Add(this.Shadow[choice]);
            }
        }

        public override void Init()
        {
            base.Init();

            // Use the Children property conceptually as a visibility list 
            // where only the current visible item is used rendered at a time.

            CopyToShadowDom();

            if(Shadow != null && Shadow.Count > 0)
                this.Children.Add(this.Shadow[this.WhichChoice]);
        }

        public override void Render(RenderingContext rc)
        {
            base.Render(rc);
        }

        #endregion
    }
}
