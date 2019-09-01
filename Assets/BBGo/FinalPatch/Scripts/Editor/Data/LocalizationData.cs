using System.Collections.Generic;
using System.Xml.Serialization;

namespace BBGo.FinalPatch
{
    public class LocalizationData
    {
        [XmlArray("Items")]
        [XmlArrayItem("Item")]
        public List<LocalizationItem> items;
    }

    public class LocalizationItem
    {
        [XmlAttribute]
        public string Key { get; set; }
        [XmlAttribute]
        public string Value { get; set; }
    }
}