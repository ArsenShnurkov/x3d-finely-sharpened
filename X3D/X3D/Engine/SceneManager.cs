﻿#define NO_DTD_VALIDATION

using System;
using System.Xml;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.IO;
using System.Xml.Linq;
using System.Xml.XPath;

using System.Net;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using X3D.Parser;

/*
createX3DFromStream
createX3DFromString
createX3DFromUrl
 */

namespace X3D.Engine
{

    public class SceneManager
    {
        public SceneGraph SceneGraph;


        ///// <summary>
        ///// Disable DTD validation if you want to gain performance benefits
        ///// or are confident in the input markup.
        ///// 
        ///// Enable validation if you want to validate markup according to its specified dtd.
        ///// </summary>
        //private static bool DTD_Validate=false;

        public static string CurrentLocation;// the cd is set upon every X3D scene fetch

        public static string BaseURL { get; set; }
        public static X3DMIMEType BaseMIME { get; set; }

        internal static Queue<ImageTexture> _texturesToBuffer = new Queue<ImageTexture>();
        internal static int[] _texturesBuffered;
        private static Regex regProto = new Regex("[a-zA-Z]+[:][/][/]", RegexOptions.Compiled);

        #region Scene Loader Methods

        public static SceneManager fromURL(string url, X3DMIMEType mime_type)
        {
            object document;

            if (Fetch(url, out document))
            {
                if (document != null && document.GetType() == typeof(SceneManager))
                {
                    return (SceneManager)document;
                }
            }
            else
            {
                return null;
            }

            return null;
        }

        public static SceneManager fromString(string data, X3DMIMEType mime_type)
        {
            switch (mime_type)
            {
                case X3DMIMEType.X3D:
                    return _x3dfromString(data);
                    //case X3DMIMEType.ClassicVRML:
                    //case X3DMIMEType.X3DBinary:
                    //case X3DMIMEType.VRML:
                    //case X3DMIMEType.UNKNOWN:
            }
            return null;
        }

        public static SceneManager fromStream(Stream data, X3DMIMEType mime_type)
        {
            switch (mime_type)
            {
                case X3DMIMEType.X3D:
                    return _x3dfromStream(data);
                    //case X3DMIMEType.ClassicVRML:
                    //case X3DMIMEType.X3DBinary:
                    //case X3DMIMEType.VRML:
                    //case X3DMIMEType.UNKNOWN:
            }
            return null;
        }

        public static SceneManager fromURL(string url)
        {
            return fromURL(url, X3D.DefaultMimeType);
        }

        public static SceneManager fromURL(string url, string mime_type)
        {
            return fromURL(url, GetMIMEType(mime_type));
        }

        public static SceneManager fromString(string data)
        {
            return _x3dfromString(data);
        }

        public static SceneManager fromString(string data, string mime_type)
        {
            return fromString(data, GetMIMEType(mime_type));
        }

        public static SceneManager fromStream(Stream data)
        {
            return fromStream(data, X3D.DefaultMimeType);
        }

        public static SceneManager fromStream(Stream data, string mime_type)
        {
            return fromStream(data, GetMIMEType(mime_type));
        }

        #endregion

        #region Asset Resourcing Methods

        public static bool Fetch(string url_mfstring, out object resource)
        {
            if (X3DTypeConverters.IsMFString(url_mfstring))
            {
                string[] urls = X3DTypeConverters.GetMFString(url_mfstring);

                foreach (string url in urls)
                {
                    if (FetchSingle(url, out resource))
                    {
                        return true;
                    }
                }
                resource = null;
                return false;

            }
            else
            {
                return FetchSingle(url_mfstring, out resource);
            }

            resource = null;
            return false;
        }

        private static bool DoesURLHaveProtocol(string url)
        {
            return regProto.Match(url).Success;
        }

        private static bool isrelative(string url)
        {

            //return Uri.IsWellFormedUriString(url, UriKind.Absolute) == false;

            Uri uri;

            Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out uri);

            return !uri.IsAbsoluteUri;
        }

        public static bool isAbsolute(string url)
        {
            //return Uri.IsWellFormedUriString(url, UriKind.Absolute) == true;

            Uri uri = new Uri(url);

            return uri.IsAbsoluteUri;
        }

