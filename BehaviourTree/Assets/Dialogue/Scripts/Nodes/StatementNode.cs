using System.Collections.Generic;
using Dialogue.Editor;
using Dialogue.GameData.Dialogs;
using Dialogue.Nodes.CustomAttributes;
using Dialogue.Scripts.Nodes;
using UnityEngine;

public class StatementNode : BaseNode
{
    public string statementText = "Hello";
    public string id;

    [ReadOnly] public string nextstatementId;

    // This contains the actual details, but we hide them
    public List<DialogResponseEntry> ResponseEntries = new List<DialogResponseEntry>();
    [HideInInspector] public List<BaseNode> Children = new List<BaseNode>();

    // These display the friendly name for the responses.
    [ReadOnly] public List<string> ChildrenNames = new List<string>();

    public override void Update()
    {
        if (string.IsNullOrEmpty(id)) id = guid;
        ChildrenNames.Clear();
        
        nodeView?.UpdateTitle();
        
        var currentGraph = DialogueEditor.GetCurrentGraphView();
        List<BaseNode> childrenToRemove = new List<BaseNode>();
        foreach (var child in Children)
        {
            if (child is ActionNode actionNode)
            {
                childrenToRemove.Add(child);
            }

            if (child is ResponseNode response)
            {
                var localized = ConfigurationManager.GetLocalisedValue(response.responseText);
                if (ChildrenNames.Contains(localized))
                {
                    // This already exists.
                    childrenToRemove.Add(response);
                    continue;
                }

                ChildrenNames.Add(localized);

                // Do a check here to see if there's a Goto linked to this child, so then we can remove this child.
                foreach (var node in response.Children)
                {
                    if (node is not GotoNode gotoNode) continue;
                    if (gotoNode.statementId == id)
                    {
                        childrenToRemove.Add(response);
                        break;
                    }
                }
            }
            else if (child is StatementNode statementNode)
            {
                nextstatementId =
                    $"{statementNode.id} ( {ConfigurationManager.GetLocalisedValue(statementNode.statementText)} )";
                ChildrenNames.Add(statementNode.statementText);
            }
        }

        foreach (var child in childrenToRemove)
        {
            if (child is ResponseNode responseNode)
                ChildrenNames.Remove(ConfigurationManager.GetLocalisedValue(responseNode.responseText));
            currentGraph?.DialogGraph.RemoveChild(this, child);

        }

        // Action subscribed from the inspector's view, so it can refresh the list of items.
        OnNodeUpdate?.Invoke();
        // if (forceRefresh)
        // {
        //     var currentGraph = DialogueEditor.GetCurrentGraphView();
        //     if (currentGraph == null) return;
        //     currentGraph.PopulateView(currentGraph.DialogGraph);
        // }
        //     

    }

    public override BaseNode Clone()
    {
        var node = Instantiate(this);
        foreach (var child in Children)
        {
            node.Children.Add(child.Clone());
        }

        node.Update();
        return node;
    }
}