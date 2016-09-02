using OpenTK;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using X3D.Core;
using X3D.Core.Shading;
using X3D.Core.Shading.DefaultUniforms;
using X3D.Engine;
using X3D.Parser;

namespace X3D
{
    //TODO: calculate bounding boxes of geometry in this node
    /* 
        SFVec3f []       bboxCenter 0 0 0    (-∞,∞)
         SFVec3f []       bboxSize   -1 -1 -1 [0,∞) or −1 −1 −1
    */
    public partial class Shape : X3DShapeNode
    {
        #region Private Fields

        private bool hasShaders;
        private List<X3DShaderNode> shaders;
        private readonly float tessLevelInner = 137; // 3
        private readonly float tessLevelOuter = 115; // 2

        private List<ShaderMaterial> shaderMaterials;
        //private static Random r = new Random();

        // GEOMETRY
        private GeometryHandle _handle;
        //private int vbo3, vbo4, NumVerticies3, NumVerticies4;
        private BoundingBox bbox;
        //private bool hasGeometry = false;
        private bool loadedGeometry = false;

        private Vector3 size = new Vector3(1, 1, 1);
        private Vector3 scale = new Vector3(0.05f, 0.05f, 0.05f);
        //var scale = new Vector3(0.04f, 0.04f, 0.04f); //Sphere
        private ComposedShader quadShader = null;
        private bool coloring = false;
        private bool texturing = false;


        #endregion

        #region Public Properties

        //[XmlIgnore]
        //public X3DAppearanceNode appearance { get; set; }

        //[XmlIgnore]
        //public X3DGeometryNode geometry { get; set; }

        #endregion

        #region Public Fields


        [XmlIgnore]
        public bool drawBoundingBox = true;

        [XmlAttribute("depthMask")]
        public bool depthMask = true;

        [XmlIgnore]
        public List<ComposedShader> ComposedShaders = new List<ComposedShader>();

        [XmlIgnore]
        public bool texturingEnabled;

        [XmlIgnore]
        public int uniformModelview, uniformProjection;

        [XmlIgnore]
        public ShaderUniformsPNCT uniforms = new ShaderUniformsPNCT();

        [XmlIgnore]
        public ShaderMaterialUniforms Materials = new ShaderMaterialUniforms();

        [XmlIgnore]
        public TessShaderUniforms Uniforms = new TessShaderUniforms();

        [XmlIgnore]
        public Matrix3 NormalMatrix = Matrix3.Identity;

        [XmlIgnore]
        public ComposedShader CurrentShader = null;

        [XmlIgnore]
        public Vector3 centerOfRotation = new Vector3(0.0f, -0.09f, 0.0f); // TODO: calculate from bounding box center

        #endregion

        #region Public Methods

        public void CollectMaterials()
        {
            ShaderMaterial shaderMaterial;
            List<Material> materials;
            Appearance appearance;

            materials = new List<Material>();
            appearance = this.ItemsByType<Appearance>().FirstOrDefault();

            if (appearance != null)
                materials = appearance.ItemsByType<Material>();

            shaderMaterials = new List<ShaderMaterial>();

            foreach (Material material in materials)
            {
                shaderMaterial = ShaderMaterial.FromX3DMaterial(material);

                shaderMaterials.Add(shaderMaterial);
            }
        }

        public void ApplyMaterials(ComposedShader shader)
        {
            List<Material> materials;
            Appearance appearance;

            materials = new List<Material>();
            appearance = this.ItemsByType<Appearance>().FirstOrDefault();

            if (appearance != null)
                materials = appearance.ItemsByType<Material>();

            shader.SetFieldValue("materialsEnabled", materials.Any() ? 1 : 0);

            if (materials.Any())
            {

            }
            else
            {
                //shaderMaterials.Add(new ShaderMaterial()
                //{
                //     ambientIntensity = 0.1f,
                //     diffuseColor = new Vector4(0,0,0,0),
                //     emissiveColor = new Vector4(0,0,0,0),
                //    specularColor = new Vector4(1, 1, 1, 1),
                //    transparency = 1.0f,
                //    shininess = 0.2f
                //});
            }

            shader.SetFieldValue("materialsCount", shaderMaterials.Count);

            Buffering.BufferMaterials(shader, shaderMaterials, "materials");
        }


