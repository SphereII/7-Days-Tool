using System.Collections.Generic;
using Dialogue.Scripts.Nodes;
using UnityEngine;

namespace Dialogue.Scripts.GraphView
{
    public class ViewLayouts
    {
        private DialogueGraphView _graphView;
        private DialogGraph _dialogGraph;
        private Vector2 rootPosition;
        private const int WidthSpacing = 50;
        private const int HeightSpacing = 200;

        public ViewLayouts(DialogueGraphView view, DialogGraph graph)
        {
            _graphView = view;
            _dialogGraph = graph;
        }

        public void UpdateLayout()
        {
            var rootNode = _dialogGraph.rootNode as RootNode;
            if (rootNode == null)
            {
                Debug.Log("No root node!");
                return;
            }

            foreach (var node in _dialogGraph.nodes)
            {
                node.nodeView.alreadyMoved = false;
            }
            var rootNodeView = FindNodeView(_dialogGraph.rootNode);
            var nodePosition = rootNodeView.GetPosition();
            // starting position
            rootPosition = nodePosition.position;

            var firstStatement = rootNode.child as StatementNode;
            if (firstStatement == null) return;

            AdjustPosition(firstStatement, rootPosition);
            foreach (var node in _dialogGraph.nodes)
            {
                node.nodeView.alreadyMoved = false;
            }
        }

        private void AdjustPosition(StatementNode statementNode, Vector2 position)
        {
            if (statementNode.nodeView.alreadyMoved) return;
            
            // Adjust the parent statement and position it correctly.
            var statementNodePosition = statementNode.nodeView.GetPosition();
            statementNodePosition.x = position.x;
            statementNodePosition.y = position.y;
            statementNodePosition.x += statementNodePosition.width + WidthSpacing;
            
            {
                statementNode.nodeView.SetPosition(statementNodePosition);
                statementNode.nodeView.alreadyMoved = true;
            }

            // Save that position as a response node position, so we can move the responses around.
            var subNodePosition = statementNodePosition;
            subNodePosition.x += statementNode.nodeView.GetPosition().width + WidthSpacing;
            foreach (var node in statementNode.Children)
            {
                switch (node)
                {
                    case StatementNode newStatementNode:
                        // If we are a statement node, base the position off the start statement, not the current response position, so it lines up correctly.
                        AdjustPosition(newStatementNode, statementNodePosition.position);
                        continue;
                    case ResponseNode responseNode:
                    {
                        node.nodeView.SetPosition(subNodePosition);
                        foreach (var subnode in responseNode.Children)
                        {
                            switch (subnode)
                            {
                                case StatementNode subStatement:
                                {
                                    var newColumn = subNodePosition;
                                    AdjustPosition(subStatement, newColumn.position);
                                    break;
                                }
                                default:
                                {
                                    var actionPosition = subNodePosition;
                                    actionPosition.y += responseNode.nodeView.GetPosition().height;
                                    actionPosition.x += 50;
                                    if (!subnode.nodeView.alreadyMoved)
                                    {
                                        subnode.nodeView.SetPosition(actionPosition);
                                        subnode.nodeView.alreadyMoved = true;
                                    }

                                    break;
                                }
                            }
                        }

                        break;
                    }
                }


                subNodePosition.y += node.nodeView.GetPosition().height + HeightSpacing;
            }
        }

        private NodeView FindNodeView(BaseNode node)
        {
            return _graphView.GetNodeByGuid(node.guid) as NodeView;
        }
    }
}