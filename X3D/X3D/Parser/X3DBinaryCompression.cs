﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using System.IO;
using X3D.Engine;
using System.IO.Compression;
using LiquidTechnologies.FastInfoset;
using System.Xml;
using System.Xml.Linq;

namespace X3D.Parser.X3DB
{
    /// <summary>
    /// X3D Binary Compression and Decompression using Web3D Fast Info Set Vocabulary.
    /// </summary>
    public class X3DBinaryCompression
    {

        #region Vocabulary Store

        private static FIExternalVocabulary vocab33 = CreateX3DBVocabulary(new Uri("urn:web3d:x3d:fi-vocabulary-3.3"));
        private static FIExternalVocabulary vocab32 = CreateX3DBVocabulary(new Uri("urn:web3d:x3d:fi-vocabulary-3.2"));
        private static FIExternalVocabulary vocab31 = CreateX3DBVocabulary(new Uri("urn:web3d:x3d:fi-vocabulary-3.1"));
        private static FIExternalVocabulary vocab30 = CreateX3DBVocabulary(new Uri("urn:web3d:x3d:fi-vocabulary-3.0"));

        private static FIVocabularyManager vocabularyProvider = CreateVocabStore();

        #endregion

        #region Public Static Methods

        /// <summary>
        /// Decompresses an X3DB file direct-to SceneGraph
        /// </summary>
        /// <param name="x3db_stream"></param>
        /// <returns></returns>
        public static SceneGraph DecompressToSG(Stream x3db_stream)
        {
            //TODO: bypass decompression into XDocument, and Xml String, and instead write direct to Scene Graph
            //TODO: to support this loader optimisation, we need to implement an XMLReader that generates a SceneGraph.

            throw new NotImplementedException("not yet implemented");
        }

        public static SceneManager FromStream(Stream x3db_stream)
        {
            string xml;
            SceneManager s;

            xml = DecompressToXML(x3db_stream);

            s = SceneManager.fromString(xml, X3DMIMEType.X3D);
            
            return s;
        }

        /// <summary>
        /// Converts a Compressed X3D File (a .x3db file) to XML using a Fast Info Set provider.
        /// </summary>
        public static string DecompressToXML(Stream x3db_stream)
        {
            XDocument document;
            XmlReader fiReader;
            FIReader fastInfosetReader;
            string xml;

            x3db_stream.Position = 0;

            fastInfosetReader = new FIReader(x3db_stream, vocabularyProvider);

            fiReader = XmlReader.Create(fastInfosetReader, null);
            document = XDocument.Load(fiReader);
            fiReader.Close();

            xml = document.ToString();

            return xml;
        }

        /// <summary>
        /// Compresses XML representation of an X3D Scene into a corresponding X3D Binary Encoded format Fast Info Set.
        /// </summary>
        /// <param name="xml_string"></param>
        /// <returns></returns>
        public static Stream CompressToX3DB(string xml_string)
        {
            XDocument document;
            XmlWriter fiWriter;
            FIWriter fastInfosetWriter;
            MemoryStream x3dbStream;

            x3dbStream = new MemoryStream();

            using (XmlTextReader xtr = new XmlTextReader(new StringReader(xml_string)))
            {
                xtr.WhitespaceHandling = WhitespaceHandling.All; // preserve line endings in attribute values
                                                                 //xtr.Normalization = false; // another way to preserve line endings in attribute values
                                                                 // however turning off normalization makes processing much slower
                document = XDocument.Load(xtr, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo);
            }

            fastInfosetWriter = new FIWriter(x3dbStream, vocab33);

            fiWriter = XmlWriter.Create(fastInfosetWriter);
            document.WriteTo(fiWriter);
            fiWriter.Close();

            x3dbStream.Position = 0;

            return x3dbStream;
        }

        #endregion

        #region Private Static Methods

        /// <summary>
        /// Creates a Vocabulary Store using same vocab (X3D Version 3.3) backing all previous versions 3.0 - 3.3.
        /// </summary>
        private static FIVocabularyManager CreateVocabStore()
        {
            FIVocabularyManager vocabularyProvider;

            vocabularyProvider = new FIVocabularyManager();
            vocabularyProvider.AddVocabulary(vocab33);
            vocabularyProvider.AddVocabulary(vocab32);
            vocabularyProvider.AddVocabulary(vocab31);
            vocabularyProvider.AddVocabulary(vocab30);

            return vocabularyProvider;
        }

