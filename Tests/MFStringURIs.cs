using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using X3D.Core;
using X3D.Engine;
using X3D.Parser;

namespace Tests
{
    [TestClass]
    public class MFStringURIs
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


            success = SceneManager.FetchSingle(uris[1], out resource);
        }

        [TestMethod]
        public void TestWebURIs()
        {

        }

        [TestMethod]
        public void TestAbsoluteWebURIs()
        {

        }

        [TestMethod]
        public void TestRelativeWebURIs()
        {

        }

        #endregion

        #region data:uri content data URI tests

        public void TestContentDataURIs()
        {

        }

        #endregion
    }
}
