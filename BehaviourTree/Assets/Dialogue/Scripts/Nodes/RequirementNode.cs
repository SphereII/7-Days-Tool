using Dialogue;
using Dialogue.Editor;

public class RequirementNode : BaseNode
{
    
    // Currently not used, as requirements are embeded into the Response Node
    public BaseNode child;
 
    public override BaseNode Clone()
    {
        var node = Instantiate(this);
        node.child = child.Clone();
        return node;
    }
}
