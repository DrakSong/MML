using System;
using System.Xml.Serialization;

namespace BBGo.FinalPatch
{
    [Serializable]
    public class BundleData
    {
        [XmlAttribute]
        public string Name { get; set; }
        [XmlAttribute]
        public string Hash { get; set; }
        [XmlAttribute]
        public long Size { get; set; }
        [XmlAttribute]
        public int Version { get; set; }

        public BundleData Clone()
        {
            BundleData clone = new BundleData
            {
                Name = Name,
                Hash = Hash,
                Size = Size,
            };
            return clone;
        }
    }
}