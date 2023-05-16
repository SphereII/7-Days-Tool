using System;
using Dialogue;
using Dialogue.Nodes.CustomAttributes;
using UnityEngine;

public abstract class BaseNode : ScriptableObject
{
    public Action OnNodeUpdate;

    public NodeView nodeView;
    // Read only Attribute class defined in the ReadOnlyAttribute.cs, and a matching PropertyDrawer.
    // Why is this so convoluted, Unity?
    [ReadOnly] 
    public string guid;
    
    [HideInInspector]
    public Vector2 position;

    public virtual void Update()
    {
    }

    public virtual BaseNode Clone()
    {
        return Instantiate(this);
    }
}
