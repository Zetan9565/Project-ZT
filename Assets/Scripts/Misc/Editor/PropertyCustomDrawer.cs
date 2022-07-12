using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using ZetanStudio.ItemSystem;
using ZetanStudio.ItemSystem.Editor;
using ZetanStudio.ItemSystem.Module;
using ZetanStudio.ConditionSystem;

public class ItemInfoListDrawer
{
    public ReorderableList List { get; }

    public ItemInfoListDrawer(SerializedObject owner, SerializedProperty property, float lineHeight, float lineHeightSpace, string listTitle = "道具数量列表")
    {
        List = new ReorderableList(owner, property, true, true, true, true)
        {
            drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                SerializedProperty itemInfo = property.GetArrayElementAtIndex(index);
                SerializedProperty item = itemInfo.FindPropertyRelative("item");
                SerializedProperty amount = itemInfo.FindPropertyRelative("amount");
                if (item.objectReferenceValue != null)
                    EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, lineHeight), $"[{index}] {(item.objectReferenceValue as Item).Name}");
                else
                    EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, lineHeight), $"[{index}] (空)");
                EditorGUI.PropertyField(new Rect(rect.x + rect.width / 2f, rect.y, rect.width / 2f, lineHeight),
                    item, new GUIContent(string.Empty));
                EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace, rect.width, lineHeight),
                    amount, new GUIContent("数量"));
                if (amount.intValue < 1) amount.intValue = 1;
            },

            elementHeightCallback = (int index) =>
            {
                return 2 * lineHeightSpace;
            },

            onCanRemoveCallback = (list) =>
            {
                return list.IsSelected(list.index);
            },

            drawHeaderCallback = (rect) =>
                {
                    int notCmpltCount = 0;
                    SerializedProperty item;
                    for (int i = 0; i < property.arraySize; i++)
                    {
                        item = property.GetArrayElementAtIndex(i);
                        if (!item.FindPropertyRelative("item").objectReferenceValue || item.FindPropertyRelative("amount").intValue < 1)
                            notCmpltCount++;
                    }
                    EditorGUI.LabelField(rect, listTitle, "数量：" + property.arraySize + (notCmpltCount > 0 ? "\t未补全：" + notCmpltCount : string.Empty));
                },

            drawNoneElementCallback = (rect) =>
            {
                EditorGUI.LabelField(rect, "空列表");
            }
        };
    }

    public void DoLayoutDraw()
    {
        List?.DoLayoutList();
    }

    public void DoDraw(Rect rect)
    {
        List?.DoList(rect);
    }

    public float GetDrawHeight()
    {
        if (List == null) return 0;
        return List.GetHeight();
    }
}

public class MaterialListDrawer
{
    public ReorderableList List { get; }

