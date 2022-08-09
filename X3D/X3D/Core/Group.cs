using System.Xml.Serialization;

namespace X3D
{
    public partial class Group
    {
        private bool _isHidden;

        /// <summary>
        ///     If true, disabled the node from any Render() calls. KeepAlive() is then called instead.
        /// </summary>
        [XmlIgnore]
        public new bool Hidden
        {
            get => _isHidden;
            set
            {
                _isHidden = value;

                Decendants().ForEach(n => n.Hidden = value);
            }
        }
    }
}