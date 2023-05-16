using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Dialogue.Editor.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(StatementNode))]
    public class StatementDrawer : PropertyDrawer
    {
        private const string Uxml = "Assets/Dialogue/UI/StatementNodeView.uxml";
        private VisualElement _ve;
        private GroupBox _responseGroup;
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Uxml);
            _ve = visualTreeAsset.CloneTree();
            return _ve;
        }
    }
}