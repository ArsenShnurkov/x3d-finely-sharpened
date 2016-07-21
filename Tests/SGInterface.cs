using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using X3D;
using X3D.Engine;
using X3D.Parser;

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
