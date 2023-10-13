using System.Collections.Generic;
using Dialogue.GameData.Dialogs;
using Dialogue.Scripts.Nodes;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Dialogue.Editor.EditorWindows
{
  
    [CustomEditor(typeof(StatementNode))]
    public class StatementEditor : UnityEditor.Editor
    {
        private const string Uxml = "Assets/Dialogue/UI/StatementNodeView.uxml";
        private const string LinkedResponseUxml = "Assets/Dialogue/UI/LinkedResponse.uxml";
        private VisualElement _ve;
        private GroupBox _groupBoxResponses;
        private StatementNode _currentStatement;
        private ListView _listView;
        public override VisualElement CreateInspectorGUI()
        {
            var visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Uxml);
            _ve = visualTreeAsset.CloneTree();

            _currentStatement= serializedObject.targetObject as StatementNode;
            if (_currentStatement == null)
            {
                Debug.Log("Not a valid Statement ID. You should never see this.");
                return _ve;
            }

            _currentStatement.OnNodeUpdate += RefreshRespones;
            _listView = _ve.Q<ListView>("ResponsesListView");
            HandleAddNewResponse();

            _groupBoxResponses = _ve.Q<GroupBox>("LinkedResponses");
            RefreshRespones();
            return _ve;
        }

        private void HandleAddNewResponse()
        {
            var textField = _ve.Q<TextField>("AddNewResponse");
            var saveButton = _ve.Q<Button>("btnSaveNewResponse");
            saveButton.clickable = new Clickable(() =>
            {
                var textEntry = textField.value;
                if (string.IsNullOrEmpty(textEntry))
                    return;
                
                var dialogGraphView = _ve.GetFirstAncestorOfType<DialogueGraphView>();
                var newResponse = dialogGraphView.DialogGraph.CreateNode(typeof(ResponseNode)) as ResponseNode;
                if (newResponse == null) return;
                newResponse.position = _currentStatement.position;
                newResponse.position.x += _currentStatement.nodeView.GetPosition().width + 100;
                newResponse.position.y += 100;
                newResponse.responseText = textEntry;
                dialogGraphView.DialogGraph.AddChild(_currentStatement, newResponse);

                // Clear the old value.
                textField.value = "";
                
                // Force a refresh so it shows up.
                dialogGraphView.PopulateView(dialogGraphView.DialogGraph);
                
            });
        }
        

        private void RefreshRespones()
        {
            _groupBoxResponses.Clear();

            var responseAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(LinkedResponseUxml);
            foreach (var response in _currentStatement.Children)
            {
                if (response is ResponseNode responseNode)
                {
                    var ve = responseAsset.CloneTree();
                    var button = ve.Q<Button>("GoToResponse");
                    button.clickable = new Clickable(() =>
                    {
                        var graphView = _ve.GetFirstAncestorOfType<DialogueGraphView>();
                        graphView.ClearSelection();
                        graphView.AddToSelection(responseNode.nodeView);
                        graphView.FrameSelection();
                    });
                    button.text = responseNode.responseText;
                    _groupBoxResponses.Add(ve);
                }
                else if (response is ImportNode importNode)
                {
                    if (importNode == null) continue;
                    var children = importNode.GetChildren();
                    foreach (var newChild in children)
                    {
                        var newResponse = newChild as ResponseNode;
                        if (newResponse == null) continue;
                        var localized = ConfigurationManager.GetLocalisedValue(newResponse.responseText);
                        var ve = responseAsset.CloneTree();
                        var button = ve.Q<Button>("GoToResponse");
                        button.clickable = new Clickable(() =>
                        {
                            var graphView = _ve.GetFirstAncestorOfType<DialogueGraphView>();
                            graphView.ClearSelection();
                            graphView.AddToSelection(importNode.nodeView);
                            graphView.FrameSelection();
                        });
                        button.text = localized;
                        //  if ( _groupBoxResponses.Contains(ve)) continue;

                        _groupBoxResponses.Add(ve);
                    }
                }
            }
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