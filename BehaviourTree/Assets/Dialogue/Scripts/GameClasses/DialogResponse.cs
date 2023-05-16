using System.Collections.Generic;
using System.Xml.Serialization;

namespace Dialogue.GameData.Dialogs
{
    [XmlRoot(ElementName = "response")]
    public class DialogResponse
    {
        [XmlAttribute(AttributeName = "id")] public string Id = null;
        [XmlAttribute(AttributeName = "text")] public string Text = null;

        [XmlElement(ElementName = "requirement")]
        public List<RequirementBase> Requirement = new List<RequirementBase>();

        [XmlElement(ElementName = "action")] 
        public List<DialogAction> Actions = new List<DialogAction>();

        [XmlAttribute(AttributeName = "nextstatementid")]
        public string Nextstatementid = null;
        
        [XmlAttribute(AttributeName = "ref_text")]
        public string RefText = null;
    }
}