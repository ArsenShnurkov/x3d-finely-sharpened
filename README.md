# x3d-finely-sharpened
X3D browser/engine written in C#


Intro
```

With software like Blender as well as libraries like OpenTK, X3D as a markup for 3D scenes makes for a viable alternative to the COLLADA XML dialect and other 3D formats.

"X3D is used for building 3-dimensional models, both simple and sophisticated, X3D can show animated objects from different viewpoint perspectives, allowing user insight and interaction. X3D models can be further combined and connected to build sophisticated 3D virtual environments running across the Internet." (Brutzman and Daly, 2007)

A copy of the OpenTK library ships along as the engine uses it at the moment as a point of reference to OpenGL. Currently we're a bit low on documentation but I have included a few X3D examples pulled from random sources on the www.
```



Example 1 - test of tessellated Sphere X3D node: X3D Runtime 3.3 Core using OpenGL Version 4 
![alt text](screenshots/screenshot1.png "X3D Runtime Viewer Example 1")



Example 2 - test of IndexedFaceSet: X3D Runtime 3.3 Core using OpenGL Version 4 
![alt text](screenshots/screenshot2.png "X3D Runtime Viewer Example 2")



Notices
```

This is no full X3D framework .. Yet however in addition to a primary focus on C#, there is an experimental port to the dart language for the Web. The C# codebase is focusing on implementing rendering of basic FaceSet nodes and nailing down X3D Core early so the project's foundations adhere to the spec more thoroughly 

This project will remain here for archive purposes. I have nearly completed the rendering of and nodes according to specification There's experimental code released to address some of the items listed The latest source is in the repo `
```

Todo list
```

PARTLY: URL/URI/filesys/data:text/plain resource fetching
TODO: Scripting using JavaScript (JIT compiled using the V8 Engine)
TODO: Node Prototyping
PARTLY: abstract types defined according to X3D 3.3 spec
PARTLY: DEF and USE
PARTLY: grouping
TODO: event propagation
TODO: networking: inline, import, export, etc
PARTLY: scene graph and event graph models
PARTLY: transformations
PARTLY: all texturing related nodes esp. TextureProperties
TODO: animations & interpolators
TODO: lighting
TODO: global scene nodes like Viewpoint
PARTLY: geometry nodes
PARTLY: shaders
TODO: Simple volume rendering using slices fetched from sets of image files e.g. *.png, *.jpg
TODO: volume rendering using either: vox or nrrd formats or whatever will be defined by ISO
PARTLY: SceneGraph rendering
TODO: documentation
TODO: complete range of X3D examples covering all node usages
```

Mission statement
```

A library implementing the X3D 3.3 specification, frugal by design, with no platform specific boundaries, and an X3D Scene that can be accessed/modified by whatever scripting language is present in the platform.
```

Info
```

http://en.wikipedia.org/wiki/X3D

http://edutechwiki.unige.ch/en/X3D

http://www.web3d.org/x3d/

http://www.web3d.org/files/specifications/19775-1/V3.3/Part01/concepts.html

http://www.web3d.org/files/specifications/19775-1/V3.3/Part01/components/core.html

http://www.web3d.org/files/specifications/19775-1/V3.3/Part01/componentIndex.html
```

Licence

```

New BSD Licence

Copyright © 2013 - 2016, Mr Gerallt G. Franke of Tasmania, Australia

All rights reserved.

Redistribution and use in source and binary forms, with or without

modification, are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.

* Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.

* Neither the name of the library nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND

ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED

WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE

DISCLAIMED. IN NO EVENT SHALL MR GERALLT G. FRANKE BE LIABLE FOR ANY

DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES

(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;

LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND

ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT

(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS

SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
