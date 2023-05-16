

public class RootNode : BaseNode
{
    public BaseNode child;
    public string dialogId;
    public string localizationPrefix;
    public bool createLocalization;

    public override BaseNode Clone()
    {
        var node = Instantiate(this);
        node.child = child.Clone();
        return node;
    }
}
