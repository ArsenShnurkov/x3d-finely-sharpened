using System.Collections.Generic;
using System.Linq;

namespace X3D
{
    public partial class ProtoBody
    {
        public override void Load()
        {
            base.Load();

            List<SceneGraphNode> renderable;
            SceneGraphNode baseDefinition;

            baseDefinition = Children.FirstOrDefault();

            if (baseDefinition == null) return;

            //renderable = this.Children
            //    .Where(c => (typeof(X3DShapeNode).IsInstanceOfType(c) 
            //    || typeof(X3DGeometryNode).IsInstanceOfType(c)
            //    || typeof(X3DComposedGeometryNode).IsInstanceOfType(c))).ToList();

            renderable = new List<SceneGraphNode>();
            renderable.AddRange(DecendantsByType<X3DShapeNode>());
            renderable.AddRange(DecendantsByType<X3DGeometryNode>());
            renderable.AddRange(DecendantsByType<X3DComposedGeometryNode>());


            renderable.Remove(baseDefinition);

            // Hide Renderable Decendants that are not from baseDefinition
            foreach (var node in renderable) node.Hidden = true;
            //node.PassthroughAllowed = false;
            // now only the baseDefinition is renderable
        }

        public override void PreRender()
        {
            base.PreRender();
        }

        public override void Render(RenderingContext rc)
        {
            base.Render(rc);
        }
    }
}