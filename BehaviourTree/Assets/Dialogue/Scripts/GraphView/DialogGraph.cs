using System;
using System.Collections.Generic;
using Dialogue.Scripts.GraphView;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace Dialogue
{
    [CreateAssetMenu()]
    public class DialogGraph : ScriptableObject
    {
        public BaseNode rootNode;
        public List<BaseNode> nodes = new List<BaseNode>();
        public List<string> cvars = new List<string>();
        public BaseNode CreateNode(Type type)
        {
            var node = CreateInstance(type) as BaseNode;
            if (node == null) return null;
            node.name = type.Name;
            node.guid = GUID.Generate().ToString();

            Undo.RecordObject(this, "Dialog Add Node");
            nodes.Add(node);

            
            AssetDatabase.AddObjectToAsset(node, this);
            Undo.RegisterCreatedObjectUndo(node, "Dialog Create Node");
            AssetDatabase.SaveAssets();
            return node;
        }
       
        public void DeleteNode(BaseNode node)
        {
            Undo.RecordObject(this, "Dialog Delete Node");

            nodes.Remove(node);
            node.Update();

            Undo.DestroyObjectImmediate(node);
            AssetDatabase.SaveAssets();
        }

        public void AddChild(BaseNode parent, BaseNode child)
        {
            if (parent == null || child == null) return;
            var statement = parent as StatementNode;
            if (statement)
            {
                Undo.RecordObject(statement, "Dialog Add Child");
                statement.Children.Add(child);
                if (child is ResponseNode responseNode) 
                    responseNode.parent = statement;
            }

            var target = parent as RootNode;
            if (target)
            {
                if (child is not StatementNode ) return;
                Undo.RecordObject(target, "Dialog Add Child to root");
                target.child = child;
            }

            var response = parent as ResponseNode;
            if (response)
            {
                Undo.RecordObject(response, "Dialog Add Child");
                response.Children.Add(child);
            }
            EditorUtility.SetDirty(parent);
            EditorUtility.SetDirty(child);
            parent.Update();
            child.Update();

        
        }

        public void RemoveChild(BaseNode parent, BaseNode child)
        {
            var statement = parent as StatementNode;
            if (statement)
            {
                Undo.RecordObject(statement, "Dialog Remove Child");
                statement.Children.Remove(child );
            }

            var target = parent as RootNode;
            if (target)
            {
                Undo.RecordObject(target, "Dialog Remove child from root");
                target.child = null;
            }

            if (parent != null)
            {
                EditorUtility.SetDirty(parent);
                parent.Update();
            }

            if (child != null)
            {
                EditorUtility.SetDirty(child);
                child.Update();
            }

            

        }

        public List<BaseNode> GetChildren(BaseNode parent)
        {
            var children = new List<BaseNode>();
            var statement = parent as StatementNode;
            if (statement)
            {
                statement.Update();
                return statement.Children ;
            }

            var responseNode = parent as ResponseNode;
            if (responseNode)
            {
                responseNode.Update();
                return responseNode.Children;
            }

            var node = parent as RootNode;
            if (node && node.child != null)
                children.Add(node.child);
            return children;
        }

        private void Traverse(BaseNode node, Action<BaseNode> visitor)
        {
            if (node == null) return;
            visitor.Invoke(node);
            var children = GetChildren(node);
            children.ForEach((n) => Traverse(n, visitor));
        }


        public List<StatementNode> GetStatements()
        {
            var statementNodes = new List<StatementNode>();
            foreach (var node in nodes)
            {
                if (node is not StatementNode statementNode) continue;
                statementNodes.Add(statementNode);
            }
            return statementNodes;
        }
        public DialogGraph Clone()
        {
            var tree = Instantiate(this);
            tree.rootNode = tree.rootNode.Clone();
            tree.nodes = new List<BaseNode>();
            Traverse(tree.rootNode, (n) => { tree.nodes.Add(n); });
            return tree;
        }
    }
}