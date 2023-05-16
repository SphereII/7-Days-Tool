using System.Collections.Generic;
using Dialogue.Editor;
using Dialogue.GameData.Dialogs;
using Dialogue.Scripts.Editor.PropertyDrawers;
using UnityEditor;

using UnityEngine.UIElements;
namespace Dialogue.Scripts.Editor.EditorWindows
{
    [CustomPropertyDrawer(typeof(RequirementBase))]
    public class RequirementDrawer : BaseDrawer
    {
        protected override void SetupCallbacks()
        {
            AvailableTypesLabel.text = "Requirement Type";

          //  var guid = CurrentProperty.FindPropertyRelative("guid").stringValue;

            AvailableTypes.viewDataKey = Guid;
            var choices = new List<string>();
            foreach (var requirement in ConfigurationManager.Requirements)
            {
                choices.Add(requirement.Value.className);
            }
            AvailableTypes.choices = choices;
            AvailableTypes.RegisterValueChangedCallback(Refresh);

            Operator.viewDataKey = $"operator_{Guid}";
            Operator.choices = Cfg.Operators;
            Refresh(null);
          
        }

        private void Refresh(ChangeEvent<string> evt)
        {
            var newType = "";
            if ( evt == null )
                newType = CurrentProperty.FindPropertyRelative("type").stringValue;
            else
                newType = evt.newValue;
            ConfigMap = Cfg.GetRequirementMapByClass(newType);
            Description.text = ConfigMap.description;
            ShowVisualElement(OperatorGroup, false);

            ConfigureValue();
            ConfigureID();
        }
        

    }
}