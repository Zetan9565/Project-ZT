using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using ZetanStudio.Editor;

public class QuestEditor : EditorWindow
{
    private List<(QuestGroup group, Quest quest)> datas = new List<(QuestGroup group, Quest quest)>();

    private Editor editor;
    private QuestEditorSettings settings;
    private IMGUIContainer inspector;
    private UnityEngine.UIElements.ListView list;
    private ToolbarSearchField searchField;
    private ZetanStudio.Editor.TabbedBar tabBar;
    private string keyword;

    [MenuItem("Window/Zetan Studio/任务编辑器")]
    public static void Create()
    {
        QuestEditor wnd = GetWindow<QuestEditor>();
        wnd.minSize = QuestEditorSettings.GetOrCreate().minWindowSize;
        wnd.titleContent = new GUIContent("任务编辑器");
    }

    public void CreateGUI()
    {
        settings = settings ? settings : QuestEditorSettings.GetOrCreate();

        VisualElement root = rootVisualElement;

        var visualTree = settings.treeUxml;
        visualTree.CloneTree(root);
        var styleSheet = settings.treeUss;
        root.styleSheets.Add(styleSheet);
        list = root.Q<UnityEngine.UIElements.ListView>("quest-list");
        list.makeItem = () =>
        {
            var label = new Label();
            label.AddManipulator(new ContextualMenuManipulator((evt) =>
            {
                evt.menu.AppendAction("删除", a =>
                {
                    var data = ((QuestGroup group, Quest quest))label.userData;
                    if (EditorUtility.DisplayDialog("警告", $"确定要删除{(data.group ? "任务组" : "任务")} <{(data.group ? data.group.Name : data.quest.Title)}> 吗", "确定", "取消"))
                    {
                        ScriptableObject obj;
                        if (data.group) obj = data.group;
                        else obj = data.quest;
                        AssetDatabase.MoveAssetToTrash(AssetDatabase.GetAssetPath(obj));
                        datas.Remove(data);
                        if (editor.target == obj)
                        {
                            inspector.Clear();
                            inspector.onGUIHandler = null;
                            list.ClearSelection();
                        }
                        RefreshList();
                    }
                });
            }));
            return label;
        };
        list.bindItem = (e, i) =>
        {
            e.userData = datas[i];
            if (datas[i].group) (e as Label).text = datas[i].group.Name;
            else (e as Label).text = datas[i].quest.Title;
        };
        list.onSelectionChange += (o) =>
        {
            if (o != null && o.FirstOrDefault() is not null)
            {
                (QuestGroup group, Quest quest) data = ((QuestGroup, Quest))o.FirstOrDefault();
                //SerializedObject serializedObject = new SerializedObject(o.FirstOrDefault() as Quest);
                if (data.group) editor = Editor.CreateEditor(data.group);
                else editor = Editor.CreateEditor(data.quest);
                if (editor is QuestInspector ins) ins.AddAnimaListener(Repaint);
                inspector.Clear();
                inspector.onGUIHandler = () =>
                {
                    //serializedObject.UpdateIfRequiredOrScript();
                    //EditorGUI.BeginChangeCheck();
                    string nameBef = null;
                    if (editor.target is QuestGroup group1) nameBef = group1.Name;
                    else if (editor.target is Quest quest1) nameBef = quest1.Title;
                    if (editor.target) editor.OnInspectorGUI();
                    if (editor.target is QuestGroup group2 && group2.Name != nameBef || editor.target is Quest quest2 && quest2.Title != nameBef)
                        list.RefreshItem(datas.IndexOf(data));
                    //if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
                };
            }
        };
        inspector = root.Q<IMGUIContainer>("quest-inspector");
        tabBar = root.Q<ZetanStudio.Editor.TabbedBar>();
        tabBar.Refresh(new string[] { "任务", "任务组" }, OnTabChanged);
        tabBar.onRightClick = OnTabMenu;
        tabBar.SetSelected(1);
        searchField = root.Q<ToolbarSearchField>();
        searchField.RegisterValueChangedCallback(new EventCallback<ChangeEvent<string>>(evt =>
        {
            keyword = evt.newValue;
        }));
        searchField.RegisterCallback(new EventCallback<KeyDownEvent>(evt =>
        {
            if (!string.IsNullOrEmpty(searchField.value) && searchField.focusController.focusedElement == searchField && (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter))
                SearchByKeyword();
        }));
        ToolbarButton search = root.Q<ToolbarButton>("search-button");
        search.clicked += SearchByKeyword;
        ToolbarButton reset = root.Q<ToolbarButton>("reset-results");
        reset.clicked += () => OnTabChanged(tabBar.SelectedIndex);
    }
    private void OnFocus()
    {
        RefreshList();
    }
    private void SearchByKeyword()
    {
        switch (tabBar.SelectedIndex)
        {
            case 1:
                datas.RemoveAll(x => !x.quest || !x.quest.ID.Contains(keyword) && !x.quest.Title.Contains(keyword) && !x.quest.Description.Contains(keyword));
                break;
            case 2:
                datas.RemoveAll(x => !x.group || !x.group.ID.Contains(keyword) && !x.group.Name.Contains(keyword));
                break;
            default:
                break;
        }
        list.ClearSelection();
        RefreshList();
        searchField.value = null;
    }
    private void OnTabChanged(int index)
    {
        switch (index)
        {
            case 1:
                MakeQuestList();
                break;
            case 2:
                MakeGroupList();
                break;
            default:
                break;
        }
    }

