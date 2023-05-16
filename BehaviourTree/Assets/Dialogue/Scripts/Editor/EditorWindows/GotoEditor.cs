using System.Collections.Generic;
using Dialogue.Scripts.Nodes;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Dialogue.Scripts.Editor.EditorWindows
{
    [CustomEditor(typeof(GotoNode))]
    public class GotoEditor : UnityEditor.Editor
    {
        private const string Uxml = "Assets/Dialogue/UI/GotoNode.uxml";
        private VisualElement _ve;
        private GotoNode _gotoNode;
        private DropdownField _dropdownField;
        private Dictionary<string, string> _statementNodes = new Dictionary<string, string>();
        public override VisualElement CreateInspectorGUI()
        {
            var visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Uxml);
            _ve = visualTreeAsset.CloneTree();

            _gotoNode = serializedObject.targetObject as GotoNode;
            if (_gotoNode == null)
            {
                Debug.Log("Not a valid Goto. You should never see this.");
                return _ve;
            }

            _gotoNode.OnNodeUpdate += RefreshStatements;
            _dropdownField = _ve.Q<DropdownField>();
            _dropdownField.RegisterValueChangedCallback(UpdateReferenceStatement);

            _dropdownField.viewDataKey = _gotoNode.guid;
            RefreshStatements();
            return _ve;
        }

        private void UpdateReferenceStatement(ChangeEvent<string> evt)
        {
            foreach (var entry in _statementNodes)
            {
                if (evt.newValue == entry.Value)
                {
                    _gotoNode.statementId = entry.Key;
                    return;
                }
            }
            
        }

        private void RefreshStatements()
        {
            var choices = new List<string>();
            _statementNodes.Clear();
            var graphView = DialogueEditor.GetCurrentGraphView();
            if (graphView == null) return;
            foreach (var node in graphView.DialogGraph.nodes)
            {
                if (node is not StatementNode statementNode) continue;
                var display = $"{statementNode.nodeView.GetText(40)}";
                _statementNodes.Add(statementNode.id, display);
                choices.Add(display);
            }
            _dropdownField.choices = choices;
            
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            //DrawDefaultInspector();
            serializedObject.Update();

            // Any other processing?
            serializedObject.ApplyModifiedProperties();
        }
        

    }
}