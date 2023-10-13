using System;
using System.Collections.Generic;
using Dialogue.Scripts.Nodes;
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

        private DialogueGraphView _graphView;
        private Dictionary<string, List<string>> _dictionary = new Dictionary<string, List<string>>();
        private List<StatementNode> _statementNodes = new List<StatementNode>();
        private string startstatementID = "";
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
            _close.RegisterCallback<ClickEvent>(_ => Close());
            _create.RegisterCallback<ClickEvent>(_ => Create());
        }

        private void Create()
        {
            _graphView = DialogueEditor.GetCurrentGraphView();
            if (_graphView == null)
            {
                Debug.Log("Could not find a Graph View.");
                return;
            }

            var nodes = _textField.value.Split("\n");
            if (nodes.Length == 0) return;
            if (nodes[0].Contains("==="))
            {
                ClearNodes();
                GenerateStatements();
                GenerateResponses();
                _graphView.PopulateView(_graphView.DialogGraph);
                Close();
                return;
            }
            
            ScanAndValidate();

            foreach (var entry in _dictionary)
            {
                var node = _graphView.DialogGraph.CreateNode(typeof(StatementNode)) as StatementNode;
                if (node == null)
                {
                    Debug.Log($"not a valid statement: {entry.Key}");
                    continue;
                }
                node.statementText = entry.Key;
                foreach (var response in entry.Value)
                {
                    var newResponse = _graphView.DialogGraph.CreateNode(typeof(ResponseNode)) as ResponseNode;
                    if (newResponse == null) return;
                    newResponse.responseText = response;
                    _graphView.DialogGraph.AddChild(node, newResponse);
                }
            }
            _graphView.PopulateView(_graphView.DialogGraph);
            Close();
        }

        private void ClearNodes()
        {

            var rootNode = _graphView.DialogGraph.rootNode as RootNode;
            if (rootNode == null) return;
            var statement = rootNode.child as StatementNode;
            if (statement == null) return;
            _graphView.DialogGraph.RemoveChild(rootNode, statement);
        }

        private void GenerateResponses()
        {
            var nodes = _textField.value.Split("\n");
            var key = "";
            ResponseNode previouseResponse = null;
            StatementNode previousStatement = null;
            for (var i = 0; i < nodes.Length - 1; i++)
            {
                var line = nodes[i];
                if (line.Contains("===")) // Next line is a key
                {
                    key = line.Replace("===", "").Trim().Replace(" ", "");
                    previousStatement = FindStatementByID(key);
                    continue;
                }

                if ( string.IsNullOrEmpty(line )) continue;
                if (!line.Contains("\t"))
                {
                    previousStatement = FindNodeByLine(line);
                    continue;
                }
                line = line.Trim();
                if ( string.IsNullOrEmpty(line )) continue;

                
                if (line.Contains(">"))
                {
                    var statementKey = line.Split('>')[1].Trim().Replace(" ", "");
                    if (statementKey == startstatementID)
                    {
                        var gotoNode = _graphView.DialogGraph.CreateNode(typeof(GotoNode)) as GotoNode;
                        if (gotoNode == null) return;
                        gotoNode.statementId = startstatementID;
                        _graphView.DialogGraph.AddChild(previouseResponse, gotoNode);
                        continue;
                    }
                    
                    Debug.Log($"Searching for key: {statementKey}");
                    var statementNode = FindStatementByID(statementKey);
                    if (statementNode == null)
                    {
                        Debug.Log($"No Valid Statement Key for {line} on Row: {i}");
                        continue;
                    }

                    _graphView.DialogGraph.AddChild(previouseResponse, statementNode);
                    continue;
                }

                var newResponse = _graphView.DialogGraph.CreateNode(typeof(ResponseNode)) as ResponseNode;
                if (newResponse == null) return;
                newResponse.responseText = line;
                previouseResponse = newResponse;
               if (previousStatement != null)
                    _graphView.DialogGraph.AddChild(previousStatement, previouseResponse);
            }
        }

        private StatementNode FindStatementByID(string id)
        {
            foreach (var node in _graphView.DialogGraph.nodes)
            {
                if (node is not StatementNode statementNode) continue;
                if (statementNode.id == id) return statementNode;
            }

            return null;
        }

        private StatementNode FindNodeByLine(string line)
        {
            foreach (var node in _graphView.DialogGraph.nodes)
            {
                if (node is not StatementNode statementNode) continue;
                if (statementNode.statementText == line) return statementNode;
            }

            return null;
        }

        private void GenerateStatements()
        {
            var nodes = _textField.value.Split("\n");
            var key = "";
            StatementNode previousStatement = null;
            for (var i = 0; i < nodes.Length - 1; i++)
            {
                var line = nodes[i];
                if (string.IsNullOrEmpty(line)) continue;
                if (line.StartsWith("\t")) continue;
                if (line.Contains("===")) // Next line is a the statement. This is th ekey.
                {
                    key = line.Replace("===", "").Trim().Replace(" ", "");
                    previousStatement = null;
                    continue;
                }

                if (line.StartsWith("The kids"))
                {
                    
                }
                var statementNode = _graphView.DialogGraph.CreateNode(typeof(StatementNode)) as StatementNode;
                if (statementNode == null) return;
                statementNode.statementText = line;

                if (string.IsNullOrEmpty(key))
                {
                    key = GUID.Generate().ToString();
                    statementNode.id = key;
                    if (previousStatement != null)
                    {
                        previousStatement.nextstatementId = key;
                        _graphView.DialogGraph.AddChild(previousStatement, statementNode);
                    }
                }
                else
                {
                    statementNode.id = key;                    
                }

                key = "";

                if (i == 1)
                {
                    startstatementID = statementNode.id;
                    _graphView.DialogGraph.AddChild(_graphView.DialogGraph.rootNode, statementNode);
                }
                previousStatement = statementNode;
            }
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

                    _dictionary.TryAdd(currentStatement, new List<string>());
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