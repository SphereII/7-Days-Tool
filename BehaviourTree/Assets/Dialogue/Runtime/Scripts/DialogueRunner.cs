using System.Collections;
using System.Collections.Generic;
using Dialogue;
using Dialogue.Scripts.Nodes;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class DialogueRunner : MonoBehaviour
{

    private UIDocument _uiDocument;
    private VisualElement _root;
    public DialogGraph dialogGraph;

    private Label _currentStatementText;
    private GroupBox _playerResponseGrp;

    private StatementNode _currentStatementNode;
    private RootNode _rootNode;

    private readonly List<StatementNode> _statementNodes = new List<StatementNode>();
    private readonly List<ResponseNode> _responseNodes = new List<ResponseNode>();
    private readonly List<ActionNode> _actionNodes = new List<ActionNode>();
    
    protected const string Uxml = "Assets/Dialogue/UI/Scene/Response.uxml";
    void Start()
    {
        _uiDocument = GetComponent<UIDocument>();
        _root = _uiDocument.rootVisualElement;
        SetupScreen();
        InitializeDialog();
    }

    private void SetupScreen()
    {
        _currentStatementText = _root.Q<Label>("NPCStatementLabel");
        _playerResponseGrp = _root.Q<GroupBox>("PlayerResponses");
    }

    private void InitializeDialog()
    {
        if (dialogGraph == null)
        {
            Debug.Log("Missing Dialogue Scriptable Object!");
            return;
        }
        _statementNodes.Clear();
        _responseNodes.Clear();
        _actionNodes.Clear();
        _playerResponseGrp.Clear();

        _rootNode = dialogGraph.rootNode as RootNode;
        if (_rootNode == null)
        {
            Debug.Log("No root node in the selected dialog!");
            return;
        }
        AddDialog(dialogGraph);
        foreach (var node in dialogGraph.nodes)
        {
            if (node is StatementNode statementNode)
            {
                var newchildren = new List<BaseNode>();
                foreach (var child in statementNode.Children)
                {
                    if (child is ImportNode importNode)
                    {
                        var graph = importNode.importedDialog;
                        if (graph == null) continue;
                        var rootNode = graph.rootNode as RootNode;
                        if (rootNode == null) continue;
                        var importStatement = rootNode.child as StatementNode;
                        if ( importStatement == null ) continue;
                        newchildren.AddRange(importStatement.Children);
                        AddDialog(importNode.importedDialog);
                    }
                }
                statementNode.Children.AddRange(newchildren);
            }
        }
        _currentStatementNode = _rootNode.child as StatementNode;
        LoadStatement(_currentStatementNode);
    }

    private void AddDialog(DialogGraph dialog)
    {
        foreach (var node in dialog.nodes)
        {
            if (node is StatementNode statementNode)
            {
                if (_statementNodes.Contains(statementNode)) continue;
                _statementNodes.Add(statementNode);
            }

            if (node is ResponseNode responseNode)
            {
                if (_responseNodes.Contains(responseNode)) continue;
                _responseNodes.Add(responseNode);
            }

            if (node is ActionNode actionNode)
            {
                if ( _actionNodes.Contains(actionNode)) continue;
                
                _actionNodes.Add(actionNode);
            }

        }
    }
    private void RefreshScreen()
    {
        if (_currentStatementNode == null)
        {
            Debug.Log("Resetting to first statement.");
            _currentStatementNode = _rootNode.child as StatementNode;
        }

        if (_currentStatementNode == null)
            return;
        _currentStatementText.text = _currentStatementNode.statementText;
        _playerResponseGrp.Clear();
        foreach (var baseNode in _currentStatementNode.Children)
        {
            if (baseNode is not ResponseNode responseNode) continue;

            var visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Uxml);
            var ve = visualTreeAsset.CloneTree();

            var button = ve.Q<Button>("ResponseButton");
            button.text = responseNode.responseText;
            var nextStatement = GetStatementByID(responseNode.nextstatementId);
            button.clickable = new Clickable(_ => LoadStatement(nextStatement));
            button.SetEnabled(true);
            button.tooltip = "Click here to continue the conversation.";
            
            // Check to see if this needs to be locked
            var reqToggle = ve.Q<Toggle>("HasRequirements");
            ShowVisualElement(reqToggle, false);
            
            if (responseNode.requirements.Count > 0)
            {
                ShowVisualElement(reqToggle, true);
                reqToggle.RegisterValueChangedCallback(_ => button.SetEnabled(reqToggle.value));
                button.SetEnabled(false);
                var toolTip = "This response has the following requirement(s): ";
                foreach (var requirement in responseNode.requirements)
                {
                    toolTip += $"\n{requirement}";
                }

                button.tooltip = toolTip;
            }
                
            _playerResponseGrp.Add(ve);
        }
    }

    private void LoadStatement(StatementNode statementNode)
    {
        _currentStatementNode = statementNode;
        RefreshScreen();
    }

    private void ShowVisualElement(VisualElement visualElement, bool state)
    {
        if (visualElement == null) return;
        visualElement.style.display = (state) ? DisplayStyle.Flex : DisplayStyle.None;
    }

   
    private StatementNode GetStatementByID(string id)
    {
        foreach (var statement in _statementNodes)
        {
            if (statement.id == id)
                return statement;
        }

        return null;
    }
  
}
