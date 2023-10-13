using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Dialogue.GameData.Dialogs
{
    [Serializable]
    [XmlRoot(ElementName = "dialogs")]
    public class Dialogs
    {
        [XmlElement(ElementName = "dialog")] public List<Dialog> Dialog = new List<Dialog>();
    }
    
}