        public Matrix4 ApplyGeometricTransformations(RenderingContext rc, ComposedShader shader, SceneGraphNode context)
        {

            RefreshDefaultUniforms(shader);
            //RefreshMaterialUniforms();

            if (shader.IsTessellator)
                RefreshTessUniforms(shader);


            Matrix4 view = Matrix4.LookAt(new Vector3(4, 3, 3),  // Camera is at (4,3,3), in World Space
                new Vector3(0, 0, 0),  // and looks at the origin
                new Vector3(0, 1, 0) // Head is up (set to 0,-1,0 to look upside-down)
            );

            Matrix4 model; // applied transformation hierarchy

            SceneGraphNode transform_context = context == null ? this : context;

            List<Transform> transformationHierarchy = transform_context
                .AscendantByType<Transform>()
                .Select(t => (Transform)t)
                .Where(t => t.Hidden == false)
                .ToList();

            Matrix4 modelview = Matrix4.Identity;// * rc.matricies.worldview;

            // using Def_Use/Figure02.1Hut.x3d Cone and Cylinder 
            Vector3 x3dScale = new Vector3(0.06f, 0.06f, 0.06f); // scaling down to conform with X3D standard (note this was done manually and might need tweaking)

            //x3dScale = Vector3.One;

            Quaternion modelrotation = Quaternion.Identity;
            Matrix4 modelLocalRotation = Matrix4.Identity;


            //if (rc.cam.OrbitLocalOrientation != Vector2.Zero)
            //{
            //    // Center of Rotation based on center of bounding box 
            //    Quaternion qLocal = QuaternionExtensions.EulerToQuat(0, -rc.cam.OrbitLocalOrientation.X, -rc.cam.OrbitLocalOrientation.Y);
            //    Quaternion qAdjust = QuaternionExtensions.EulerToQuat(MathHelpers.PIOver2, 0.0f, 0.0f); 

            //    Matrix4 mat4CenterOfRotation = Matrix4.CreateTranslation(centerOfRotation);
            //    Matrix4 origin = Matrix4.CreateTranslation(new Vector3(0, 0, 0));

            //    modelLocalRotation = mat4CenterOfRotation * Matrix4.CreateFromQuaternion(qLocal) * Matrix4.CreateFromQuaternion(qAdjust);
            //}

            //const float bbscale = 0.0329999961f;

            Vector3 centerOffset = Vector3.Zero;

            foreach (Transform transform in transformationHierarchy)
            {
                modelview = SceneEntity.ApplyX3DTransform(centerOffset,
                                                     Vector3.Zero,
                                                     transform.Scale,
                                                     Vector3.Zero,
                                                     transform.Translation * x3dScale,
                                                     modelview);

                //modelview *= Matrix4.CreateTranslation(transform.Translation * x3dScale);

                //modelrotation = new Quaternion(transform.Rotation.X, transform.Rotation.Y, transform.Rotation.Z, transform.Rotation.W);
                //modelrotations *= Matrix4.CreateFromQuaternion(modelrotation);

                //modelrotations *= MathHelpers.CreateRotation(ref modelrotation);
            }

            //Vector3 center = modelview.ExtractTranslation();
            //Vector3 centerOffsetVector = center + (bbox.Maximum - bbox.Minimum);
            //Matrix4 centerOffset = Matrix4.CreateTranslation(centerOffsetVector);



            model = modelview;

            Matrix4 cameraTransl = Matrix4.CreateTranslation(rc.cam.Position);

            Quaternion q = rc.cam.Orientation;

            Matrix4 cameraRot;

            cameraRot = Matrix4.CreateFromQuaternion(q); // cameraRot = MathHelpers.CreateRotation(ref q);


            Matrix4 MVP = ((modelLocalRotation * model) * cameraTransl) * cameraRot; // position and orient the Shape relative to the world and camera

                

            //shader.SetFieldValue("size", new Vector3(bbox.Width, bbox.Height, bbox.Depth) * bbscale);
            shader.SetFieldValue("modelview", ref MVP); //GL.UniformMatrix4(uniformModelview, false, ref rc.matricies.modelview);
            shader.SetFieldValue("projection", ref rc.matricies.projection);
            shader.SetFieldValue("camscale", rc.cam.Scale.X); //GL.Uniform1(uniformCameraScale, rc.cam.Scale.X);
            shader.SetFieldValue("X3DScale", rc.matricies.Scale); //GL.Uniform3(uniformX3DScale, rc.matricies.Scale);
            shader.SetFieldValue("coloringEnabled", this.coloring ? 1 : 0); //GL.Uniform1(uniforms.a_coloringEnabled, 0);
            shader.SetFieldValue("texturingEnabled", this.texturingEnabled ? 1 : 0); //GL.Uniform1(uniforms.a_texturingEnabled, this.texturingEnabled ? 1 : 0);
            shader.SetFieldValue("lightingEnabled", 1);

            if (shader.IsBuiltIn == false)
            {
                shader.ApplyFieldsAsUniforms(rc);
            }

            return MVP;
        }

