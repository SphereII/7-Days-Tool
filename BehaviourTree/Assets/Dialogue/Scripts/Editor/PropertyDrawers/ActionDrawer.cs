using System.Collections.Generic;
using Dialogue.Editor;
using Dialogue.Editor.EditorWindows;
using Dialogue.GameData.Dialogs;
using Dialogue.Scripts.Editor.PropertyDrawers;
using Dialogue.Scripts.Nodes;
using TMPro;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Dialogue.Scripts.Editor.EditorWindows
{
    [CustomPropertyDrawer(typeof(DialogAction))]
    public class ActionDrawer : BaseDrawer
    {
        protected override void SetupCallbacks()
        {
        //    var guid = CurrentProperty.FindPropertyRelative("guid").stringValue;
            AvailableTypes.viewDataKey = Guid;
            var choices = new List<string>();
            foreach (var action in ConfigurationManager.Actions)
            {
                choices.Add(action.Value.className);
            }
            AvailableTypes.choices = choices;
            AvailableTypes.RegisterValueChangedCallback(Refresh);

            Operator.viewDataKey = $"operator_{Guid}";
            Operator.choices = Cfg.ActionOperator;
            
            Refresh(null);
        }

        private void Refresh(ChangeEvent<string> evt)
        {
            var newType = "";
            if ( evt == null )
                newType = CurrentProperty.FindPropertyRelative("type").stringValue;
            else
                newType = evt.newValue;
            
            ConfigMap = Cfg.GetActionMapByClass(newType);
            Description.text = ConfigMap.description;
            AvailableTypesLabel.text = "Action Type";
            
            ShowVisualElement(OperatorGroup, false);

            ConfigureValue();
            ConfigureID();
        }

   
    }
}