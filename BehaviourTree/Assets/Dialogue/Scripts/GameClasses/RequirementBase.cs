using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.UI;

namespace Dialogue.GameData.Dialogs
{
    [Serializable]
    public class RequirementBase
    {
        [XmlIgnore]
        [HideInInspector]
        public string guid = new Guid().ToString();
        
        [XmlAttribute(AttributeName = "type")] public string type;

        [XmlAttribute(AttributeName = "requirementtype")]
        [HideInInspector] public string Requirementtype = "Hide";
        
        [XmlAttribute(AttributeName = "value")]
        public string Value = null;
        
        [XmlAttribute(AttributeName = "operator")] public string op = null;

        [XmlAttribute(AttributeName = "id")]
        public string Id =null;
        
        public override string ToString()
        {
            var display = $"Req: {type}";
            if (!string.IsNullOrEmpty(Value))
                display += $" Value: {Value}";
            if (!string.IsNullOrEmpty(op))
                display += $" {op}";
            if (!string.IsNullOrEmpty(Id))
                display += $" Id: {Id}";
            return display;
        }
        public bool Invoke()
        {
            return false;
        }
    }
    
}