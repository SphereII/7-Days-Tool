using System;
using System.Xml.Serialization;
using UnityEngine;

namespace Dialogue.GameData.Dialogs
{
    [Serializable]
    [XmlRoot(ElementName = "action")]
    public class DialogAction
    {
        [XmlIgnore]
        [HideInInspector]
        public string guid = new Guid().ToString();
        
        [XmlAttribute(AttributeName = "type")] public string type;
        
        [XmlAttribute(AttributeName = "id")] public string Id = null;
        
        [XmlAttribute(AttributeName = "operator")] public string op = null;

        [XmlAttribute(AttributeName = "value")] public string Value = null;

        public override string ToString()
        {
            var display = $"Action: {type}";
            if (!string.IsNullOrEmpty(Id))
                display += $" Id: {Id}";
            if (!string.IsNullOrEmpty(op))
                display += $" {op}";
            if (!string.IsNullOrEmpty(Value))
                display += $" Value: {Value}";
            return display;
        }
    }

}