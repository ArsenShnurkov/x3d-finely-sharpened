using Microsoft.VisualStudio.TestTools.UnitTesting;
using X3D;

namespace Tests
{
    public class SGInterface
    {
        [TestMethod]
        public void TestSceneGraphSetAttributes()
        {
            SceneGraphNode element;
            string value;

            element = new X3D.X3D();
            element.setAttribute("profile", "Interactive");
            value = (string)element.getAttribute("profile");

            Assert.AreEqual(value, "Interactive");
            Assert.AreEqual(((X3D.X3D)element).profile, "Interactive");
        }
    }
}