using System.Collections.Generic;
using System;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

public abstract class ConfigurationEditor<T> : EditorWindow where T : ScriptableObject
{
    protected List<T> objects;
    protected List<SearchResult> results;
    protected Vector2 scrollPos;
    protected int pageEach = 10;
    protected int page = 1;
    protected int maxPage = 1;
    protected string latestFolder;
    protected string keyWords;
    protected bool waitingRepaint;
    protected bool searching;

    protected ReorderableList objectsList;
    protected float lineHeight;
    protected float lineHeightSpace;

    private void OnEnable()
    {
        results = new List<SearchResult>();
        lineHeight = EditorGUIUtility.singleLineHeight;
        lineHeightSpace = lineHeight + 2;
        latestFolder = GetDefaultFolder();
        page = 1;
        Refresh();
    }

    private void OnGUI()
    {
        scrollPos = GUILayout.BeginScrollView(scrollPos);
        int pageBef = page;
        page = EditorGUILayout.IntSlider("当前页", page, 1, maxPage);
        if (pageBef != page) Refresh();
        if (!searching)
            EditorGUILayout.LabelField($"{GetConfigurationName()}配置文件数量", objects.Count.ToString());
        else
            EditorGUILayout.LabelField($"找到的{GetConfigurationName()}配置文件数量", results.Count.ToString());
        objectsList.DoLayoutList();
        objectsList.displayRemove = objectsList.selectedIndices.Count > 0;
        GUILayout.EndScrollView();

        waitingRepaint = false;
    }

    private void Refresh()
    {
        results.Clear();
        objects = ZetanEditorUtility.LoadAssets<T>();
        foreach (var obj in objects)
        {
            if (!searching)
                results.Add(new SearchResult(obj, GetElementNameLabel() + "：" + GetElementName(obj)));
            else if (CompareKey(obj, out var remark))
                results.Add(new SearchResult(obj, remark));
        }
        maxPage = Mathf.CeilToInt(results.Count * 1.0f / pageEach);
        while ((page - 1) * pageEach > results.Count && page > 1)
        {
            page--;
        }
        RefreshList();
    }

    protected virtual void DrawElementOperator(T element, Rect rect)
    {

    }

