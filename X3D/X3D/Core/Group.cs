using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using System.Xml.Serialization;

namespace X3D
{
    public partial class Group
    {
        private bool _isHidden;

        /// <summary>
        /// If true, disabled the node from any Render() calls. KeepAlive() is then called instead.
        /// </summary>
        [XmlIgnore]
        public new bool Hidden
        {
            get
            {
                return _isHidden;
            }
            set
            {
                _isHidden = value;

                this.Decendants().ForEach(n => n.Hidden = value);
            }
        }
    }
}
