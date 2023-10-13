using UnityEditor;
using UnityEngine.UIElements;

namespace Dialogue.Editor.EditorWindows
{
    [CustomEditor(typeof(ActionNode))]
    public class ActionEditor : UnityEditor.Editor
    {
        private const string Uxml = "Assets/Dialogue/UI/ActionNodeView.uxml";
        private VisualElement _ve;
        private ActionNode _actionNode;
        public override VisualElement CreateInspectorGUI()
        {
            var visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Uxml);
            _ve = visualTreeAsset.CloneTree();
            
            _actionNode= serializedObject.targetObject as ActionNode;
            return _ve;
        }
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            serializedObject.Update();
            _actionNode.Update();

            serializedObject.ApplyModifiedProperties();
        }
    }
}