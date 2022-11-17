using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace ZetanStudio.DialogueSystem.Editor
{
    using Extension;

    public class DialogueView : GraphView
    {
        public new class UxmlFactory : UxmlFactory<DialogueView, UxmlTraits> { }

        #region 声明
        public readonly DialogueEditorSettings settings;
        public Action<DialogueNode> nodeSelectedCallback;
        public Action<DialogueNode> nodeUnselectedCallback;
        private readonly MiniMap miniMap;
        private DialogueNode entry;
        private DialogueNode exit;
        private DialogueGroup invalid;
        private Dialogue dialogue;
        private readonly VisualElement errors;
        public SerializedObject serializedDialog;

        private List<DialogueContentGroup> copiedGroups;
        private List<DialogueContent> copiedContents;
        private GenericData copiedData;
        private Vector2 localMousePosition;
        private Vector2 copiedPosition;
        #endregion

        #region 属性
        public Dialogue Dialogue { get => dialogue; set => ViewDialgoue(value); }
        public SerializedProperty SerializedContents { get; private set; }
        public SerializedProperty SerializedGroups { get; private set; }
        protected override bool canPaste => copiedContents != null && copiedData != null;
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
            v.style.alignItems = Align.FlexStart;
            Add(v);
            v.Add(errors = new VisualElement());
            errors.style.flexDirection = FlexDirection.ColumnReverse;
            errors.style.paddingLeft = 3f;
            RegisterCallback<GeometryChangedEvent>(evt =>
            {
                miniMap.style.width = miniMap.maxWidth = evt.newRect.width / 4;
                miniMap.style.height = miniMap.maxHeight = evt.newRect.height / 4;
            });
            Undo.undoRedoPerformed += OnUndoPerformed;
            RegisterCallback<MouseEnterEvent>(evt =>
            {
                localMousePosition = evt.localMousePosition;
            });
            RegisterCallback<MouseMoveEvent>(evt =>
            {
                localMousePosition = evt.localMousePosition;
            });

            serializeGraphElements = OnSerializeGraphElements;
            unserializeAndPaste = OnUnserializeAndPaste;
            elementsAddedToGroup = OnElementsAddedToGroup;
            elementsRemovedFromGroup = OnElementsRemovedFromGroup;
        }

        #region 重写
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            if (!Dialogue) return;
            if (evt.target == this)
            {
                Vector2 position = this.ChangeCoordinatesTo(contentViewContainer, evt.localMousePosition);
                evt.menu.AppendAction(Tr("分组"), a =>
                {
                    CreateGroup(position);
                });
                if (canPaste) evt.menu.AppendAction(Tr("粘贴"), a => PasteCallback());
                evt.menu.AppendSeparator();
                foreach (var type in TypeCache.GetTypesDerivedFrom<DialogueContent>())
                {
                    if (!type.IsAbstract && type != typeof(EntryContent) && type != typeof(ExitContent))
                        evt.menu.AppendAction($"{GetMenuGroup(type)}{DialogueContent.GetName(type)}", a => CreateContent(type, position));
                }
            }
            else if (evt.target is DialogueNode node)
            {
                if (node.Content is not EntryContent && node != exit)
                {
                    evt.menu.AppendAction(Tr("删除"), a => DeleteSelection());
                    evt.menu.AppendAction(Tr("复制"), a => CopySelectionCallback());
                }
                IEnumerable<Node> dnodes = selection.FindAll(s => s is DialogueNode).Cast<Node>();
                if (node.GetContainingScope() is DialogueGroup group && dnodes.All(s => (s as DialogueNode)!.GetContainingScope() == group))
                {
                    evt.menu.AppendAction(Tr("移出本组"), a =>
                    {
                        var nodes = dnodes.Where(s => s is DialogueNode n && n.GetContainingScope() == group);
                        OnBeforeModify();
                        foreach (var n in nodes)
                        {
                            group.Group.contents.Remove(n.viewDataKey);
                            n.SetPosition(new Rect(n.layout.position + new Vector2(-10, 10), n.layout.size));
                        }
                        group.RemoveElementsWithoutNotification(nodes);
                    });
                }
                if (dnodes.All(s => (s as DialogueNode)!.GetContainingScope() is null))
                {
                    evt.menu.AppendAction(Tr("添加到") + '/' + Tr("新建分组"), a =>
                    {
                        OnBeforeModify();
                        List<Vector2> positions = new List<Vector2>();
                        foreach (var s in selection)
                        {
                            if (s is Node n)
                                positions.Add(n.layout.center);
                        }
                        var group = CreateGroup(GetCenter(positions));
                        group.AddElements(dnodes);
                    });
                    List<DialogueGroup> groups = new List<DialogueGroup>(graphElements.Where(g => g is DialogueGroup).Cast<DialogueGroup>());
                    if (groups.Count > 0)
                    {
                        OnBeforeModify();
                        evt.menu.AppendSeparator("添加到/");
                        Dictionary<string, int> d = new Dictionary<string, int>();
                        foreach (var g in groups)
                        {
                            var count = 0;
                            if (d.ContainsKey(g.title)) count = d[g.title] + 1;
                            d[g.title] = count;
                            evt.menu.AppendAction(Tr("添加到") + '/' + g.title + (count > 0 ? $" (重名 {count})" : string.Empty), a =>
                            {
                                g.AddElements(dnodes);
                            });
                        }
                    }
                }
                if (node.Content is RecursionSuffix recursion && Dialogue.Reachable(recursion))
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
                if (Dialogue.Reachable(content(endPort), content(startPort))) return false;
                if (content(startPort) is BranchContent && content(endPort) is not ConditionContent) return false;
                return content(endPort).CanLinkFrom(content(startPort), startPort.userData as DialogueOption);

                static DialogueContent content(Port port)
                {
                    return port.node.userData as DialogueContent;
                }
            }
        }
        public override EventPropagation DeleteSelection()
        {
            RemoveFromSelection(entry);
            RemoveFromSelection(exit);
            selection.ForEach(s =>
            {
                if (s is DialogueGroup group)
                {
                    try
                    {
                        group.RemoveElementsWithoutNotification(new GraphElement[] { entry, exit });
                    }
                    catch { }
                }
            });
            return base.DeleteSelection();
        }
        #endregion

        #region 创建相关
        private void CreateContent(Type type, Vector2 position, Action<DialogueNode> callback = null)
        {
            OnBeforeModify();
            var content = Dialogue.Editor.AddContent(Dialogue, type);
            if (content != null)
            {
                content._position = position;
                serializedDialog.Update();
                var node = CreateNode(content);
                callback?.Invoke(node);
                ClearSelection();
                AddToSelection(node);
            }
        }
        private DialogueGroup CreateGroup(Vector2 position)
        {
            var group = new DialogueContentGroup(Tr("新分组"), position);
            OnBeforeModify();
            dialogue.groups.Add(group);
            var g = CreateGroup(group);
            g.FocusTitleTextField();
            return g;
        }

        private DialogueNode CreateNode(DialogueContent content)
        {
            var node = new DialogueNode(this, content, OnBeforeModify, nodeSelectedCallback, nodeUnselectedCallback, OnDeleteOutput);
            AddElement(node);
            if (content is EntryContent) entry = node;
            return node;
        }
        private void CreateEdges(DialogueContent content)
        {
            if (!content) return;
            DialogueNode parent = GetNodeByGuid(content.ID) as DialogueNode;
            if (content.ExitHere && content is not SuffixContent) AddElement(parent.Outputs[0].ConnectTo(exit.Input));
            else
            {
                if (content is not SuffixContent)
                    for (int i = 0; i < content.Options.Count; i++)
                    {
                        var o = content.Options[i];
                        if (o.Content != null)
                        {
                            DialogueNode child = GetNodeByGuid(o.Content.ID) as DialogueNode;
                            AddElement(parent.Outputs[i].ConnectTo(child?.Input));
                        }
                    }
            }
        }
        private DialogueGroup CreateGroup(DialogueContentGroup g)
        {
            var contents = new HashSet<string>(g.contents);
            DialogueGroup group = new DialogueGroup(nodes.Where(n => contents.Contains(n.viewDataKey)), g, OnGroupRightClick, OnBeforeModify);
            AddElement(group);
            return group;
        }
        #endregion

        #region 操作回调
        public void OnEdgeDropOutside(DialogueOutput output, Vector2 nodePosition)
        {
            DialogueNode from = output.node;
            if (from.userData is RecursionSuffix || nodes.Any(x => x.ContainsPoint(x.WorldToLocal(nodePosition)))) return;
            var option = output.userData as DialogueOption;
            nodePosition = contentViewContainer.WorldToLocal(nodePosition);
            var exitHere = from.Content.ExitHere;
            GenericMenu menu = new GenericMenu();
            if (from.Outputs.Count == 1 && output.Option.IsMain && !exitHere && from.Content is not IMainOptionOnly and not BranchContent)
            {
                menu.AddItem(new GUIContent(DialogueContent.GetName(typeof(ExitContent))), false, () =>
                {
                    OnBeforeModify();
                    DialogueContent.Editor.SetAsExit(from.Content);
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
                    AddElement(output.ConnectTo(exit.Input));
                    from.RefreshOptionButton();
                });
                menu.AddSeparator("");
            }
            foreach (var type in TypeCache.GetTypesDerivedFrom<DialogueContent>())
            {
                if (type.IsAbstract || type.IsGenericType || type.IsGenericTypeDefinition || typeof(EntryContent) == type || typeof(ExitContent) == type
                    || from.Content is BranchContent && !typeof(ConditionContent).IsAssignableFrom(type)) continue;
                var temp = Activator.CreateInstance(type) as DialogueContent;
                if (!temp.CanLinkFrom(from.userData as DialogueContent, output.Option)) continue;
                menu.AddItem(new GUIContent($"{GetMenuGroup(type)}{DialogueContent.GetName(type)}"), false, () => CreateContent(type, nodePosition, followUp));
            }
            menu.ShowAsContext();

            void followUp(DialogueNode child)
            {
                DialogueOption.Editor.SetContent(option, child.Content);
                if (exitHere) SetAsExit(child);
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
                AddElement(output.ConnectTo(child.Input));
                if (exitHere)
                {
                    DialogueContent.Editor.SetAsExit(from.Content, false);
                    serializedDialog.Update();
                }
            }
        }
        private void OnUndoPerformed()
        {
            List<string> selectedNodes = new List<string>();
            foreach (var item in selection)
            {
                if (item is Node node) selectedNodes.Add(node.viewDataKey);
            }
            ViewDialgoue(Dialogue);
            foreach (var id in selectedNodes)
            {
                if (GetNodeByGuid(id) is Node node)
                    AddToSelection(node);
            }
        }
        private void OnDeleteOutput(DialogueOutput output)
        {
            DeleteElements(output.connections);
        }
        private void OnGroupRightClick(DialogueGroup group, ContextualMenuPopulateEvent evt)
        {
            if (selection.Count == 1)
            {
                evt.menu.AppendAction(Tr("全选"), a =>
                {
                    ClearSelection();
                    foreach (var e in group.containedElements)
                    {
                        AddToSelection(e);
                    }
                });
                if (invalid != group)
                {
                    if (group.containedElements.Any(c => c.userData is DialogueContent content && content is not EntryContent and not ExitContent))
                    {
                        evt.menu.AppendAction(Tr("复制"), a => CopySelectionCallback());
                        evt.menu.AppendAction(Tr("清空"), a => clear(group));
                        evt.menu.AppendAction(Tr("全部删除"), a => DeleteSelection());
                    }
                    evt.menu.AppendAction(Tr("仅删除组"), a =>
                    {
                        clear(group);
                        dialogue.groups.Remove(group.Group);
                        RemoveElement(group);
                    });
                }
                else evt.menu.AppendAction(Tr("全部删除"), a => DeleteSelection());
            }

            void clear(DialogueGroup group)
            {
                OnBeforeModify();
                group.Group.contents.Clear();
                group.RemoveElementsWithoutNotification(new List<GraphElement>(group.containedElements));
            }
        }
        private void OnElementsAddedToGroup(Group group, IEnumerable<GraphElement> elements)
        {
            if (elements.Any())
            {
                OnBeforeModify();
                HashSet<string> exist = new HashSet<string>((group.userData as DialogueContentGroup).contents);
                foreach (var e in elements)
                {
                    if (!exist.Contains(e.viewDataKey))
                    {
                        (group.userData as DialogueContentGroup).contents.Add(e.viewDataKey);
                    }
                }
            }
        }
        private void OnElementsRemovedFromGroup(Group group, IEnumerable<GraphElement> elements)
        {
            if (elements.Any())
            {
                OnBeforeModify();
                foreach (var e in elements)
                {
                    (group.userData as DialogueContentGroup).contents.Remove(e.viewDataKey);
                }
                if (group == invalid && group.containedElements.None())
                {
                    RemoveElement(group);
                    invalid = null;
                    ViewDialgoue(dialogue);
                }
            }
        }

        private void OnBeforeModify() => Undo.RecordObject(Dialogue, Tr("修改 {0}", Dialogue.name));

        private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
        {
            if (graphViewChange.elementsToRemove != null)
            {
                HashSet<DialogueNode> removedNodes = new HashSet<DialogueNode>();
                graphViewChange.elementsToRemove.ForEach(elem =>
                {
                    if (elem is DialogueNode node)
                    {
                        OnBeforeModify();
                        Dialogue.Editor.RemoveContent(Dialogue, node.Content);
                        removedNodes.Add(node);
                    }
                });
                if (removedNodes.Count > 0)
                    nodes.ForEach(n =>
                    {
                        if (!removedNodes.Contains(n)) (n as DialogueNode).RefreshProperty();
                    });
                graphViewChange.elementsToRemove.ForEach(elem =>
                {
                    if (elem is Edge edge)
                    {
                        DialogueNode parent = edge.output.node as DialogueNode;
                        parent.Content.lastExitHere = parent.Content.ExitHere;
                        if (parent.Content.ExitHere)
                        {
                            OnBeforeModify();
                            DialogueContent.Editor.SetAsExit(parent.Content, false);
                            parent.RefreshOptionButton();
                        }
                        else
                        {
                            DialogueNode child = edge.input.node as DialogueNode;
                            OnBeforeModify();
                            var index = parent.Outputs.IndexOf(edge.output as DialogueOutput);
                            if (index >= 0 && index < parent.Content.Options.Count)
                                DialogueOption.Editor.SetContent(parent.Content.Options[index], null);
                        }
                    }
                    if (elem is DialogueGroup group)
                    {
                        OnBeforeModify();
                        Dialogue.groups.Remove(group.Group);
                    }
                });
            }
            if (graphViewChange.edgesToCreate != null)
            {
                graphViewChange.edgesToCreate.ForEach(edge =>
                {
                    OnBeforeModify();
                    DialogueNode parent = edge.output.node as DialogueNode;
                    if (edge.input.node.userData is ExitContent)
                    {
                        DialogueContent.Editor.SetAsExit(parent.Content);
                        parent.Content.lastExitHere = false;
                        parent.RefreshOptionButton();
                    }
                    else
                    {
                        var exitHere = parent.Content.lastExitHere;
                        parent.Content.lastExitHere = false;
                        DialogueContent.Editor.SetAsExit(parent.Content, false);
                        DialogueNode child = edge.input.node as DialogueNode;
                        if (edge.output is DialogueOutput parentOutput)
                        {
                            if (exitHere && (child.Content.Options.Count < 1 || child.Content.Options[0].IsMain && !child.Content.Options[0].Content))
                                SetAsExit(child);
                            DialogueOption.Editor.SetContent(parentOutput.Option, child.Content);
                        }
                    }
                });
            }

            if (graphViewChange.elementsToRemove != null || graphViewChange.edgesToCreate != null)
            {
                EditorUtility.SetDirty(Dialogue);
                serializedDialog.UpdateIfRequiredOrScript();
            }

            return graphViewChange;
        }

        private string OnSerializeGraphElements(IEnumerable<GraphElement> elements)
        {
            copiedGroups = new List<DialogueContentGroup>();
            copiedContents = new List<DialogueContent>();
            copiedData = new GenericData();
            List<Vector2> positions = new List<Vector2>();
            foreach (var element in elements)
            {
                if (element is DialogueNode node && node.Content is not EntryContent and not ExitContent)
                {
                    var nd = new GenericData();
                    var copy = node.Content.Copy();
                    nd["ID"] = copy.ID;
                    copiedContents.Add(copy);
                    nd.WriteAll(node.Content.Options.Where(o => o.Content).Select(o => o.Content.ID));
                    copiedData.Write(node.viewDataKey, nd);
                    positions.Add(node.layout.center);
                }
                else if (element is DialogueGroup group)
                    copiedGroups.Add(new DialogueContentGroup(group.Group.name, group.Group.position));
            }
            if (positions.Count > 0) copiedPosition = GetCenter(positions);
            else
            {
                copiedContents = null;
                copiedData = null;
            }
            if (copiedGroups.Count < 1) copiedGroups = null;
            return "Copy";//无实际意义
        }
        private void OnUnserializeAndPaste(string operationName, string data)
        {
            if (copiedContents != null && copiedData != null && copiedContents.Count > 0)
            {
                OnBeforeModify();
                var offset = copiedPosition - this.ChangeCoordinatesTo(contentViewContainer, localMousePosition);
                var nodes = new List<DialogueNode>();
                foreach (var copy in copiedContents)
                {
                    Dialogue.Editor.PasteContent(dialogue, copy);
                    copy._position -= offset;
                    nodes.Add(CreateNode(copy));
                }
                foreach (var nd in copiedData.ReadDataDict().Values)
                {
                    var node = GetNodeByGuid(nd.ReadString("ID"));
                    if (node is DialogueNode n)
                    {
                        var cds = nd.ReadStringList();
                        for (int i = 0; i < cds.Count; i++)
                        {
                            try
                            {
                                var child = GetNodeByGuid(realID(cds[i]));
                                if (child is DialogueNode c) DialogueOption.Editor.SetContent(n.Content[i], c.Content);
                            }
                            catch { }
                        }
                    }
                }
                copiedContents.ForEach(c => CreateEdges(c));
                if (copiedGroups != null)
                {
                    ClearSelection();
                    foreach (var group in copiedGroups)
                    {
                        group.position -= offset;
                        group.contents = group.contents.ConvertAll(c => realID(c));
                        dialogue.groups.Add(group);
                        AddToSelection(CreateGroup(group));
                    }
                }
                else
                {
                    ClearSelection();
                    foreach (var node in nodes)
                    {
                        AddToSelection(node);
                    }
                }
            }
            copiedGroups = null;
            copiedContents = null;
            copiedData = null;

            string realID(string oldID) => copiedData.ReadData(oldID).ReadString("ID");
        }
        #endregion

        #region 其它
        public void ShowHideMiniMap(bool show)
        {
            if (miniMap != null) miniMap.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
        }
        public void ViewDialgoue(Dialogue newDialogue)
        {
            Vacate();
            dialogue = newDialogue;
            if (dialogue)
            {
                serializedDialog = new SerializedObject(Dialogue);
                SerializedContents = serializedDialog.FindProperty("contents");
                SerializedGroups = serializedDialog.FindProperty("groups");
                exit = CreateNode(Dialogue.exit);
                List<DialogueContent> invalid = new List<DialogueContent>(dialogue.Contents.Where(x => x is null));
                if (invalid.Count > 0)
                {
                    AddElement(this.invalid = new DialogueGroup(null, null, OnGroupRightClick, OnBeforeModify));
                    this.invalid.title = "无效的结点";
                    var row = -1;
                    for (int i = 0; i < invalid.Count; i++)
                    {
                        if (i % 5 == 0) row++;
                        var n = CreateNode(invalid[i]);
                        n.SetPosition(new Rect(new Vector2(i % 5 * 235f, row * 40f), Vector2.zero));
                        this.invalid.AddElement(n);
                    }
                }
                else
                {
                    dialogue.Contents.ForEach(c => CreateNode(c));
                    dialogue.Contents.ForEach(c => CreateEdges(c));
                    dialogue.groups.ForEach(g => CreateGroup(g));
                }
            }
            else
            {
                entry = null;
                exit = null;
                invalid = null;
                serializedDialog = null;
                SerializedContents = null;
                SerializedGroups = null;
            }
        }
        private void Vacate()
        {
            graphViewChanged -= OnGraphViewChanged;
            DeleteElements(graphElements);
            graphViewChanged += OnGraphViewChanged;
        }
        public void CheckErrors()
        {
            errors.Clear();
            if (Dialogue)
            {

                if (!Dialogue.Exitable)
                {
                    var label = new Label(Utility.ColorText($"{Tr("错误")}: {Tr("对话无结束点")}", Color.red)) { enableRichText = true };
                    label.RegisterCallback<PointerDownEvent>(evt =>
                    {
                        EditorApplication.delayCall += () =>
                        {
                            ClearSelection();
                            AddToSelection(exit);
                            FrameSelection();
                        };
                    });
                    errors.Add(label);
                }
                if (Dialogue.Traverse(dialogue.Entry, c => c.Options.Count > 0 && c.Options.All(x => x.Content is DecoratorContent && x.Content.Exitable)))
                    errors.Add(new Label(Utility.ColorText($"{Tr("警告")}: {Tr("对话可能无结束点")}", Color.yellow)) { enableRichText = true });
                for (int i = 0; i < dialogue.Contents.Count; i++)
                {
                    var content = dialogue.Contents[i];
                    if (dialogue.Reachable(content))
                    {
                        if (!content.IsValid)
                        {
                            var label = new Label(Utility.ColorText($"{Tr("错误")}: {Tr("第{0}个内容填写错误", i)}", Color.red)) { enableRichText = true };
                            label.RegisterCallback<PointerDownEvent>(evt =>
                            {
                                EditorApplication.delayCall += () =>
                                {
                                    ClearSelection();
                                    AddToSelection(GetNodeByGuid(content.ID));
                                    FrameSelection();
                                };
                            });
                            errors.Add(label);
                        }
                        if (!content.ExitHere && content.Options.Any(x => x.Content is null))
                        {
                            var label = new Label(Utility.ColorText($"{Tr("错误")}: {Tr("第{0}个内容存在无效选项", i)}", Color.red)) { enableRichText = true };
                            label.RegisterCallback<PointerDownEvent>(evt =>
                            {
                                EditorApplication.delayCall += () =>
                                {
                                    ClearSelection();
                                    AddToSelection(GetNodeByGuid(content.ID));
                                    FrameSelection();
                                };
                            });
                            errors.Add(label);
                        }
                    }
                }
            }
        }
        private void SetAsExit(DialogueNode node)
        {
            if (node.Content is SuffixContent || node.Content is ExternalOptionsContent) return;
            DialogueContent.Editor.SetAsExit(node.Content);
            serializedDialog.UpdateIfRequiredOrScript();
            DialogueOutput output = node.Outputs.Count < 1 ? node.InsertOutput(node.Content.Options[0]) : node.Outputs[0];
            AddElement(output.ConnectTo(exit.Input));
            node.RefreshOptionButton();
        }
        private static string GetMenuGroup(Type type)
        {
            string group = DialogueContent.GetGroup(type);
            if (string.IsNullOrEmpty(group)) return group;
            else return group.EndsWith("/") ? group : (group + "/");
        }
        private Vector2 GetCenter(IEnumerable<Vector2> positions)
        {
            Vector2 min = new Vector2(positions.Min(p => p.x), positions.Min(p => p.y));
            Vector2 max = new Vector2(positions.Max(p => p.x), positions.Max(p => p.y));
            return Utility.CenterBetween(min, max);
        }
        #endregion

        private string Tr(string text) => L.Tr(settings.language, text);
        private string Tr(string text, params object[] args) => L.Tr(settings.language, text, args);
    }
}