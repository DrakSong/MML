using System.Collections.Generic;
using System.Xml.Serialization;

namespace BBGo.FinalPatch
{
    public class DeployData
    {
        [XmlArray("Channels")]
        [XmlArrayItem("Channel")]
        public List<ChannelData> Channels { get; set; }
    }

    public class ChannelData
    {
        [XmlAttribute]
        public string Name { get; set; }
        [XmlAttribute]
        public string URL { get; set; } = "http://localhost:8000/AssetBundles";
        [XmlAttribute]
        public string Build { get; set; }
        [XmlAttribute]
        public int Version { get; set; }
        [XmlAttribute]
        public bool Foldout { get; set; } = true;
        [XmlIgnore]
        public bool IsEdit { get; set; }
    }
}