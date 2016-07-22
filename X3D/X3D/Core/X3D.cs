using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace X3D
{
    /// <summary>
    /// http://www.web3d.org/documents/specifications/19775-1/V3.3/Part01/Architecture.html
    /// </summary>
    public partial class X3D
    {
        public const X3DMIMEType DefaultMimeType = X3DMIMEType.X3D;

        [XmlIgnore]
        public static double X3DEngineVersionNum = 3.3;

        [XmlIgnore]
        public static x3dVersion X3DEngineVersion = x3dVersion.X3D_3_3;

        [XmlIgnore]
        public static profileNames X3DEngineProfile = profileNames.Interactive;

        private static bool _runtimeExecutionEnabled = true;
        private static bool _runtimePresentationEnabled = true;
        private static bool _loaderOnlyEnabled = false;

        /// <summary>
        /// When Runtime execution is enabled, in addition to Geometry in the X3D Scene being presented, 
        /// the run-time aspects of the specification are evaluated.
        /// 
        /// Otherwise when runtime execution is disabled,
        /// Geometry in the X3D Scene is presented in the browser as though time has not run
        /// 
        /// ~~~~ A time zero loader is typically found in modelling tools that 
        /// intend to construct or modify existing X3D content 
        /// without evaluating the run-time aspects of the specification. ~~~~
        /// 
        /// See: http://www.web3d.org/documents/specifications/19775-1/V3.3/Part01/concepts.html#X3DLoaders
        /// </summary>
        [XmlIgnore]
        public static bool RuntimeExecutionEnabled
        {
            get { return _runtimeExecutionEnabled; }
            set { _runtimeExecutionEnabled = value; }
        }

        /// <summary>
        /// If runtime presentation is enabled, the Renderer displays geometry in the scene.
        /// </summary>
        [XmlIgnore]
        public static bool RuntimePresentationEnabled
        {
            get { return _runtimePresentationEnabled; }
            set { _runtimePresentationEnabled = value; }
        }

        /// <summary>
        /// If LoaderOnlyEnabled is enabled, the Renderer only displays geometry in the scene 
        /// and does not execute any behaviors.
        /// </summary>
        [XmlIgnore]
        public static bool LoaderOnlyEnabled
        {
            get { return _loaderOnlyEnabled; }
            set { _loaderOnlyEnabled = value; }
        }

        public X3D()
        {
            this._version = x3dVersion.X3D_3_3;
            this._profile = profileNames.Interchange;
        }

        public override void Load()
        {
            base.Load();

            if(!string.IsNullOrEmpty(this.version) && float.Parse(this.version) > 3.3)
            {
                Console.WriteLine("X3D Document requires version {0} which is not available current browser version is X3D v{1}", this.version, X3DEngineVersionNum);
            }
            if (!(profile == profileNames.Core || profile == profileNames.Interchange || profile == profileNames.Interactive))
            {
                Console.WriteLine("X3D Document requires profile {0} which is not available current browser supports {1} profile", this.profile, X3DEngineProfile);
            }
            else
            {
                Console.WriteLine("Browser supports {0} profile", X3DEngineProfile);
            }

        }
    }
}