    public MaterialListDrawer(SerializedProperty property, float? lineHeight = null, float? lineHeightSpace = null, string listTitle = "材料列表")
    {
        lineHeight ??= EditorGUIUtility.singleLineHeight;
        lineHeightSpace ??= EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        List = new ReorderableList(property.serializedObject, property, true, true, true, true)
        {
            drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                SerializedProperty material = property.GetArrayElementAtIndex(index);
                SerializedProperty materialType = material.FindPropertyRelative("materialType");
                SerializedProperty costType = material.FindPropertyRelative("costType");
                SerializedProperty item = material.FindPropertyRelative("item");
                SerializedProperty amount = material.FindPropertyRelative("amount");

                string headLabel = $"[{index}] (空)";
                if (item.objectReferenceValue && (MaterialCostType)costType.enumValueIndex == MaterialCostType.SingleItem)
                    headLabel = $"[{index}] {(item.objectReferenceValue as Item).Name}";
                else if ((MaterialCostType)costType.enumValueIndex == MaterialCostType.SameType)
                {
                    headLabel = $"[{index}] {MaterialTypeEnum.Instance[materialType.intValue]}";
                }
                EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, (float)lineHeight), headLabel);
                EditorGUI.PropertyField(new Rect(rect.x + rect.width / 2f, rect.y, rect.width / 2f, (float)lineHeight), costType, new GUIContent(string.Empty));
                if (costType.enumValueIndex == (int)MaterialCostType.SameType)
                {
                    var typeBef = materialType.enumValueIndex;
                    EditorGUI.PropertyField(new Rect(rect.x, rect.y + (float)lineHeightSpace, rect.width, (float)lineHeight), materialType, new GUIContent("所需种类"));
                    if (typeBef != materialType.enumValueIndex)
                        for (int i = 0; i < property.arraySize; i++)
                        {
                            if (i != index)
                            {
                                SerializedProperty element = property.GetArrayElementAtIndex(i);
                                SerializedProperty eMaterialType = element.FindPropertyRelative("materialType");
                                SerializedProperty eMakingType = element.FindPropertyRelative("costType");
                                SerializedProperty eItem = element.FindPropertyRelative("item");
                                if (eMakingType.enumValueIndex == (int)MaterialCostType.SingleItem)
                                {
                                    if (eItem.objectReferenceValue is Item ei)
                                        if (MaterialModule.SameType(MaterialTypeEnum.Instance[materialType.intValue], ei))
                                        {
                                            EditorUtility.DisplayDialog("错误", $"与第 {i + 1} 个材料的道具类型冲突", "确定");
                                            materialType.enumValueIndex = typeBef;
                                        }
                                }
                                else
                                {
                                    if (eMaterialType.intValue == materialType.intValue)
                                    {
                                        EditorUtility.DisplayDialog("错误", $"第 {i + 1} 个材料已使用该类型", "确定");
                                        materialType.enumValueIndex = typeBef;
                                    }
                                }
                            }
                        }
                }
                else
                {
                    var itemBef = item.objectReferenceValue;
                    EditorGUI.PropertyField(new Rect(rect.x, rect.y + (float)lineHeightSpace, rect.width, (float)lineHeight), item, new GUIContent("所需材料"));
                    if (itemBef != item.objectReferenceValue && item.objectReferenceValue is Item itemNow)
                        for (int i = 0; i < property.arraySize; i++)
                        {
                            if (i != index)
                            {
                                SerializedProperty element = property.GetArrayElementAtIndex(i);
                                SerializedProperty eMaterialType = element.FindPropertyRelative("materialType");
                                SerializedProperty eMakingType = element.FindPropertyRelative("costType");
                                if (eMakingType.enumValueIndex == (int)MaterialCostType.SameType)
                                    if (MaterialModule.SameType(MaterialTypeEnum.Instance[eMaterialType.enumValueIndex], itemNow))
                                    {
                                        EditorUtility.DisplayDialog("错误", $"第 {i + 1} 个材料的类型 [{MaterialTypeEnum.IndexToName(eMaterialType.intValue)}] 已包括这个道具", "确定");
                                        item.objectReferenceValue = itemBef;
                                    }
                            }
                        }
                }
                EditorGUI.PropertyField(new Rect(rect.x, rect.y + (float)lineHeightSpace * 2, rect.width, (float)lineHeight),
                    amount, new GUIContent("所需数量"));
                if (amount.intValue < 1) amount.intValue = 1;
            },

            elementHeightCallback = (int index) =>
            {
                return 3 * (float)lineHeightSpace;
            },

            onAddCallback = (list) =>
            {
                property.InsertArrayElementAtIndex(property.arraySize);
                SerializedProperty material = property.GetArrayElementAtIndex(property.arraySize - 1);
                list.Select(property.arraySize - 1);
                SerializedProperty materialType = material.FindPropertyRelative("materialType");
                materialType.intValue = 0;
                SerializedProperty costType = material.FindPropertyRelative("costType");
                costType.enumValueIndex = (int)MaterialCostType.SingleItem;
                SerializedProperty item = material.FindPropertyRelative("item");
                item.objectReferenceValue = null;
                SerializedProperty amount = material.FindPropertyRelative("amount");
                amount.intValue = 1;
            },

            onCanRemoveCallback = (list) =>
            {
                return list.IsSelected(list.index);
            },

            drawHeaderCallback = (rect) =>
            {
                int notCmpltCount = 0;
                SerializedProperty material;
                for (int i = 0; i < property.arraySize; i++)
                {
                    material = property.GetArrayElementAtIndex(i);
                    if (material.FindPropertyRelative("costType").enumValueIndex == (int)MaterialCostType.SingleItem && !material.FindPropertyRelative("item").objectReferenceValue)
                        notCmpltCount++;
                }
                EditorGUI.LabelField(rect, listTitle, notCmpltCount > 0 ? "未补全：" + notCmpltCount : string.Empty);
            },

            drawNoneElementCallback = (rect) =>
            {
                EditorGUI.LabelField(rect, "空列表");
            }
        };
    }

    public void DoLayoutDraw()
    {
        List?.DoLayoutList();
    }

    public void DoDraw(Rect rect)
    {
        List?.DoList(rect);
    }

    public float GetDrawHeight()
    {
        if (List == null) return 0;
        return List.GetHeight();
    }
}

public class DropItemListDrawer
{
    public ReorderableList List { get; }