        //TODO: refactor Shader code 
        public void IncludeDefaultShader(string vertexShaderSource, string fragmentShaderSource)
        {
            CurrentShader = ShaderCompiler.ApplyShader(vertexShaderSource, fragmentShaderSource);

            //IncludeComposedShader(CurrentShader);
        }

        public void IncludeComposedShader(ComposedShader shader)
        {
            shader.Link();
            shader.Use();

            CurrentShader = shader;

            RefreshDefaultUniforms(shader);

            if (shader.IsTessellator)
            {
                RefreshTessUniforms(shader);
            }

            ComposedShaders.Add(shader);
        }

        public void IncludeTesselationShaders(string tessControlShaderSource, string tessEvalShaderSource,
                                              string geometryShaderSource)
        {
            CurrentShader = ShaderCompiler.ApplyShader(DefaultShader.vertexShaderSource,
                                                       DefaultShader.fragmentShaderSource,
                                                       tessControlShaderSource,
                                                       tessEvalShaderSource,
                                                       geometryShaderSource);



            //IncludeComposedShader(CurrentShader);


        }

        public void RefreshTessUniforms(ComposedShader shader = null)
        {
            if (shader == null) shader = CurrentShader;
            if (shader.HasErrors) return;

            Uniforms.Modelview = GL.GetUniformLocation(shader.ShaderHandle, "modelview");
            Uniforms.Projection = GL.GetUniformLocation(shader.ShaderHandle, "projection");
            Uniforms.NormalMatrix = GL.GetUniformLocation(shader.ShaderHandle, "normalmatrix");
            Uniforms.LightPosition = GL.GetUniformLocation(shader.ShaderHandle, "LightPosition");
            Uniforms.AmbientMaterial = GL.GetUniformLocation(shader.ShaderHandle, "AmbientMaterial");
            Uniforms.DiffuseMaterial = GL.GetUniformLocation(shader.ShaderHandle, "DiffuseMaterial");
            //Uniforms.TessLevelInner = GL.GetUniformLocation(shader.ShaderHandle, "TessLevelInner");
            //Uniforms.TessLevelOuter = GL.GetUniformLocation(shader.ShaderHandle, "TessLevelOuter");
        }

        public void RefreshDefaultUniforms(ComposedShader shader = null)
        {
            if (shader == null) shader = CurrentShader;
            if (shader.HasErrors) return;

            //uniforms.a_position = GL.GetAttribLocation(shader.ShaderHandle, "position");
            //uniforms.a_normal = GL.GetAttribLocation(shader.ShaderHandle, "normal");
            //uniforms.a_color = GL.GetAttribLocation(shader.ShaderHandle, "color");
            //uniforms.a_texcoord = GL.GetAttribLocation(shader.ShaderHandle, "texcoord");

            uniforms.a_coloringEnabled = GL.GetUniformLocation(shader.ShaderHandle, "coloringEnabled");
            uniforms.a_texturingEnabled = GL.GetUniformLocation(shader.ShaderHandle, "texturingEnabled");
            uniforms.sampler = GL.GetUniformLocation(shader.ShaderHandle, "_MainTex");
        }

        public Vector3 GetPosition(RenderingContext rc)
        {
            Vector3 pos = Vector3.Zero;
            Vector3 center = Vector3.Zero;
            Matrix4 model; // applied transformation hierarchy
            Matrix4 modelview;
            List<Transform> transformationHierarchy;
            Vector3 x3dScale;

            transformationHierarchy = 
                AscendantByType<Transform>()
                .Select(t => (Transform)t)
                .Where(t => t.Hidden == false)
                .ToList();

            modelview = Matrix4.Identity;

            x3dScale = new Vector3(0.06f, 0.06f, 0.06f); // scaling down to conform with X3D standard (note this was done manually and might need tweaking)

            foreach (Transform transform in transformationHierarchy)
            {
                modelview *= SceneEntity.ApplyX3DTransform(Vector3.Zero,
                                                     Vector3.Zero,
                                                     transform.Scale,
                                                     Vector3.Zero,
                                                     transform.Translation * x3dScale,
                                                     modelview);
                //modelview *= Matrix4.CreateTranslation(transform.Translation * x3dScale);
            }



            model = modelview;

            Matrix4 cameraTransl = rc.cam.GetModelTranslation();

            Matrix4 MVP = ((model));


            center = modelview.ExtractTranslation(); // position is from the center of the geometry


            pos = center;

            return pos;
        }

