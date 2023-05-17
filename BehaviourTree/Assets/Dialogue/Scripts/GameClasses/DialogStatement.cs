using System.Collections.Generic;
using System.Xml.Serialization;

namespace Dialogue.GameData.Dialogs
{
    [XmlRoot(ElementName = "statement")]

    public class DialogStatement
    {
        [XmlElement(ElementName = "response_entry")]
        public List<DialogResponseEntry> responseEntry = new List<DialogResponseEntry>();

        [XmlAttribute(AttributeName = "id")] public string id = null;
        [XmlAttribute(AttributeName = "text")] public string text= null;

        [XmlAttribute(AttributeName = "ref_text")]
        public string RefText = "";
        [XmlAttribute(AttributeName = "nextstatementid")] public string Nextstatementid = null;


        [XmlElement(ElementName = "quest_entry")]
        public List<DialogQuestEntry> questEntry = null;

        /*public string Localized()
        {
            return Localization.GetLocalization(Text);
        }*/
    }
}