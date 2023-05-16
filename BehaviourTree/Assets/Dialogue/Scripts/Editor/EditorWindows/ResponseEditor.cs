using UnityEditor;
using UnityEngine.UIElements;

namespace Dialogue.Editor.EditorWindows
{
    [CustomEditor(typeof(ResponseNode))]
    public class ResponseEditor : UnityEditor.Editor
    {
        private const string Uxml = "Assets/Dialogue/UI/ResponseNodeView.uxml";
        private VisualElement _ve;
        private ResponseNode currentResponse;

        public override VisualElement CreateInspectorGUI()
        {
            var visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Uxml);
            _ve = visualTreeAsset.CloneTree();
            
            currentResponse= serializedObject.targetObject as ResponseNode;
            if (currentResponse == null) return _ve;
            
            return _ve;
        }
        
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            serializedObject.Update();
            currentResponse.Update();
            serializedObject.ApplyModifiedProperties();
        }
    }
}