        #endregion

        #region Render Methods

        public override void Load()
        {
            base.Load();


            //Geometry(); //TODO: cant access children
        }

        private void Geometry(RenderingContext rc)
        {
            if (geometry == null)
            {
                geometry = (X3DGeometryNode)this.Children.First(n => (typeof(X3DGeometryNode)).IsInstanceOfType(n));
            }

            if (geometry != null)
            {
                //TODO: refactor geometry interleaving code and make it callable here

                //TODO: should then be able to calculate bounding boxes of arbitrary geometry here
                
                geometry.CollectGeometry(rc, out _handle, out bbox, out coloring, out texturing);


                // BUFFER GEOMETRY
                if (_handle.NumVerticies3 > 0)
                {
                    // use CurrentShader
                }


                if (_handle.NumVerticies4 > 0)
                {
                    quadShader = ShaderCompiler.CreateNewInstance(CurrentShader, true);
                }

                loadedGeometry = _handle.HasGeometry;

                if (drawBoundingBox)
                {
                    bbox.EnableRendering(GetPosition(rc));
                }
            }
        }

        public override void PreRenderOnce(RenderingContext rc)
        {
            base.PreRenderOnce(rc);

            if (typeof(Text).IsInstanceOfType(geometry))
            {

                // 2D Text Shader, used to apply transparancy. Since alpha blending didnt work transparent background
                IncludeDefaultShader(ColorReplaceShader.vertexShaderSource,
                                     ColorReplaceShader.fragmentShaderSource);
            }
            else if (typeof(Sphere).IsInstanceOfType(geometry))
            {
                // TESSELLATION
                IncludeTesselationShaders(TriangleTessShader.tessControlShader,
                                          TriangleTessShader.tessEvalShader,
                                          TriangleTessShader.geometryShaderSource);

                //CurrentShader.SetFieldValue("spherical", 1);
            }
            else
            {
                CurrentShader = ShaderCompiler.BuildDefaultShader();
            }

            if (ComposedShaders.Any())
            {
                CurrentShader = ComposedShaders.First();
            }

            CurrentShader.Link();
            CurrentShader.Use();

            CollectMaterials();
            RefreshDefaultUniforms();

            Geometry(rc);
        }

        public override void PreRender()
        {
            base.PreRender();

            texturingEnabled = GL.IsEnabled(EnableCap.Texture2D);

            shaders = this.DecendantsByType(typeof(X3DShaderNode)).Select(n => (X3DShaderNode)n).ToList();
            hasShaders = shaders.Any();
        }

        private void ApplyAppearance(RenderingContext rc)
        {
            if(appearance == null)
            {
                appearance = (X3DAppearanceNode)this.Children.FirstOrDefault(n => (typeof(X3DAppearanceNode)).IsInstanceOfType(n));
            }

            Appearance ap = (Appearance)appearance;


            ApplyMaterials(CurrentShader); // apply materials should really be done once at load but havent figured it out fully

            
            this.DecendantsByType<ImageTexture>().ForEach(TeXtUrE => TeXtUrE.Bind());

            if (typeof(Text).IsInstanceOfType(geometry))
            {
                Text txt = (Text)geometry;
                txt.BindTextures(rc);
                //CurrentShader.SetFieldValue("threshold", new Vector4(0.1f, 0.1f, 0.1f, 1.0f));
            }
        }

