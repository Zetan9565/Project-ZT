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

    public class DialogueGraphNode : Node
    {
        #region 端口声明
        public DialoguePort Input { get; private set; }
        private readonly List<DialogueOutput> outputs = new List<DialogueOutput>();
        public ReadOnlyCollection<DialogueOutput> Outputs => new ReadOnlyCollection<DialogueOutput>(outputs);
        #endregion

        #region 序列化相关
        public DialogueNode Target { get; private set; }
        public SerializedProperty SerializedNode { get; private set; }
        public SerializedProperty SerializedOptions { get; private set; }
        private readonly HashSet<string> hiddenFields = new HashSet<string>();
        #endregion

        #region 回调
        private readonly Action onBeforeModify;
        private readonly Action<DialogueGraphNode> onSelected;
        private readonly Action<DialogueGraphNode> onUnselected;
        private readonly Action<DialogueOutput> onDeleteOutput;
        #endregion

        #region 控件声明
        private readonly Button addOption;
        private readonly IMGUIContainer inspector;
        private readonly DialogueGraph graph;
#if ZTDS_ENABLE_PORTRAIT
        private readonly PropertyField portrait;
        private readonly EnumField portraitSide;
#endif
        private readonly TextField talker;
        private readonly TextArea text;
        private readonly CurveField interval;
        #endregion

        /// <summary>
        /// 对话编辑器结点构造函数
        /// </summary>
        /// <param name="graph">包含此结点的视图</param>
        /// <param name="node">此结点要绘制的对话结点</param>
        /// <param name="onBeforeModify">修改前回调</param>
        /// <param name="onSelected">选中回调</param>
        /// <param name="onUnselected">取消选中回调</param>
        /// <param name="onDeleteOutput">选项删除回调</param>
        public DialogueGraphNode(DialogueGraph graph, DialogueNode node, Action onBeforeModify, Action<DialogueGraphNode> onSelected, Action<DialogueGraphNode> onUnselected, Action<DialogueOutput> onDeleteOutput)
        {
            titleContainer.style.height = 25;
            expanded = true;
            m_CollapseButton.pickingMode = PickingMode.Ignore;
            m_CollapseButton.Q("icon").visible = false;

            this.graph = graph;
            Target = node;
            if (!node)
            {
                title = Tr("类型丢失的结点");
                titleButtonContainer.Add(new Button(() => graph.DeleteElements(new GraphElement[] { this })) { text = Tr("删除") });
                inputContainer.Add(new Button(() => EditorWindow.GetWindow<ReferencesFixing>().Show()) { text = Tr("尝试修复") });
                return;
            }

            title = Tr(node.GetName());
            style.left = node._position.x;
            style.top = node._position.y;
            userData = node;
            viewDataKey = node.ID;
            hiddenFields = node.GetHiddenFields();
            var width = DialogueNode.Editor.GetWidth(node.GetType());
            if (width <= 0) inputContainer.style.minWidth = 228f;
            else inputContainer.style.minWidth = width;

            this.onBeforeModify = onBeforeModify;
            this.onSelected = onSelected;
            this.onUnselected = onUnselected;
            this.onDeleteOutput = onDeleteOutput;

            RefreshProperty();

            #region 初始化入口
            if (node is not EntryNode)
            {
                inputContainer.Add(Input = new DialogueInput());
#if !ZTDS_ENABLE_PORTRAIT
                Input.portName = Target is SentenceNode ? Tr("对话人") : string.Empty;
#else
                Input.portName = string.Empty;
#endif
                Input.AddManipulator(new ContextualMenuManipulator(evt =>
                {
                    if (Input.connections.Any())
                        evt.menu.AppendAction(Tr("断开所有"), a =>
                        {
                            Input.Graph.DeleteElements(Input.connections);
                        });
                }));
            }
            #endregion

            #region 初始化选项功能
            if (node is not SuffixNode)
            {
                if (node is not ISoloMainOption)
                {
                    addOption = new Button(AddOption) { text = Tr("新选项") };
                    titleButtonContainer.Add(addOption);
                }
                for (int i = 0; i < node.Options.Count; i++)
                {
                    AddOutputInternal(node.Options[i]);
                }
            }
            #endregion

            #region 初始化文本区
            if (node is SentenceNode)
            {
                var talkerContainer = new VisualElement();
                talkerContainer.style.maxWidth = width > 0 ? width : 228f;
#if ZTDS_ENABLE_PORTRAIT
                talkerContainer.style.flexDirection = FlexDirection.Row;
#endif
                if (Target is not EntryNode)
                {
                    talkerContainer.style.marginTop = -22;
#if !ZTDS_ENABLE_PORTRAIT
                    talkerContainer.style.marginBottom = 1;
                    talkerContainer.style.marginLeft = 80;
#else
                    talkerContainer.style.marginLeft = 20;
#endif
                }
#if ZTDS_ENABLE_PORTRAIT
                talkerContainer.Add(portrait = new PropertyField());
                portrait.BindProperty(SerializedNode.FindAutoProperty("Portrait"));
                portrait.label = string.Empty;
                portrait.style.width = 64;
                portrait.style.marginLeft = -4;
                var right = new VisualElement();
                right.style.flexGrow = 1;
                right.Add(portraitSide = new EnumField(Tr("头像位置")));
                portraitSide.BindProperty(SerializedNode.FindAutoProperty("PortrSide"));
                portraitSide.labelElement.style.minWidth = 60;
                right.Add(talker = new TextArea(Tr("对话人")));
                talkerContainer.Add(right);
#else
                talkerContainer.Add(talker = new TextField(Target is not EntryNode ? string.Empty : Tr("对话人")) { multiline = true });
                talker.labelElement.style.minWidth = 60f;
                talker.Q("unity-text-input").style.whiteSpace = WhiteSpace.Normal;
#endif
                talker.BindProperty(SerializedNode.FindAutoProperty("Talker"));
                EditorMiscFunction.SetAsKeywordsField(talker);
                inputContainer.Add(talkerContainer);
                inputContainer.Add(text = new TextArea(string.Empty, 35f));
                text.style.maxWidth = width > 0 ? width : 228f;
                text.BindProperty(SerializedNode.FindAutoProperty("Text"));
                EditorMiscFunction.SetAsKeywordsField(text);
                titleContainer.RegisterCallback<GeometryChangedEvent>(evt =>
                {
                    talkerContainer.style.maxWidth = inputContainer.layout.width;
                    text.style.maxWidth = inputContainer.layout.width;
                });
                titleButtonContainer.Insert(1, interval = new CurveField());
                interval.BindProperty(SerializedNode.FindAutoProperty("SpeakInterval"));
                interval.style.minWidth = 160f;
                interval.style.alignItems = Align.Center;
                interval.label = Tr("吐字间隔");
                interval.labelElement.style.minWidth = 0f;
                interval.labelElement.style.paddingTop = 0f;
            }
            #endregion

            #region 初始化检查器
            if (node is not ExitNode)
            {
                inspector = new IMGUIContainer(() =>
                {
                    if (SerializedNode != null && SerializedNode.serializedObject != null && SerializedNode.serializedObject.targetObject)
                    {
                        float oldLW = EditorGUIUtility.labelWidth;
                        EditorGUIUtility.labelWidth = 80;
                        SerializedNode.serializedObject.UpdateIfRequiredOrScript();
                        EditorGUI.BeginChangeCheck();
                        using var copy = SerializedNode.Copy();
                        SerializedProperty end = copy.GetEndProperty();
                        bool enter = true;
                        while (copy.NextVisible(enter) && !SerializedProperty.EqualContents(copy, end))
                        {
                            enter = false;
                            if (!hiddenFields.Contains(copy.name))
                                EditorGUILayout.PropertyField(copy, true);
                        }
                        if (EditorGUI.EndChangeCheck()) SerializedNode.serializedObject.ApplyModifiedProperties();
                        EditorGUIUtility.labelWidth = oldLW;
                    }
                });
                inspector.style.marginTop = 1;
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

            #region Tooltip
            titleContainer.Q<Label>("title-label").tooltip = Tr(DialogueNode.GetDescription(node.GetType()));
            this.RegisterTooltipCallback(() =>
            {
                if (node is SentenceNode sentence) return sentence.Preview();
                else if (node is OtherDialogueNode other && other.Dialogue) return other.Dialogue.Entry.Preview();
                else return null;
            });
            #endregion
        }

        #region 选项相关
        private void AddOption()
        {
            if (Target is BranchNode) AddOption(true);
            else if (Target.Options.Count < 1)
            {
                if (Target is not ExternalOptionsNode)
                {
                    GenericMenu menu = new GenericMenu();
                    menu.AddItem(new GUIContent(Tr("主要选项")), false, () => AddOption(true));
                    menu.AddItem(new GUIContent(Tr("普通选项")), false, () => AddOption(false));
                    menu.ShowAsContext();
                }
                else AddOption(false);
            }
            else if (!Target.Options.FirstOrDefault().IsMain || Target is ExternalOptionsNode) AddOption(false);
        }
        private void AddOption(bool main)
        {
            if (!CanAddOption()) return;
            onBeforeModify?.Invoke();
            var option = DialogueNode.Editor.AddOption(Target, main, main ? Tr("继续") : ObjectNames.GetUniqueName(Target.Options.Select(x => x.Title).ToArray(), Tr("新选项")));
            if (option != null)
            {
                SerializedNode.serializedObject.UpdateIfRequiredOrScript();
                AddOutputInternal(option);
                RefreshPorts();
            }
        }

        private void DeleteOutput(DialogueOutput output)
        {
            onDeleteOutput?.Invoke(output);
            outputs.Remove(output);
            outputContainer.Remove(output);
            onBeforeModify?.Invoke();
            DialogueNode.Editor.RemoveOption(Target, output.userData as DialogueOption);
            RefreshPorts();
            RefreshOptionButton();
            outputs.ForEach(o => o.RefreshProperty());
        }
        public DialogueOutput AddOutput(DialogueOption option)
        {
            if (!CanAddOption()) return null;
            return AddOutputInternal(option);
        }
        private DialogueOutput AddOutputInternal(DialogueOption option)
        {
            var output = new DialogueOutput(option, Target is ISoloMainOption and ConditionNode ? null : DeleteOutput);
            if (option.IsMain) output.portName = Tr(Target is BranchNode ? "分支" : "主要");
            outputs.Add(output);
            outputContainer.Add(output);
            output.AddManipulator(new ContextualMenuManipulator(evt =>
            {
                if (Target.Options.Count == 1)
                {
                    if (option.IsMain && !Target.ExitHere && Target is not ISoloMainOption and not BranchNode)
                        evt.menu.AppendAction(Tr("转为普通选项"), a =>
                        {
                            onBeforeModify?.Invoke();
                            DialogueOption.Editor.SetIsMain(option, false);
                            SerializedNode.serializedObject.UpdateIfRequiredOrScript();
                            output.SerializedOption.FindAutoProperty("Title").stringValue = ObjectNames.GetUniqueName(Target.Options.Select(x => x.Title).ToArray(), Tr("新选项"));
                            SerializedNode.serializedObject.ApplyModifiedPropertiesWithoutUndo();
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
                            SerializedNode.serializedObject.UpdateIfRequiredOrScript();
                            output.SerializedOption.FindAutoProperty("Title").stringValue = string.Empty;
                            SerializedNode.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                            output.RefreshIsMain();
                            output.RefreshProperty();
                            RefreshOptionButton();
                            output.portName = Tr("主要");
                        });
                }
                else if (!option.IsMain)
                {
                    var index = Target.Options.IndexOf(option);
                    if (index > 0)
                        evt.menu.AppendAction(Tr("上移选项"), a =>
                        {
                            onBeforeModify?.Invoke();
                            DialogueNode.Editor.MoveOptionUpward(Target, index);
                            SerializedOptions.serializedObject.UpdateIfRequiredOrScript();
                            output.PlaceBehind(outputs[index - 1]);
                            (outputs[index], outputs[index - 1]) = (outputs[index - 1], outputs[index]);
                            outputs.ForEach(o => o.RefreshProperty());
                        });
                    if (index < Target.Options.Count - 1)
                        evt.menu.AppendAction(Tr("下移选项"), a =>
                        {
                            onBeforeModify?.Invoke();
                            DialogueNode.Editor.MoveOptionDownward(Target, index);
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
            return !(Target is SuffixNode || Target is ISoloMainOption && Target.Options.Count > 1) || Target is BranchNode;
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
            if (!Target) return;
            onBeforeModify?.Invoke();
            Target._position.x = newPos.xMin;
            Target._position.y = newPos.yMin;
        }
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt) { }
        #endregion

        #region 刷新相关
        public void RefreshProperty()
        {
            if (Target is not ExitNode)
            {
                graph.SerializedNodes.serializedObject.Update();
                SerializedNode = graph.SerializedNodes.GetArrayElementAtIndex(graph.Dialogue.Nodes.IndexOf(Target));
                if (Target is not SuffixNode) SerializedOptions = SerializedNode.FindPropertyRelative("options");
                if (Target is SentenceNode)
                {
                    talker?.BindProperty(SerializedNode.FindAutoProperty("Talker"));
                    text?.BindProperty(SerializedNode.FindAutoProperty("Text"));
                    interval?.BindProperty(SerializedNode.FindAutoProperty("SpeakInterval"));
#if ZTDS_ENABLE_PORTRAIT
                    portrait?.BindProperty(SerializedNode.FindAutoProperty("Portrait"));
                    portraitSide?.BindProperty(SerializedNode.FindAutoProperty("PortrSide"));
#endif
                }
                outputs.ForEach(o => o.RefreshProperty());
            }
        }

        public void RefreshOptionButton()
        {
            addOption?.SetEnabled(Target is ExternalOptionsNode or BranchNode || Target.Options.Count < 1
                                  || Target.Options.Count > 0 && !Target.Options[0].IsMain && Target is SentenceNode && !Target.ExitHere);
        }
        #endregion

        private string Tr(string text) => L.Tr(graph.settings.language, text);
    }
}