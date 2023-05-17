using System;
using System.Collections.Generic;
using UnityEngine.Serialization;

namespace Dialogue.Scripts.Nodes
{
    public class ImportNode : BaseNode
    {
        public DialogGraph importedDialog;

        public DialogGraph CloneDialogGraph()
        {
            return importedDialog == null ? null : Instantiate(importedDialog);
        }

        public List<BaseNode> GetChildren()
        {
            var children = new List<BaseNode>();
            var statementNode = GetStatementNode();
            return statementNode == null ? children : statementNode.Children;
        }

        public string GetOriginalStatementID()
        {
            var statementID = string.Empty;
            var statementNode = GetStatementNode();
            return statementNode == null ? statementID : statementNode.id;
        }

        private StatementNode GetStatementNode()
        {
            if (importedDialog == null) return null;
            if (importedDialog.rootNode == null) return null;
            var root = importedDialog.rootNode as RootNode;
            if (root == null) return null;
            var statementNode = root.child as StatementNode;
            return statementNode;
        }
    }
}