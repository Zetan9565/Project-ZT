using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace ZetanStudio.DialogueSystem.Editor
{
    using Extension.Editor;

    public abstract class DialoguePort : Port
    {
        public DialogueGraph Graph => m_GraphView as DialogueGraph;

        public DialoguePort(Direction portDirection, Capacity capacity) : base(Orientation.Horizontal, portDirection, capacity, typeof(bool))
        {
            m_EdgeConnector = new EdgeConnector<Edge>(new NodeEdgeConnectorListener());
            this.AddManipulator(m_EdgeConnector);
        }

        public override bool ContainsPoint(Vector2 localPoint)
        {
            return new Rect(0, 0, layout.width, layout.height).Contains(localPoint);
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

            public void OnDropOutsidePort(Edge edge, Vector2 position)
            {
                if (edge.input == null && edge.output is DialogueOutput output)
                    output.Graph.OnEdgeDropOutside(output, position);
            }
        }
    }

    public sealed class DialogueInput : DialoguePort
    {
        public DialogueInput() : base(Direction.Input, Capacity.Multi)
        {
            style.width = 30;
            m_ConnectorBox.style.minWidth = 8;
            m_ConnectorText.style.textOverflow = TextOverflow.Ellipsis;
        }
    }
    public sealed class DialogueOutput : DialoguePort
    {
        public SerializedProperty SerializedOption { get; private set; }
        private TextField titleField;
        public DialogueOption Option => userData as DialogueOption;

#pragma warning disable IDE1006 // 命名样式
        public new DialogueGraphNode node => base.node as DialogueGraphNode;
#pragma warning restore IDE1006 // 命名样式

        public DialogueOutput(DialogueOption option, Action<DialogueOutput> delete) : base(Direction.Output, Capacity.Single)
        {
            userData = option;
            if (delete != null)
            {
                var button = new Button(() => delete(this));
                button.text = "×";
                button.style.marginLeft = -2;
                button.style.marginRight = -2;
                contentContainer.Add(button);
            }
            RefreshIsMain();
        }
        public void RefreshProperty()
        {
            SerializedOption = node.SerializedOptions.GetArrayElementAtIndex(node.Target.Options.IndexOf(Option));
            titleField?.BindProperty(SerializedOption.FindAutoProperty("Title"));
        }
        public void RefreshIsMain()
        {
            if (!Option.IsMain)
            {
                m_ConnectorText.style.display = DisplayStyle.None;
                if (titleField == null)
                {
                    titleField = new TextField();
                    contentContainer.Insert(1, titleField);
                }
                else titleField.style.display = DisplayStyle.Flex;
            }
            else
            {
                m_ConnectorText.style.display = DisplayStyle.Flex;
                if (titleField != null) titleField.style.display = DisplayStyle.None;
            }
        }
    }
}