    public DropItemListDrawer(SerializedProperty property, float lineHeight, float lineHeightSpace, string listTitle = "产出列表", IEnumerable<Item> items = null)
    {
        items ??= Item.Editor.GetItems();
        List = new ReorderableList(property.serializedObject, property, true, true, true, true)
        {
            drawElementCallback = (Rect position, int index, bool isActive, bool isFocused) =>
            {
                SerializedProperty itemInfo = property.GetArrayElementAtIndex(index);
                SerializedProperty item = itemInfo.FindPropertyRelative("item");
                if (item.objectReferenceValue != null)
                    EditorGUI.PropertyField(new Rect(position.x + 8f, position.y, position.width / 2f, lineHeight), itemInfo, new GUIContent($"[{index}] {(item.objectReferenceValue as Item).Name}"));
                else
                    EditorGUI.PropertyField(new Rect(position.x + 8f, position.y, position.width / 2f, lineHeight), itemInfo, new GUIContent($"[{index}] (空)"));
                SerializedProperty amount = itemInfo.FindPropertyRelative("Amount");
                SerializedProperty onlyDropForQuest = itemInfo.FindPropertyRelative("onlyDropForQuest");
                ItemSelectorDrawer.Draw(new Rect(position.x + position.width / 2f, position.y, position.width / 2f, lineHeight), item, new GUIContent(string.Empty), () => items);
                if (itemInfo.isExpanded)
                {
                    int lineCount = 1;
                    EditorGUI.PropertyField(new Rect(position.x, position.y + lineHeightSpace * lineCount, position.width, lineHeight * 2 + EditorGUIUtility.standardVerticalSpacing),
                        amount, new GUIContent("数量"));
                    lineCount += 2;
                    EditorGUI.PropertyField(new Rect(position.x, position.y + lineHeightSpace * lineCount, position.width, lineHeight),
                        onlyDropForQuest, new GUIContent("只为此任务产出"));
                }
            },

            elementHeightCallback = (int index) =>
            {
                if (index < 0 || index > property.arraySize - 1) return 0;
                int lineCount = 1;
                SerializedProperty dropItem = property.GetArrayElementAtIndex(index);
                if (dropItem.isExpanded)
                {
                    lineCount += 3;//数量、百分比、只在
                }
                return lineCount * lineHeightSpace;
            },

            onCanRemoveCallback = (list) =>
            {
                return list.IsSelected(list.index);
            },

            drawHeaderCallback = (rect) =>
            {
                int notCmpltCount = 0;
                SerializedProperty dropItem;
                for (int i = 0; i < property.arraySize; i++)
                {
                    dropItem = property.GetArrayElementAtIndex(i);
                    if (!dropItem.FindPropertyRelative("item").objectReferenceValue)
                        notCmpltCount++;
                }
                EditorGUI.LabelField(rect, listTitle, notCmpltCount > 0 ? "未补全：" + notCmpltCount : string.Empty);
            },

            drawNoneElementCallback = (rect) =>
            {
                EditorGUI.LabelField(rect, "空列表");
            }
        };
    }

    public void DoLayoutDraw()
    {
        List?.DoLayoutList();
    }

    public void DoDraw(Rect rect)
    {
        List?.DoList(rect);
    }

    public float GetDrawHeight()
    {
        if (List == null) return 0;
        return List.GetHeight();
    }
}

public class ConditionGroupDrawer
{
    private readonly SerializedProperty property;
    private readonly List<TalkerInformation> talkers;
    private readonly List<Quest> quests;

    private readonly float lineHeightSpace;

    public ReorderableList List { get; }

    public ConditionGroupDrawer(SerializedProperty property, float lineHeight, float lineHeightSpace, string listTitle = "条件列表")
    {
        this.property = property;
        this.lineHeightSpace = lineHeightSpace;
        SerializedProperty conditions = property.FindPropertyRelative("conditions");
        talkers = ZetanUtility.Editor.LoadAssets<TalkerInformation>("").FindAll(x => x.Enable);
        quests = ZetanUtility.Editor.LoadAssets<Quest>("").FindAll(x => x != property.serializedObject.targetObject);
        List = new ReorderableList(property.serializedObject, conditions, true, true, true, true)
        {
            drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                SerializedProperty condition = conditions.GetArrayElementAtIndex(index);
                EditorGUI.PropertyField(rect, condition, new GUIContent(Condition.GetName(condition.managedReferenceValue.GetType())), true);
            },

            elementHeightCallback = (index) =>
            {
                return EditorGUI.GetPropertyHeight(conditions.GetArrayElementAtIndex(index), true);
            },

            onCanRemoveCallback = (list) =>
            {
                return list.IsSelected(list.index);
            },

            onAddDropdownCallback = (rect, list) =>
            {
                GenericMenu menu = new GenericMenu();
                foreach (var type in TypeCache.GetTypesDerivedFrom<Condition>())
                {
                    if (!type.IsAbstract)
                    {
                        string group = Condition.GetGroup(type);
                        string name = Condition.GetName(type);
                        if (!string.IsNullOrEmpty(group))
                            group = group.EndsWith('/') ? group : group + '/';
                        if (string.IsNullOrEmpty(name)) name = type.Name;
                        menu.AddItem(new GUIContent($"{group}{name}"), false, insert, type);
                    }
                }
                menu.ShowAsContext();

                void insert(object type)
                {
                    conditions.arraySize++;
                    var prop = conditions.GetArrayElementAtIndex(conditions.arraySize - 1);
                    prop.managedReferenceValue = Activator.CreateInstance(type as Type);
                    conditions.serializedObject.ApplyModifiedProperties();
                    List.Select(conditions.arraySize - 1);
                }
            },

            drawHeaderCallback = (rect) =>
            {
                int notCmpltCount = 0;
                for (int i = 0; i < conditions.arraySize; i++)
                {
                    if (conditions.GetArrayElementAtIndex(i).managedReferenceValue is Condition condition && !condition.IsValid) notCmpltCount++;
                }
                EditorGUI.LabelField(rect, listTitle, "数量：" + conditions.arraySize + (notCmpltCount > 0 ? "\t未补全：" + notCmpltCount : string.Empty));
            },

            drawNoneElementCallback = (rect) =>
            {
                EditorGUI.LabelField(rect, "空列表");
            }
        };
    }

    public void DoLayoutDraw()
    {
        if (List != null && List.count > 0)
        {
            EditorGUI.BeginChangeCheck();
            var re = property.FindPropertyRelative("relational");
            EditorGUILayout.PropertyField(re, new GUIContent("(?)条件关系表达式", re.tooltip));
            if (EditorGUI.EndChangeCheck()) property.serializedObject.ApplyModifiedProperties();
        }
        List?.DoLayoutList();
    }

    public void DoDraw(Rect rect)
    {
        if (List != null && List.count > 0)
        {
            EditorGUI.BeginChangeCheck();
            var re = property.FindPropertyRelative("relational");
            EditorGUI.PropertyField(rect, re, new GUIContent("(?)条件关系表达式", re.tooltip));
            if (EditorGUI.EndChangeCheck()) property.serializedObject.ApplyModifiedProperties();
        }
        List?.DoList(List != null && List.count > 0 ? new Rect(rect.x, rect.y + lineHeightSpace, rect.width, rect.height) : rect);
    }

    public float GetDrawHeight()
    {
        if (List == null) return 0;
        float height = List.GetHeight();
        if (List.count > 0)
            height += lineHeightSpace;
        return height;
    }
}

