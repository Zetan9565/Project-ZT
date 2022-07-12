using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace ZetanStudio.DialogueSystem.Editor
{
    using Extension.Editor;
    using ZetanStudio.Editor;

    public class DialogueNode : Node
    {
        public DialogueContent content;
        public DialoguePort input;
        public readonly List<DialogueOutput> outputs = new List<DialogueOutput>();

        private readonly Action<DialogueNode> onSelected;
        private readonly Action<DialogueNode> onUnselected;
        private readonly Action<DialogueNode, Vector2> onSetPosition;
        private readonly Action<DialogueOutput> onDeleteOutput;

        public SerializedProperty SerializedContent { get; private set; }
        public SerializedProperty SerializedOptions { get; private set; }

        private readonly DialogueEditorSettings settings;
        private readonly Button newOption;
        private readonly IMGUIContainer inspector;
        private readonly DialogueView view;
        private readonly TextField talker;
        private readonly TextArea text;
        private readonly CurveField interval;

        public DialogueNode(DialogueView view, DialogueContent content, Action<DialogueNode> onSelected, Action<DialogueNode> onUnselected,
                          Action<DialogueNode, Vector2> onSetPosition, Action<DialogueOutput> onDeleteOutput, DialogueEditorSettings settings)
        {
            this.view = view;
            this.content = content;
            this.onSelected = onSelected;
            this.onUnselected = onUnselected;
            this.onSetPosition = onSetPosition;
            this.onDeleteOutput = onDeleteOutput;
            this.settings = settings;
            RefreshProperty();
            style.left = content._position.x;
            style.top = content._position.y;
            userData = content;
            viewDataKey = content.ID;
            expanded = true;
            title = content.GetName();
            titleContainer.style.height = 25;
            m_CollapseButton.pickingMode = PickingMode.Ignore;
            m_CollapseButton.Q("icon").visible = false;

            var width = DialogueContent.Editor.GetWidth(content.GetType());
            if (width <= 0) inputContainer.style.minWidth = 228f;
            else inputContainer.style.minWidth = width;
            #region ��ʼ����ڶ�
            if (content is not EntryContent)
            {
                inputContainer.Add(input = new DialogueInput());
                input.portName = string.Empty;
                input.AddManipulator(new ContextualMenuManipulator(evt =>
                {
                    evt.menu.AppendAction(Tr("�Ͽ�����"), a =>
                    {
                        input.View.DeleteElements(input.connections);
                    });
                }));
            }
            #endregion
            #region ��ʼ��ѡ���
            if (content is not INonOption)
            {
                if (content is not DecoratorContent)
                {
                    newOption = new Button(NewOption) { text = Tr("��ѡ��") };
                    titleButtonContainer.Add(newOption);
                }
                for (int i = 0; i < content.Options.Count; i++)
                {
                    InsertOutput(content.Options[i], i == 0);
                }
            }
            #endregion
            #region ��ʼ���ı���
            if (content is TextContent)
            {
                var talkerContianer = new VisualElement();
                talkerContianer.style.maxWidth = width <= 0 ? 228f : width;
                if (content is not EntryContent)
                {
                    talkerContianer.style.marginTop = -22;
                    talkerContianer.style.marginBottom = 1;
                    talkerContianer.style.marginLeft = 20;
                }
                inputContainer.Add(talkerContianer);
                talkerContianer.Add(talker = new TextField(Tr("������")));
                talker.multiline = true;
                talker.Q("unity-text-input").style.whiteSpace = WhiteSpace.Normal;
                talker.BindProperty(SerializedContent.FindAutoPropertyRelative("Talker"));
                EditorMiscFunction.SetAsKeywordsField(talker);
                talker.labelElement.style.minWidth = 60;
                inputContainer.Add(text = new TextArea(string.Empty, 35));
                text.style.maxWidth = width <= 0 ? 228f : width;
                text.BindProperty(SerializedContent.FindAutoPropertyRelative("Text"));
                EditorMiscFunction.SetAsKeywordsField(text);
                interval = new CurveField();
                interval.style.alignItems = Align.Center;
                interval.style.minWidth = 160f;
                interval.label = Tr("���ּ��");
                interval.BindProperty(SerializedContent.FindAutoPropertyRelative("UtterInterval"));
                interval.Q<Label>().style.minWidth = 0f;
                interval.Q<Label>().style.paddingTop = 0f;
                titleButtonContainer.Insert(1, interval);
                titleContainer.RegisterCallback<GeometryChangedEvent>(evt =>
                {
                    talkerContianer.style.maxWidth = inputContainer.layout.width;
                    text.style.maxWidth = inputContainer.layout.width;
                });
            }
            #endregion
            #region ��ʼ�������
            if (content is not ExitContent)
            {
                inspector = new IMGUIContainer(() =>
                {
                    if (SerializedContent != null && SerializedContent.serializedObject != null && SerializedContent.serializedObject.targetObject)
                    {
                        float oldLW = EditorGUIUtility.labelWidth;
                        EditorGUIUtility.labelWidth = 80;
                        SerializedContent.serializedObject.UpdateIfRequiredOrScript();
                        EditorGUI.BeginChangeCheck();
                        using var copy = SerializedContent.Copy();
                        SerializedProperty end = copy.GetEndProperty();
                        bool enter = true;
                        while (copy.NextVisible(enter) && !SerializedProperty.EqualContents(copy, end))
                        {
                            enter = false;
                            if (!copy.IsName("options") && !copy.IsName("events") && !copy.IsName("ID") && !copy.IsName("ExitHere") &&
                            !copy.IsName("Text") && !copy.IsName("Talker") && !copy.IsName("UtterInterval"))
                                EditorGUILayout.PropertyField(copy, true);
                        }
                        if (EditorGUI.EndChangeCheck()) SerializedContent.serializedObject.ApplyModifiedProperties();
                        EditorGUIUtility.labelWidth = oldLW;
                    }
                });
                inspector.style.marginLeft = 3;
                inspector.style.marginRight = 3;
                inspector.AddManipulator(new ContextualMenuManipulator(evt =>
                {

                }));
                inputContainer.Add(inspector);
            }
            #endregion
            RefreshExpandedState();
            RefreshPorts();
            RefreshOptionButton();
            EditorMiscFunction.RegisterTooltipCallback(this, () =>
            {
                StringBuilder sb = new StringBuilder();
                if (content is TextContent || content is BranchContent)
                {
                    string talker = null;
                    string words = null;
                    bool append = false;
                    if (content is TextContent text)
                    {
                        talker = text.Talker;
                        words = text.Text;
                        append = true;
                    }
                    else if (content is BranchContent branch && branch.Dialogue)
                    {
                        talker = branch.Dialogue.Entry.Talker;
                        words = branch.Dialogue.Entry.Text;
                        append = true;
                    }
                    if (append)
                    {
                        sb.Append('[');
                        if (string.IsNullOrEmpty(talker))
                            sb.Append(Tr("(δ����)"));
                        else if (talker.ToUpper() == "[NPC]") sb.Append("��������");
                        else if (talker.ToUpper() == "[PLAYER]") sb.Append(Tr("���"));
                        else sb.Append(Keywords.Editor.HandleKeyWords(talker));
                        sb.Append(']');
                        sb.Append(Tr("˵"));
                        sb.Append(": ");
                        if (string.IsNullOrEmpty(words))
                            sb.Append(Tr("(������)"));
                        else sb.Append(Keywords.Editor.HandleKeyWords(words));
                    }
                }
                return sb.ToString();
            });
        }

        #region ѡ�����
        private void NewOption()
        {
            if (content.Options.Count < 1)
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent(Tr("��Ҫѡ��")), false, () => NewOption(true));
                menu.AddItem(new GUIContent(Tr("��ͨѡ��")), false, () => NewOption(false));
                menu.ShowAsContext();
            }
            else if (!content.Options.FirstOrDefault().IsMain) NewOption(false);
        }
        private void NewOption(bool main)
        {
            if (content is INonOption || content is not TextContent && content is not BranchContent && content.Options.Count > 0) return;
            Undo.RecordObject(SerializedContent.serializedObject.targetObject, Tr("�޸� {0}", SerializedContent.serializedObject.targetObject.name));
            var option = DialogueContent.Editor.AddOption(content, main, main ? Tr("����") : ObjectNames.GetUniqueName(content.Options.Select(x => x.Title).ToArray(), Tr("��ѡ��")));
            if (option != null)
            {
                SerializedContent.serializedObject.UpdateIfRequiredOrScript();
                InsertOutput(option, main);
                RefreshPorts();
            }
        }
        private void DeleteOutput(DialogueOutput output)
        {
            onDeleteOutput?.Invoke(output);
            outputs.Remove(output);
            outputContainer.Remove(output);
            Undo.RecordObject(SerializedContent.serializedObject.targetObject, Tr("�޸� {0}", SerializedContent.serializedObject.targetObject.name));
            DialogueContent.Editor.RemoveOption(content, output.userData as DialogueOption);
            RefreshPorts();
            RefreshOptionButton();
            outputs.ForEach(o => o.RefreshProperty());
        }
        public DialogueOutput InsertOutput(DialogueOption option, bool main)
        {
            if (content is INonOption || content is not TextContent && content is not BranchContent && outputs.Count > 0) return null;
            var output = new DialogueOutput(option, content is DecoratorContent ? null : DeleteOutput);
            if (main) output.portName = Tr("��Ҫ");
            outputs.Add(output);
            outputContainer.Add(output);
            output.AddManipulator(new ContextualMenuManipulator(evt =>
            {
                if (option.IsMain && !content.ExitHere && content is not DecoratorContent)
                    evt.menu.AppendAction(Tr("תΪ��ͨѡ��"), a =>
                    {
                        Undo.RecordObject(SerializedContent.serializedObject.targetObject, Tr("�޸� {0}", SerializedContent.serializedObject.targetObject.name));
                        DialogueOption.Editor.SetIsMain(option, false);
                        SerializedContent.serializedObject.UpdateIfRequiredOrScript();
                        output.SerializedOption.FindAutoPropertyRelative("Title").stringValue = ObjectNames.GetUniqueName(content.Options.Select(x => x.Title).ToArray(), Tr("��ѡ��"));
                        SerializedContent.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                        output.RefreshIsMain();
                        output.RefreshProperty();
                        RefreshOptionButton();
                        output.portName = string.Empty;
                    });
                else if (!option.IsMain && content.Options.Count == 1)
                    evt.menu.AppendAction(Tr("תΪ��Ҫѡ��"), a =>
                    {
                        Undo.RecordObject(SerializedContent.serializedObject.targetObject, Tr("�޸� {0}", SerializedContent.serializedObject.targetObject.name));
                        DialogueOption.Editor.SetIsMain(option, true);
                        SerializedContent.serializedObject.UpdateIfRequiredOrScript();
                        output.SerializedOption.FindAutoPropertyRelative("Title").stringValue = string.Empty;
                        SerializedContent.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                        output.RefreshIsMain();
                        output.RefreshProperty();
                        RefreshOptionButton();
                        output.portName = Tr("��Ҫ");
                    });
            }));
            RefreshPorts();
            RefreshOptionButton();
            output.RefreshProperty();
            return output;
        }
        #endregion

        #region ��д
        protected override void ToggleCollapse() { }
        public override void OnSelected()
        {
            base.OnSelected();
            BringToFront();
            onSelected?.Invoke(this);
        }
        public override void OnUnselected()
        {
            base.OnUnselected();
            onUnselected?.Invoke(this);
        }
        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);
            onSetPosition?.Invoke(this, content._position);
            content._position.x = newPos.xMin;
            content._position.y = newPos.yMin;
        }
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt) { }
        #endregion

        #region ˢ�����
        public void RefreshProperty()
        {
            if (content is not ExitContent)
            {
                view.SerializedContents.serializedObject.Update();
                SerializedContent = view.SerializedContents.GetArrayElementAtIndex(view.Dialogue.Contents.IndexOf(content));
                if (content is not INonOption) SerializedOptions = SerializedContent.FindPropertyRelative("options");
                if (content is TextContent)
                {
                    talker?.BindProperty(SerializedContent.FindAutoPropertyRelative("Talker"));
                    text?.BindProperty(SerializedContent.FindAutoPropertyRelative("Text"));
                    interval?.BindProperty(SerializedContent.FindAutoPropertyRelative("UtterInterval"));
                }
                outputs.ForEach(o => o.RefreshProperty());
            }
        }

        public void RefreshOptionButton()
        {
            newOption?.SetEnabled(content.Options.Count < 1 || content.Options.Count > 0 && !content.Options[0].IsMain && (content is TextContent || content is BranchContent) && !content.ExitHere);
        }
        #endregion

        private string Tr(string text) => L.Tr(settings.language, text);
        private string Tr(string text, params object[] args) => L.Tr(settings.language, text, args);
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

