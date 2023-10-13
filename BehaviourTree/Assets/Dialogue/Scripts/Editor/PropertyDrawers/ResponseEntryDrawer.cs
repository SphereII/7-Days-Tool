using System.Collections.Generic;
using Dialogue.Editor;
using Dialogue.GameData.Dialogs;
using Dialogue.Scripts.Editor.PropertyDrawers;
using UnityEditor;

using UnityEngine.UIElements;
namespace Dialogue.Scripts.Editor.EditorWindows
{
    [CustomPropertyDrawer(typeof(DialogResponseEntry))]
    public class ResponseEntryDrawer : PropertyDrawer
    {
        private const string Uxml = "Assets/Dialogue/UI/LinkedResponse.uxml";
        private VisualElement _ve;
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Uxml);
            _ve = visualTreeAsset.CloneTree();
            var button = _ve.Q<Button>("GoToResponse");
            button.clickable = new Clickable(() =>
            {
                var graphView = _ve.GetFirstAncestorOfType<DialogueGraphView>();
                graphView.ClearSelection();
                foreach (var node in graphView.DialogGraph.nodes)
                {
                    if (node is not ResponseNode responseNode) continue;
                    if (responseNode.id != button.text) continue;
                    graphView.AddToSelection(responseNode.nodeView);
                    break;
                }
                graphView.FrameSelection();
            });
            button.text = property.FindPropertyRelative("Id").stringValue;
            return _ve;
        }
}
}