using System.Collections.Generic;
using System.ComponentModel;
using Dialogue.Scripts.Nodes;
using UnityEngine;
using UnityEngine.Serialization;

public class RootNode : BaseNode
{
    public BaseNode child;
    public string dialogId;
    public string localizationPrefix;
    public bool createLocalization;
    public bool createSounds;
    public bool createVoices;
    public bool createSoundActions;
    
    public List<DialogCVar> dialogCVarsList = new List<DialogCVar>();

    public override BaseNode Clone()
    {
        var node = Instantiate(this);
        node.child = child.Clone();
        return node;
    }
}