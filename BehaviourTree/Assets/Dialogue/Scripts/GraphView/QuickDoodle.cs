using System.Collections.Generic;

using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Dialogue.Scripts.Editor.EditorWindows
{
    public class QuickDoodleEditor : EditorWindow
    {
        private const string Uxml = "Assets/Dialogue/UI/QuickDoodle.uxml";
        private VisualElement _ve;
        private TextField _textField;
        private Button _create;
        private Button _close;

        private Dictionary<string, List<string>> _dictionary = new Dictionary<string, List<string>>();
        private List<StatementNode> _statementNodes = new List<StatementNode>();
        public void CreateGUI()
        {
            var visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Uxml);
            _ve = visualTreeAsset.CloneTree();

            _textField = _ve.Q<TextField>("targetTextField");
            _create = _ve.Q<Button>("ButtonCreate");
            _close = _ve.Q<Button>("ButtonClose");
            
            SetupControls();
            rootVisualElement.Add(_ve);
        }

        private void SetupControls()
        {
            _close.RegisterCallback<ClickEvent>( _ => Close());
            _create.RegisterCallback<ClickEvent>(_ =>Create());
        }

        private void Create()
        {
            var dialogGraphView = DialogueEditor.GetCurrentGraphView();
            if (dialogGraphView == null)
            {
                Debug.Log("Could not find a Graph View.");
                return;
            }

            ScanAndValidate();

            foreach (var entry in _dictionary)
            {
                var node = dialogGraphView.DialogGraph.CreateNode(typeof(StatementNode)) as StatementNode;
                if (node == null)
                {
                    Debug.Log($"not a valid statement: {entry.Key}");
                    continue;
                }
                node.statementText = entry.Key;
                foreach (var response in entry.Value)
                {
                    var newResponse = dialogGraphView.DialogGraph.CreateNode(typeof(ResponseNode)) as ResponseNode;
                    if (newResponse == null) return;
                    newResponse.responseText = response;
                    dialogGraphView.DialogGraph.AddChild(node, newResponse);
                }
            }
            dialogGraphView.PopulateView(dialogGraphView.DialogGraph);
            Close();
        }

        private void ScanAndValidate()
        {
            var currentStatement = "";
            _dictionary.Clear();
            foreach (var line in _textField.value.Split("\n"))
            {
                if (!line.StartsWith("\t"))
                {
                    currentStatement = line.Trim();
                    _dictionary.Add(currentStatement, new List<string>());                    
                }
                else
                {
                    if (string.IsNullOrEmpty(currentStatement)) continue;
                    _dictionary[currentStatement].Add(line.Trim());
                }
            }
        }

    }
}