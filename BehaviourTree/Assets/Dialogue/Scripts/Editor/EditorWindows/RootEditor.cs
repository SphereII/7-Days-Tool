using Dialogue.Scripts.Nodes;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Dialogue.Editor.EditorWindows
{
  
    [CustomEditor(typeof(RootNode))]
    public class RootEditor : UnityEditor.Editor
    {
        private const string Uxml = "Assets/Dialogue/UI/RootNodeView.uxml";
        private VisualElement _ve;
        public override VisualElement CreateInspectorGUI()
        {
            var visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Uxml);
            _ve = visualTreeAsset.CloneTree();
            return _ve;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            serializedObject.Update();

            // Any other processing?
            serializedObject.ApplyModifiedProperties();
        }
    }
}