    private void RefreshList()
    {
        List<SearchResult> pagedList = new List<SearchResult>();
        for (int i = (page - 1) * pageEach; i < page * pageEach && i < results.Count; i++)
        {
            pagedList.Add(results[i]);
        }
        if (objectsList == null)
        {
            objectsList = new ReorderableList(pagedList, typeof(T), false, true, true, true);
            objectsList.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                if (waitingRepaint) return;
                int lineCount = 0;
                EditorGUI.LabelField(new Rect(rect.x, rect.y + lineCount * lineHeightSpace, rect.width, lineHeight), GetResult(objectsList.list[index]).remark);
                DrawElementOperator(GetResult(objectsList.list[index]).find, new Rect(rect.x, rect.y, rect.width - 80, lineHeight));
                if (GUI.Button(new Rect(rect.x + rect.width - 80, rect.y + lineCount * lineHeightSpace, 40, lineHeight), "移动"))
                {
                selection:
                    string folder = EditorUtility.OpenFolderPanel("选择移动文件夹", latestFolder, "");
                    if (!string.IsNullOrEmpty(folder))
                    {
                        if (ZetanEditorUtility.IsValidPath(folder))
                        {
                            try
                            {
                                AssetDatabase.MoveAsset(AssetDatabase.GetAssetPath(GetResult(objectsList.list[index]).find), $"{ZetanEditorUtility.ConvertToAssetsPath(folder)}/{ZetanEditorUtility.GetFileName(AssetDatabase.GetAssetPath(GetResult(objectsList.list[index]).find))}");
                                Refresh();
                                latestFolder = folder;
                            }
                            catch
                            {
                                if (EditorUtility.DisplayDialog("移动失败", "请选择Assets目录以下的文件夹。", "确定"))
                                    goto selection;
                            }
                        }
                        else
                        {
                            if (EditorUtility.DisplayDialog("提示", "请选择Assets目录以下的文件夹。", "确定"))
                                goto selection;
                        }
                    }
                }
                if (GUI.Button(new Rect(rect.x + rect.width - 40, rect.y + lineCount * lineHeightSpace, 40, lineHeight), "编辑"))
                    EditorUtility.OpenPropertyEditor(GetResult(objectsList.list[index]).find);
                lineCount++;
                EditorGUI.LabelField(new Rect(rect.x, rect.y + lineCount * lineHeightSpace, rect.width / 2, lineHeight), "配置文件：");
                GUI.enabled = false;
                EditorGUI.ObjectField(new Rect(rect.x + rect.width - 280, rect.y + lineCount * lineHeightSpace, 280, lineHeight), GetResult(objectsList.list[index]).find, typeof(T), false);
                GUI.enabled = true;
                lineCount++;
                EditorGUI.LabelField(new Rect(rect.x, rect.y + lineCount * lineHeightSpace, rect.width, lineHeight), $"路径：{AssetDatabase.GetAssetPath(GetResult(objectsList.list[index]).find)}");
                lineCount++;
            };
            objectsList.drawElementBackgroundCallback = (rect, index, isActive, isFocused) =>
            {
                if (waitingRepaint) return;
                EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, lineHeightSpace * 3), isActive ? new Color(0.5f, 0.5f, 0.5f, 0.5f) : (index % 2 != 0 ? Color.clear : new Color(1, 1, 1, 0.25f)));
            };
            objectsList.elementHeightCallback = (index) =>
            {
                return lineHeightSpace * 3;
            };
            objectsList.onAddDropdownCallback = (rect, list) =>
            {
                if (waitingRepaint) return;
                GenericMenu menu = new GenericMenu();
                MakeDropDownMenu(menu);
                menu.DropDown(rect);
            };
            objectsList.onRemoveCallback = (list) =>
            {
                if (waitingRepaint) return;
                if (list.selectedIndices.Count < 1) return;
                T obj = GetResult(list.list[list.selectedIndices[0]]).find;
                if (EditorUtility.DisplayDialog("警告", $"确定将{GetConfigurationName()} [{GetElementName(obj)}] 放入回收站吗？", "确定", "取消"))
                {
                    AssetDatabase.MoveAssetToTrash(AssetDatabase.GetAssetPath(obj));
                    AssetDatabase.Refresh();
                    Refresh();
                }
            };
            objectsList.onCanRemoveCallback = (list) =>
            {
                return list.IsSelected(list.index);
            };
            objectsList.drawHeaderCallback = (rect) =>
            {
                if (waitingRepaint) return;
                EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, lineHeight), "配置列表");
                if (GUI.Button(new Rect(rect.x + 55, rect.y, 40, lineHeight), "刷新"))
                {
                    GUI.FocusControl(null);
                    Refresh();
                }
                GUI.SetNextControlName("keyWords");
                keyWords = EditorGUI.TextField(new Rect(rect.x + rect.width - 300, rect.y, 100, lineHeight), keyWords);
                if (string.IsNullOrEmpty(keyWords)) searching = false;
                if (!searching)
                {
                    if (string.IsNullOrEmpty(keyWords)) GUI.enabled = false;
                    if (GUI.Button(new Rect(rect.x + rect.width - 200, rect.y, 40, lineHeight), "查找"))
                    {
                        GUI.FocusControl(null);
                        Search();
                    }
                    if (string.IsNullOrEmpty(keyWords)) GUI.enabled = true;
                }
                else if (GUI.Button(new Rect(rect.x + rect.width - 200, rect.y, 40, lineHeight), "返回"))
                {
                    GUI.FocusControl(null);
                    keyWords = string.Empty;
                    searching = false;
                    Refresh();
                }
                EditorGUI.LabelField(new Rect(rect.x + rect.width - 150, rect.y, 30, lineHeight), $"{page}/{maxPage}");
                if (GUI.Button(new Rect(rect.x + rect.width - 120, rect.y, 60, lineHeight), "上一页"))
                    if (page > 1)
                    {
                        GUI.FocusControl(null);
                        page--;
                        Refresh();
                    }
                if (GUI.Button(new Rect(rect.x + rect.width - 60, rect.y, 60, lineHeight), "下一页"))
                    if (page * pageEach <= results.Count)
                    {
                        GUI.FocusControl(null);
                        page++;
                        Refresh();
                    }
            };
            objectsList.drawNoneElementCallback = (rect) =>
            {
                EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, lineHeight), searching ? $"没有找到相关{GetConfigurationName()}配置" : $"暂无{GetConfigurationName()}配置，请点击 + 新建");
            };
            objectsList.multiSelect = false;
        }
        else objectsList.list = pagedList;
        objectsList.displayAdd = !searching;
        objectsList.ClearSelection();
        waitingRepaint = true;

        static SearchResult GetResult(object listObject)
        {
            return listObject as SearchResult;
        }
    }

    protected virtual void MakeDropDownMenu(GenericMenu menu)
    {
        menu.AddItem(new GUIContent($"增加新{GetConfigurationName()}"), false, CreateNewConfig, typeof(T));
    }

    protected virtual void CreateNewConfig(object args)
    {
        Type subType = args as Type;
    selection:
        string folder = EditorUtility.OpenFolderPanel("选择保存文件夹", latestFolder, "");
        if (!string.IsNullOrEmpty(folder))
        {
            if (ZetanEditorUtility.IsValidPath(folder))
            {
                try
                {
                    T objectInstance = (T)CreateInstance(subType);
                    AssetDatabase.CreateAsset(objectInstance, AssetDatabase.GenerateUniqueAssetPath($"{folder.Replace(Application.dataPath, "Assets")}/{GetNewFileName(subType)}.asset"));
                    AssetDatabase.Refresh();

                    EditorUtility.OpenPropertyEditor(objectInstance);

                    Refresh();
                    latestFolder = folder;
                }
                catch
                {
                    if (EditorUtility.DisplayDialog("新建失败", "请选择Assets目录以下的文件夹。", "确定"))
                        goto selection;
                }
            }
            else
            {
                if (EditorUtility.DisplayDialog("提示", "请选择Assets目录以下的文件夹。", "确定"))
                    goto selection;
            }
        }
    }

    private void Search()
    {
        searching = true;
        Refresh();
    }

    protected virtual bool CompareKey(T element, out string remark)
    {
        if (element && GetElementName(element).Contains(keyWords))
        {
            remark = "名称：" + ZetanEditorUtility.TrimContentByKey(GetElementName(element), keyWords, 16);
            return true;
        }
        else
        {
            remark = string.Empty;
            return false;
        }
    }

    protected virtual string GetDefaultFolder()
    {
        return $"{Application.dataPath}/Resources/Configuration";
    }

    protected virtual string GetNewFileName(Type subType)
    {
        return "new config";
    }


    protected virtual string GetConfigurationName()
    {
        return string.Empty;
    }

    protected virtual string GetElementNameLabel()
    {
        return "名称";
    }

    protected virtual string GetElementName(T element)
    {
        if (!element) return string.Empty;
        return element.name;
    }

    protected class SearchResult
    {
        public readonly T find;
        public readonly string remark;

        public SearchResult(T find, string remark)
        {
            this.find = find;
            this.remark = remark;
        }
    }
}