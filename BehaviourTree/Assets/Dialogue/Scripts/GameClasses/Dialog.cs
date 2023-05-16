using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Dialogue.GameData.Dialogs
{
    [Serializable]
    [XmlRoot(ElementName = "dialog")]
    public class Dialog
    {
        [XmlElement(ElementName = "statement")]
        public List<DialogStatement> Statement = new List<DialogStatement>();

        [XmlElement(ElementName = "response")] public List<DialogResponse> Response = new List<DialogResponse>();
        [XmlAttribute(AttributeName = "id")] public string Id = null;

        [XmlAttribute(AttributeName = "startstatementid")]
        public string Startstatementid = null;
    }
}