using System.Xml;
using System.Xml.Serialization;

namespace IdiotPlugin.VoteRewards
{
    [XmlRoot("RewardItem")]
    public class RewardItem
    {
        [XmlElement("ItemType")]
        public string ItemType = "Ingot";
        [XmlElement("ItemSubType")]
        public string ItemSubType = "Stone";
        [XmlElement("Always")]
        public bool Always = false;
        [XmlElement("Min")]
        public int Min = 1;
        [XmlElement("Max")]
        public int Max = 1;
        [XmlElement("Weight")]
        public int Weight = 1;

    }
}