        public override void Render(RenderingContext rc)
        {
            base.Render(rc);

            rc.PushMatricies();

            //if (!loadedGeometry) return;

            // PREPARE shape for rendering
            
            NormalMatrix = new Matrix3(rc.matricies.modelview); // NormalMatrix = M4GetUpper3x3(ModelviewMatrix);

            var shader = CurrentShader;

            if (shader != null)
            {
               // CurrentShader = linkedShaders;


                shader.Use();

                if (shader.IsTessellator)
                {
                    if (shader.IsBuiltIn)
                    {
                        // its a built in system shader so we are using the the fixed parameter inbuilt tesselator
                        CurrentShader.SetFieldValue("TessLevelInner", this.tessLevelInner);
                        CurrentShader.SetFieldValue("TessLevelOuter", this.tessLevelOuter);


                    }
                }

                shader.SetFieldValue("lightingEnabled", 1);
                shader.SetFieldValue("headlightEnabled", 0);
                shader.SetFieldValue("calib1", rc.cam.calibTrans);
                shader.SetFieldValue("calib2", rc.cam.calibOrient);

                if (depthMask)
                {
                    Matrix4 MVP = ApplyGeometricTransformations(rc, shader, this);
                    Vector3 lookat = QuaternionExtensions.Rotate(rc.cam.Orientation, Vector3.UnitZ);
                    Vector3 forward = new Vector3(lookat.X, 0, lookat.Z).Normalized();
                    Vector3 up = Vector3.UnitY;
                    Vector3 left = up.Cross(forward);

                    Vector2 orient;
                    Vector3 position;

                    orient = QuaternionExtensions.ExtractPitchYawRoll(rc.cam.Orientation.Inverted()).Xy; // pitch and yaw only
                    position = rc.cam.Position;

                    shader.SetFieldValue("headlightEnabled", NavigationInfo.HeadlightEnabled ? 1 : 0);
                    shader.SetFieldValue("sceneCameraPosition", position);
                    shader.SetFieldValue("model", ref MVP);
                    shader.SetFieldValue("orientation", orient);
                    shader.SetFieldValue("lookat", rc.cam.Direction);
                    shader.SetFieldValue("forward", forward);
                    shader.SetFieldValue("up", up);
                    shader.SetFieldValue("left", left);

                }
                else
                {
                    //REFACTOR!!

                    RefreshDefaultUniforms(shader);

                    if (shader.IsTessellator)
                        RefreshTessUniforms(shader);

                    //Matrix4 MVP = rc.cam.GetWorldOrientation() ;

                    //shader.SetFieldValue("modelview", ref MVP);

                    //shader.SetFieldValue("modelview", ref MVP); //GL.UniformMatrix4(uniformModelview, false, ref rc.matricies.modelview);
                    shader.SetFieldValue("projection", ref rc.matricies.projection);
                    shader.SetFieldValue("camscale", rc.cam.Scale.X); //GL.Uniform1(uniformCameraScale, rc.cam.Scale.X);
                    shader.SetFieldValue("X3DScale", rc.matricies.Scale); //GL.Uniform3(uniformX3DScale, rc.matricies.Scale);
                    shader.SetFieldValue("coloringEnabled", coloring ? 1 : 0);
                    shader.SetFieldValue("texturingEnabled", this.texturingEnabled ? 1 : 0); //GL.Uniform1(uniforms.a_texturingEnabled, this.texturingEnabled ? 1 : 0);
                    shader.SetFieldValue("normalmatrix", ref NormalMatrix);

                }
                
                ApplyAppearance(rc);

                // RENDER shape
                RenderShape(rc);

            }

            if(drawBoundingBox)
                RenderBoundingBox(rc);
        }

        private void RenderBoundingBox(RenderingContext rc)
        {
            rc.PushMatricies();

            bbox.Render(this, rc);

            rc.PopMatricies();
        }