public class SceneSelectionDrawer
{
    private readonly string[] sceneNames;

    private readonly SerializedProperty property;
    private readonly string label;

    public SceneSelectionDrawer(SerializedProperty property, string label = "场景", string nameNull = "未选择")
    {
        List<string> sceneNames = new List<string>() { nameNull };
        foreach (var scene in EditorBuildSettings.scenes)
        {
            var find = (AssetDatabase.LoadAssetAtPath<SceneAsset>(scene.path));
            if (find) sceneNames.Add(find.name);
        }
        this.sceneNames = sceneNames.ToArray();
        this.property = property;
        this.label = label;
    }

    public int DoLayoutDraw()
    {
        int index = Array.FindIndex(sceneNames, x => x == property.stringValue);
        index = index < 0 ? 0 : index;
        index = EditorGUILayout.Popup(label, index, sceneNames);
        if (index < 1 || index > sceneNames.Length) property.stringValue = string.Empty;
        else property.stringValue = sceneNames[index];
        return index;
    }

    public int DoDraw(Rect rect)
    {
        int index = Array.FindIndex(sceneNames, x => x == property.stringValue);
        index = index < 0 ? 0 : index;
        index = EditorGUI.Popup(rect, label, index, sceneNames);
        if (index < 1 || index > sceneNames.Length) property.stringValue = string.Empty;
        else property.stringValue = sceneNames[index];
        return index;
    }
}

public class ObjectSelectionDrawer<T> where T : UnityEngine.Object
{
    private readonly T[] objects;
    private readonly string[] objectNames;

    private readonly SerializedProperty property;
    private readonly string label;

    public ObjectSelectionDrawer(SerializedProperty property, string fieldAsName, string path, string label = "", string nameNull = "未选择") :
        this(property, fieldAsName, filter: null, path, label, nameNull)
    { }
    public ObjectSelectionDrawer(SerializedProperty property, string fieldAsName, Func<T, string> groupPicker, string path, string label = "", string nameNull = "未选择") :
        this(property, fieldAsName, groupPicker, null, path, label, nameNull)
    { }

    public ObjectSelectionDrawer(SerializedProperty property, string fieldAsName, Func<T, bool> filter, string path, string label = "", string nameNull = "未选择") :
        this(property, fieldAsName, null, filter, path, label, nameNull)
    { }
    public ObjectSelectionDrawer(SerializedProperty property, string fieldAsName, Func<T, string> groupPicker, Func<T, bool> filter, string path, string label = "", string nameNull = "未选择")
    {
        if (filter == null) objects = ZetanUtility.Editor.LoadAssets<T>(string.IsNullOrEmpty(path) ? string.Empty : path).ToArray();
        else objects = ZetanUtility.Editor.LoadAssets<T>(string.IsNullOrEmpty(path) ? string.Empty : path).Where(x => filter.Invoke(x)).ToArray();
        objectNames = HandleByGroup(fieldAsName, groupPicker, nameNull);
        this.property = property;
        this.label = label;
    }

    public ObjectSelectionDrawer(SerializedProperty property, string fieldAsName, T[] resources, string label = "", string nameNull = "未选择") :
        this(property, fieldAsName, null, resources, label, nameNull)
    { }
    public ObjectSelectionDrawer(SerializedProperty property, string fieldAsName, Func<T, string> groupPicker, T[] resources, string label = "", string nameNull = "未选择")
    {
        objects = resources;
        objectNames = HandleByGroup(fieldAsName, groupPicker, nameNull);
        this.property = property;
        this.label = label;
    }

