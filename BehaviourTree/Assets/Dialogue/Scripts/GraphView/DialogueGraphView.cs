using System;
using System.Collections.Generic;
using Dialogue.Scripts.Nodes;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Dialogue
{
    public class DialogueGraphView : GraphView
    {
        public new class UxmlFactory : UxmlFactory<DialogueGraphView, UxmlTraits>
        {
        }

        public DialogGraph DialogGraph;
        public Action<NodeView> OnNodeSelected;

        public DialogueGraphView()
        {
            Insert(0, new GridBackground());

            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new FreehandSelector());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());


            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Dialogue/UI/uss/DialogueEditor.uss");
            styleSheets.Add(styleSheet);
            Undo.undoRedoPerformed += OnUndoRedo;
        }

        private void OnUndoRedo()
        {
            PopulateView(DialogGraph);
            AssetDatabase.SaveAssets();
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            var position = viewTransform.matrix.inverse.MultiplyPoint(evt.localMousePosition);

            evt.menu.AppendAction($"Add Statement", (_) => CreateNodeAtPosition(typeof(StatementNode), position));
            evt.menu.AppendAction($"Add Response", (_) => CreateNodeAtPosition(typeof(ResponseNode), position));
            evt.menu.AppendAction($"Add Action", (_) => CreateNodeAtPosition(typeof(ActionNode), position));
            evt.menu.AppendAction($"Add GoTo", (_) => CreateNodeAtPosition(typeof(GotoNode), position));

            if (evt.target is NodeView target)
            {
                evt.menu.AppendSeparator();
                if (target.Node is not StatementNode) // only clone non-statement node.
                    evt.menu.AppendAction($"Clone {target.Node.GetType()}", _ => Clone(target.Node));

                if (target.Node is not RootNode) // Don't allow them to delete the root node.
                    evt.menu.AppendAction($"Delete {target.Node.GetType()}", _ => DeleteNode(target));

                evt.menu.AppendSeparator();
                evt.menu.AppendAction($"Collapse {target.Node.GetType()}", _ => Collapse(target));
                evt.menu.AppendAction($"Expand {target.Node.GetType()}", _ => Expand(target));
            }

            evt.menu.AppendSeparator();
            evt.menu.AppendAction($"Collapse All", _ => Collapse(null));
            evt.menu.AppendAction($"Collapse All Full", _ => Collapse(null, true));
            evt.menu.AppendSeparator();
            evt.menu.AppendAction($"Expand All", _ => Expand(null));
            evt.menu.AppendAction($"Expand All Full", _ => Expand(null, true));
            evt.menu.AppendSeparator();
            evt.menu.AppendAction("Auto Sort", _ => UpdateLayout());
        }

        private void Expand(NodeView target, bool full = false)
        {
            if (target == null)
            {
                foreach (var node in DialogGraph.nodes)
                {
                    node.nodeView.Expand(full);
                }
            }
            else
            {
                target.Expand(full);
            }
        }

        private void Collapse(NodeView target, bool full = false)
        {
            if (target == null)
            {
                foreach (var node in DialogGraph.nodes)
                {
                    node.nodeView.Collapse(full);
                }
            }
            else
            {
                target.Collapse(full);
            }
        }

        private void DeleteNode(NodeView nodeView)
        {
            DialogGraph.DeleteNode(nodeView.Node);
            PopulateView(DialogGraph);
        }

        private void Clone(BaseNode originalNode)
        {
            var type = originalNode.GetType();
            switch (originalNode)
            {
                case ActionNode actionNode:
                {
                    var node = DialogGraph.CreateNode(type) as ActionNode;
                    if (node == null) return;
                    node.actions = actionNode.actions;
                    node.position.x = originalNode.position.x + 100;
                    node.position.y = originalNode.position.y + 100;
                    CreateNodeView(node);
                    break;
                }
                case ResponseNode responseNode:
                {
                    var node = DialogGraph.CreateNode(type) as ResponseNode;
                    if (node == null) return;
                    node.nextstatementId = responseNode.nextstatementId;
                    node.Children = responseNode.Children;
                    node.requirements = responseNode.requirements;
                    node.responseText = responseNode.responseText;
                    node.position.x = originalNode.position.x + 100;
                    node.position.y = originalNode.position.y + 100;
                    CreateNodeView(node);
                    break;
                }
            }
        }

        private NodeView FindNodeView(BaseNode node)
        {
            return GetNodeByGuid(node.guid) as NodeView;
        }

        private void CreateNodeAtPosition(Type type, Vector2 position)
        {
            // Trying to be mindful of using { } around single line if calls
            if (DialogGraph == null)
            {
                DialogGraph = ScriptableObject.CreateInstance<DialogGraph>();
            }

            var node = DialogGraph.CreateNode(type);
            if (node == null) return;
            node.position = position;
            CreateNodeView(node);
        }

        public void PopulateView(DialogGraph tree)
        {
            DialogGraph = tree;

            graphViewChanged -= OnGraphViewChanged;

            // Clear out any possible elements.
            DeleteElements(graphElements);
            graphViewChanged += OnGraphViewChanged;

            // If there's no root node, then we must be in a new graph, so generate a template to give the users something to start with.
            if (DialogGraph.rootNode == null)
            {
                DialogGraph.rootNode = DialogGraph.CreateNode(typeof(RootNode)) as RootNode;
                var rootNode = DialogGraph.rootNode as RootNode;
                if (rootNode != null)
                {
                    rootNode.dialogId = DialogGraph.name;
                    var statementNode = DialogGraph.CreateNode(typeof(StatementNode)) as StatementNode;
                    if (statementNode != null)
                    {
                        statementNode.statementText = "Hello there";
                        var responseNode = DialogGraph.CreateNode(typeof(ResponseNode)) as ResponseNode;
                        if (responseNode != null)
                        {
                            responseNode.responseText = "Hi";
                            DialogGraph.AddChild(statementNode, responseNode);
                        }

                        DialogGraph.AddChild(rootNode, statementNode);
                    }
                }

                EditorUtility.SetDirty(DialogGraph);
                AssetDatabase.SaveAssets();
            }

            tree.nodes.ForEach(CreateNodeView);

            // create the edges
            tree.nodes.ForEach(n =>
            {
                var children = tree.GetChildren(n);
                children.ForEach(c =>
                {
                    var parentView = FindNodeView(n);
                    var childView = FindNodeView(c);
                    if (childView == null || parentView == null) return;


                    var edge = parentView.Output.ConnectTo(childView.Input);
                    AddElement(edge);
                });
            });
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var tempPorts = new List<Port>();
            foreach (var port in ports.ToList())
            {
                // Don't map to itself
                if (port.direction == startPort.direction) continue;
                if (port.node == startPort.node) continue;

                tempPorts.Add(port);
            }

            return tempPorts;
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange graphviewchange)
        {
            if (graphviewchange.elementsToRemove != null)
            {
                graphviewchange.elementsToRemove.ForEach(elem =>
                {
                    switch (elem)
                    {
                        case NodeView nodeView:
                            DialogGraph.DeleteNode(nodeView.Node);
                            break;
                        case Edge edge:
                        {
                            var parentView = edge.output.node as NodeView;
                            var childView = edge.input.node as NodeView;
                            DialogGraph.RemoveChild(parentView?.Node, childView?.Node);
                            break;
                        }
                    }
                });
            }

            if (graphviewchange.edgesToCreate != null)
            {
                graphviewchange.edgesToCreate.ForEach(edge =>
                {
                    var parentView = edge.output.node as NodeView;
                    var childView = edge.input.node as NodeView;
                    if (parentView == null || childView == null) return;

                    DialogGraph.AddChild(parentView.Node, childView.Node);
                });
            }

            return graphviewchange;
        }
        private void CreateNodeView(BaseNode node)
        {
            var nodeView = new NodeView(node);
            nodeView.OnNodeSelected += OnNodeSelected;
            AddElement(nodeView);
        }

        public void UpdateLayout()
        {
            var rootNode = DialogGraph.rootNode as RootNode;
            if (rootNode == null)
            {
                Debug.Log("No root node!");
                return;
            }

            var nodeView = FindNodeView(DialogGraph.rootNode);
            var position = nodeView.GetPosition().position;
            AdjustPosition(DialogGraph.rootNode, ref position);
            var counter = 0;
            foreach (var node in DialogGraph.nodes)
            {
                // We've already placed the root, we want it in the default position, and everything off of it.
                if (node is RootNode) continue;
                AdjustPosition(node, ref position);
                
                // After the counter reaches its limit, it resets to a new row on the graph.
                counter++;
                if (counter <= 5) continue;
                position.x = nodeView.GetPosition().position.x + 100;
                position.y += 100;
                counter = 0;
            }
        }

        private void AdjustPosition(BaseNode node, ref Vector2 position)
        {
            var nodeView = FindNodeView(node);
            var nodePosition = nodeView.GetPosition();

            nodePosition.x = position.x;
            nodePosition.y = position.y;
            nodeView.SetPosition(nodePosition);
            position.y += 50;
            position.x += nodePosition.width + 50;
        }
    }
}