        private void RenderShape(RenderingContext rc)
        {
            // Refactor tessellation 
            var shader = CurrentShader;

            if (shader != null)
            {

                shader.Use();

                shader.SetFieldValue("bboxMaxWidth", bbox.Width);
                shader.SetFieldValue("bboxMaxHeight", bbox.Height);
                shader.SetFieldValue("bboxMaxDepth", bbox.Depth);

                shader.SetFieldValue("bbox_x", bbox.Width);
                shader.SetFieldValue("bbox_y", bbox.Height);
                shader.SetFieldValue("bbox_z", bbox.Depth);
                shader.SetFieldValue("coloringEnabled", coloring ? 1 : 0);

                if (typeof(ElevationGrid).IsInstanceOfType(geometry))
                {
                    shader.SetFieldValue("lightingEnabled", 0);
                    shader.SetFieldValue("texturingEnabled", 1);
                }

                Vector3 tmp = rc.matricies.Scale;
                if (typeof(Text).IsInstanceOfType(geometry))
                {
                    rc.PushMatricies();
                    rc.matricies.Scale *= 200f;
                }

                if (depthMask == false)
                {
                    //REFACTOR!!
                    Matrix4 mat4 = Matrix4.Identity;
                    //Quaternion qRotFix = QuaternionExtensions.EulerToQuat(rc.cam.calibOrient.X, rc.cam.calibOrient.Y, rc.cam.calibOrient.Z);
                    //mat4 *= Matrix4.CreateTranslation(rc.cam.calibTrans) * Matrix4.CreateFromQuaternion(qRotFix);
                    Quaternion qRotFix = QuaternionExtensions.EulerToQuat(0.15f, 3.479997f, 0f);
                    mat4 *= Matrix4.CreateTranslation(new Vector3(0f, 0f, -0.29f)) * Matrix4.CreateFromQuaternion(qRotFix);
                    // test weapon/gun rendering fixed in front of player
                    //TODO: port this to X3D

                    shader.SetFieldValue("modelview", ref mat4);
                    if (quadShader != null) quadShader.SetFieldValue("modelview", ref mat4);
                    //GL.DepthMask(false);
                }

                shader.SetFieldValue("size", size);
                shader.SetFieldValue("scale", scale);

                if (loadedGeometry)
                {
                    if (shader.IsTessellator)
                    {
                        RenderTessellator(rc);
                    }
                    else
                    {
                        if (typeof(IndexedLineSet).IsInstanceOfType(geometry))
                        {
                            RenderLines(rc);
                        }
                        else
                        {
                            RenderTriangles(rc);
                            RenderQuads(rc);
                        }
                    }
                }


                if (depthMask == false)
                {
                    //GL.DepthMask(true);
                }

                if (typeof(Text).IsInstanceOfType(geometry))
                {
                    rc.PopMatricies();
                    rc.matricies.Scale = tmp;
                }

            }
        }

        private void RenderTessellator(RenderingContext rc)
        {
            //Vector4 lightPosition = new Vector4(0.25f, 0.25f, 1f, 0f);
            var shader = CurrentShader;

            //shader.SetFieldValue("normalmatrix", ref NormalMatrix);
            //GL.UniformMatrix3(parentShape.Uniforms.NormalMatrix, false, ref parentShape.NormalMatrix);
            //GL.Uniform3(Uniforms.LightPosition, 1, ref lightPosition.X);
            //GL.Uniform3(Uniforms.AmbientMaterial, X3DTypeConverters.ToVec3(OpenTK.Graphics.Color4.Aqua)); // 0.04f, 0.04f, 0.04f
            //GL.Uniform3(Uniforms.DiffuseMaterial, 0.0f, 0.75f, 0.75f);

            int PatchMatrix = GL.GetUniformLocation(CurrentShader.ShaderHandle, "B");
            int TransposedPatchMatrix = GL.GetUniformLocation(CurrentShader.ShaderHandle, "BT");

            Matrix4 bezier = new Matrix4
                (-1, 3, -3, 1,
                3, -6, 3, 0,
                -3, 3, 0, 0,
                1, 0, 0, 0);

            GL.UniformMatrix4(PatchMatrix, false, ref bezier);
            GL.UniformMatrix4(TransposedPatchMatrix, true, ref bezier);

            if (_handle.NumVerticies3 > 0)
            {
                GL.UseProgram(shader.ShaderHandle);

                shader.SetFieldValue("size", size);
                shader.SetFieldValue("scale", scale);

                GL.PatchParameter(PatchParameterInt.PatchVertices, 3);
                GL.BindBuffer(BufferTarget.ArrayBuffer, _handle.vbo3);
                Buffering.ApplyBufferPointers(shader);
                GL.DrawArrays(PrimitiveType.Patches, 0, _handle.NumVerticies3);
            }


            if (_handle.NumVerticies4 > 0)
            {
                shader = quadShader;

                GL.UseProgram(shader.ShaderHandle);

                shader.SetFieldValue("size", size);
                shader.SetFieldValue("scale", scale);

                GL.PatchParameter(PatchParameterInt.PatchVertices, 16);
                GL.BindBuffer(BufferTarget.ArrayBuffer, _handle.vbo4);
                Buffering.ApplyBufferPointers(shader);
                GL.DrawArrays(PrimitiveType.Patches, 0, _handle.NumVerticies4);
            }
        }