    public int DoLayoutDraw()
    {
        int index = Array.IndexOf(objects, property.objectReferenceValue) + 1;
        index = index < 0 ? 0 : index;
        Rect rect = EditorGUILayout.GetControlRect();
        index = EditorGUI.Popup(new Rect(rect.x, rect.y, rect.width, rect.height), label, index, objectNames);
        if (index < 1 || index > objects.Length) property.objectReferenceValue = null;
        else property.objectReferenceValue = objects[index - 1];
        return index;
    }

    public int DoDraw(Rect rect)
    {
        int index = Array.FindIndex(objects, x => x == property.objectReferenceValue) + 1;
        index = index < 0 ? 0 : index;
        index = EditorGUI.Popup(new Rect(rect.x, rect.y, rect.width, rect.height), label, index, objectNames);
        if (index < 1 || index > objects.Length) property.objectReferenceValue = null;
        else property.objectReferenceValue = objects[index - 1];
        return index;
    }

    private string[] HandleByGroup(string fieldAsName, Func<T, string> groupPicker, string nameNull)
    {
        List<string> objectNames = new List<string>() { nameNull };
        int ungrouped = 0;
        Dictionary<string, List<char>> counters = new Dictionary<string, List<char>>();
        foreach (var obj in objects)
        {
            if (!string.IsNullOrEmpty(fieldAsName))
            {
                var field = obj.GetType().GetField(fieldAsName, ZetanUtility.CommonBindingFlags);
                if (field != null) objectNames.Add(GetGroupedName(obj, field.GetValue(obj).ToString()));
                else objectNames.Add(GetGroupedName(obj, obj.name));
            }
            else objectNames.Add(GetGroupedName(obj, obj.name));
        }
        return objectNames.ToArray();

        string GetGroupedName(T obj, string name)
        {
            if (groupPicker == null)
            {
                ungrouped++;
                return $"[{ungrouped}] {name}";
            }
            string group = groupPicker(obj);
            if (string.IsNullOrEmpty(group))
            {
                ungrouped++;
                return $"[{ungrouped}] {name}";
            }
            else
            {
                int index = 1;
                if (counters.TryGetValue(group, out var counter))
                {
                    counter.Add(' ');
                    index = counter.Count;
                }
                else counters.Add(group, new List<char>() { ' ' });
                return $"{group}/[{index}] {name}";
            }
        }
    }
}

public class PaginatedReorderableList : IDisposable
{
    private IList list;
    private Type eType;
    private SerializedProperty property;
    private SerializedObject serializedObject;

    public string title;
    private IList resultList;
    private ReorderableList orderList;
    private bool m_Draggable;
    private bool m_DisplayAdd;
    private bool m_DisplayRemove;

    private bool m_DisplaySearch;
    private bool searching;
    private string keyWords;
    private int pageBef;

    private readonly float lineHeight;
    private readonly float lineHeightSpace;
    private const float headerHeight = 20.0f;
    private const float headerHeightSearch = 45.0f;

    private IList pagedList;
    private int IndexOffset => (Page - 1) * m_PageSize;
    private int page = 1;
    private int Page { get => page; set => page = value < 1 ? 1 : value; }
    private int maxPage = 1;
    private int MaxPage { get => maxPage; set => maxPage = value < 1 ? 1 : value; }
    private int m_PageSize = 10;

    private Action<Rect, PaginatedReorderableList> m_OnAddDropdownCallback;
    private Action<Rect> m_DrawFooterCallback;

    private bool enableBef;
    private int oldCount;

#pragma warning disable IDE1006 // 命名样式
    public Action<Rect, int, bool, bool> drawElementCallback { get; set; }
    public Func<int, float> elementHeightCallback { get; set; }
    public Action<PaginatedReorderableList> onAddCallback { get; set; }
    public Action<PaginatedReorderableList> onRemoveCallback { get; set; }
    public Func<PaginatedReorderableList, bool> onCanAddCallback { get; set; }
    public Func<PaginatedReorderableList, bool> onCanRemoveCallback { get; set; }
    public Func<string, SerializedProperty, bool> searchFilter { get; set; }
    public Action<Rect> drawHeaderCallback { get; set; }
    public Action<Rect> drawFooterCallback
    {
        get => m_DrawFooterCallback;
        set
        {
            if (value != m_DrawFooterCallback)
            {
                m_DrawFooterCallback = value;
                if (value != null)
                    orderList.drawFooterCallback = (rect) =>
                    {
                        m_DrawFooterCallback(rect);
                    };
            }
            else if (value == null) orderList.drawFooterCallback = null;
        }
    }
    public Action<Rect, PaginatedReorderableList> onAddDropdownCallback
    {
        get => m_OnAddDropdownCallback;
        set
        {
            if (value != m_OnAddDropdownCallback)
            {
                m_OnAddDropdownCallback = value;
                if (value != null)
                    orderList.onAddDropdownCallback = (rect, list) =>
                    {
                        m_OnAddDropdownCallback(rect, this);
                    };
            }
            else if (value == null) orderList.onAddDropdownCallback = null;
        }
    }

