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
        public static string X3DExamplesDirectory = System.IO.Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory + "..\\..\\..\\..\\x3d-examples\\");

        #region Mixed MFString URI

        [TestMethod]
        public void TestMixedURIs()
        {
            string url;
            string[] uris;

            url = "\"c.jpg\" 'a.jpg' 'b.jpg' \"d.jpg\" 'e.jpg' \"f.jpg\" 'test-helloworld' "
    + "\"subfolder0\\file0.png\\\" "
    +"\"Figure14.2ElevationGridMountain.x3d\" "
    + "'http://www.web3d.org/x3d/content/examples/Vrml2.0Sourcebook/Chapter14-ElevationGrid/Figure14.2ElevationGridMountain.x3d' "
    +"\"http://www.web3d.org/x3d/content/examples/Vrml2.0Sourcebook/Chapter14-ElevationGrid/Figure14.2ElevationGridMountain.x3d\" "
    + "'subfolder1\\subfolder1-subfolder\\file1.ext'";

            uris = X3DTypeConverters.GetMFString(url);

            Assert.IsTrue(uris.Length == 12);

            Assert.AreEqual(uris[0], "c.jpg");
            Assert.AreEqual(uris[1], "a.jpg");
            Assert.AreEqual(uris[2], "b.jpg");
            Assert.AreEqual(uris[3], "d.jpg");
            Assert.AreEqual(uris[4], "e.jpg");
            Assert.AreEqual(uris[5], "f.jpg");
            Assert.AreEqual(uris[6], "test-helloworld");
            Assert.AreEqual(uris[7], "subfolder0\\file0.png\\");
            Assert.AreEqual(uris[8], "Figure14.2ElevationGridMountain.x3d");
            Assert.AreEqual(uris[9], "http://www.web3d.org/x3d/content/examples/Vrml2.0Sourcebook/Chapter14-ElevationGrid/Figure14.2ElevationGridMountain.x3d");
            Assert.AreEqual(uris[10], "http://www.web3d.org/x3d/content/examples/Vrml2.0Sourcebook/Chapter14-ElevationGrid/Figure14.2ElevationGridMountain.x3d");
            Assert.AreEqual(uris[11], "subfolder1\\subfolder1-subfolder\\file1.ext");
        }

        #endregion

        #region Files on local system URI tests

        [TestMethod]
        public void TestFileSystemURIs()
        {
            SceneManager.CurrentLocation = X3DExamplesDirectory;

            string url;
            string[] uris;
            object resource;
            bool success;

            // TEST MFString PARSING
            // uris formatted with double quotes
            url = "spectrum.jpg";
            uris = X3DTypeConverters.GetMFString(url);
            Assert.IsTrue(uris.Length == 1);
            Assert.AreEqual(uris[0], "spectrum.jpg");
            url = "\"spectrum.jpg\"";
            uris = X3DTypeConverters.GetMFString(url);
            Assert.IsTrue(uris.Length == 1);
            Assert.AreEqual(uris[0], "spectrum.jpg");

            url = "Background\\texture\\earth.jpg";
            uris = X3DTypeConverters.GetMFString(url);
            Assert.IsTrue(uris.Length == 1);
            Assert.AreEqual(uris[0], "Background\\texture\\earth.jpg");
            url = "\"Background\\texture\\earth.jpg\"";
            uris = X3DTypeConverters.GetMFString(url);
            Assert.IsTrue(uris.Length == 1);
            Assert.AreEqual(uris[0], "Background\\texture\\earth.jpg");

            // uris formatted with single quotes
            url = "'spectrum.jpg'";
            uris = X3DTypeConverters.GetMFString(url);
            Assert.IsTrue(uris.Length == 1);
            Assert.AreEqual(uris[0], "spectrum.jpg");

            url = "'Background\\texture\\earth.jpg'";
            uris = X3DTypeConverters.GetMFString(url);
            Assert.IsTrue(uris.Length == 1);
            Assert.AreEqual(uris[0], "Background\\texture\\earth.jpg");


            // TEST MFString URI fetching
            url = "spectrum.jpg";
            success = SceneManager.FetchSingle(url, out resource);
            Assert.IsTrue(success);
            Assert.IsTrue(resource is Stream);
            url = "\"spectrum.jpg\"";
            success = SceneManager.FetchSingle(url, out resource);
            Assert.IsTrue(success);
            Assert.IsTrue(resource is Stream);
            url = "Background\\texture\\earth.jpg";
            success = SceneManager.FetchSingle(url, out resource);
            Assert.IsTrue(success);
            Assert.IsTrue(resource is Stream);

            SceneManager.CurrentLocation = X3DExamplesDirectory + "Background\\";
            url = "'texture\\generic\\BK.png' 'texture\\generic\\DN.png' 'texture\\generic\\FR.png' 'texture\\generic\\LF.png' 'texture\\generic\\RT.png' 'texture\\generic\\UP.png'";
            uris = X3DTypeConverters.GetMFString(url);
            success = SceneManager.FetchSingle(uris[0], out resource);
            Assert.IsTrue(success);
            Assert.IsTrue(resource is Stream);
            success = SceneManager.FetchSingle(uris[1], out resource);
            Assert.IsTrue(success);
            Assert.IsTrue(resource is Stream);
            success = SceneManager.FetchSingle(uris[2], out resource);
            Assert.IsTrue(success);
            Assert.IsTrue(resource is Stream);
            success = SceneManager.FetchSingle(uris[3], out resource);
            Assert.IsTrue(success);
            Assert.IsTrue(resource is Stream);
            success = SceneManager.FetchSingle(uris[4], out resource);
            Assert.IsTrue(success);
            Assert.IsTrue(resource is Stream);
            success = SceneManager.FetchSingle(uris[5], out resource);
            Assert.IsTrue(success);
            Assert.IsTrue(resource is Stream);
        }

        #endregion

        #region Web URIs HTTP/HTTPS
        public void TestURIWithBackupURI()
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
        public void TestAbsoluteWebURIs()
        {
            string url;
            string[] uris;
            object resource;
            bool success;

            url = "\"http://www.web3d.org/x3d/content/examples/Basic/DistributedInteractiveSimulation/images/left.png\" \"http://www.web3d.org/x3d/content/examples/Basic/DistributedInteractiveSimulation/images/right.png\" \"http://www.web3d.org/x3d/content/examples/Basic/DistributedInteractiveSimulation/images/front.png\"";

            uris = X3DTypeConverters.GetMFString(url);

            Assert.IsTrue(uris.Length == 3);

            success = SceneManager.FetchSingle(uris[0], out resource);
            Assert.IsTrue(success);
            Assert.IsTrue(resource is Stream);

            success = SceneManager.FetchSingle(uris[1], out resource);
            Assert.IsTrue(success);
            Assert.IsTrue(resource is Stream);

            success = SceneManager.FetchSingle(uris[2], out resource);
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

            url = "\"images/left.png\" \"images/right.png\" \"images/front.png\" \"images/back.png\" \"images/top.png\" \"images/bottom.png\"";

            uris = X3DTypeConverters.GetMFString(url);

            Assert.IsTrue(uris.Length == 6);

            success = SceneManager.FetchSingle(uris[0], out resource);
            Assert.IsTrue(success);
            Assert.IsTrue(resource is Stream);

            success = SceneManager.FetchSingle(uris[1], out resource);
            Assert.IsTrue(success);
            Assert.IsTrue(resource is Stream);

            success = SceneManager.FetchSingle(uris[2], out resource);
            Assert.IsTrue(success);
            Assert.IsTrue(resource is Stream);

            success = SceneManager.FetchSingle(uris[3], out resource);
            Assert.IsTrue(success);
            Assert.IsTrue(resource is Stream);

            success = SceneManager.FetchSingle(uris[4], out resource);
            Assert.IsTrue(success);
            Assert.IsTrue(resource is Stream);

            success = SceneManager.FetchSingle(uris[5], out resource);
            Assert.IsTrue(success);
            Assert.IsTrue(resource is Stream);
        }

        #endregion

        #region data:uri content data URI tests

        [TestMethod]
        public void TestContentDataURIs()
        {
            string dataUri;
            string[] uris;
            object resource;
            bool success;

            dataUri = @"'data:text/plain
#version 420 core &#13;
layout(location = 0) in vec3 position; &#13;'";


            uris = X3DTypeConverters.GetMFString(dataUri);
            Assert.IsTrue(uris.Length == 1);
            success = SceneManager.FetchSingle(uris[0], out resource);
            Assert.IsTrue(success);
            Assert.IsTrue(resource is Stream);
            StreamReader reader = new StreamReader(resource as Stream);
            string dataTextPlain = reader.ReadToEnd();
            string someTestShaderCodeSample = "#version 420 core &#13;\nlayout(location = 0) in vec3 position; &#13;";
            Assert.AreEqual(dataTextPlain, someTestShaderCodeSample);


            //TODO: complete tests for data:uri as seen in https://developer.mozilla.org/en-US/docs/Web/HTTP/data_URIs

        }

        #endregion
    }
}