        /// <summary>
        /// http://www.web3d.org/documents/specifications/19776-3/V3.2/Part03/tables.html#Description
        /// http://www.web3d.org/documents/specifications/19776-3/V3.2/Part03/tables.html#t-ElementNameTableInitialValues
        /// http://www.web3d.org/documents/specifications/19776-3/V3.3/Part03/tables.html#t-AttributeNameTableInitialValues
        /// </summary>
        private static FIExternalVocabulary CreateX3DBVocabulary(Uri uri)
        {
            FIExternalVocabulary vocab = new FIExternalVocabulary(uri);

            // X3D Elements
            vocab.AddElementNameSurrogate("Shape");
            vocab.AddElementNameSurrogate("Appearance");
            vocab.AddElementNameSurrogate("Material");
            vocab.AddElementNameSurrogate("IndexedFaceSet");
            vocab.AddElementNameSurrogate("ProtoInstance");
            vocab.AddElementNameSurrogate("Transform");
            vocab.AddElementNameSurrogate("ImageTexture");
            vocab.AddElementNameSurrogate("TextureTransform");
            vocab.AddElementNameSurrogate("Coordinate");
            vocab.AddElementNameSurrogate("Normal");
            vocab.AddElementNameSurrogate("Color");
            vocab.AddElementNameSurrogate("ColorRGBA");
            vocab.AddElementNameSurrogate("TextureCoordinate");
            vocab.AddElementNameSurrogate("ROUTE");
            vocab.AddElementNameSurrogate("fieldValue");
            vocab.AddElementNameSurrogate("Group");
            vocab.AddElementNameSurrogate("LOD");
            vocab.AddElementNameSurrogate("Switch");
            vocab.AddElementNameSurrogate("Script");
            vocab.AddElementNameSurrogate("IndexedTriangleFanSet");
            vocab.AddElementNameSurrogate("IndexedTriangleSet");
            vocab.AddElementNameSurrogate("IndexedTriangleStripSet");
            vocab.AddElementNameSurrogate("MultiTexture");
            vocab.AddElementNameSurrogate("MultiTextureCoordinate");
            vocab.AddElementNameSurrogate("MultiTextureTransform");
            vocab.AddElementNameSurrogate("IndexedLineSet");
            vocab.AddElementNameSurrogate("PointSet");
            vocab.AddElementNameSurrogate("StaticGroup");
            vocab.AddElementNameSurrogate("Sphere");
            vocab.AddElementNameSurrogate("Box");
            vocab.AddElementNameSurrogate("Cone");
            vocab.AddElementNameSurrogate("Anchor");
            vocab.AddElementNameSurrogate("Arc2D");
            vocab.AddElementNameSurrogate("ArcClose2D");
            vocab.AddElementNameSurrogate("AudioClip");
            vocab.AddElementNameSurrogate("Background");
            vocab.AddElementNameSurrogate("Billboard");
            vocab.AddElementNameSurrogate("BooleanFilter");
            vocab.AddElementNameSurrogate("BooleanSequencer");
            vocab.AddElementNameSurrogate("BooleanToggle");
            vocab.AddElementNameSurrogate("BooleanTrigger");
            vocab.AddElementNameSurrogate("Circle2D");
            vocab.AddElementNameSurrogate("Collision");
            vocab.AddElementNameSurrogate("ColorInterpolator");
            vocab.AddElementNameSurrogate("Contour2D");
            vocab.AddElementNameSurrogate("ContourPolyline2D");
            vocab.AddElementNameSurrogate("CoordinateDouble");
            vocab.AddElementNameSurrogate("CoordinateInterpolator");
            vocab.AddElementNameSurrogate("CoordinateInterpolator2D");
            vocab.AddElementNameSurrogate("Cylinder");
            vocab.AddElementNameSurrogate("CylinderSensor");
            vocab.AddElementNameSurrogate("DirectionalLight");
            vocab.AddElementNameSurrogate("Disk2D");
            vocab.AddElementNameSurrogate("EXPORT");
            vocab.AddElementNameSurrogate("ElevationGrid");
            vocab.AddElementNameSurrogate("EspduTransform");
            vocab.AddElementNameSurrogate("ExternProtoDeclare");
            vocab.AddElementNameSurrogate("Extrusion");
            vocab.AddElementNameSurrogate("FillProperties");
            vocab.AddElementNameSurrogate("Fog");
            vocab.AddElementNameSurrogate("FontStyle");
            vocab.AddElementNameSurrogate("GeoCoordinate");
            vocab.AddElementNameSurrogate("GeoElevationGrid");
            vocab.AddElementNameSurrogate("GeoLOD");
            vocab.AddElementNameSurrogate("GeoLocation");
            vocab.AddElementNameSurrogate("GeoMetadata");
            vocab.AddElementNameSurrogate("GeoOrigin");
            vocab.AddElementNameSurrogate("GeoPositionInterpolator");
            vocab.AddElementNameSurrogate("GeoTouchSensor");
            vocab.AddElementNameSurrogate("GeoViewpoint");
            vocab.AddElementNameSurrogate("HAnimDisplacer");
            vocab.AddElementNameSurrogate("HAnimHumanoid");
            vocab.AddElementNameSurrogate("HAnimJoint");
            vocab.AddElementNameSurrogate("HAnimSegment");
            vocab.AddElementNameSurrogate("HAnimSite");
            vocab.AddElementNameSurrogate("IMPORT");
            vocab.AddElementNameSurrogate("IS");
            vocab.AddElementNameSurrogate("Inline");
            vocab.AddElementNameSurrogate("IntegerSequencer");
            vocab.AddElementNameSurrogate("IntegerTrigger");
            vocab.AddElementNameSurrogate("KeySensor");
            vocab.AddElementNameSurrogate("LineProperties");
            vocab.AddElementNameSurrogate("LineSet");
            vocab.AddElementNameSurrogate("LoadSensor");
            vocab.AddElementNameSurrogate("MetadataDouble");
            vocab.AddElementNameSurrogate("MetadataFloat");
            vocab.AddElementNameSurrogate("MetadataInteger");
            vocab.AddElementNameSurrogate("MetadataSet");
            vocab.AddElementNameSurrogate("MetadataString");
            vocab.AddElementNameSurrogate("MovieTexture");
            vocab.AddElementNameSurrogate("NavigationInfo");
            vocab.AddElementNameSurrogate("NormalInterpolator");
            vocab.AddElementNameSurrogate("NurbsCurve");
            vocab.AddElementNameSurrogate("NurbsCurve2D");
            vocab.AddElementNameSurrogate("NurbsOrientationInterpolator");
            vocab.AddElementNameSurrogate("NurbsPatchSurface");
            vocab.AddElementNameSurrogate("NurbsPositionInterpolator");
            vocab.AddElementNameSurrogate("NurbsSet");
            vocab.AddElementNameSurrogate("NurbsSurfaceInterpolator");
            vocab.AddElementNameSurrogate("NurbsSweptSurface");
            vocab.AddElementNameSurrogate("NurbsSwungSurface");
            vocab.AddElementNameSurrogate("NurbsTextureCoordinate");
            vocab.AddElementNameSurrogate("NurbsTrimmedSurface");
            vocab.AddElementNameSurrogate("OrientationInterpolator");
            vocab.AddElementNameSurrogate("PixelTexture");
            vocab.AddElementNameSurrogate("PlaneSensor");
            vocab.AddElementNameSurrogate("PointLight");
            vocab.AddElementNameSurrogate("Polyline2D");
            vocab.AddElementNameSurrogate("Polypoint2D");
            vocab.AddElementNameSurrogate("PositionInterpolator");
            vocab.AddElementNameSurrogate("PositionInterpolator2D");
            vocab.AddElementNameSurrogate("ProtoBody");
            vocab.AddElementNameSurrogate("ProtoDeclare");
            vocab.AddElementNameSurrogate("ProtoInterface");
            vocab.AddElementNameSurrogate("ProximitySensor");
            vocab.AddElementNameSurrogate("ReceiverPdu");
            vocab.AddElementNameSurrogate("Rectangle2D");
            vocab.AddElementNameSurrogate("ScalarInterpolator");
            vocab.AddElementNameSurrogate("Scene");
            vocab.AddElementNameSurrogate("SignalPdu");
            vocab.AddElementNameSurrogate("Sound");
            vocab.AddElementNameSurrogate("SphereSensor");
            vocab.AddElementNameSurrogate("SpotLight");
            vocab.AddElementNameSurrogate("StringSensor");
            vocab.AddElementNameSurrogate("Text");
            vocab.AddElementNameSurrogate("TextureBackground");
            vocab.AddElementNameSurrogate("TextureCoordinateGenerator");
            vocab.AddElementNameSurrogate("TimeSensor");
            vocab.AddElementNameSurrogate("TimeTrigger");
            vocab.AddElementNameSurrogate("TouchSensor");
            vocab.AddElementNameSurrogate("TransmitterPdu");
            vocab.AddElementNameSurrogate("TriangleFanSet");
            vocab.AddElementNameSurrogate("TriangleSet");
            vocab.AddElementNameSurrogate("TriangleSet2D");
            vocab.AddElementNameSurrogate("TriangleStripSet");
            vocab.AddElementNameSurrogate("Viewpoint");
            vocab.AddElementNameSurrogate("VisibilitySensor");
            vocab.AddElementNameSurrogate("WorldInfo");
            vocab.AddElementNameSurrogate("X3D");
            vocab.AddElementNameSurrogate("component");
            vocab.AddElementNameSurrogate("connect");
            vocab.AddElementNameSurrogate("field");
            vocab.AddElementNameSurrogate("head");
            vocab.AddElementNameSurrogate("humanoidBodyType");
            vocab.AddElementNameSurrogate("meta");
            vocab.AddElementNameSurrogate("CADAssembly");
            vocab.AddElementNameSurrogate("CADFace");
            vocab.AddElementNameSurrogate("CADLayer");
            vocab.AddElementNameSurrogate("CADPart");
            vocab.AddElementNameSurrogate("ComposedCubeMapTexture");
            vocab.AddElementNameSurrogate("ComposedShader");
            vocab.AddElementNameSurrogate("ComposedTexture3D");
            vocab.AddElementNameSurrogate("FloatVertexAttribute");
            vocab.AddElementNameSurrogate("FogCoordinate");
            vocab.AddElementNameSurrogate("GeneratedCubeMapTexture");
            vocab.AddElementNameSurrogate("ImageCubeMapTexture");
            vocab.AddElementNameSurrogate("ImageTexture3D");
            vocab.AddElementNameSurrogate("IndexedQuadSet");
            vocab.AddElementNameSurrogate("LocalFog");
            vocab.AddElementNameSurrogate("Matrix3VertexAttribute");
            vocab.AddElementNameSurrogate("Matrix4VertexAttribute");
            vocab.AddElementNameSurrogate("PackagedShader");
            vocab.AddElementNameSurrogate("PixelTexture3D");
            vocab.AddElementNameSurrogate("ProgramShader");
            vocab.AddElementNameSurrogate("QuadSet");
            vocab.AddElementNameSurrogate("ShaderPart");
            vocab.AddElementNameSurrogate("ShaderProgram");
            vocab.AddElementNameSurrogate("TextureCoordinate3D");
            vocab.AddElementNameSurrogate("TextureCoordinate4D");
            vocab.AddElementNameSurrogate("TextureTransform3D");
            vocab.AddElementNameSurrogate("TextureTransformMatrix3D");
            vocab.AddElementNameSurrogate("BallJoint");
            vocab.AddElementNameSurrogate("BoundedPhysicsModel");
            vocab.AddElementNameSurrogate("ClipPlane");
            vocab.AddElementNameSurrogate("CollidableOffset");
            vocab.AddElementNameSurrogate("CollidableShape");
            vocab.AddElementNameSurrogate("CollisionCollection");
            vocab.AddElementNameSurrogate("CollisionSensor");
            vocab.AddElementNameSurrogate("CollisionSpace");
            vocab.AddElementNameSurrogate("ColorDamper");
            vocab.AddElementNameSurrogate("ConeEmitter");
            vocab.AddElementNameSurrogate("Contact");
            vocab.AddElementNameSurrogate("CoordinateDamper");
            vocab.AddElementNameSurrogate("DISEntityManager");
            vocab.AddElementNameSurrogate("DISEntityTypeMapping");
            vocab.AddElementNameSurrogate("DoubleAxisHingeJoint");
            vocab.AddElementNameSurrogate("EaseInEaseOut");
            vocab.AddElementNameSurrogate("ExplosionEmitter");
            vocab.AddElementNameSurrogate("ForcePhysicsModel");
            vocab.AddElementNameSurrogate("GeoProximitySensor");
            vocab.AddElementNameSurrogate("GeoTransform");
            vocab.AddElementNameSurrogate("Layer");
            vocab.AddElementNameSurrogate("LayerSet");
            vocab.AddElementNameSurrogate("Layout");
            vocab.AddElementNameSurrogate("LayoutGroup");
            vocab.AddElementNameSurrogate("LayoutLayer");
            vocab.AddElementNameSurrogate("LinePickSensor");
            vocab.AddElementNameSurrogate("MotorJoint");
            vocab.AddElementNameSurrogate("OrientationChaser");
            vocab.AddElementNameSurrogate("OrientationDamper");
            vocab.AddElementNameSurrogate("OrthoViewpoint");
            vocab.AddElementNameSurrogate("ParticleSystem");
            vocab.AddElementNameSurrogate("PickableGroup");
            vocab.AddElementNameSurrogate("PointEmitter");
            vocab.AddElementNameSurrogate("PointPickSensor");
            vocab.AddElementNameSurrogate("PolylineEmitter");
            vocab.AddElementNameSurrogate("PositionChaser");
            vocab.AddElementNameSurrogate("PositionChaser2D");
            vocab.AddElementNameSurrogate("PositionDamper");
            vocab.AddElementNameSurrogate("PositionDamper2D");
            vocab.AddElementNameSurrogate("PrimitivePickSensor");
            vocab.AddElementNameSurrogate("RigidBody");
            vocab.AddElementNameSurrogate("RigidBodyCollection");
            vocab.AddElementNameSurrogate("ScalarChaser");
            vocab.AddElementNameSurrogate("ScreenFontStyle");
            vocab.AddElementNameSurrogate("ScreenGroup");
            vocab.AddElementNameSurrogate("SingleAxisHingeJoint");
            vocab.AddElementNameSurrogate("SliderJoint");
            vocab.AddElementNameSurrogate("SplinePositionInterpolator");
            vocab.AddElementNameSurrogate("SplinePositionInterpolator2D");
            vocab.AddElementNameSurrogate("SplineScalarInterpolator");
            vocab.AddElementNameSurrogate("SquadOrientationInterpolator");
            vocab.AddElementNameSurrogate("SurfaceEmitter");
            vocab.AddElementNameSurrogate("TexCoordDamper2D");
            vocab.AddElementNameSurrogate("TextureProperties");
            vocab.AddElementNameSurrogate("TransformSensor");
            vocab.AddElementNameSurrogate("TwoSidedMaterial");
            vocab.AddElementNameSurrogate("UniversalJoint");
            vocab.AddElementNameSurrogate("ViewpointGroup");
            vocab.AddElementNameSurrogate("Viewport");
            vocab.AddElementNameSurrogate("VolumeEmitter");
            vocab.AddElementNameSurrogate("VolumePickSensor");
            vocab.AddElementNameSurrogate("WindPhysicsModel");
            vocab.AddElementNameSurrogate("BlendedVolumeStyle");
            vocab.AddElementNameSurrogate("BoundaryEnhancementVolumeStyle");
            vocab.AddElementNameSurrogate("CartoonVolumeStyle");
            vocab.AddElementNameSurrogate("ComposedVolumeStyle");
            vocab.AddElementNameSurrogate("EdgeEnhancementVolumeStyle");
            vocab.AddElementNameSurrogate("IsoSurfaceVolumeData");
            vocab.AddElementNameSurrogate("MetadataBoolean");
            vocab.AddElementNameSurrogate("OpacityMapVolumeStyle");
            vocab.AddElementNameSurrogate("ProjectionVolumeStyle");
            vocab.AddElementNameSurrogate("SegmentedVolumeData");
            vocab.AddElementNameSurrogate("ShadedVolumeStyle");
            vocab.AddElementNameSurrogate("SilhouetteEnhancementVolumeStyle");
            vocab.AddElementNameSurrogate("ToneMappedVolumeStyle");
            vocab.AddElementNameSurrogate("VolumeData");
            vocab.AddElementNameSurrogate("ColorChaser");
            vocab.AddElementNameSurrogate("CoordinateChaser");
            vocab.AddElementNameSurrogate("ScalarDamper");
            vocab.AddElementNameSurrogate("TexCoordChaser2D");
            vocab.AddElementNameSurrogate("unit");

            // X3D Attributes
            vocab.AddAttributeNameSurrogate("DEF");
            vocab.AddAttributeNameSurrogate("USE");
            vocab.AddAttributeNameSurrogate("containerField");
            vocab.AddAttributeNameSurrogate("fromNode");
            vocab.AddAttributeNameSurrogate("fromField");
            vocab.AddAttributeNameSurrogate("toNode");
            vocab.AddAttributeNameSurrogate("toField");
            vocab.AddAttributeNameSurrogate("name");
            vocab.AddAttributeNameSurrogate("value");
            vocab.AddAttributeNameSurrogate("color");
            vocab.AddAttributeNameSurrogate("colorIndex");
            vocab.AddAttributeNameSurrogate("coordIndex");
            vocab.AddAttributeNameSurrogate("texCoordIndex");
            vocab.AddAttributeNameSurrogate("normalIndex");
            vocab.AddAttributeNameSurrogate("colorPerVertex");
            vocab.AddAttributeNameSurrogate("normalPerVertex");
            vocab.AddAttributeNameSurrogate("rotation");
            vocab.AddAttributeNameSurrogate("scale");
            vocab.AddAttributeNameSurrogate("center");
            vocab.AddAttributeNameSurrogate("scaleOrientation");
            vocab.AddAttributeNameSurrogate("translation");
            vocab.AddAttributeNameSurrogate("url");
            vocab.AddAttributeNameSurrogate("repeatS");
            vocab.AddAttributeNameSurrogate("repeatT");
            vocab.AddAttributeNameSurrogate("point");
            vocab.AddAttributeNameSurrogate("vector");
            vocab.AddAttributeNameSurrogate("range");
            vocab.AddAttributeNameSurrogate("ambientIntensity");
            vocab.AddAttributeNameSurrogate("diffuseColor");
            vocab.AddAttributeNameSurrogate("emissiveColor");
            vocab.AddAttributeNameSurrogate("shininess");
            vocab.AddAttributeNameSurrogate("specularColor");
            vocab.AddAttributeNameSurrogate("transparency");
            vocab.AddAttributeNameSurrogate("whichChoice");
            vocab.AddAttributeNameSurrogate("index");
            vocab.AddAttributeNameSurrogate("mode");
            vocab.AddAttributeNameSurrogate("source");
            vocab.AddAttributeNameSurrogate("function");
            vocab.AddAttributeNameSurrogate("alpha");
            vocab.AddAttributeNameSurrogate("vertexCount");
            vocab.AddAttributeNameSurrogate("radius");
            vocab.AddAttributeNameSurrogate("size");
            vocab.AddAttributeNameSurrogate("height");
            vocab.AddAttributeNameSurrogate("solid");
            vocab.AddAttributeNameSurrogate("ccw");
            vocab.AddAttributeNameSurrogate("key");
            vocab.AddAttributeNameSurrogate("keyValue");
            vocab.AddAttributeNameSurrogate("enabled");
            vocab.AddAttributeNameSurrogate("direction");
            vocab.AddAttributeNameSurrogate("position");
            vocab.AddAttributeNameSurrogate("orientation");
            vocab.AddAttributeNameSurrogate("bboxCenter");
            vocab.AddAttributeNameSurrogate("bboxSize");
            vocab.AddAttributeNameSurrogate("AS");
            vocab.AddAttributeNameSurrogate("InlineDEF");
            vocab.AddAttributeNameSurrogate("accessType");
            vocab.AddAttributeNameSurrogate("actionKeyPress");
            vocab.AddAttributeNameSurrogate("actionKeyRelease");
            vocab.AddAttributeNameSurrogate("address");
            vocab.AddAttributeNameSurrogate("altKey");
            vocab.AddAttributeNameSurrogate("antennaLocation");
            vocab.AddAttributeNameSurrogate("antennaPatternLength");
            vocab.AddAttributeNameSurrogate("antennaPatternType");
            vocab.AddAttributeNameSurrogate("applicationID");
            vocab.AddAttributeNameSurrogate("articulationParameterArray");
            vocab.AddAttributeNameSurrogate("articulationParameterChangeIndicatorArray");
            vocab.AddAttributeNameSurrogate("articulationParameterCount");
            vocab.AddAttributeNameSurrogate("articulationParameterDesignatorArray");
            vocab.AddAttributeNameSurrogate("articulationParameterIdPartAttachedArray");
            vocab.AddAttributeNameSurrogate("articulationParameterTypeArray");
            vocab.AddAttributeNameSurrogate("attenuation");
            vocab.AddAttributeNameSurrogate("autoOffset");
            vocab.AddAttributeNameSurrogate("avatarSize");
            vocab.AddAttributeNameSurrogate("axisOfRotation");
            vocab.AddAttributeNameSurrogate("backUrl");
            vocab.AddAttributeNameSurrogate("beamWidth");
            vocab.AddAttributeNameSurrogate("beginCap");
            vocab.AddAttributeNameSurrogate("bindTime");
            vocab.AddAttributeNameSurrogate("bottom");
            vocab.AddAttributeNameSurrogate("bottomRadius");
            vocab.AddAttributeNameSurrogate("bottomUrl");
            vocab.AddAttributeNameSurrogate("centerOfMass");
            vocab.AddAttributeNameSurrogate("centerOfRotation");
            vocab.AddAttributeNameSurrogate("child1Url");
            vocab.AddAttributeNameSurrogate("child2Url");
            vocab.AddAttributeNameSurrogate("child3Url");
            vocab.AddAttributeNameSurrogate("child4Url");
            vocab.AddAttributeNameSurrogate("class");
            vocab.AddAttributeNameSurrogate("closureType");
            vocab.AddAttributeNameSurrogate("collideTime");
            vocab.AddAttributeNameSurrogate("content");
            vocab.AddAttributeNameSurrogate("controlKey");
            vocab.AddAttributeNameSurrogate("controlPoint");
            vocab.AddAttributeNameSurrogate("convex");
            vocab.AddAttributeNameSurrogate("coordinateSystem");
            vocab.AddAttributeNameSurrogate("copyright");
            vocab.AddAttributeNameSurrogate("creaseAngle");
            vocab.AddAttributeNameSurrogate("crossSection");
            vocab.AddAttributeNameSurrogate("cryptoKeyID");
            vocab.AddAttributeNameSurrogate("cryptoSystem");
            vocab.AddAttributeNameSurrogate("cutOffAngle");
            vocab.AddAttributeNameSurrogate("cycleInterval");
            vocab.AddAttributeNameSurrogate("cycleTime");
            vocab.AddAttributeNameSurrogate("data");
            vocab.AddAttributeNameSurrogate("dataFormat");
            vocab.AddAttributeNameSurrogate("dataLength");
            vocab.AddAttributeNameSurrogate("dataUrl");
            vocab.AddAttributeNameSurrogate("date");
            vocab.AddAttributeNameSurrogate("deadReckoning");
            vocab.AddAttributeNameSurrogate("deletionAllowed");
            vocab.AddAttributeNameSurrogate("description");
            vocab.AddAttributeNameSurrogate("detonateTime");
            vocab.AddAttributeNameSurrogate("dir");
            vocab.AddAttributeNameSurrogate("directOutput");
            vocab.AddAttributeNameSurrogate("diskAngle");
            vocab.AddAttributeNameSurrogate("displacements");
            vocab.AddAttributeNameSurrogate("documentation");
            vocab.AddAttributeNameSurrogate("elapsedTime");
            vocab.AddAttributeNameSurrogate("ellipsoid");
            vocab.AddAttributeNameSurrogate("encodingScheme");
            vocab.AddAttributeNameSurrogate("endAngle");
            vocab.AddAttributeNameSurrogate("endCap");
            vocab.AddAttributeNameSurrogate("enterTime");
            vocab.AddAttributeNameSurrogate("enteredText");
            vocab.AddAttributeNameSurrogate("entityCategory");
            vocab.AddAttributeNameSurrogate("entityCountry");
            vocab.AddAttributeNameSurrogate("entityDomain");
            vocab.AddAttributeNameSurrogate("entityExtra");
            vocab.AddAttributeNameSurrogate("entityID");
            vocab.AddAttributeNameSurrogate("entityKind");
            vocab.AddAttributeNameSurrogate("entitySpecific");
            vocab.AddAttributeNameSurrogate("entitySubCategory");
            vocab.AddAttributeNameSurrogate("exitTime");
            vocab.AddAttributeNameSurrogate("extent");
            vocab.AddAttributeNameSurrogate("family");
            vocab.AddAttributeNameSurrogate("fanCount");
            vocab.AddAttributeNameSurrogate("fieldOfView");
            vocab.AddAttributeNameSurrogate("filled");
            vocab.AddAttributeNameSurrogate("finalText");
            vocab.AddAttributeNameSurrogate("fireMissionIndex");
            vocab.AddAttributeNameSurrogate("fired1");
            vocab.AddAttributeNameSurrogate("fired2");
            vocab.AddAttributeNameSurrogate("firedTime");
            vocab.AddAttributeNameSurrogate("firingRange");
            vocab.AddAttributeNameSurrogate("firingRate");
            vocab.AddAttributeNameSurrogate("fogType");
            vocab.AddAttributeNameSurrogate("forceID");
            vocab.AddAttributeNameSurrogate("frequency");
            vocab.AddAttributeNameSurrogate("frontUrl");
            vocab.AddAttributeNameSurrogate("fuse");
            vocab.AddAttributeNameSurrogate("geoCoords");
            vocab.AddAttributeNameSurrogate("geoGridOrigin");
            vocab.AddAttributeNameSurrogate("geoSystem");
            vocab.AddAttributeNameSurrogate("groundAngle");
            vocab.AddAttributeNameSurrogate("groundColor");
            vocab.AddAttributeNameSurrogate("hatchColor");
            vocab.AddAttributeNameSurrogate("hatchStyle");
            vocab.AddAttributeNameSurrogate("hatched");
            vocab.AddAttributeNameSurrogate("headlight");
            vocab.AddAttributeNameSurrogate("horizontal");
            vocab.AddAttributeNameSurrogate("horizontalDatum");
            vocab.AddAttributeNameSurrogate("http-equiv");
            vocab.AddAttributeNameSurrogate("image");
            vocab.AddAttributeNameSurrogate("importedDEF");
            vocab.AddAttributeNameSurrogate("info");
            vocab.AddAttributeNameSurrogate("innerRadius");
            vocab.AddAttributeNameSurrogate("inputFalse");
            vocab.AddAttributeNameSurrogate("inputNegate");
            vocab.AddAttributeNameSurrogate("inputSource");
            vocab.AddAttributeNameSurrogate("inputTrue");
            vocab.AddAttributeNameSurrogate("integerKey");
            vocab.AddAttributeNameSurrogate("intensity");
            vocab.AddAttributeNameSurrogate("jump");
            vocab.AddAttributeNameSurrogate("justify");
            vocab.AddAttributeNameSurrogate("keyPress");
            vocab.AddAttributeNameSurrogate("keyRelease");
            vocab.AddAttributeNameSurrogate("knot");
            vocab.AddAttributeNameSurrogate("lang");
            vocab.AddAttributeNameSurrogate("language");
            vocab.AddAttributeNameSurrogate("leftToRight");
            vocab.AddAttributeNameSurrogate("leftUrl");
            vocab.AddAttributeNameSurrogate("length");
            vocab.AddAttributeNameSurrogate("lengthOfModulationParameters");
            vocab.AddAttributeNameSurrogate("level");
            vocab.AddAttributeNameSurrogate("limitOrientation");
            vocab.AddAttributeNameSurrogate("lineSegments");
            vocab.AddAttributeNameSurrogate("linearAcceleration");
            vocab.AddAttributeNameSurrogate("linearVelocity");
            vocab.AddAttributeNameSurrogate("linetype");
            vocab.AddAttributeNameSurrogate("linewidthScaleFactor");
            vocab.AddAttributeNameSurrogate("llimit");
            vocab.AddAttributeNameSurrogate("load");
            vocab.AddAttributeNameSurrogate("loadTime");
            vocab.AddAttributeNameSurrogate("localDEF");
            vocab.AddAttributeNameSurrogate("location");
            vocab.AddAttributeNameSurrogate("loop");
            vocab.AddAttributeNameSurrogate("marking");
            vocab.AddAttributeNameSurrogate("mass");
            vocab.AddAttributeNameSurrogate("maxAngle");
            vocab.AddAttributeNameSurrogate("maxBack");
            vocab.AddAttributeNameSurrogate("maxExtent");
            vocab.AddAttributeNameSurrogate("maxFront");
            vocab.AddAttributeNameSurrogate("maxPosition");
            vocab.AddAttributeNameSurrogate("metadataFormat");
            vocab.AddAttributeNameSurrogate("minAngle");
            vocab.AddAttributeNameSurrogate("minBack");
            vocab.AddAttributeNameSurrogate("minFront");
            vocab.AddAttributeNameSurrogate("minPosition");
            vocab.AddAttributeNameSurrogate("modulationTypeDetail");
            vocab.AddAttributeNameSurrogate("modulationTypeMajor");
            vocab.AddAttributeNameSurrogate("modulationTypeSpreadSpectrum");
            vocab.AddAttributeNameSurrogate("modulationTypeSystem");
            vocab.AddAttributeNameSurrogate("momentsOfInertia");
            vocab.AddAttributeNameSurrogate("multicastRelayHost");
            vocab.AddAttributeNameSurrogate("multicastRelayPort");
            vocab.AddAttributeNameSurrogate("munitionApplicationID");
            vocab.AddAttributeNameSurrogate("munitionEndPoint");
            vocab.AddAttributeNameSurrogate("munitionEntityID");
            vocab.AddAttributeNameSurrogate("munitionQuantity");
            vocab.AddAttributeNameSurrogate("munitionSiteID");
            vocab.AddAttributeNameSurrogate("munitionStartPoint");
            vocab.AddAttributeNameSurrogate("mustEvaluate");
            vocab.AddAttributeNameSurrogate("navType");
            vocab.AddAttributeNameSurrogate("networkMode");
            vocab.AddAttributeNameSurrogate("next");
            vocab.AddAttributeNameSurrogate("nodeField");
            vocab.AddAttributeNameSurrogate("offset");
            vocab.AddAttributeNameSurrogate("on");
            vocab.AddAttributeNameSurrogate("order");
            vocab.AddAttributeNameSurrogate("originator");
            vocab.AddAttributeNameSurrogate("outerRadius");
            vocab.AddAttributeNameSurrogate("parameter");
            vocab.AddAttributeNameSurrogate("pauseTime");
            vocab.AddAttributeNameSurrogate("pitch");
            vocab.AddAttributeNameSurrogate("points");
            vocab.AddAttributeNameSurrogate("port");
            vocab.AddAttributeNameSurrogate("power");
            vocab.AddAttributeNameSurrogate("previous");
            vocab.AddAttributeNameSurrogate("priority");
            vocab.AddAttributeNameSurrogate("profile");
            vocab.AddAttributeNameSurrogate("progress");
            vocab.AddAttributeNameSurrogate("protoField");
            vocab.AddAttributeNameSurrogate("radioEntityTypeCategory");
            vocab.AddAttributeNameSurrogate("radioEntityTypeCountry");
            vocab.AddAttributeNameSurrogate("radioEntityTypeDomain");
            vocab.AddAttributeNameSurrogate("radioEntityTypeKind");
            vocab.AddAttributeNameSurrogate("radioEntityTypeNomenclature");
            vocab.AddAttributeNameSurrogate("radioEntityTypeNomenclatureVersion");
            vocab.AddAttributeNameSurrogate("radioID");
            vocab.AddAttributeNameSurrogate("readInterval");
            vocab.AddAttributeNameSurrogate("receivedPower");
            vocab.AddAttributeNameSurrogate("receiverState");
            vocab.AddAttributeNameSurrogate("reference");
            vocab.AddAttributeNameSurrogate("relativeAntennaLocation");
            vocab.AddAttributeNameSurrogate("resolution");
            vocab.AddAttributeNameSurrogate("resumeTime");
            vocab.AddAttributeNameSurrogate("rightUrl");
            vocab.AddAttributeNameSurrogate("rootUrl");
            vocab.AddAttributeNameSurrogate("rotateYUp");
            vocab.AddAttributeNameSurrogate("rtpHeaderExpected");
            vocab.AddAttributeNameSurrogate("sampleRate");
            vocab.AddAttributeNameSurrogate("samples");
            vocab.AddAttributeNameSurrogate("shiftKey");
            vocab.AddAttributeNameSurrogate("side");
            vocab.AddAttributeNameSurrogate("siteID");
            vocab.AddAttributeNameSurrogate("skinCoordIndex");
            vocab.AddAttributeNameSurrogate("skinCoordWeight");
            vocab.AddAttributeNameSurrogate("skyAngle");
            vocab.AddAttributeNameSurrogate("skyColor");
            vocab.AddAttributeNameSurrogate("spacing");
            vocab.AddAttributeNameSurrogate("spatialize");
            vocab.AddAttributeNameSurrogate("speed");
            vocab.AddAttributeNameSurrogate("speedFactor");
            vocab.AddAttributeNameSurrogate("spine");
            vocab.AddAttributeNameSurrogate("startAngle");
            vocab.AddAttributeNameSurrogate("startTime");
            vocab.AddAttributeNameSurrogate("stiffness");
            vocab.AddAttributeNameSurrogate("stopTime");
            vocab.AddAttributeNameSurrogate("string");
            vocab.AddAttributeNameSurrogate("stripCount");
            vocab.AddAttributeNameSurrogate("style");
            vocab.AddAttributeNameSurrogate("summary");
            vocab.AddAttributeNameSurrogate("tdlType");
            vocab.AddAttributeNameSurrogate("tessellation");
            vocab.AddAttributeNameSurrogate("tessellationScale");
            vocab.AddAttributeNameSurrogate("time");
            vocab.AddAttributeNameSurrogate("timeOut");
            vocab.AddAttributeNameSurrogate("timestamp");
            vocab.AddAttributeNameSurrogate("title");
            vocab.AddAttributeNameSurrogate("toggle");
            vocab.AddAttributeNameSurrogate("top");
            vocab.AddAttributeNameSurrogate("topToBottom");
            vocab.AddAttributeNameSurrogate("topUrl");
            vocab.AddAttributeNameSurrogate("touchTime");
            vocab.AddAttributeNameSurrogate("transmitFrequencyBandwidth");
            vocab.AddAttributeNameSurrogate("transmitState");
            vocab.AddAttributeNameSurrogate("transmitterApplicationID");
            vocab.AddAttributeNameSurrogate("transmitterEntityID");
            vocab.AddAttributeNameSurrogate("transmitterRadioID");
            vocab.AddAttributeNameSurrogate("transmitterSiteID");
            vocab.AddAttributeNameSurrogate("transparent");
            vocab.AddAttributeNameSurrogate("triggerTime");
            vocab.AddAttributeNameSurrogate("triggerTrue");
            vocab.AddAttributeNameSurrogate("triggerValue");
            vocab.AddAttributeNameSurrogate("type");
            vocab.AddAttributeNameSurrogate("uDimension");
            vocab.AddAttributeNameSurrogate("uKnot");
            vocab.AddAttributeNameSurrogate("uOrder");
            vocab.AddAttributeNameSurrogate("uTessellation");
            vocab.AddAttributeNameSurrogate("ulimit");
            vocab.AddAttributeNameSurrogate("vDimension");
            vocab.AddAttributeNameSurrogate("vKnot");
            vocab.AddAttributeNameSurrogate("vOrder");
            vocab.AddAttributeNameSurrogate("vTessellation");
            vocab.AddAttributeNameSurrogate("version");
            vocab.AddAttributeNameSurrogate("verticalDatum");
            vocab.AddAttributeNameSurrogate("vertices");
            vocab.AddAttributeNameSurrogate("visibilityLimit");
            vocab.AddAttributeNameSurrogate("visibilityRange");
            vocab.AddAttributeNameSurrogate("warhead");
            vocab.AddAttributeNameSurrogate("weight");
            vocab.AddAttributeNameSurrogate("whichGeometry");
            vocab.AddAttributeNameSurrogate("writeInterval");
            vocab.AddAttributeNameSurrogate("xDimension");
            vocab.AddAttributeNameSurrogate("xSpacing");
            vocab.AddAttributeNameSurrogate("yScale");
            vocab.AddAttributeNameSurrogate("zDimension");
            vocab.AddAttributeNameSurrogate("zSpacing");
            vocab.AddAttributeNameSurrogate("visible");
            vocab.AddAttributeNameSurrogate("repeatR");
            vocab.AddAttributeNameSurrogate("texture");
            vocab.AddAttributeNameSurrogate("back");
            vocab.AddAttributeNameSurrogate("front");
            vocab.AddAttributeNameSurrogate("left");
            vocab.AddAttributeNameSurrogate("right");
            vocab.AddAttributeNameSurrogate("parts");
            vocab.AddAttributeNameSurrogate("isSelected");
            vocab.AddAttributeNameSurrogate("isValid");
            vocab.AddAttributeNameSurrogate("numComponents");
            vocab.AddAttributeNameSurrogate("depth");
            vocab.AddAttributeNameSurrogate("update");
            vocab.AddAttributeNameSurrogate("fogCoord");
            vocab.AddAttributeNameSurrogate("texCoord");
            vocab.AddAttributeNameSurrogate("activate");
            vocab.AddAttributeNameSurrogate("programs");
            vocab.AddAttributeNameSurrogate("matrix");
            vocab.AddAttributeNameSurrogate("anchorPoint");
            vocab.AddAttributeNameSurrogate("body1");
            vocab.AddAttributeNameSurrogate("body2");
            vocab.AddAttributeNameSurrogate("forceOutput");
            vocab.AddAttributeNameSurrogate("body1AnchorPoint");
            vocab.AddAttributeNameSurrogate("body2AnchorPoint");
            vocab.AddAttributeNameSurrogate("plane");
            vocab.AddAttributeNameSurrogate("appliedParameters");
            vocab.AddAttributeNameSurrogate("bounce");
            vocab.AddAttributeNameSurrogate("frictionCoefficients");
            vocab.AddAttributeNameSurrogate("minBounceSpeed");
            vocab.AddAttributeNameSurrogate("slipFactors");
            vocab.AddAttributeNameSurrogate("softnessConstantForceMix");
            vocab.AddAttributeNameSurrogate("softnessErrorCorrection");
            vocab.AddAttributeNameSurrogate("surfaceSpeed");
            vocab.AddAttributeNameSurrogate("isActive");
            vocab.AddAttributeNameSurrogate("useGeometry");
            vocab.AddAttributeNameSurrogate("set_destination");
            vocab.AddAttributeNameSurrogate("set_value");
            vocab.AddAttributeNameSurrogate("tau");
            vocab.AddAttributeNameSurrogate("tolerance");
            vocab.AddAttributeNameSurrogate("value_changed");
            vocab.AddAttributeNameSurrogate("initialDestination");
            vocab.AddAttributeNameSurrogate("initialValue");
            vocab.AddAttributeNameSurrogate("angle");
            vocab.AddAttributeNameSurrogate("variation");
            vocab.AddAttributeNameSurrogate("surfaceArea");
            vocab.AddAttributeNameSurrogate("frictionDirection");
            vocab.AddAttributeNameSurrogate("slipCoefficients");
            vocab.AddAttributeNameSurrogate("category");
            vocab.AddAttributeNameSurrogate("country");
            vocab.AddAttributeNameSurrogate("domain");
            vocab.AddAttributeNameSurrogate("extra");
            vocab.AddAttributeNameSurrogate("kind");
            vocab.AddAttributeNameSurrogate("specific");
            vocab.AddAttributeNameSurrogate("subcategory");
            vocab.AddAttributeNameSurrogate("axis1");
            vocab.AddAttributeNameSurrogate("axis2");
            vocab.AddAttributeNameSurrogate("desiredAngularVelocity1");
            vocab.AddAttributeNameSurrogate("desiredAngularVelocity2");
            vocab.AddAttributeNameSurrogate("maxAngle1");
            vocab.AddAttributeNameSurrogate("maxTorque1");
            vocab.AddAttributeNameSurrogate("maxTorque2");
            vocab.AddAttributeNameSurrogate("minAngle1");
            vocab.AddAttributeNameSurrogate("stopBounce1");
            vocab.AddAttributeNameSurrogate("stopConstantForceMix1");
            vocab.AddAttributeNameSurrogate("stopErrorCorrection1");
            vocab.AddAttributeNameSurrogate("suspensionErrorCorrection");
            vocab.AddAttributeNameSurrogate("suspensionForce");
            vocab.AddAttributeNameSurrogate("body1Axis");
            vocab.AddAttributeNameSurrogate("body2Axis");
            vocab.AddAttributeNameSurrogate("hinge1Angle");
            vocab.AddAttributeNameSurrogate("hinge1AngleRate");
            vocab.AddAttributeNameSurrogate("hinge2Angle");
            vocab.AddAttributeNameSurrogate("hinge2AngleRate");
            vocab.AddAttributeNameSurrogate("set_fraction");
            vocab.AddAttributeNameSurrogate("easeInEaseOut");
            vocab.AddAttributeNameSurrogate("modifiedFraction_changed");
            vocab.AddAttributeNameSurrogate("force");
            vocab.AddAttributeNameSurrogate("geoCenter");
            vocab.AddAttributeNameSurrogate("centerOfRotation_changed");
            vocab.AddAttributeNameSurrogate("geoCoord_changed");
            vocab.AddAttributeNameSurrogate("orientation_changed");
            vocab.AddAttributeNameSurrogate("position_changed");
            vocab.AddAttributeNameSurrogate("isPickable");
            vocab.AddAttributeNameSurrogate("viewport");
            vocab.AddAttributeNameSurrogate("activeLayer");
            vocab.AddAttributeNameSurrogate("align");
            vocab.AddAttributeNameSurrogate("offsetUnits");
            vocab.AddAttributeNameSurrogate("scaleMode");
            vocab.AddAttributeNameSurrogate("sizeUnits");
            vocab.AddAttributeNameSurrogate("layout");
            vocab.AddAttributeNameSurrogate("objectType");
            vocab.AddAttributeNameSurrogate("pickedNormal");
            vocab.AddAttributeNameSurrogate("pickedPoint");
            vocab.AddAttributeNameSurrogate("pickedTextureCoordinate");
            vocab.AddAttributeNameSurrogate("intersectionType");
            vocab.AddAttributeNameSurrogate("sortOrder");
            vocab.AddAttributeNameSurrogate("axis1Angle");
            vocab.AddAttributeNameSurrogate("axis1Torque");
            vocab.AddAttributeNameSurrogate("axis2Angle");
            vocab.AddAttributeNameSurrogate("axis2Torque");
            vocab.AddAttributeNameSurrogate("axis3Angle");
            vocab.AddAttributeNameSurrogate("axis3Torque");
            vocab.AddAttributeNameSurrogate("enabledAxies");
            vocab.AddAttributeNameSurrogate("motor1Axis");
            vocab.AddAttributeNameSurrogate("motor2Axis");
            vocab.AddAttributeNameSurrogate("motor3Axis");
            vocab.AddAttributeNameSurrogate("stop1Bounce");
            vocab.AddAttributeNameSurrogate("stop1ErrorCorrection");
            vocab.AddAttributeNameSurrogate("stop2Bounce");
            vocab.AddAttributeNameSurrogate("stop2ErrorCorrection");
            vocab.AddAttributeNameSurrogate("stop3Bounce");
            vocab.AddAttributeNameSurrogate("stop3ErrorCorrection");
            vocab.AddAttributeNameSurrogate("motor1Angle");
            vocab.AddAttributeNameSurrogate("motor1AngleRate");
            vocab.AddAttributeNameSurrogate("motor2Angle");
            vocab.AddAttributeNameSurrogate("motor2AngleRate");
            vocab.AddAttributeNameSurrogate("motor3Angle");
            vocab.AddAttributeNameSurrogate("motor3AngleRate");
            vocab.AddAttributeNameSurrogate("autoCalc");
            vocab.AddAttributeNameSurrogate("duration");
            vocab.AddAttributeNameSurrogate("retainUserOffsets");
            vocab.AddAttributeNameSurrogate("isBound");
            vocab.AddAttributeNameSurrogate("appearance");
            vocab.AddAttributeNameSurrogate("createParticles");
            vocab.AddAttributeNameSurrogate("lifetimeVariation");
            vocab.AddAttributeNameSurrogate("maxParticles");
            vocab.AddAttributeNameSurrogate("particleLifetime");
            vocab.AddAttributeNameSurrogate("particleSize");
            vocab.AddAttributeNameSurrogate("colorKey");
            vocab.AddAttributeNameSurrogate("geometryType");
            vocab.AddAttributeNameSurrogate("texCoordKey");
            vocab.AddAttributeNameSurrogate("pickable");
            vocab.AddAttributeNameSurrogate("angularDampingFactor");
            vocab.AddAttributeNameSurrogate("angularVelocity");
            vocab.AddAttributeNameSurrogate("autoDamp");
            vocab.AddAttributeNameSurrogate("autoDisable");
            vocab.AddAttributeNameSurrogate("disableAngularSpeed");
            vocab.AddAttributeNameSurrogate("disableLinearSpeed");
            vocab.AddAttributeNameSurrogate("disableTime");
            vocab.AddAttributeNameSurrogate("finiteRotationAxis");
            vocab.AddAttributeNameSurrogate("fixed");
            vocab.AddAttributeNameSurrogate("forces");
            vocab.AddAttributeNameSurrogate("inertia");
            vocab.AddAttributeNameSurrogate("linearDampingFactor");
            vocab.AddAttributeNameSurrogate("torques");
            vocab.AddAttributeNameSurrogate("useFiniteRotation");
            vocab.AddAttributeNameSurrogate("useGlobalForce");
            vocab.AddAttributeNameSurrogate("constantForceMix");
            vocab.AddAttributeNameSurrogate("constantSurfaceThickness");
            vocab.AddAttributeNameSurrogate("errorCorrection");
            vocab.AddAttributeNameSurrogate("iterations");
            vocab.AddAttributeNameSurrogate("maxCorrectionSpeed");
            vocab.AddAttributeNameSurrogate("preferAccuracy");
            vocab.AddAttributeNameSurrogate("pointSize");
            vocab.AddAttributeNameSurrogate("stopBounce");
            vocab.AddAttributeNameSurrogate("stopErrorCorrection");
            vocab.AddAttributeNameSurrogate("angleRate");
            vocab.AddAttributeNameSurrogate("maxSeparation");
            vocab.AddAttributeNameSurrogate("minSeparation");
            vocab.AddAttributeNameSurrogate("separation");
            vocab.AddAttributeNameSurrogate("separationRate");
            vocab.AddAttributeNameSurrogate("closed");
            vocab.AddAttributeNameSurrogate("keyVelocity");
            vocab.AddAttributeNameSurrogate("normalizeVelocity");
            vocab.AddAttributeNameSurrogate("surface");
            vocab.AddAttributeNameSurrogate("anisotropicDegree");
            vocab.AddAttributeNameSurrogate("borderColor");
            vocab.AddAttributeNameSurrogate("borderWidth");
            vocab.AddAttributeNameSurrogate("boundaryModeS");
            vocab.AddAttributeNameSurrogate("boundaryModeT");
            vocab.AddAttributeNameSurrogate("boundaryModeR");
            vocab.AddAttributeNameSurrogate("magnificationFilter");
            vocab.AddAttributeNameSurrogate("minificationFilter");
            vocab.AddAttributeNameSurrogate("textureCompression");
            vocab.AddAttributeNameSurrogate("texturePriority");
            vocab.AddAttributeNameSurrogate("generateMipMaps");
            vocab.AddAttributeNameSurrogate("targetObject");
            vocab.AddAttributeNameSurrogate("backAmbientIntensity");
            vocab.AddAttributeNameSurrogate("backDiffuseColor");
            vocab.AddAttributeNameSurrogate("backEmissiveColor");
            vocab.AddAttributeNameSurrogate("backShininess");
            vocab.AddAttributeNameSurrogate("backSpecularColor");
            vocab.AddAttributeNameSurrogate("separateBackColor");
            vocab.AddAttributeNameSurrogate("displayed");
            vocab.AddAttributeNameSurrogate("clipBoundary");
            vocab.AddAttributeNameSurrogate("internal");
            vocab.AddAttributeNameSurrogate("gustiness");
            vocab.AddAttributeNameSurrogate("turbulence");
            vocab.AddAttributeNameSurrogate("unitCategory");
            vocab.AddAttributeNameSurrogate("unitName");
            vocab.AddAttributeNameSurrogate("unitConversionFactor");
            vocab.AddAttributeNameSurrogate("weightConstant1");
            vocab.AddAttributeNameSurrogate("weightConstant2");
            vocab.AddAttributeNameSurrogate("weightFunction1");
            vocab.AddAttributeNameSurrogate("weightFunction2");
            vocab.AddAttributeNameSurrogate("boundaryOpacity");
            vocab.AddAttributeNameSurrogate("opacityFactor");
            vocab.AddAttributeNameSurrogate("retainedOpacity");
            vocab.AddAttributeNameSurrogate("colorSteps");
            vocab.AddAttributeNameSurrogate("orthogonalColor");
            vocab.AddAttributeNameSurrogate("parallelColor");
            vocab.AddAttributeNameSurrogate("ordered");
            vocab.AddAttributeNameSurrogate("edgeColor");
            vocab.AddAttributeNameSurrogate("gradientThreshold");
            vocab.AddAttributeNameSurrogate("contourStepSize");
            vocab.AddAttributeNameSurrogate("dimensions");
            vocab.AddAttributeNameSurrogate("surfaceTolerance");
            vocab.AddAttributeNameSurrogate("surfaceValues");
            vocab.AddAttributeNameSurrogate("intensityThreshold");
            vocab.AddAttributeNameSurrogate("segmentEnabled");
            vocab.AddAttributeNameSurrogate("lighting");
            vocab.AddAttributeNameSurrogate("shadows");
            vocab.AddAttributeNameSurrogate("phaseFunction");
            vocab.AddAttributeNameSurrogate("silhouetteBoundaryOpacity");
            vocab.AddAttributeNameSurrogate("silhouetteRetainedOpacity");
            vocab.AddAttributeNameSurrogate("silhouetteSharpness");
            vocab.AddAttributeNameSurrogate("coolColor");
            vocab.AddAttributeNameSurrogate("warmColor");

            // X3D Attribute Value table
            vocab.AddAttributeValue("false");
            vocab.AddAttributeValue("true");

            // X3D-specific encoding algorithm table
            vocab.AddEncodingAlgorithm(new QuantizedFloatArrayEncoder());
            vocab.AddEncodingAlgorithm(new DeltazlibIntArrayEncoder());
            vocab.AddEncodingAlgorithm(new QuantizedzlibFloatArrayEncoder());
            vocab.AddEncodingAlgorithm(new zlibFloatArrayEncoder());
            vocab.AddEncodingAlgorithm(new QuantizedDoubleArrayEncoder());
            vocab.AddEncodingAlgorithm(new zlibDoubleArrayEncoder());
            vocab.AddEncodingAlgorithm(new QuantizedzlibDoubleArrayEncoder());
            vocab.AddEncodingAlgorithm(new RangeIntArrayEncoder());

            return vocab;
        }

        #endregion
    }
}