    public int index { get => ToRealIndex(orderList.index); set => Select(value); }

    public int count
    {
        get
        {
            if (property != null)
            {
                if (property.minArraySize > serializedObject.maxArraySizeForMultiEditing && serializedObject.isEditingMultipleObjects)
                {
                    return 0;
                }

                return property.minArraySize;
            }

            return list.Count;
        }
    }
    public IEnumerable<int> selectedIndices => orderList.selectedIndices.Select(x => ToRealIndex(x));
    public bool multiSelect { get => orderList.multiSelect; set => orderList.multiSelect = value; }
    public bool draggable { get => m_Draggable; set => orderList.draggable = m_Draggable = value; }
    public bool displayAdd { get => m_DisplayAdd; set => orderList.displayAdd = m_DisplayAdd = value; }
    public bool displayRemove { get => m_DisplayRemove; set => orderList.displayRemove = m_DisplayRemove = value; }
    public bool displaySearch
    {
        get => m_DisplaySearch;
        set
        {
            if (value != m_DisplaySearch)
            {
                m_DisplaySearch = value;
                Refresh();
            }
        }
    }
    public int pageSize
    {
        get => m_PageSize;
        set
        {
            if (value > 0 && value != m_PageSize)
            {
                m_PageSize = value;
                Refresh();
            }
        }
    }
    public SerializedProperty serializedProperty
    {
        get => property;
        set
        {
            if (!SerializedProperty.EqualContents(value, property))
                InitList(value, m_PageSize);
        }
    }
#pragma warning restore IDE1006 // 命名样式

    private int ToRealIndex(int index)
    {
        if (index < 0 || index > pagedList.Count - 1) return -1;
        return list.IndexOf(pagedList[index]);
    }
    private int ToLocalIndex(int index)
    {
        if (index < 0 || index > list.Count - 1) return -1;
        return pagedList.IndexOf(list[index]);
    }
    public void Select(int index)
    {
        TurnPageToIndex(index);
        orderList.Select(ToLocalIndex(index));
    }
    public void SelectRange(int indexFrom, int indexTo)
    {
        TurnPageToIndex(indexFrom);
        orderList.SelectRange(ToLocalIndex(indexFrom), ToLocalIndex(indexTo));
    }
    private void TurnPageToIndex(int index)
    {
        if (index < -1 || index > list.Count - 1) return;
        var oldPage = page;
        while (index < (page - 1) * pageSize && page > 1)
        {
            Page--;
        }
        while (index > page * pageSize - 1 && page < maxPage)
        {
            Page++;
        }
        if (oldPage != page) Refresh();
    }

    public void Deselect(int index) => orderList.Deselect(ToLocalIndex(index));
    public void ClearSelection() => orderList.ClearSelection();
    public bool IsSelected(int index) => orderList.IsSelected(ToLocalIndex(index));

    public PaginatedReorderableList(string title, SerializedProperty property, int pageSize = 10, bool displaySearch = true, bool draggable = true, bool displayAddButton = true, bool displayRemoveButton = true)
    {
        lineHeight = EditorGUIUtility.singleLineHeight;
        lineHeightSpace = lineHeight + 2;
        Undo.undoRedoPerformed += Refresh;
        m_Draggable = draggable;
        m_DisplayAdd = displayAddButton;
        m_DisplayRemove = displayRemoveButton;
        m_DisplaySearch = displaySearch;
        this.title = title;
        InitList(property, pageSize);
    }
    public PaginatedReorderableList(SerializedProperty property, int pageSize = 10, bool displaySearch = true, bool draggable = true, bool displayAddButton = true, bool displayRemoveButton = true)
        : this(null, property, pageSize, displaySearch, draggable, displayAddButton, displayRemoveButton) { }

    ~PaginatedReorderableList()
    {
        Undo.undoRedoPerformed -= Refresh;
    }

    private void InitList(SerializedProperty property, int pageSize)
    {
        if (!ZetanUtility.Editor.TryGetValue(property, out var value, out var field))
            throw new ArgumentException($"路径 {property.propertyPath} 不存在");

        var type = field.FieldType;
        if (!typeof(IList).IsAssignableFrom(type))
            throw new ArgumentException($"路径 {property.propertyPath} 不是数组或列表");

        this.property = property;
        serializedObject = property.serializedObject;
        if (value is null)
        {
            value = Activator.CreateInstance(type);
            ZetanUtility.Editor.TrySetValue(property, value);
            serializedObject.UpdateIfRequiredOrScript();
        }
        list = value as IList;
        eType = type.GetGenericArguments()[0];
        resultList = ZetanUtility.CreateListInstance(eType);
        pagedList = ZetanUtility.CreateListInstance(eType);
        Page = 1;
        m_PageSize = pageSize > 0 ? pageSize : 10;
        Refresh();
    }

