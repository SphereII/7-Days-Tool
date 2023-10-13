using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Dialogue.GameData.Dialogs;

namespace Dialogue.Scripts.GameClasses.ExportsOnly
{
    [Serializable]
    [XmlRoot(ElementName = "dialogs")]
    public class DialogsExport
    {
        [XmlElement(ElementName = "append")] public Append append  = new Append();
    }


    public class Append
    {
        [XmlAttribute(AttributeName = "xpath")] public string xpath = "//dialogs";
        [XmlElement(ElementName = "dialog")] public List<Dialog> Dialog = new List<Dialog>();
        
    }
  
}