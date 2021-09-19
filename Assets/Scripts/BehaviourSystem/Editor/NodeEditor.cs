using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;


namespace ZetanStudio.BehaviourTree
{
    public sealed class NodeEditor : UnityEditor.Experimental.GraphView.Node
    {
        public Node node;
        public Port output;
        public Port input;

        private readonly Label abort;
        private readonly Action<NodeEditor> onSelected;
        private readonly Action<NodeEditor> onUnselected;
        private readonly Action<NodeEditor, Vector2> onSetPosition;

        public NodeEditor(Node node, Action<NodeEditor> onSelected, Action<NodeEditor> onUnselected,
                          Action<NodeEditor, Vector2> onSetPosition) : base(AssetDatabase.GetAssetPath(BehaviourTreeSettings.GetOrCreate().nodeUxml))
        {
            this.node = node;
            this.onSelected = onSelected;
            this.onUnselected = onUnselected;
            this.onSetPosition = onSetPosition;
            viewDataKey = node.guid;
            Type type = node.GetType();
            title = type.Name + (node.IsInstance && !node.IsRuntime ? "(clone)" : (node.IsRuntime ? "(R)" : string.Empty));
            var attr = type.GetCustomAttribute<NodeDescriptionAttribute>();
            if (attr != null) tooltip = attr.description;
            else tooltip = string.Empty;

            style.left = node._position.x;
            style.top = node._position.y;

            Label des = this.Q<Label>("description");
            des.bindingPath = "_description";
            des.Bind(new SerializedObject(node));

            abort = this.Q<Label>("abort");

            InitInput();
            InitOutput();
            InitClasses();
            UpdateStates();
            UpdateAbortType();
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt) { }

        private void InitInput()
        {
            if (node is Action || node is Conditional || node is Composite || node is Decorator)
                input = new NodePort(Direction.Input, Port.Capacity.Single);
            if (input != null)
            {
                input.portName = string.Empty;
                input.style.flexDirection = FlexDirection.Column;
                inputContainer.Add(input);
            }
        }
        private void InitOutput()
        {
            if (node is Composite) output = new NodePort(Direction.Output, Port.Capacity.Multi);
            else if (node is Decorator || node is Entry) output = new NodePort(Direction.Output, Port.Capacity.Single);
            if (output != null)
            {
                output.portName = string.Empty;
                output.style.flexDirection = FlexDirection.ColumnReverse;
                outputContainer.Add(output);
            }
        }

        private void InitClasses()
        {
            if (node is Action) AddToClassList("action");
            else if (node is Conditional) AddToClassList("conditional");
            else if (node is Composite) AddToClassList("composite");
            else if (node is Decorator) AddToClassList("decorator");
            else if (node is Entry) AddToClassList("entry");
            UpdateValid();
        }

        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);
            onSetPosition?.Invoke(this, node._position);
            node._position.x = newPos.xMin;
            node._position.y = newPos.yMin;
        }
        public override void OnSelected()
        {
            base.OnSelected();
            onSelected?.Invoke(this);
        }
        public override void OnUnselected()
        {
            base.OnUnselected();
            onUnselected?.Invoke(this);
        }

        public void TrySort()
        {
            if (node is Composite composite)
                composite.SortByPosition();
        }

        public void UpdateValid()
        {
            RemoveFromClassList("invalid");
            if (!node.IsValid) AddToClassList("invalid");
        }
        public void UpdateStates()
        {
            RemoveFromClassList("inactive");
            RemoveFromClassList("success");
            RemoveFromClassList("failure");
            RemoveFromClassList("running");

            if (node.IsInstance)
            {
                switch (node.State)
                {
                    case NodeStates.Inactive:
                        AddToClassList("inactive");
                        break;
                    case NodeStates.Success:
                        AddToClassList("success");
                        break;
                    case NodeStates.Failure:
                        AddToClassList("failure");
                        break;
                    case NodeStates.Running:
                        AddToClassList("running");
                        break;
                }
            }
        }
        public void UpdateAbortType()
        {
            if(node is Composite composite)
            {
                switch (composite.AbortType)
                {
                    case AbortType.Self:
                        abort.text = "↓";
                        abort.tooltip = "中止自我";
                        break;
                    case AbortType.LowerPriority:
                        abort.text = "→";
                        abort.tooltip = "中止更低优先";
                        break;
                    case AbortType.Both:
                        abort.text = "↓→";
                        abort.tooltip = "中止自我或更低优先";
                        break;
                    default:
                        abort.text = string.Empty;
                        abort.tooltip = string.Empty;
                        break;
                }
            }
        }

        private class NodePort : Port
        {
            public NodePort(Direction portDirection, Capacity portCapacity) : base(Orientation.Vertical, portDirection, portCapacity, typeof(bool))
            {
                m_EdgeConnector = new EdgeConnector<Edge>(new NodeEdgeConnectorListener());
                this.AddManipulator(m_EdgeConnector);
                style.width = 60.0f;
            }

            public override bool ContainsPoint(Vector2 localPoint)
            {
                Rect rect = new Rect(0, 0, layout.width, layout.height);
                return rect.Contains(localPoint);
            }

            private class NodeEdgeConnectorListener : IEdgeConnectorListener
            {
                private GraphViewChange graphViewChange;
                private readonly List<Edge> edgesToCreate;
                private readonly List<GraphElement> edgesToDelete;

                public NodeEdgeConnectorListener()
                {
                    graphViewChange.edgesToCreate = edgesToCreate = new List<Edge>();
                    edgesToDelete = new List<GraphElement>();
                }

                public void OnDrop(GraphView graphView, Edge edge)
                {
                    edgesToDelete.Clear();
                    if (edge.input.capacity == Capacity.Single)
                    {
                        foreach (var delete in edge.input.connections)
                        {
                            if (delete != edge) edgesToDelete.Add(delete);
                        }
                    }
                    if (edge.output.capacity == Capacity.Single)
                    {
                        foreach (var delete in edge.output.connections)
                        {
                            if (delete != edge) edgesToDelete.Add(delete);
                        }
                    }
                    if (edgesToDelete.Count > 0) graphView.DeleteElements(edgesToDelete);

                    edgesToCreate.Clear();
                    edgesToCreate.Add(edge);
                    List<Edge> edges = edgesToCreate;
                    if (graphView.graphViewChanged != null)
                        edges = graphView.graphViewChanged.Invoke(graphViewChange).edgesToCreate;
                    edges.ForEach(x => { graphView.AddElement(x); edge.input.Connect(x); edge.output.Connect(x); });
                }

                public void OnDropOutsidePort(Edge edge, Vector2 position) { }
            }
        }
    }
}