    private void Refresh()
    {
        try
        {
            serializedObject.UpdateIfRequiredOrScript();
            oldCount = property.arraySize;
            resultList.Clear();
            for (int i = 0; i < list.Count; i++)
            {
                if (string.IsNullOrEmpty(keyWords) || searchFilter != null && searchFilter(keyWords, property.GetArrayElementAtIndex(i).Copy()) || searchFilter == null && property.GetArrayElementAtIndex(i).displayName.Contains(keyWords))
                    resultList.Add(list[i]);
            }
            MaxPage = Mathf.CeilToInt(resultList.Count * 1.0f / m_PageSize);
            while (IndexOffset > resultList.Count && Page > 1)
            {
                Page--;
            }
            RefreshList();
        }
        catch { }
    }

    private void Search()
    {
        searching = true;
        pageBef = Page;
        Refresh();
    }

    private void RefreshList()
    {
        pagedList.Clear();
        for (int i = IndexOffset; i < Page * m_PageSize && i < resultList.Count; i++)
        {
            pagedList.Add(resultList[i]);
        }
        orderList = new ReorderableList(pagedList, eType, m_Draggable, true, m_DisplayAdd, m_DisplayRemove)
        {
            drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                if (drawElementCallback != null) drawElementCallback(rect, index, isActive, isFocused);
                else
                {
                    SerializedProperty element = property.GetArrayElementAtIndex(ToRealIndex(index));
                    ReorderableList.defaultBehaviours.DrawElement(new Rect(rect.x + 8f, rect.y, rect.width - 8f, rect.height), element, null, orderList.IsSelected(index), isFocused, draggable, true);
                }
            },
            elementHeightCallback = (index) =>
            {
                if (index < 0 || index >= pagedList.Count) return 0;
                if (elementHeightCallback != null) return elementHeightCallback(ToRealIndex(index));
                SerializedProperty element = property.GetArrayElementAtIndex(ToRealIndex(index));
                return EditorGUI.GetPropertyHeight(element);
            },
            onAddCallback = (list) =>
            {
                int oldSize = property.arraySize;
                int index = ToRealIndex(list.index);
                if (onAddCallback != null) onAddCallback(this);
                else
                {
                    serializedObject.UpdateIfRequiredOrScript();
                    if (property.arraySize < 1) property.InsertArrayElementAtIndex(0);
                    else property.InsertArrayElementAtIndex(index);
                    serializedObject.ApplyModifiedProperties();
                }
                Refresh();
                Select(index + 1);
            },
            onRemoveCallback = (list) =>
            {
                if (onRemoveCallback != null) onRemoveCallback(this);
                else
                {
                    serializedObject.UpdateIfRequiredOrScript();
                    property.DeleteArrayElementAtIndex(ToRealIndex(list.index));
                    serializedObject.ApplyModifiedProperties();
                }
                Refresh();
            },
            onCanRemoveCallback = (list) =>
            {
                return (onCanRemoveCallback == null || onCanRemoveCallback(this))
                       && (property.minArraySize <= serializedObject.maxArraySizeForMultiEditing
                       || !serializedObject.isEditingMultipleObjects);
            },
            headerHeight = m_DisplaySearch ? headerHeightSearch : headerHeight,
            drawHeaderCallback = (rect) =>
            {
                float inputWidth = GUI.skin.label.CalcSize(new GUIContent($"{MaxPage}")).x + 4;
                float pageWidth = GUI.skin.label.CalcSize(new GUIContent($"{MaxPage}")).x;
                float fixedWidth = GUI.skin.horizontalScrollbarLeftButton.fixedWidth;
                float rightOffset = rect.width - fixedWidth + 5;
                float pageOffset = rightOffset - pageWidth - 1;
                float inputOffset = pageOffset - inputWidth - 1;
                float leftOffset = inputOffset - fixedWidth - 1;
                float sizeWidth = GUI.skin.label.CalcSize(new GUIContent($"{pageSize}")).x + 4;
                float sizeOffset = leftOffset - sizeWidth - 1;
                float totalWidth = GUI.skin.label.CalcSize(new GUIContent($"{L10n.Tr("Total")}: {list.Count}")).x;
                float totalOffset = sizeOffset - totalWidth - 3;
                if (drawHeaderCallback != null) drawHeaderCallback(new Rect(rect.x, rect.y, totalOffset, lineHeight));
                else EditorGUI.LabelField(new Rect(rect.x, rect.y, totalOffset, lineHeight), new GUIContent(string.IsNullOrEmpty(title) ? property.displayName : title, property.tooltip), EditorStyles.boldLabel);
                enableBef = GUI.enabled;
                GUI.enabled = true;
                GUIStyle style = new GUIStyle(EditorStyles.numberField)
                {
                    alignment = TextAnchor.MiddleCenter
                };
                var newSize = EditorGUI.IntField(new Rect(rect.x + sizeOffset, rect.y + 1, sizeWidth, lineHeight - 2), m_PageSize, style);
                if (newSize < 1) newSize = 1;
                if (newSize != m_PageSize)
                {
                    m_PageSize = newSize;
                    Refresh();
                }
                EditorGUI.BeginDisabledGroup(Page <= 1);
                if (GUI.Button(new Rect(rect.x + leftOffset, rect.y + 2, fixedWidth, lineHeight), string.Empty, GUI.skin.horizontalScrollbarLeftButton))
                    if (Page > 1)
                    {
                        GUI.FocusControl(null);
                        Page--;
                        Refresh();
                    }
                EditorGUI.EndDisabledGroup();
                var newPage = EditorGUI.IntField(new Rect(rect.x + inputOffset, rect.y + 1, inputWidth, lineHeight - 2), Page, style);
                if (newPage < 1) newPage = 1;
                if (newPage > MaxPage) newPage = MaxPage;
                if (newPage != Page)
                {
                    Page = newPage;
                    Refresh();
                }
                style = ZetanUtility.Editor.Style.middleRight;
                EditorGUI.LabelField(new Rect(rect.x + totalOffset, rect.y, totalWidth, lineHeight - 2), $"{L10n.Tr("Total")}: {list.Count}", style);
                EditorGUI.LabelField(new Rect(rect.x + pageOffset, rect.y, pageWidth, lineHeight - 2), $"{MaxPage}", style);
                EditorGUI.BeginDisabledGroup(Page >= MaxPage);
                if (GUI.Button(new Rect(rect.x + rightOffset, rect.y + 2, fixedWidth, lineHeight), string.Empty, GUI.skin.horizontalScrollbarRightButton))
                    if (Page * m_PageSize <= resultList.Count)
                    {
                        GUI.FocusControl(null);
                        Page++;
                        Refresh();
                    }
                EditorGUI.EndDisabledGroup();
                if (m_DisplaySearch)
                {
                    var headerRect = new Rect(rect);
                    headerRect.xMin -= 6f;
                    headerRect.xMax += 6f;
                    headerRect.height += 2f;
                    headerRect.height -= 20f;
                    headerRect.y -= 1f;
                    headerRect.y += 20f;
                    GUI.Box(headerRect, string.Empty);
                    GUI.SetNextControlName("keyWords");
                    string oldKeyWords = keyWords;
                    keyWords = EditorGUI.TextField(new Rect(rect.x, rect.y + lineHeightSpace + 2.5f, rect.width - 52, lineHeight), keyWords, EditorStyles.toolbarSearchField);
                    if (string.IsNullOrEmpty(keyWords)) searching = false;
                    if (!string.IsNullOrEmpty(oldKeyWords) && string.IsNullOrEmpty(keyWords))
                    {
                        searching = false;
                        Page = pageBef;
                        Refresh();
                    }
                    if (!searching)
                    {
                        EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(keyWords));
                        if (GUI.Button(new Rect(rect.x + rect.width - 50, rect.y + lineHeightSpace + 2f, 50, lineHeight), L10n.Tr("Search")))
                        {
                            GUI.FocusControl(null);
                            Search();
                        }
                        EditorGUI.EndDisabledGroup();
                    }
                    else if (GUI.Button(new Rect(rect.x + rect.width - 50, rect.y + lineHeightSpace + 2f, 50, lineHeight), L10n.Tr("Clear")))
                    {
                        GUI.FocusControl(null);
                        keyWords = string.Empty;
                        searching = false;
                        Page = pageBef;
                        Refresh();
                    }
                }
                GUI.enabled = enableBef;
            },
            drawFooterCallback = drawFooterCallback == null ? null : (rect) =>
            {
                if (drawFooterCallback != null) drawFooterCallback(rect);
                else ReorderableList.defaultBehaviours.DrawFooter(rect, orderList);
            },
            onAddDropdownCallback = onAddDropdownCallback == null ? null : (rect, list) =>
            {
                m_OnAddDropdownCallback(rect, this);
            },
            onReorderCallbackWithDetails = (list, oldIndex, newIndex) =>
            {
                (oldIndex, newIndex) = (ToRealIndex(newIndex), ToRealIndex(oldIndex));
                serializedObject.UpdateIfRequiredOrScript();
                SerializedProperty element1 = property.GetArrayElementAtIndex(oldIndex);
                SerializedProperty element2 = property.GetArrayElementAtIndex(newIndex);
                (element2.isExpanded, element1.isExpanded) = (element1.isExpanded, element2.isExpanded);
                property.MoveArrayElement(oldIndex, newIndex);
                serializedObject.ApplyModifiedProperties();
                Refresh();
            },
        };
    }

    public void DoLayoutList()
    {
        if (serializedObject.isEditingMultipleObjects)
        {
            EditorGUILayout.PropertyField(property, true);
            return;
        }
        if (oldCount != property.arraySize) Refresh();
        orderList.DoLayoutList();
        orderList.draggable = !searching;
    }

    public void DoList(Rect rect)
    {
        if (serializedObject.isEditingMultipleObjects)
        {
            EditorGUI.PropertyField(rect, property, true);
            return;
        }
        if (oldCount != property.arraySize) Refresh();
        orderList.DoList(rect);
        orderList.draggable = !searching;
    }

    public void Dispose()
    {
        Undo.undoRedoPerformed -= Refresh;
    }
}