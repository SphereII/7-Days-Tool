using System.Collections.Generic;
using Dialogue.Scripts.Nodes;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Dialogue
{
    public class InspectorView : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<InspectorView, UxmlTraits>
        {
        }

        private UnityEditor.Editor _editor;

       
        public void UpdateSelection(NodeView nodeView)
        {
            Clear();
            UnityEngine.Object.DestroyImmediate(_editor);
            _editor = UnityEditor.Editor.CreateEditor(nodeView.Node);
            if (_editor.target == null) return;

            switch (nodeView.Node)
            {
                case StatementNode:
                case GotoNode:
                case ActionNode:
                case ResponseNode:
                    case RootNode:
                    var customEditor = _editor.CreateInspectorGUI();
                    customEditor.Bind(_editor.serializedObject);
                    Add(customEditor);
                    break;
                default: // No custom editor for anything else, so display unity's custom inspector.
                    var container = new IMGUIContainer(() => { _editor.OnInspectorGUI(); });
                    Add(container);
                    break;
            }
        }
    }
}