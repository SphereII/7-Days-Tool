﻿using System.Xml.Serialization;

namespace Dialogue.GameData.Dialogs
{
    [XmlRoot(ElementName = "response_entry")]
    public class DialogResponseEntry
    {
        [XmlAttribute(AttributeName = "id")] public string Id = null;
        [XmlAttribute(AttributeName = "ref_text")] public string Ref_text = null;
        
        [XmlAttribute(AttributeName = "uniqueid")] public string uniqueid = null;
    }
}