    private void MakeGroupList()
    {
        var groupTemp = ZetanUtility.Editor.LoadAssets<QuestGroup>();
        datas.Clear();
        foreach (var group in groupTemp)
        {
            datas.Add((group, null));
        }
        list.ClearSelection();
        RefreshList();
    }

    private void MakeQuestList()
    {
        var questTemp = ZetanUtility.Editor.LoadAssets<Quest>();
        datas.Clear();
        foreach (var quest in questTemp)
        {
            datas.Add((null, quest));
        }
        list.ClearSelection();
        RefreshList();
    }

    private void OnTabMenu(int index, ContextualMenuPopulateEvent evt)
    {
        switch (index)
        {
            case 1:
                evt.menu.AppendAction("新增任务", a =>
                {
                    Quest quest = ZetanUtility.Editor.SaveFilePanel(CreateInstance<Quest>, ping: true);
                    if (quest)
                    {
                        var item = (default(QuestGroup), quest);
                        datas.Add(item);
                        if (tabBar.SelectedIndex == 1)
                        {
                            list.Rebuild();
                            list.SetSelection(datas.IndexOf(item));
                            list.ScrollToItem(datas.IndexOf(item));
                        }
                    }
                });
                if (tabBar.SelectedIndex == 1)
                    evt.menu.AppendAction("刷新列表", a =>
                    {
                        MakeQuestList();
                    });
                break;
            case 2:
                evt.menu.AppendAction("新增分组", a =>
                {
                    QuestGroup group = ZetanUtility.Editor.SaveFilePanel(CreateInstance<QuestGroup>, ping: true);
                    if (group)
                    {
                        var item = (group, default(Quest));
                        datas.Add(item);
                        if (tabBar.SelectedIndex == 2)
                        {
                            list.Rebuild();
                            list.SetSelection(datas.IndexOf(item));
                            list.ScrollToItem(datas.IndexOf(item));
                        }
                    }
                });
                if (tabBar.SelectedIndex == 2)
                    evt.menu.AppendAction("刷新列表", a =>
                    {
                        MakeGroupList();
                    });
                break;
            default:
                break;
        }
    }
    private void RefreshList()
    {
        if (list != null)
        {
            list.itemsSource = datas;
            list.Rebuild();
        }
    }
    private void OnDisable()
    {
        if (editor is QuestInspector ins) ins.RemoveAnimaListener(Repaint);
    }
    private void OnDestroy()
    {
        DestroyImmediate(editor);
    }
}