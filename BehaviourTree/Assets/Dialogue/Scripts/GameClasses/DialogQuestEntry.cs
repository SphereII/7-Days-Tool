using System;
using System.Xml.Serialization;

namespace Dialogue.GameData.Dialogs
{
    [Serializable]

    [XmlRoot(ElementName = "quest_entry")]
    public class DialogQuestEntry
    {
        [XmlAttribute(AttributeName = "listindex")] public string listindex = null;
        [XmlAttribute(AttributeName = "tier")] public string tier = null;
    }
}