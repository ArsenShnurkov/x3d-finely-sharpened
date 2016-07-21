﻿using System;
using V8.Net; // https://v8dotnet.codeplex.com
using Microsoft.VisualStudio.TestTools.UnitTesting;

using X3D;
using X3D.Engine;

namespace Tests
{
    [TestClass]
    public class SGScripting
    {
        public SceneGraphNode CreateTestDOM()
        {
            SceneGraphNode document,
                           scene,
                           shape,
                           sphere;

            document = new X3D.X3D();
            scene = new X3D.Scene();
            shape = new X3D.Shape();
            sphere = new X3D.Sphere();

            shape.Children.Add(sphere);
            scene.Children.Add(shape);
            document.Children.Add(scene);
            
            scene.Parent = document;
            shape.Parent = scene;
            sphere.Parent = shape;

            return document;
        }

        [TestMethod]
        public void TestSGScripting()
        {
            ScriptingEngine engine;
            SceneGraphNode document;
            string msg;
            string test_script;

            test_script = "console.log('foo');";
            
            document = CreateTestDOM();
            engine = ScriptingEngine.CreateFromDocument(document);

            msg = engine.Execute(test_script);

            Assert.AreEqual(X3DConsole.LastMessage, "foo");
        }
    }
}