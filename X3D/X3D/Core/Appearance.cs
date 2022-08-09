namespace X3D
{
    public partial class Appearance
    {
        #region Private Fields

        private Shape parentShape;

        #endregion

        public Material material { get; set; } // set by containerField

        #region Rendering Methods

        public override void Load()
        {
            base.Load();

            parentShape = GetParent<Shape>();
        }

        public override void PreRenderOnce(RenderingContext rc)
        {
            base.PreRenderOnce(rc);
        }

        public override void Render(RenderingContext rc)
        {
            base.Render(rc);
        }

        #endregion
    }
}