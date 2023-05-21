using System;
using Dialogue.Scripts.Nodes;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Dialogue.Scripts.Editor.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(DialogCVar))]
    public class DialogCVarDrawer : PropertyDrawer
    {
        private const string Uxml = "Assets/Dialogue/UI/DialogCVarView.uxml";
        private VisualElement _ve;
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Uxml);
            _ve = visualTreeAsset.CloneTree();
            return _ve;
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {

        }
    }
}