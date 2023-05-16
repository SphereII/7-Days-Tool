using System.Collections.Generic;
using Dialogue.GameData.Dialogs;
using UnityEngine;

public class ActionNode : BaseNode
{
    [Header("A list of actions that will fire when the player selects this response.")]
    public List<DialogAction> actions = new List<DialogAction>();
}
