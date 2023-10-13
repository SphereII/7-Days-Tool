using System.Collections.Generic;
using Dialogue;
using Dialogue.Editor;
using Dialogue.GameData.Dialogs;
using Dialogue.Nodes.CustomAttributes;
using UnityEngine;

public class ResponseNode : BaseNode
{
    public string responseText;
   
    public string id;
    [ReadOnly] public string nextstatementId;
    
    [Header("All requirements must be true for the player to see this response.")]
    public List<RequirementBase> requirements = new List<RequirementBase>();
    
    // To merge DialogAction in the response node, uncomment this,
    // Then edit the ResponseNodeView.uxml to change the visibility of the Dialog Actions List.
    //public List<DialogAction> actions = new List<DialogAction>();
    
    [HideInInspector] public List<BaseNode> Children = new List<BaseNode>();


    public StatementNode parent;
    public override void Update()
    {
        nodeView?.UpdateTitle();

        if (string.IsNullOrEmpty(id)) id = guid;
        foreach (var child in Children)
        {
            if ( child is StatementNode statement )
                nextstatementId = statement.id;
            if ( child is ActionNode actionNode)
                actionNode.Update();
        }
     
        if (parent == null) return;
        parent.Update();
    }

    public override BaseNode Clone()
    {
        var node = Instantiate(this);
        foreach( var child in Children)
            node.Children.Add(child.Clone());
        return node;
    }
    
    
}
