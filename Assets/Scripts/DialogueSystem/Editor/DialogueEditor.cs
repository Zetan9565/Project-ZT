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
        public static void CreateWindow(NewDialogue dialogue)
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
            if (EditorUtility.InstanceIDToObject(instanceID) is NewDialogue tree)
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
        private DialogueView dialogueView;
        private UnityEngine.UIElements.ListView list;
        private List<NewDialogue> dialogues;
        private NewDialogue selectedDialogue;
        private IMGUIContainer inspector;
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

                dialogueView = new DialogueView();
                dialogueView.nodeSelectedCallback = OnNodeSelected;
                dialogueView.nodeUnselectedCallback = OnNodeUnselected;
                root.Q("right-container").Insert(0, dialogueView);
                dialogueView.StretchToParentSize();

                list = root.Q<UnityEngine.UIElements.ListView>();
                list.selectionType = SelectionType.Multiple;
                list.makeItem = () =>
                {
                    return new Label();
                };
                list.bindItem = (e, i) =>
                {
                    (e as Label).text = dialogues[i].name;
                    e.userData = dialogues[i];
                };
                list.onSelectionChange += (os) =>
                {
                    if (os != null) OnDialogueSelected(os.Select(x => x as NewDialogue));
                };
                RefreshDialogues();
                inspector = root.Q<IMGUIContainer>("inspector");
                if (selectedDialogue)
                {
                    list.SetSelection(dialogues.IndexOf(selectedDialogue));
                    list.ScrollToItem(dialogues.IndexOf(selectedDialogue));
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        private void OnProjectChange()
        {
            RefreshDialogues();
        }
        #endregion

        #region 各种回调
        private void OnDialogueSelected(IEnumerable<NewDialogue> dialogues)
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
                    if (node.content is not INonEvent)
                        EditorGUILayout.PropertyField(node.SerializedContent.FindPropertyRelative("events"));
                    if (EditorGUI.EndChangeCheck()) node.SerializedContent.serializedObject.ApplyModifiedProperties();
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
            NewDialogue dialogue = ZetanUtility.Editor.SaveFilePanel(CreateInstance<NewDialogue>);
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
                if (AssetDatabase.MoveAssetsToTrash(list.selectedItems.Select(x => AssetDatabase.GetAssetPath(x as NewDialogue)).ToArray(), new List<string>()))
                {
                    selectedDialogue = null;
                    list.ClearSelection();
                    InspectDialogue();
                    dialogueView?.DrawDialgoueView(selectedDialogue);
                }
        }
        #endregion

        #region 其它
        private void CopyOlds()
        {
            var olds = ZetanUtility.Editor.LoadAssets<Dialogue>();
            olds.Sort((x, y) => string.Compare(x.name, y.name));
            foreach (var old in olds)
            {
                var dialog = CreateInstance<NewDialogue>();
                NewDialogue.Editor.CopyFromOld(dialog, old);
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
            dialogues = ZetanUtility.Editor.LoadAssets<NewDialogue>();
            list.itemsSource = dialogues;
            list.Rebuild();
        }

        private string Tr(string text)
        {
            return L.Tr(settings.language, text);
        }
        #endregion

    }
}