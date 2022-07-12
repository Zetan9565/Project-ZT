using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace ZetanStudio.DialogueSystem.Editor
{
    using Extension.Editor;
    using System.Collections;

    public class DialogueEditor : EditorWindow
    {
        #region 静态方法
        [MenuItem("Window/Zetan Studio/对话编辑器")]
        public static void CreateWindow()
        {
            var settings = DialogueEditorSettings.GetOrCreate();
            DialogueEditor wnd = GetWindow<DialogueEditor>(L.Tr(settings.language, "对话编辑器"));
            wnd.minSize = settings.minWindowSize;
        }
        public static void CreateWindow(Dialogue dialogue)
        {
            var settings = DialogueEditorSettings.GetOrCreate();
            DialogueEditor wnd = GetWindow<DialogueEditor>(L.Tr(settings.language, "对话编辑器"));
            wnd.minSize = settings.minWindowSize;
            wnd.list.SetSelection(wnd.dialogues.IndexOf(dialogue));
            EditorApplication.delayCall += () => wnd.list.ScrollToItem(wnd.dialogues.IndexOf(dialogue));
        }

        [OnOpenAsset]
#pragma warning disable IDE0060 // 删除未使用的参数
        public static bool OnOpenAsset(int instanceID, int line)
#pragma warning restore IDE0060 // 删除未使用的参数
        {
            if (EditorUtility.InstanceIDToObject(instanceID) is Dialogue tree)
            {
                CreateWindow(tree);
                return true;
            }
            return false;
        }
        #endregion

        #region 变量声明
        private DialogueEditorSettings settings;
        private Button delete;
        private ToolbarSearchField searchField;
        private DialogueView dialogueView;
        private UnityEngine.UIElements.ListView list;
        private UnityEngine.UIElements.ListView searchList;
        private List<Dialogue> dialogues;
        private Dialogue selectedDialogue;
        private IMGUIContainer inspector;
        private VisualElement eventsView;
        private IMGUIContainer eventsInspector;
        #endregion

        #region Unity回调
        public void CreateGUI()
        {
            try
            {
                settings = settings ? settings : DialogueEditorSettings.GetOrCreate();

                VisualElement root = rootVisualElement;

                var visualTree = settings.treeUxml;
                visualTree.CloneTree(root);

                var styleSheet = settings.treeUss;
                root.styleSheets.Add(styleSheet);

                root.Q<Button>("create").clicked += ClickNew;
                delete = root.Q<Button>("delete");
                delete.clicked += ClickDelete;
                Toggle toggle = root.Q<Toggle>("minimap-toggle");
                toggle.RegisterValueChangedCallback(evt =>
                {
                    dialogueView?.ShowHideMiniMap(evt.newValue);
                });
                toggle.SetValueWithoutNotify(true);
                var copy = new ToolbarButton(CopyOlds) { text = "一件导入旧版" };
                root.Q<Toolbar>().Add(copy);
                searchList = root.Q<UnityEngine.UIElements.ListView>("search-list");
                searchList.selectionType = SelectionType.Single;
                searchList.makeItem = () => new Label() { enableRichText = true };
                searchList.onSelectionChange += OnSearchListSelected;

                searchField = root.Q<ToolbarSearchField>();
                searchField.RegisterValueChangedCallback(evt => DosearchList(evt.newValue));
                DosearchList();
                root.RegisterCallback<PointerDownEvent>(evt =>
                {
                    if (!string.IsNullOrEmpty(searchField.value) && !searchList.Contains(evt.target as VisualElement))
                        searchField.value = string.Empty;
                });

                dialogueView = new DialogueView();
                dialogueView.nodeSelectedCallback = OnNodeSelected;
                dialogueView.nodeUnselectedCallback = OnNodeUnselected;
                root.Q("right-container").Insert(0, dialogueView);
                dialogueView.StretchToParentSize();

                list = root.Q<UnityEngine.UIElements.ListView>("dialogue-list");
                list.selectionType = SelectionType.Multiple;
                list.makeItem = () =>
                {
                    var label = new Label();
                    label.AddManipulator(new ContextualMenuManipulator(evt =>
                    {
                        if (label.userData is Dialogue dialogue)
                            evt.menu.AppendAction(Tr("删除"), a =>
                            {
                                if (EditorUtility.DisplayDialog(Tr("删除"), Tr("确定要该对话移至回收站吗?"), Tr("确定"), Tr("取消")))
                                    if (AssetDatabase.MoveAssetToTrash(AssetDatabase.GetAssetPath(dialogue)))
                                    {
                                        if (dialogue == selectedDialogue)
                                        {
                                            selectedDialogue = null;
                                            list.ClearSelection();
                                            InspectDialogue();
                                            dialogueView?.DrawDialgoueView(null);
                                        }
                                    }
                            });
                    }));
                    return label;
                };
                list.bindItem = (e, i) =>
                {
                    (e as Label).text = dialogues[i].name;
                    e.userData = dialogues[i];
                    e.tooltip = Dialogue.Editor.Preview(dialogues[i]);
                };
                list.onSelectionChange += (os) =>
                {
                    if (os != null) OnDialogueSelected(os.Select(x => x as Dialogue));
                };
                RefreshDialogues();
                inspector = root.Q<IMGUIContainer>("inspector");
                if (selectedDialogue)
                {
                    list.SetSelection(dialogues.IndexOf(selectedDialogue));
                    list.ScrollToItem(dialogues.IndexOf(selectedDialogue));
                }

                eventsView = root.Q("events-view");
                root.Q<Button>("events-button").clicked += ClickEvents;
                eventsInspector = root.Q<IMGUIContainer>("events-inspector");
                eventsView.style.display = DisplayStyle.None;
                eventsInspector.onGUIHandler = null;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }
        private void OnInspectorUpdate()
        {
            dialogueView?.CheckErrors();
        }
        private void OnProjectChange()
        {
            RefreshDialogues();
        }
        #endregion

        #region 各种回调
        private void DosearchList(string keywords = null)
        {
            IList itemsSource = new List<object>();
            Action<VisualElement, int> bindItem = (e, i) => { };
            bool empty = string.IsNullOrEmpty(keywords);
            searchList.style.display = empty ? DisplayStyle.None : DisplayStyle.Flex;
            if (!empty)
            {
                List<string> contents = new List<string>();
                List<string> tooltips = new List<string>();
                foreach (var item in dialogues)
                {
                    if (searchID(item, out var content, out var tooltip))
                    {
                        itemsSource.Add(item);
                        contents.Add(content);
                        tooltips.Add(tooltip);
                    }
                    else if (searchName(item, out content, out tooltip))
                    {
                        itemsSource.Add(item);
                        contents.Add(content);
                        tooltips.Add(tooltip);
                    }
                    else if (searchDesc(item, out content, out tooltip))
                    {
                        itemsSource.Add(item);
                        contents.Add(content);
                        tooltips.Add(tooltip);
                    }
                    else if (searchCon(item, out content, out tooltip))
                    {
                        itemsSource.Add(item);
                        contents.Add(content);
                        tooltips.Add(tooltip);
                    }
                }
                bindItem = (e, i) =>
                {
                    (e as Label).text = contents[i];
                    e.tooltip = tooltips[i];
                };

                #region 检索
                bool searchID(Dialogue dialogue, out string content, out string tooltip)
                {
                    content = null;
                    tooltip = null;
                    bool result = dialogue.ID.Contains(keywords);
                    if (result)
                    {
                        content = $"{dialogue.name}\n({Tr("ID")}: {ZetanUtility.Editor.HighlightContentByKey(dialogue.ID, keywords, dialogue.ID.Length)})";
                        tooltip = $"{dialogue.name}\n({Tr("ID")}: {dialogue.ID})";
                    }
                    return result;
                }
                bool searchName(Dialogue dialogue, out string content, out string tooltip)
                {
                    content = null;
                    tooltip = null;
                    bool result = dialogue.name.Contains(keywords);
                    if (result)
                    {
                        content = $"{ZetanUtility.Editor.HighlightContentByKey(dialogue.name, keywords, dialogue.name.Length)}";
                        tooltip = dialogue.name;
                    }
                    return result;
                }
                bool searchDesc(Dialogue dialogue, out string content, out string tooltip)
                {
                    content = null;
                    tooltip = null;
                    bool result = dialogue.description.Contains(keywords);
                    if (result)
                    {
                        content = $"{dialogue.name}\n({Tr("描述")}: {ZetanUtility.Editor.HighlightContentByKey(dialogue.description, keywords, 30)})";
                        tooltip = $"{dialogue.name}\n({Tr("描述")}: {dialogue.description})";
                    }
                    return result;
                }
                bool searchCon(Dialogue dialogue, out string content, out string tooltip)
                {
                    content = null;
                    tooltip = null;
                    foreach (var con in dialogue.Contents)
                    {
                        if (con is TextContent textCon)
                            if (textCon.Talker.Contains(keywords))
                            {
                                content = $"{dialogue.name}\n({Tr("内容讲述人")}: {ZetanUtility.Editor.HighlightContentByKey(textCon.Talker, keywords, 30)})";
                                tooltip = $"{dialogue.name}\n({Tr("内容讲述人")}: {textCon.Talker})";
                                return true;
                            }
                            else if (Keywords.Editor.HandleKeyWords(textCon.Talker).Contains(keywords))
                            {
                                var talker = textCon.Talker;
                                var kvps = Keywords.Editor.ExtractKeyWords(talker);
                                foreach (var kvp in kvps)
                                {
                                    talker = talker.Replace(kvp.Key, $"{kvp.Key}({kvp.Value})");
                                }
                                content = $"{dialogue.name}\n({Tr("内容讲述人")}: {ZetanUtility.Editor.HighlightContentByKey(talker, keywords, 30)})";
                                tooltip = $"{dialogue.name}\n({Tr("内容讲述人")}: {talker})";
                                return true;
                            }
                            else if (textCon.Text.Contains(keywords))
                            {
                                content = $"{dialogue.name}\n({Tr("内容文字")}: {ZetanUtility.Editor.HighlightContentByKey(textCon.Text, keywords, 30)})";
                                tooltip = $"{dialogue.name}\n({Tr("内容文字")}: {textCon.Text})";
                                return true;
                            }
                            else if (Keywords.Editor.HandleKeyWords(textCon.Text).Contains(keywords))
                            {
                                var text = textCon.Text;
                                var kvps = Keywords.Editor.ExtractKeyWords(text);
                                foreach (var kvp in kvps)
                                {
                                    text = text.Replace(kvp.Key, $"{kvp.Key}({kvp.Value})");
                                }
                                content = $"{dialogue.name}\n({Tr("内容文字")}: {ZetanUtility.Editor.HighlightContentByKey(text, keywords, 30)})";
                                tooltip = $"{dialogue.name}\n({Tr("内容文字")}: {text})";
                                return true;
                            }
                    }
                    return false;
                }
                #endregion
            }
            searchList.itemsSource = itemsSource;
            searchList.bindItem = bindItem;
            searchList.RefreshItems();
        }
        private void OnSearchListSelected(IEnumerable<object> os)
        {
            if (os.FirstOrDefault() is Dialogue item)
            {
                list.SetSelection(dialogues.IndexOf(item));
                list.ScrollToItem(dialogues.IndexOf(item));
            }
            searchList.SetSelectionWithoutNotify(null);
            searchField.value = null;
        }
        private void OnDialogueSelected(IEnumerable<Dialogue> dialogues)
        {
            if (dialogues.Count() == 1) selectedDialogue = dialogues?.FirstOrDefault();
            else selectedDialogue = null;
            dialogueView?.DrawDialgoueView(selectedDialogue);
            InspectDialogue();
        }
        private void OnNodeSelected(DialogueNode node)
        {
            inspector.onGUIHandler = null;
            inspector.Clear();
            if (node == null || dialogueView.nodes.Count(x => x.selected) > 1) return;
            inspector.onGUIHandler = () =>
            {
                if (node.SerializedContent?.serializedObject?.targetObject)
                {
                    node.SerializedContent.serializedObject.UpdateIfRequiredOrScript();
                    EditorGUI.BeginChangeCheck();
                    using var copy = node.SerializedContent.Copy();
                    SerializedProperty end = copy.GetEndProperty();
                    bool enter = true;
                    while (copy.NextVisible(enter) && !SerializedProperty.EqualContents(copy, end))
                    {
                        enter = false;
                        if (!copy.IsName("options") && !copy.IsName("events") && !copy.IsName("ExitHere")
                            && (node.content is TextContent || !copy.IsName("Text") && !copy.IsName("Talker")))
                            EditorGUILayout.PropertyField(copy, true);
                    }
                    if (EditorGUI.EndChangeCheck()) node.SerializedContent.serializedObject.ApplyModifiedProperties();
                    if (node.content is not INonEvent)
                        if (GUILayout.Button(Tr("查看事件"))) InspectEvents(node.SerializedContent.FindPropertyRelative("events"));
                }
            };
        }
        private void OnNodeUnselected(DialogueNode node)
        {
            if (dialogueView.nodes.Count(x => x.selected) < 1)
                InspectDialogue();
        }
        private void ClickNew()
        {
            Dialogue dialogue = ZetanUtility.Editor.SaveFilePanel(CreateInstance<Dialogue>);
            if (dialogue)
            {
                Selection.activeObject = dialogue;
                EditorGUIUtility.PingObject(dialogue);
                dialogueView?.DrawDialgoueView(dialogue);
            }
        }
        private void ClickDelete()
        {
            if (EditorUtility.DisplayDialog(Tr("删除"), Tr("确定要将选中的对话移至回收站吗?"), Tr("确定"), Tr("取消")))
                if (AssetDatabase.MoveAssetsToTrash(list.selectedItems.Select(x => AssetDatabase.GetAssetPath(x as Dialogue)).ToArray(), new List<string>()))
                {
                    selectedDialogue = null;
                    list.ClearSelection();
                    InspectDialogue();
                    dialogueView?.DrawDialgoueView(null);
                }
        }
        private void ClickEvents()
        {
            eventsView.style.display = DisplayStyle.None;
            eventsInspector.onGUIHandler = null;
        }
        #endregion

        #region 其它
        private void CopyOlds()
        {
            var olds = ZetanUtility.Editor.LoadAssets<OldDialogue>();
            olds.Sort((x, y) => string.Compare(x.name, y.name));
            foreach (var old in olds)
            {
                var dialog = CreateInstance<Dialogue>();
                Dialogue.Editor.CopyFromOld(dialog, old);
                AssetDatabase.CreateAsset(dialog, AssetDatabase.GenerateUniqueAssetPath("Assets/" + old.name + ".asset"));
            }
            AssetDatabase.SaveAssets();
        }

        private void InspectDialogue()
        {
            if (inspector == null) return;
            inspector.Clear();
            inspector.onGUIHandler = null;
            if (selectedDialogue)
            {
                var editor = UnityEditor.Editor.CreateEditor(selectedDialogue);
                inspector.onGUIHandler = () =>
                {
                    if (editor && editor.serializedObject?.targetObject)
                        editor.OnInspectorGUI();
                };
            }
        }

        private void RefreshDialogues()
        {
            dialogues = ZetanUtility.Editor.LoadAssets<Dialogue>();
            list.itemsSource = dialogues;
            list.Rebuild();
        }

        public void InspectEvents(SerializedProperty events)
        {
            if (events != null)
            {
                eventsView.style.display = DisplayStyle.Flex;
                eventsInspector.onGUIHandler = () =>
                {
                    events.serializedObject.UpdateIfRequiredOrScript();
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(events, new GUIContent(Tr("对话事件")));
                    if (EditorGUI.EndChangeCheck()) events.serializedObject.ApplyModifiedProperties();
                };
            }
        }

        private string Tr(string text)
        {
            return L.Tr(settings.language, text);
        }
        #endregion
    }
}