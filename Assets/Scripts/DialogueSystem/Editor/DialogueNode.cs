using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
        public DialoguePort Input { get; private set; }
        private readonly List<DialogueOutput> outputs = new List<DialogueOutput>();
        public ReadOnlyCollection<DialogueOutput> Outputs => new ReadOnlyCollection<DialogueOutput>(outputs);

        private readonly Action onBeforeModify;
        private readonly Action<DialogueNode> onSelected;
        private readonly Action<DialogueNode> onUnselected;
        private readonly Action<DialogueOutput> onDeleteOutput;

        public DialogueContent Content { get; private set; }
        public SerializedProperty SerializedContent { get; private set; }
        public SerializedProperty SerializedOptions { get; private set; }

        private readonly Button newOption;
        private readonly IMGUIContainer inspector;
        private readonly DialogueView view;
        private readonly TextField talker;
        private readonly TextArea text;
        private readonly CurveField interval;

        public DialogueNode(DialogueView view, DialogueContent content, Action onBeforeModify, Action<DialogueNode> onSelected, Action<DialogueNode> onUnselected, Action<DialogueOutput> onDeleteOutput)
        {
            titleContainer.style.height = 25;
            expanded = true;
            m_CollapseButton.pickingMode = PickingMode.Ignore;
            m_CollapseButton.Q("icon").visible = false;

            this.view = view;
            Content = content;
            if (!content)
            {
                title = Tr("类型丢失的结点");
                titleButtonContainer.Add(new Button(() => view.DeleteElements(new GraphElement[] { this })) { text = Tr("删除") });
                inputContainer.Add(new Button(() => EditorWindow.GetWindow<ReferencesFixing>().Show()) { text = Tr("尝试修复") });
                return;
            }
            this.onSelected = onSelected;
            this.onUnselected = onUnselected;
            this.onDeleteOutput = onDeleteOutput;
            RefreshProperty();
            title = content.GetName();
            style.left = content._position.x;
            style.top = content._position.y;
            userData = content;
            viewDataKey = content.ID;

            var width = DialogueContent.Editor.GetWidth(content.GetType());
            if (width <= 0) inputContainer.style.minWidth = 228f;
            else inputContainer.style.minWidth = width;
            #region 初始化入口端
            if (content is not EntryContent)
            {
                inputContainer.Add(Input = new DialogueInput());
                Input.portName = string.Empty;
                Input.AddManipulator(new ContextualMenuManipulator(evt =>
                {
                    if (Input.connections.Any())
                        evt.menu.AppendAction(Tr("断开所有"), a =>
                        {
                            Input.View.DeleteElements(Input.connections);
                        });
                }));
            }
            #endregion
            #region 初始化选项功能
            if (content is not SuffixContent)
            {
                if (content is not IMainOptionOnly)
                {
                    newOption = new Button(NewOption) { text = Tr("新选项") };
                    titleButtonContainer.Add(newOption);
                }
                for (int i = 0; i < content.Options.Count; i++)
                {
                    InsertOutputInternal(content.Options[i]);
                }
            }
            #endregion
            #region 初始化文本区
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
                talkerContianer.Add(talker = new TextField(Tr("讲述人")));
                talker.multiline = true;
                talker.Q("unity-text-input").style.whiteSpace = WhiteSpace.Normal;
                talker.BindProperty(SerializedContent.FindAutoProperty("Talker"));
                EditorMiscFunction.SetAsKeywordsField(talker);
                talker.labelElement.style.minWidth = 60;
                inputContainer.Add(text = new TextArea(string.Empty, 35));
                text.style.maxWidth = width <= 0 ? 228f : width;
                text.BindProperty(SerializedContent.FindAutoProperty("Text"));
                EditorMiscFunction.SetAsKeywordsField(text);
                interval = new CurveField();
                interval.style.alignItems = Align.Center;
                interval.style.minWidth = 160f;
                interval.label = Tr("吐字间隔");
                interval.BindProperty(SerializedContent.FindAutoProperty("UtterInterval"));
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
            #region 初始化检查器
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
            this.RegisterTooltipCallback(() =>
            {
                if (content is TextContent textContent) return textContent.Preview();
                else if (content is OtherDialogueContent other && other.Dialogue) return other.Dialogue.Entry.Preview();
                else return null;
            });
            this.onBeforeModify = onBeforeModify;
        }

        #region 选项相关
        private void NewOption()
        {
            if (Content is BranchContent) NewOption(true);
            else if (Content.Options.Count < 1)
            {
                if (Content is not ExternalOptionsContent)
                {
                    GenericMenu menu = new GenericMenu();
                    menu.AddItem(new GUIContent(Tr("主要选项")), false, () => NewOption(true));
                    menu.AddItem(new GUIContent(Tr("普通选项")), false, () => NewOption(false));
                    menu.ShowAsContext();
                }
                else NewOption(false);
            }
            else if (!Content.Options.FirstOrDefault().IsMain || Content is ExternalOptionsContent) NewOption(false);
        }
        private void NewOption(bool main)
        {
            if (!CanAddOption()) return;
            onBeforeModify?.Invoke();
            var option = DialogueContent.Editor.AddOption(Content, main, main ? Tr("继续") : ObjectNames.GetUniqueName(Content.Options.Select(x => x.Title).ToArray(), Tr("新选项")));
            if (option != null)
            {
                SerializedContent.serializedObject.UpdateIfRequiredOrScript();
                InsertOutputInternal(option);
                RefreshPorts();
            }
        }

        private void DeleteOutput(DialogueOutput output)
        {
            onDeleteOutput?.Invoke(output);
            outputs.Remove(output);
            outputContainer.Remove(output);
            onBeforeModify?.Invoke();
            DialogueContent.Editor.RemoveOption(Content, output.userData as DialogueOption);
            RefreshPorts();
            RefreshOptionButton();
            outputs.ForEach(o => o.RefreshProperty());
        }
        public DialogueOutput InsertOutput(DialogueOption option)
        {
            if (!CanAddOption()) return null;
            return InsertOutputInternal(option);
        }
        private DialogueOutput InsertOutputInternal(DialogueOption option)
        {
            var output = new DialogueOutput(option, Content is IMainOptionOnly and ConditionContent ? null : DeleteOutput);
            if (option.IsMain) output.portName = Tr(Content is BranchContent ? "分支" : "主要");
            outputs.Add(output);
            outputContainer.Add(output);
            output.AddManipulator(new ContextualMenuManipulator(evt =>
            {
                if (Content.Options.Count == 1)
                {
                    if (option.IsMain && !Content.ExitHere && Content is not IMainOptionOnly and not BranchContent)
                        evt.menu.AppendAction(Tr("转为普通选项"), a =>
                        {
                            onBeforeModify?.Invoke();
                            DialogueOption.Editor.SetIsMain(option, false);
                            SerializedContent.serializedObject.UpdateIfRequiredOrScript();
                            output.SerializedOption.FindAutoProperty("Title").stringValue = ObjectNames.GetUniqueName(Content.Options.Select(x => x.Title).ToArray(), Tr("新选项"));
                            SerializedContent.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                            output.RefreshIsMain();
                            output.RefreshProperty();
                            RefreshOptionButton();
                            output.portName = string.Empty;
                        });
                    else if (!option.IsMain)
                        evt.menu.AppendAction(Tr("转为主要选项"), a =>
                        {
                            onBeforeModify?.Invoke();
                            DialogueOption.Editor.SetIsMain(option, true);
                            SerializedContent.serializedObject.UpdateIfRequiredOrScript();
                            output.SerializedOption.FindAutoProperty("Title").stringValue = string.Empty;
                            SerializedContent.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                            output.RefreshIsMain();
                            output.RefreshProperty();
                            RefreshOptionButton();
                            output.portName = Tr("主要");
                        });
                }
                else if (!option.IsMain)
                {
                    var index = Content.Options.IndexOf(option);
                    if (index > 0)
                        evt.menu.AppendAction(Tr("上移选项"), a =>
                        {
                            onBeforeModify?.Invoke();
                            DialogueContent.Editor.MoveOptionUpward(Content, index);
                            SerializedOptions.serializedObject.UpdateIfRequiredOrScript();
                            output.PlaceBehind(outputs[index - 1]);
                            (outputs[index], outputs[index - 1]) = (outputs[index - 1], outputs[index]);
                            outputs.ForEach(o => o.RefreshProperty());
                        });
                    if (index < Content.Options.Count - 1)
                        evt.menu.AppendAction(Tr("下移选项"), a =>
                        {
                            onBeforeModify?.Invoke();
                            DialogueContent.Editor.MoveOptionDownward(Content, index);
                            SerializedOptions.serializedObject.UpdateIfRequiredOrScript();
                            output.PlaceInFront(outputs[index + 1]);
                            (outputs[index], outputs[index + 1]) = (outputs[index + 1], outputs[index]);
                            outputs.ForEach(o => o.RefreshProperty());
                        });
                }
            }));
            RefreshPorts();
            RefreshOptionButton();
            output.RefreshProperty();
            return output;

        }
        private bool CanAddOption()
        {
            return !(Content is SuffixContent || Content is IMainOptionOnly && Content.Options.Count > 1) || Content is BranchContent;
        }
        #endregion

        #region 重写
        protected override void ToggleCollapse() { }
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
        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);
            if (!Content) return;
            onBeforeModify?.Invoke();
            Content._position.x = newPos.xMin;
            Content._position.y = newPos.yMin;
        }
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt) { }
        #endregion

        #region 刷新相关
        public void RefreshProperty()
        {
            if (Content is not ExitContent)
            {
                view.SerializedContents.serializedObject.Update();
                SerializedContent = view.SerializedContents.GetArrayElementAtIndex(view.Dialogue.Contents.IndexOf(Content));
                if (Content is not SuffixContent) SerializedOptions = SerializedContent.FindPropertyRelative("options");
                if (Content is TextContent)
                {
                    talker?.BindProperty(SerializedContent.FindAutoProperty("Talker"));
                    text?.BindProperty(SerializedContent.FindAutoProperty("Text"));
                    interval?.BindProperty(SerializedContent.FindAutoProperty("UtterInterval"));
                }
                outputs.ForEach(o => o.RefreshProperty());
            }
        }

        public void RefreshOptionButton()
        {
            newOption?.SetEnabled(Content is ExternalOptionsContent or BranchContent || Content.Options.Count < 1
                                  || Content.Options.Count > 0 && !Content.Options[0].IsMain && Content is TextContent && !Content.ExitHere);
        }
        #endregion

        private string Tr(string text) => L.Tr(view.settings.language, text);
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
        public new DialogueNode node => base.node as DialogueNode;
#pragma warning restore IDE1006 // 命名样式

        private ContextualMenuManipulator manipulator;

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
            SerializedOption = node.SerializedOptions.GetArrayElementAtIndex(node.Content.Options.IndexOf(Option));
            titleField?.BindProperty(SerializedOption.FindAutoProperty("Title"));
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