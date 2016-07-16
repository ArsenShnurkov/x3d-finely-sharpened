using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using X3D.Core;
using X3D.Engine;
using X3D.Parser;
using System.IO;

namespace Tests
{
    [TestClass]
    public class TestMFStringURIs
    {
        #region Files on local system URI tests

        [TestMethod]
        public void TestFileSystemURIs()
        {

        }

        #endregion

        #region Web URIs HTTP/HTTPS
        public void TestWebBackupMFStrings()
        {
            string url;
            string[] uris;
            object resource;
            bool success;

            url = "\"images/left.png\" \"http://www.web3d.org/x3d/content/examples/Basic/DistributedInteractiveSimulation/images/left.png\"";

            uris = X3DTypeConverters.GetMFString(url);

            Assert.IsTrue(uris.Length == 2);

            success = SceneManager.FetchSingle(uris[0], out resource);
            Assert.IsFalse(success);
            Assert.IsTrue(resource == null);
            // even though the relative address fails with uris[0] when looking on the file system
            // uris[1] should succeed in returning a new copy of the resource from a remote address.

            success = SceneManager.FetchSingle(uris[1], out resource);
            Assert.IsTrue(success);
            Assert.IsTrue(resource is Stream);
        }

        [TestMethod]
        public void TestWebURIs()
        {

        }

        [TestMethod]
        public void TestAbsoluteWebURIs()
        {
            string url;
            string[] uris;
            object resource;
            bool success;

            url = "\"http://www.web3d.org/x3d/content/examples/Basic/DistributedInteractiveSimulation/images/left.png\" \"http://www.web3d.org/x3d/content/examples/Basic/DistributedInteractiveSimulation/images/right.png\"";

            uris = X3DTypeConverters.GetMFString(url);

            Assert.IsTrue(uris.Length == 2);

            success = SceneManager.FetchSingle(uris[0], out resource);
            Assert.IsTrue(success);
            Assert.IsTrue(resource is Stream);

            success = SceneManager.FetchSingle(uris[1], out resource);
            Assert.IsTrue(success);
            Assert.IsTrue(resource is Stream);
        }

        [TestMethod]
        public void TestRelativeWebURIs()
        {
            SceneManager.CurrentLocation = "http://www.web3d.org/x3d/content/examples/Basic/DistributedInteractiveSimulation/";

            string url;
            string[] uris;
            object resource;
            bool success;

            url = "\"images/left.png\" \"images/right.png\"";

            uris = X3DTypeConverters.GetMFString(url);

            Assert.IsTrue(uris.Length == 2);

            success = SceneManager.FetchSingle(uris[0], out resource);
            Assert.IsTrue(success);
            Assert.IsTrue(resource is Stream);

            success = SceneManager.FetchSingle(uris[1], out resource);
            Assert.IsTrue(success);
            Assert.IsTrue(resource is Stream);
        }

        #endregion

        #region data:uri content data URI tests

        public void TestContentDataURIs()
        {

        }

        #endregion
    }
}
