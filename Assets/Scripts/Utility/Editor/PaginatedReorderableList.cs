using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace ZetanStudio.Editor
{
    public class PaginatedReorderableList : IDisposable
    {
        private IList list;
        private bool isArray;
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
        private string keywords;
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
                    if (property.minArraySize > serializedObject.maxArraySizeForMultiEditing && serializedObject.isEditingMultipleObjects) return 0;
                    else return property.minArraySize;
                else return list.Count;
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
                if (!SerializedProperty.EqualContents(value, property)) InitList(value, m_PageSize);
            }
        }
#pragma warning restore IDE1006 // 命名样式

        private int ToRealIndex(int index) => index < 0 || index > pagedList.Count - 1 ? -1 : list.IndexOf(pagedList[index]);
        private int ToLocalIndex(int index) => index < 0 || index > list.Count - 1 ? -1 : pagedList.IndexOf(list[index]);
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
            if (!Utility.Editor.TryGetValue(property, out var value, out var field))
                throw new ArgumentException($"路径 {property.propertyPath} 不存在");

            if (!property.isArray)
                throw new ArgumentException($"路径 {property.propertyPath} 不是数组或列表");

            this.property = property;
            serializedObject = property.serializedObject;

            var type = field.FieldType;
            isArray = type.IsArray;
            eType = isArray ? type.GetElementType() : type.GetGenericArguments()[0];
            if (value is null)
            {
                value = isArray ? Array.CreateInstance(eType, 0) : Activator.CreateInstance(type);
                Utility.Editor.TrySetValue(property, value);
                serializedObject.UpdateIfRequiredOrScript();
            }
            list = value as IList;
            resultList = Activator.CreateInstance(typeof(List<>).MakeGenericType(eType)) as IList;
            pagedList = Activator.CreateInstance(typeof(List<>).MakeGenericType(eType)) as IList;
            Page = 1;
            m_PageSize = pageSize > 0 ? pageSize : 10;
            Refresh();
        }

        private void Refresh()
        {
            try
            {
                serializedObject.UpdateIfRequiredOrScript();
                if (isArray)
                {
                    Utility.Editor.TryGetValue(property, out var value);
                    list = value as IList;
                }
                oldCount = property.arraySize;
                resultList.Clear();
                for (int i = 0; i < list.Count; i++)
                {
                    if (string.IsNullOrEmpty(keywords) || searchFilter != null && searchFilter(keywords, property.GetArrayElementAtIndex(i).Copy()) || searchFilter == null && property.GetArrayElementAtIndex(i).displayName.Contains(keywords))
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

        public void Search(string keywords)
        {
            this.keywords = keywords;
            GUI.FocusControl(null);
            Search();
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
                    style = Utility.Editor.Style.middleRight;
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
                        string oldKeyWords = keywords;
                        GUI.SetNextControlName("PaginatedList Search");
                        keywords = EditorGUI.TextField(new Rect(rect.x, rect.y + lineHeightSpace + 2.5f, rect.width - 52, lineHeight), keywords, EditorStyles.toolbarSearchField);
                        if (string.IsNullOrEmpty(keywords)) searching = false;
                        if (!string.IsNullOrEmpty(oldKeyWords) && string.IsNullOrEmpty(keywords))
                        {
                            searching = false;
                            Page = pageBef;
                            Refresh();
                        }
                        if (!searching)
                        {
                            EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(keywords));
                            if (GUI.Button(new Rect(rect.x + rect.width - 50, rect.y + lineHeightSpace + 2f, 50, lineHeight), L10n.Tr("Search"))
                               || GUI.GetNameOfFocusedControl() == "PaginatedList Search" && (Event.current.keyCode == KeyCode.KeypadEnter || Event.current.keyCode == KeyCode.Return))
                            {
                                GUI.FocusControl(null);
                                Search();
                            }
                            EditorGUI.EndDisabledGroup();
                        }
                        else if (GUI.Button(new Rect(rect.x + rect.width - 50, rect.y + lineHeightSpace + 2f, 50, lineHeight), L10n.Tr("Clear")))
                        {
                            GUI.FocusControl(null);
                            keywords = string.Empty;
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
                    oldIndex = IndexOffset + oldIndex;
                    newIndex = IndexOffset + newIndex;
                    SerializedProperty element1 = property.GetArrayElementAtIndex(oldIndex);
                    SerializedProperty element2 = property.GetArrayElementAtIndex(newIndex);
                    (element1.isExpanded, element2.isExpanded) = (element2.isExpanded, element1.isExpanded);
                    Undo.RegisterCompleteObjectUndo(serializedObject.targetObject, "Reorder Element In Array");
                    property.MoveArrayElement(oldIndex, newIndex);
                    serializedObject.ApplyModifiedPropertiesWithoutUndo();
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
}