        private static bool isWebUrl(string uriString)
        {
            Uri uriResult;
            bool result = Uri.TryCreate(uriString, UriKind.Absolute, out uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

            return result;
        }

        //private static bool isFileSystemUri(string uriString)
        //{
        //    Uri uriResult;

        //    return result;
        //}

        public static bool FetchSingle(string url, out object resource)
        {
            //TODO: how to handle relative files/paths?
            // use cd to determine exactly how to process relative paths
            Uri uri;

            if (string.IsNullOrEmpty(url))
            {
                resource = null;
                return false;
            }

            url = X3DTypeConverters.removeQuotes(url);

            if (!CapabilityEnabled(System.IO.Path.GetExtension(url)))
            {
                switch (System.Windows.Forms.MessageBox.Show("File Extension of type " + System.IO.Path.GetExtension(url) + " is not implemented yet", "Not imp", System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Information))
                {
                    case System.Windows.Forms.DialogResult.Yes:
                        break;
                    case System.Windows.Forms.DialogResult.No:
                        System.Windows.Forms.MessageBox.Show("Perhaps this feature will be implemented soon");
                        break;
                }
                resource = null;
                return false;
            }

            if (url.ToLower().StartsWith("file://"))
            {
                if (url.ToLower().StartsWith("file:///"))
                {
                    url = url.Remove(0, 8);
                }
                else
                {
                    url = url.Remove(0, 7);
                }
                url.Replace('/', '\\');
            }

            //if(GetMIMETypeByURL(url)!=X3DMIMEType.UNKNOWN) {
            //CurrentLocation=url.TrimEnd().TrimEnd(System.IO.Path.GetFileName(url).ToCharArray());
            //}
            Uri www_url;
            //MessageBox.Show("for debugging purposes..");

            if (isrelative(url))
            {
                Uri base_uri;

                if (isWebUrl(CurrentLocation))
                {
                    url = url.Replace("\\", "/");
                }
                else
                {
                    url = url.Replace("/", "\\");
                }

                
                if (url.StartsWith("/"))
                {
                    url = "." + url;
                }

                //if(CurrentLocation
                if (GetMIMETypeByURL(url) != X3DMIMEType.UNKNOWN)
                {
                    // get the current path to the html document
                    set_cd(BaseURL);
                    // what if the *.x3d file is a relative address and no CurrentLocation can be found??? SEE THIS X3D example:
                    // http://www.web3d.org/x3d/content/examples/Conformance/Appearance/ImageTexture/_pages/page13.html
                    //http://www.web3d.org/x3d/content/examples/Conformance/Appearance/ImageTexture/ElevationGrid.x3d
                }

                if(!isWebUrl(CurrentLocation) && isrelative(url))
                {
                    // Is on the file system. Unix or Windows.

                    url = SceneManager.CurrentLocation + "\\" + url;

                }
                else if(isWebUrl(CurrentLocation) && isrelative(url))
                {
                    base_uri = new Uri(CurrentLocation);

                    if (Uri.TryCreate(base_uri, url, out www_url))
                    {
                        url = www_url.ToString();
                        if (System.IO.Path.GetFileName(url) == System.IO.Path.GetFileName(BaseURL))
                        {
                            url = BaseURL;
                        }
                    }
                    else
                    {
                        resource = null;
                        return false;
                    }
                }


            }

            if (url.ToLower().StartsWith("file://"))
            {
                if (url.ToLower().StartsWith("file:///"))
                {
                    url = url.Remove(0, 8);
                }
                else
                {
                    url = url.Remove(0, 7);
                }
                url.Replace('/', '\\');
            }

            uri = new Uri(url);

            if (uri.IsFile)
            {
                // it should be a file on the local file system
                SceneManager.CurrentLocation = (new System.IO.DirectoryInfo(url)).Parent.FullName;

                if (File.Exists(url))
                {
                    FileStream fs = File.OpenRead(url);
                    //try {
                    //using() {
                    if (GetMIMETypeByURL(url) == X3DMIMEType.UNKNOWN)
                    {
                        resource = (Stream)fs;
                    }
                    else
                    {
                        resource = fromStream((Stream)fs);
                    }
                    //}
                    return true;
                    //}
                    //finally {
                    //fs.Close();
                    //}
                }
                else
                {
                    resource = null;
                    return false;
                }
            }
            else
            {
                HttpWebRequest request;
                HttpWebResponse response;
                X3DMIMEType m;

                request = (HttpWebRequest)WebRequest.Create(url);

                request.Method = "POST";
                //request.Method="post";

                response = (HttpWebResponse)request.GetResponse();

                if (response.StatusCode == HttpStatusCode.OK)
                { // if the Status code == 200
                    m = GetMIMETypeByURL(url);

                    if (string.IsNullOrEmpty(response.ContentType) || m == X3DMIMEType.UNKNOWN)
                    {
                        if (m == X3DMIMEType.UNKNOWN)
                        {
                            // response is a generic resource
                            resource = response.GetResponseStream();
                        }
                        else
                        {
                            // response is a scene
                            resource = fromStream(response.GetResponseStream(), m);

                            set_cd(url);
                        }
                    }
                    else
                    {
                        // response is a scene
                        resource = fromStream(response.GetResponseStream(), m);

                        set_cd(url);
                    }
                    return true;
                }
                else
                {
                    resource = null;
                    return false;
                }
            }
        }

        private static void set_cd(string url)
        {
            //Uri u;
            //u=new Uri(url);
            //CurrentLocation=u.AbsolutePath;

            CurrentLocation = url.TrimEnd().TrimEnd(System.IO.Path.GetFileName(url).ToCharArray());
        }

        #endregion

        #region MIME Type Helper Methods

        public static X3DMIMEType GetMIMETypeByURL(string url)
        {
            string file_extension;

            file_extension = System.IO.Path.GetExtension(url);

            switch (file_extension.ToLower().Trim())
            {
                case ".x3d":
                    return X3DMIMEType.X3D;
                case ".xml":
                    return X3DMIMEType.X3D;
                case ".x3db":
                    return X3DMIMEType.X3DBinary;
                case ".x3dv":
                    return X3DMIMEType.ClassicVRML;
                case ".x3dz":
                    return X3DMIMEType.X3D;
                case ".x3dbz":
                    return X3DMIMEType.X3DBinary;
                case ".x3dvz":
                    return X3DMIMEType.ClassicVRML;
                case ".wrl":
                    return X3DMIMEType.VRML;
                default:
                    return X3DMIMEType.UNKNOWN;
            }
        }

        public static X3DMIMEType GetMIMEType(string mime_type)
        {
            switch (mime_type.ToLower())
            {
                case "model/x3d+xml":
                    return X3DMIMEType.X3D;
                case "model/x3d+binary":
                    return X3DMIMEType.X3DBinary;
                case "model/x3d+vrml":
                    return X3DMIMEType.ClassicVRML;
                case "x-world/x-vrml":
                    return X3DMIMEType.VRML;
                case "model/vrml":
                    return X3DMIMEType.VRML;
                default:
                    return X3DMIMEType.UNKNOWN;
            }
        }

        #endregion

        #region Scene Helper Methods

        /// <summary>
        /// Informally: Convert the XML DOM into a X3D DOM by building a tree of X3DNodes.
        /// Formally: Each XML element is deserialized automatically into an X3DNode,
        /// then a X3D DOM is formed inside the scene graph.
        /// </summary>
        /// <returns>
        /// A scene with a newly formed scene graph
        /// </returns>
        private static SceneManager _x3dfromStream(Stream xml_stream)
        {
            SceneManager s;
            XDocument xml;

            s = new SceneManager();

            xml = XDocument.Load(xml_stream);

            s.SceneGraph = new SceneGraph(xml);

            return s;
        }

        private static SceneManager _x3dfromString(string xml_string)
        {
            SceneManager s;
            XDocument xml;

            s = new SceneManager();

            xml = XDocument.Load(xml_string);

            s.SceneGraph = new SceneGraph(xml);

            return s;
        }

        private static bool IsURLScene(string url)
        {
            return CapabilityEnabled(System.IO.Path.GetExtension(url));
        }

        private static bool IsScene(string file_extension)
        {
            return CapabilityEnabled(file_extension);
        }

        private static bool CapabilityEnabled(string capability)
        {
            switch (capability.ToLower().Trim())
            {
                case ".x3d":
                    return true;
                case ".xml":
                    return true;
                case "model/x3d+xml":
                    return true;
                case "model/x3d+binary":
                    return false;
                case "model/x3d+vrml":
                    return false;
                case "x-world/x-vrml":
                    return false;
                case "model/vrml":
                    return false;
                case ".x3db":
                    return false;
                case ".x3dv":
                    return false;
                case ".x3dz":
                    return false;
                case ".x3dbz":
                    return false;
                case ".x3dvz":
                    return false;
                case ".wrl":
                    return false;
                default:
                    return true;
            }
        }

        #endregion

        #region Scene Rendering Methods

        public void Draw(RenderingContext rc)
        {
            Draw(this, rc);
        }

        public static void Draw(SceneManager scene, RenderingContext rc)
        {
            Renderer.Scene(scene, rc);
        }

        public static int CreateTexture(ImageTexture indexableTexture)
        {
            int i;

            i = _texturesToBuffer.Count;
            
            _texturesToBuffer.Enqueue(indexableTexture);

            return i;
        }

        #endregion
    }
}