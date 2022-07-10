﻿using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using ZetanStudio.Extension.Editor;

namespace ZetanStudio.DialogueSystem.Editor
{
    using Extension;

    public class DialogueView : GraphView
    {
        public new class UxmlFactory : UxmlFactory<DialogueView, UxmlTraits> { }

        #region 声明
        private readonly DialogueEditorSettings settings;
        private NewDialogue dialogueBef;
        private SerializedObject serializedDialog;
        public Action<DialogueNode> nodeSelectedCallback;
        public Action<DialogueNode> nodeUnselectedCallback;
        private readonly MiniMap miniMap;
        private DialogueNode exit;
        private NewDialogue dialogue;
        private readonly Label errors;
        #endregion

        #region 属性
        #region 属性重写
        protected override bool canCopySelection => false;
        protected override bool canDuplicateSelection => false;
        protected override bool canPaste => false;
        protected override bool canCutSelection => false;
        protected override bool canDeleteSelection
        {
            get
            {
                int index = selection.FindIndex(x => x is DialogueNode node && node.content is EntryContent);
                if (index > 0) RemoveFromSelection(selection[index]);
                RemoveFromSelection(exit);
                return base.canDeleteSelection;
            }
        }
        #endregion

        public NewDialogue Dialogue { get => dialogue; set => DrawDialgoueView(value); }
        public SerializedProperty SerializedContents { get; private set; }
        #endregion

        public DialogueView()
        {
            Insert(0, new GridBackground());

            this.AddManipulator(new ContentZoomer() { minScale = 0.15f });
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            settings = DialogueEditorSettings.GetOrCreate();
            styleSheets.Add(settings.treeUss);

            Add(miniMap = new MiniMap());
            miniMap.style.backgroundColor = new StyleColor(new Color32(29, 29, 30, 200));

            var v = new VisualElement();
            v.pickingMode = PickingMode.Ignore;
            v.style.position = Position.Absolute;
            v.style.left = 0;
            v.style.right = 0;
            v.style.top = 0;
            v.style.bottom = 0;
            v.style.flexDirection = FlexDirection.ColumnReverse;
            Add(v);
            v.Add(errors = new Label());
            errors.enableRichText = true;
            errors.pickingMode = PickingMode.Ignore;
            CheckErrors();
            RegisterCallback<GeometryChangedEvent>(evt =>
            {
                miniMap.style.width = miniMap.maxWidth = evt.newRect.width / 4;
                miniMap.style.height = miniMap.maxHeight = evt.newRect.height / 4;
            });
            Undo.undoRedoPerformed += OnUndoPerformed;
        }

        #region 重写
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            if (!Dialogue) return;
            if (evt.target == this)
            {
                Vector2 nodePosition = this.ChangeCoordinatesTo(contentViewContainer, evt.localMousePosition);
                foreach (var type in TypeCache.GetTypesDerivedFrom<DialogueContent>())
                {
                    if (!type.IsAbstract && type != typeof(EntryContent) && type != typeof(ExitContent))
                        evt.menu.AppendAction($"{Group(type)}{DialogueContent.GetName(type)}", a => CreateContent(type, nodePosition));
                }
            }
            else if (evt.target is DialogueNode node)
            {
                if (node.content is not EntryContent && node != exit)
                    evt.menu.AppendAction(Tr("删除"), a =>
                    {
                        DeleteSelection();
                    });
                if (node.content is RecursionSuffix recursion && Dialogue.Reachable(recursion))
                    evt.menu.AppendAction(Tr("选中递归点"), a =>
                    {
                        if (recursion.FindRecursionPoint(Dialogue.Entry) is DialogueContent find)
                        {
                            ClearSelection();
                            AddToSelection(GetNodeByGuid(find.ID));
                            FrameSelection();
                        }
                    });
            }

        }
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            return ports.Where(endPort => canLink(startPort, endPort)).ToList();