#pragma warning disable IDE1006 // ������ʽ
        public new DialogueNode node => base.node as DialogueNode;
#pragma warning restore IDE1006 // ������ʽ

        public DialogueOutput(DialogueOption option, Action<DialogueOutput> delete) : base(Direction.Output, Capacity.Single)
        {
            userData = option;
            if (delete != null)
            {
                var button = new Button(() => delete(this));
                button.text = "��";
                button.style.marginLeft = -2;
                button.style.marginRight = -2;
                contentContainer.Add(button);
            }
            RefreshIsMain();
        }
        public void RefreshProperty()
        {
            SerializedOption = node.SerializedOptions.GetArrayElementAtIndex(node.content.Options.IndexOf(Option));
            titleField?.BindProperty(SerializedOption.FindAutoPropertyRelative("Title"));
        }
        public void RefreshIsMain()
        {
            if (!Option.IsMain)
            {
                m_ConnectorText.style.display = DisplayStyle.None;
                titleField = new TextField();
                contentContainer.Insert(1, titleField);
            }
            else if (titleField != null)
            {
                m_ConnectorText.style.display = DisplayStyle.Flex;
                contentContainer.Remove(titleField);
            }
        }
    }

    public abstract class DialoguePort : Port
    {
        public DialogueView View => m_GraphView as DialogueView;

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
                    output.View.OnEdgeDropOutside(output, position);
            }
        }
    }
}