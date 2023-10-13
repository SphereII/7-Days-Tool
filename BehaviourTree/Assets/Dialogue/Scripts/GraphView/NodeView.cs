using System;
using Dialogue.Scripts.Nodes;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Dialogue
{
    public class NodeView : Node
    {
        public BaseNode Node;
        public Action<NodeView> OnNodeSelected;
        
        public Port Input;
        public Port Output;
        private SerializedObject _serializedObject;

        public bool alreadyMoved = false;

        public NodeView(BaseNode node) : base("Assets/Dialogue/UI/Node.uxml")
        {
            Node = node;
            title = node.name;
            viewDataKey = node.guid;

            // Sets the position of the node
            style.left = node.position.x;
            style.top = node.position.y;

            CreateInputPorts();
            CreateOutputPorts();
            SetupClasses();

            // Each node  has an inspector, for us to draw out the editor / drawers to display the properties.
            var inspectorView = this.Q<InspectorView>();
            inspectorView.UpdateSelection(this);

            Node.nodeView = this;

            // Add the unique key value for this fold out, so it'll remember the state.
            var foldout = this.Q<Foldout>();
            foldout.viewDataKey = node.guid;
            foldout.text = node.name;
        }

        // Helper method that will return a potentially concatenated display of long texts, useful for menus
        // and other controls where we want to show what's there, but not the full long text.
        public string GetText(int maxLength = -1)
        {
            var text = Node switch
            {
                StatementNode statementNode => statementNode.statementText,
                ResponseNode responseNode => responseNode.responseText,
                _ => typeof(Node).ToString()
            };

            if (string.IsNullOrEmpty(text))
                return "<No Response>";
            if (maxLength > 0 && text.Length > maxLength)
                return text.Substring(0, maxLength) + "...";
            return text;
        }

        public string GetCleanText(int maxLength = -1)
        {
            var text = Node switch
            {
                StatementNode statementNode => statementNode.statementText,
                ResponseNode responseNode => responseNode.responseText,
                _ => typeof(Node).ToString()
            };

            if (string.IsNullOrEmpty(text))
                return "<No Response>";
            if (maxLength > 0 && text.Length > maxLength)
                return text.Substring(0, maxLength);
            return text;
        }

        public void UpdateTitle()
        {
            title = GetText(30);
        }
        // Attaches the uss /css style sheets to the nodes.
        private void SetupClasses()
        {
            switch (Node)
            {
                case ImportNode:
                case GotoNode:
                    AddToClassList("GotoNode");
                    break;
                case RootNode:
                    AddToClassList("RootNode");
                    break;
                case RequirementNode:
                case ActionNode:
                    AddToClassList("ActionNode");
                    break;
                case ResponseNode:
                    AddToClassList("ResponseNode");
                    break;
                case StatementNode:
                    AddToClassList("StatementNode");
                    break;
            }
        }

        private void CreateOutputPorts()
        {
            var label = "Out";
            switch (Node)
            {
                case RootNode:
                case ResponseNode:
                    Output = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi,
                        typeof(bool));
                    label = "To Statement and/or actions";
                    break;
                case RequirementNode:
                    Output = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single,
                        typeof(bool));
                    label = "To Statement...";
                    break;
                case ImportNode:
                case ActionNode:
                    break;
                case StatementNode:
                    Output = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi,
                        typeof(bool));
                    label = "To Response...";
                    break;
            }

            if (Output == null) return;
            Output.portName = label;
            outputContainer.Add(Output);
        }

        private void CreateInputPorts()
        {
            var label = "In";

            switch (Node)
            {
                case RootNode:
                    break;

                case ResponseNode:
                case GotoNode:
                case ActionNode:
                    label = "From Response...";
                    Input = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi,
                        typeof(bool));
                    break;
                case ImportNode:
                case StatementNode:
                    label = "From Response...";
                    Input = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi,
                        typeof(bool));
                    break;
            }

            if (Input == null) return;
            Input.portName = label;
            inputContainer.Add(Input);
        }

      
        public sealed override string title
        {
            get => base.title;
            set => base.title = value;
        }

        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);
            Undo.RecordObject(Node, "Set Position");
            Node.position.x = newPos.xMin;
            Node.position.y = newPos.yMin;
            Node.Order = newPos.yMin;
            EditorUtility.SetDirty(Node); // Helps with save for record object
            alreadyMoved = true;

            if (Node is not ResponseNode responseNode) return;
            responseNode.parent.Update();
        }

        public void Collapse(bool full = false)
        {
            foreach (var foldout in this.Query<Foldout>().ToList())
            {
                if ( full == false && foldout.name != "CollapseFoldOut") continue;
                foldout.value = false;
            }
        }
        public void Expand(bool full = false)
        {
            foreach (var foldout in this.Query<Foldout>().ToList())
            {
                if ( full == false && foldout.name != "CollapseFoldOut") continue;
                foldout.value = true;
            }
        }
        public override void OnSelected()
        {
            base.OnSelected();
            Node.Update();
            OnNodeSelected?.Invoke(this);
        }
    }
}