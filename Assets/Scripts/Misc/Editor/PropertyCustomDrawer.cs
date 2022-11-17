using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using ZetanStudio.CharacterSystem;
using ZetanStudio.ConditionSystem;
using ZetanStudio.ItemSystem;
using ZetanStudio.ItemSystem.Editor;
using ZetanStudio.ItemSystem.Module;
using ZetanStudio.QuestSystem;

namespace ZetanStudio
{
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
            talkers = Utility.Editor.LoadAssets<TalkerInformation>("").FindAll(x => x.Enable);
            quests = Utility.Editor.LoadAssets<Quest>("").FindAll(x => x != property.serializedObject.targetObject);
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
            if (filter == null) objects = Utility.Editor.LoadAssets<T>(string.IsNullOrEmpty(path) ? string.Empty : path).ToArray();
            else objects = Utility.Editor.LoadAssets<T>(string.IsNullOrEmpty(path) ? string.Empty : path).Where(x => filter.Invoke(x)).ToArray();
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
                    var field = obj.GetType().GetField(fieldAsName, Utility.CommonBindingFlags);
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
}