            static bool canLink(Port startPort, Port endPort)
            {
                if (endPort.direction == startPort.direction) return false;
                if (endPort.node == startPort.node) return false;
                if (startPort.direction == Direction.Input) (startPort, endPort) = (endPort, startPort);
                if (NewDialogue.Reachable(content(endPort), content(startPort))) return false;
                return content(endPort).CanLinkFrom(content(startPort), startPort.userData as DialogueOption);

                static DialogueContent content(Port port)
                {
                    return port.node.userData as DialogueContent;
                }
            }
        }
        #endregion

        #region 创建相关
        private void CreateContent(Type type, Vector2 position, Action<DialogueNode> callback = null)
        {
            Undo.RecordObject(Dialogue, Tr("修改{0}", Dialogue.name));
            var content = NewDialogue.Editor.AddContent(Dialogue, type);
            if (content != null)
            {
                content._position = position;
                serializedDialog.Update();
                var node = CreateNode(content);
                callback?.Invoke(node);
            }
        }
        private DialogueNode CreateNode(DialogueContent content)
        {
            var node = new DialogueNode(this, content, nodeSelectedCallback, nodeUnselectedCallback, OnNodePositionChanged, OnDeleteOutput, settings);
            AddElement(node);
            return node;
        }
        private void CreateEdges(DialogueContent content)
        {
            DialogueNode parent = GetNodeByGuid(content.ID) as DialogueNode;
            if (content.ExitHere && content is not SuffixContent) AddElement(parent.outputs[0].ConnectTo(exit.input));
            else
            {
                if (content is not INonOption)
                    for (int i = 0; i < content.Options.Count; i++)
                    {
                        var o = content.Options[i];
                        if (o.Content != null)
                        {
                            DialogueNode child = GetNodeByGuid(o.Content.ID) as DialogueNode;
                            AddElement(parent.outputs[i].ConnectTo(child?.input));
                        }
                    }
            }
        }
        #endregion

        #region 操作回调
        public void OnEdgeDropOutside(DialogueOutput output, Vector2 nodePosition)
        {
            if (output.node.userData is RecursionSuffix || nodes.Any(x => x.ContainsPoint(x.WorldToLocal(nodePosition)))) return;
            var option = output.userData as DialogueOption;
            nodePosition = contentViewContainer.WorldToLocal(nodePosition);
            var exitHere = output.node.content.ExitHere;
            GenericMenu menu = new GenericMenu();
            if (output.node.outputs.Count == 1 && output.Option.IsMain && !exitHere && output.node.content is not DecoratorContent)
            {
                menu.AddItem(new GUIContent(DialogueContent.GetName(typeof(ExitContent))), false, () =>
                {
                    Undo.RecordObject(Dialogue, Tr("修改{0}", Dialogue.name));
                    DialogueContent.Editor.SetAsExit(output.node.content);
                    serializedDialog.Update();
                    if (output.connections is not null)
                    {
                        foreach (var edge in output.connections)
                        {
                            edge.input.Disconnect(edge);
                            RemoveElement(edge);
                        }
                        output.DisconnectAll();
                    }
                    AddElement(output.ConnectTo(exit.input));
                    CheckErrors();
                    output.node.RefreshOptionButton();
                });
                menu.AddSeparator("");
            }
            foreach (var type in TypeCache.GetTypesDerivedFrom<DialogueContent>())
            {
                if (type.IsAbstract || output.Option.IsMain && typeof(DecoratorContent).IsAssignableFrom(type) || type == typeof(EntryContent) ||
                    type == typeof(ExitContent) || output.node.content is DecoratorContent decorator && decorator.GetType() == type ||
                    output.node.content is EntryContent && typeof(SuffixContent).IsAssignableFrom(type)) continue;
                menu.AddItem(new GUIContent($"{Group(type)}{DialogueContent.GetName(type)}"), false, () => CreateContent(type, nodePosition, followUp));
            }
            menu.ShowAsContext();

            void followUp(DialogueNode node)
            {
                DialogueOption.Editor.SetContent(option, node.content);
                if (exitHere) SetAsExit(node);
                serializedDialog.Update();
                if (output.connections is not null)
                {
                    foreach (var edge in output.connections)
                    {
                        edge.input.Disconnect(edge);
                        RemoveElement(edge);
                    }
                    output.DisconnectAll();
                }
                AddElement(output.ConnectTo(node.input));
                if (exitHere)
                {
                    DialogueContent.Editor.SetAsExit(output.node.content, false);
                    serializedDialog.Update();
                }
            }
        }
        private void OnUndoPerformed()
        {
            DrawDialgoueView(Dialogue);
        }
        private void OnNodePositionChanged(DialogueNode editor, Vector2 oldPos)
        {
            Undo.RegisterCompleteObjectUndo(Dialogue, Tr("修改{0}", Dialogue.name));
        }
        private void OnDeleteOutput(DialogueOutput output)
        {
            DeleteElements(output.connections);
        }
        private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
        {
            if (graphViewChange.elementsToRemove != null)
            {
                HashSet<DialogueNode> removedNodes = new HashSet<DialogueNode>();
                //为了保证撤销重做的顺利进行，先遍历结点，再遍历连线
                graphViewChange.elementsToRemove.ForEach(elem =>
                {
                    if (elem is DialogueNode node)
                    {
                        Undo.RecordObject(Dialogue, Tr("修改{0}", Dialogue.name));
                        NewDialogue.Editor.RemoveContent(Dialogue, node.content);
                        removedNodes.Add(node);
                    }
                });
                if (removedNodes.Count > 0)
                    nodes.ForEach(n =>
                    {
                        if (!removedNodes.Contains(n))
                            (n as DialogueNode).RefreshProperty();
                    });
                graphViewChange.elementsToRemove.ForEach(elem =>
                {
                    if (elem is Edge edge)
                    {
                        DialogueNode parent = edge.output.node as DialogueNode;
                        parent.content.lastExitHere = parent.content.ExitHere;
                        if (parent.content.ExitHere)
                        {
                            Undo.RecordObject(Dialogue, Tr("修改{0}", Dialogue.name));
                            DialogueContent.Editor.SetAsExit(parent.content, false);
                            parent.RefreshOptionButton();
                        }
                        else
                        {
                            DialogueNode child = edge.input.node as DialogueNode;
                            Undo.RecordObject(Dialogue, Tr("修改{0}", Dialogue.name));
                            var index = parent.outputs.IndexOf(edge.output as DialogueOutput);
                            if (index >= 0 && index < parent.content.Options.Count)
                                DialogueOption.Editor.SetContent(parent.content.Options[index], null);
                        }
                    }
                });
            }
            if (graphViewChange.edgesToCreate != null)
            {
                graphViewChange.edgesToCreate.ForEach(edge =>
                {
                    Undo.RecordObject(Dialogue, Tr("修改{0}", Dialogue.name));
                    DialogueNode parent = edge.output.node as DialogueNode;
                    if (edge.input.node.userData is ExitContent)
                    {
                        DialogueContent.Editor.SetAsExit(parent.content);
                        parent.content.lastExitHere = false;
                        parent.RefreshOptionButton();
                    }
                    else
                    {
                        var exitHere = parent.content.lastExitHere;
                        parent.content.lastExitHere = false;
                        DialogueContent.Editor.SetAsExit(parent.content, false);
                        DialogueNode child = edge.input.node as DialogueNode;
                        if (edge.output is DialogueOutput parentOutput)
                        {
                            if (exitHere && (child.content.Options.Count < 1 || child.content.Options[0].IsMain && !child.content.Options[0].Content))
                                SetAsExit(child);
                            DialogueOption.Editor.SetContent(parentOutput.Option, child.content);
                        }
                    }
                });
            }

            if (graphViewChange.elementsToRemove != null || graphViewChange.edgesToCreate != null)
            {
                EditorUtility.SetDirty(Dialogue);
                serializedDialog.UpdateIfRequiredOrScript();
                CheckErrors();
            }

            return graphViewChange;
        }
        #endregion

        #region 其它
        public void ShowHideMiniMap(bool show)
        {
            if (miniMap != null) miniMap.style.display = new StyleEnum<DisplayStyle>(show ? DisplayStyle.Flex : DisplayStyle.None);
        }
        public void DrawDialgoueView(NewDialogue newDialogue)
        {
            if (newDialogue)
            {
                if (dialogueBef != newDialogue) Undo.ClearUndo(dialogueBef);
                dialogue = newDialogue;
            }
            Vocate();
            if (Dialogue)
            {
                dialogueBef = Dialogue;
                serializedDialog = new SerializedObject(Dialogue);
                SerializedContents = serializedDialog.FindProperty("contents");
            }
            else
            {
                serializedDialog = null;
                SerializedContents = null;
                return;
            }
            exit = CreateNode(Dialogue.exit);
            Dialogue.Contents.ForEach(c => CreateNode(c));
            Dialogue.Contents.ForEach(c => CreateEdges(c));
            CheckErrors();
        }
        public void Vocate()
        {
            graphViewChanged -= OnGraphViewChanged;
            DeleteElements(nodes.ToList());
            DeleteElements(edges.ToList());
            graphViewChanged += OnGraphViewChanged;
        }
        private void CheckErrors()
        {
            if (Dialogue)
            {
                errors.text = Tr("无错误");
                if (!Dialogue.Exitable)
                    errors.text = ZetanUtility.ColorText($"{Tr("错误")}: {Tr("对话无结束点")}", Color.red);
            }
            else errors.text = Tr("未选择对话");
        }
        private void SetAsExit(DialogueNode node)
        {
            if (node.content is SuffixContent) return;
            DialogueContent.Editor.SetAsExit(node.content);
            serializedDialog.UpdateIfRequiredOrScript();
            DialogueOutput output;
            if (node.outputs.Count < 1)
                output = node.InsertOutput(node.content.Options[0], true);
            else output = node.outputs[0];
            AddElement(output.ConnectTo(exit.input));
            node.RefreshOptionButton();
            CheckErrors();
        }
        private static string Group(Type type)
        {
            string group = DialogueContent.GetGroup(type);
            if (string.IsNullOrEmpty(group)) return group;
            else return group.Replace("/", "") + "/";
        }
        #endregion

        private string Tr(string text) => L.Tr(settings.language, text);
        private string Tr(string text, params object[] args) => L.Tr(settings.language, text, args);
    }
}