        private void RenderLines(RenderingContext rc)
        {
            if (_handle.NumVerticies3 > 0)
            {
                GL.UseProgram(CurrentShader.ShaderHandle);

                CurrentShader.SetFieldValue("lightingEnabled", 0);
                CurrentShader.SetFieldValue("headlightEnabled", 0);

                CurrentShader.SetFieldValue("size", size);
                CurrentShader.SetFieldValue("scale", scale);

                GL.LineWidth(8.0f); // todo: LineProperties

                IndexedLineSet ils = (IndexedLineSet)geometry;

                GL.BindBuffer(BufferTarget.ArrayBuffer, _handle.vbo3);
                Buffering.ApplyBufferPointers(CurrentShader);
                GL.DrawArrays(ils.PrimativeType, 0, _handle.NumVerticies3);
                

                GL.UseProgram(0);
                GL.LineWidth(1.0f);
            }
        }

        private void RenderTriangles(RenderingContext rc)
        {
            if (_handle.NumVerticies3 > 0)
            {
                GL.UseProgram(CurrentShader.ShaderHandle);

                CurrentShader.SetFieldValue("size", size);
                CurrentShader.SetFieldValue("scale", scale);

                GL.BindBuffer(BufferTarget.ArrayBuffer, _handle.vbo3);
                Buffering.ApplyBufferPointers(CurrentShader);
                GL.DrawArrays(PrimitiveType.Triangles, 0, _handle.NumVerticies3);

                GL.UseProgram(0);
            }
        }

        private void RenderQuads(RenderingContext rc)
        {
            if (_handle.NumVerticies4 > 0)
            {
                GL.UseProgram(quadShader.ShaderHandle);

                if (depthMask)
                {
                    ApplyGeometricTransformations(rc, quadShader, this);
                }
                else
                {
                    //REFACTOR!!
                    Matrix4 mat4 = Matrix4.Identity;
                    //Quaternion qRotFix = QuaternionExtensions.EulerToQuat(rc.cam.calibOrient.X, rc.cam.calibOrient.Y, rc.cam.calibOrient.Z);
                    //mat4 *= Matrix4.CreateTranslation(rc.cam.calibTrans) * Matrix4.CreateFromQuaternion(qRotFix);
                    Quaternion qRotFix = QuaternionExtensions.EulerToQuat(0.15f, 3.479997f, 0f);
                    mat4 *= Matrix4.CreateTranslation(new Vector3(0f, 0f, -0.29f)) * Matrix4.CreateFromQuaternion(qRotFix);

                    // test weapon/gun rendering fixed in front of player
                    //TODO: port this to X3D

                    quadShader.SetFieldValue("modelview", ref mat4);

                    RefreshDefaultUniforms(quadShader);

                    quadShader.SetFieldValue("bbox_x", bbox.Width);
                    quadShader.SetFieldValue("bbox_y", bbox.Height);
                    quadShader.SetFieldValue("bbox_z", bbox.Depth);
                    quadShader.SetFieldValue("projection", ref rc.matricies.projection);
                    quadShader.SetFieldValue("camscale", rc.cam.Scale.X); //GL.Uniform1(uniformCameraScale, rc.cam.Scale.X);
                    quadShader.SetFieldValue("X3DScale", rc.matricies.Scale); //GL.Uniform3(uniformX3DScale, rc.matricies.Scale);
                    quadShader.SetFieldValue("coloringEnabled", coloring ? 1 : 0);
                    quadShader.SetFieldValue("texturingEnabled", texturingEnabled ? 1 : 0); //GL.Uniform1(uniforms.a_texturingEnabled, this.texturingEnabled ? 1 : 0);

                }
                quadShader.SetFieldValue("size", size);
                quadShader.SetFieldValue("scale", scale);

                ApplyMaterials(quadShader);

                GL.BindBuffer(BufferTarget.ArrayBuffer, _handle.vbo4);
                Buffering.ApplyBufferPointers(quadShader);
                GL.DrawArrays(PrimitiveType.Quads, 0, _handle.NumVerticies4);

                GL.UseProgram(0);
            }
        }

        public override void PostRender(RenderingContext rc)
        {
            base.PostRender(rc);

            CurrentShader.Deactivate();

            this.DecendantsByType<ImageTexture>().ForEach(TeXtUrE => TeXtUrE.Deactivate());

            if (typeof(Text).IsInstanceOfType(geometry))
            {
                Text txt = (Text)geometry;
                txt.UnbindTextures(rc);
            }

            rc.PopMatricies();
        }

        #endregion
    }
}
