using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace ZetanStudio.Editor
{
    public sealed class TableView<T> : TreeView
    {
        #region 委托声明
        public delegate void RowGUICallback(T data, int index, bool focused, bool selected);
        public delegate void DrawCellCallback(Rect rect, T data, int column, int index, bool focused, bool selected);
        public delegate float GetCustomRowHeightCallback(int index, T data);
        public delegate void DrawFooterCallback(ref Rect rect);
        public delegate int SortingCallback(int column, T left, T right);

        public delegate bool DropRowsCallback(int[] draggedIndices, int inserAtIndex, out int[] newIndices);
        public delegate void DataModifiedCallback(IList<T> data);

        public delegate void SingleClickedItemCallback(int index, T data);
        public delegate void DoubleClickedItemCallback(int index, T data);
        public delegate void ContextClickedItemCallback(int index, T data);

        public delegate bool SearchCallback(string searchString, T data, int column);
        public delegate void CustomSearchRectCallback(ref Rect rect);

        public delegate void SelectionChangedCallback(IList<int> selectedindices);
        public delegate int CheckErrorsCallback(int column, out string errorString);

        public delegate void ClickedInsertRowCallback(int index);
        public delegate void ClickedDeleteRowsCallback(IList<int> indices);
        public delegate int ClickedReplaceCallback(string replaceString, T[] visableData, bool replaceAll);

        public delegate void ClickedDeleteColumnCallback(int column);
        #endregion

        #region 搜索相关声明
        public bool displaySearchField = true;
        public bool displaySearchDropdown = true;
        public bool displayIgnoreCase = true;
        public bool displayWholeMatching = true;
        #endregion

        #region 页脚相关声明
        public bool displayFooter;
        public bool displayRowButtons = true;
        public bool displayColumnButtons = true;
        public bool displayReplaceButtons = true;
        public bool displayLocateButton = true;
        public int minColumnCanDelete;
        #endregion

        #region 功能回调
        public RowGUICallback rowGUICallback;
        public DrawCellCallback drawCellCallback;
        public GetCustomRowHeightCallback rowHeightCallback;
        public DrawFooterCallback drawFooterCallback;
        private SortingCallback m_SortingCallback;

        public DropRowsCallback dropRowsCallback;
        public DataModifiedCallback dataModified;

        public SingleClickedItemCallback clickedRowCallback;
        public DoubleClickedItemCallback doubleClickedRowCallback;
        public ContextClickedItemCallback rightClickedRowCallback;

        public SearchCallback searchCallback;
        public CustomSearchRectCallback searchRectCallback;

        public SelectionChangedCallback selectionChanged;
        public CheckErrorsCallback checkErrorsCallback;
        #endregion

        #region 按钮点击回调
        public ClickedInsertRowCallback insertClicked;
        public ClickedDeleteRowsCallback deleteClicked;
        public ClickedReplaceCallback replaceClicked;

        public ClickedDeleteColumnCallback deleteColumnClicked;
        public Action appendClicked;
        public Action addColumnClicked;
        #endregion

        public bool multiSelect = true;
        public bool draggable;

        private IList<T> m_Data;

        private int m_SearchColumn;
        private bool m_IgnoreCase = true;
        private bool m_WholeMatching;
        private int oldSearchColumn;
        private bool oldIgnoreCase;
        private bool oldWholeMatching;
        private readonly SearchField searchField;
        private readonly string[] searchDropdownNames;
        private readonly int[] searchDropdownValues;

        private readonly Texture2D errorTexture = EditorGUIUtility.FindTexture("CollabError");
        private int errorRow;

        private readonly bool firstRowIsHeader;
        private readonly bool initialized;

        private int columnToDelete;
        private string replaceString;
        private bool dirty;
        private readonly HashSet<int> visableRowIndices = new HashSet<int>();
        private static readonly PropertyInfo indexer;

        private const string genericDragID = "TableViewGenericDragColumnDragging";

#pragma warning disable IDE1006
        public new TableViewState state => base.state as TableViewState;
        public IList<T> data
        {
            get => m_Data;
            set
            {
                if (m_Data != value)
                {
                    m_Data = value;
                    Reload();
                }
            }
        }

        public SortingCallback sortingCallback
        {
            get => m_SortingCallback;
            set
            {
                if (m_SortingCallback != value)
                {
                    m_SortingCallback = value;
                    SetDirty();
                }
            }
        }
        public int searchColumn
        {
            get => m_SearchColumn;
            set
            {
                if (m_SearchColumn != value)
                {
                    m_SearchColumn = value;
                    if (hasSearch) SetDirty();
                }
            }
        }
        public bool ignoreCase
        {
            get => m_IgnoreCase;
            set
            {
                if (m_IgnoreCase != value)
                {
                    m_IgnoreCase = value;
                    if (hasSearch) SetDirty();
                }
            }
        }
        public bool wholeMatching
        {
            get => m_WholeMatching;
            set
            {
                if (m_WholeMatching != value)
                {
                    m_WholeMatching = value;
                    if (hasSearch) SetDirty();
                }
            }
        }

        public float totalHeightWithSearchField => totalHeight + (displaySearchField ? 20 : 0);

        private bool inOrder => multiColumnHeader.state.sortedColumnIndex == 0 && multiColumnHeader.IsSortedAscending(0);
#pragma warning restore IDE1006

        #region 点击回调
        protected override void SingleClickedItem(int id)
        {
            if (FindItem(id, rootItem) is TableRow<T> tr) clickedRowCallback?.Invoke(tr.id, tr.data);
        }
        protected override void ContextClickedItem(int id)
        {
            if (FindItem(id, rootItem) is TableRow<T> tr) rightClickedRowCallback?.Invoke(tr.id, tr.data);
        }
        protected override void DoubleClickedItem(int id)
        {
            if (FindItem(id, rootItem) is TableRow<T> tr) doubleClickedRowCallback?.Invoke(tr.id, tr.data);
        }
        #endregion

        #region 构造函数
        static TableView()
        {
            foreach (var pro in typeof(T).GetProperties())
            {
                if (pro.GetIndexParameters().Length == 1 && pro.GetIndexParameters()[0].ParameterType == typeof(int))
                {
                    indexer = pro;
                    break;
                }
            }
        }

        public TableView(TableViewState state, IList<T> data, TableColumn[] columns, DrawCellCallback drawCellCallback = null, SortingCallback sortingCallback = null) :
            this(new MultiColumnHeader(GenerateHeaderState(columns, sortingCallback)), state, data, drawCellCallback, sortingCallback, false)
        { }

        public TableView(TableViewState state, IList<T> data, int column, int primary, DrawCellCallback drawCellCallback = null, SortingCallback sortingCallback = null) :
            this(new MultiColumnHeader(GenerateHeaderState(column, primary, data, sortingCallback)), state, data, drawCellCallback, sortingCallback, true, true)
        { }

        public TableView(TableViewState state, IList<T> data, string[] columns, int primary, DrawCellCallback drawCellCallback = null, SortingCallback sortingCallback = null) :
            this(new MultiColumnHeader(GenerateHeaderState(columns, null, primary, sortingCallback)), state, data, drawCellCallback, sortingCallback)
        { }

        public TableView(TableViewState state, IList<T> data, string[] columns, int primary, Func<int, string> columTooltipCallback, DrawCellCallback drawCellCallback = null, SortingCallback sortingCallback = null) :
            this(new MultiColumnHeader(GenerateHeaderState(columns, columTooltipCallback, primary, sortingCallback)), state, data, drawCellCallback, sortingCallback)
        { }

        private TableView(MultiColumnHeader multicolumnHeader, TableViewState state, IList<T> data, DrawCellCallback drawCellCallback, SortingCallback sortingCallback, bool resizeToFit = true, bool firstRowIsHeader = false) : base(state, multicolumnHeader)
        {
            this.drawCellCallback = drawCellCallback;
            this.m_SortingCallback = sortingCallback;
            m_Data = data;
            this.firstRowIsHeader = firstRowIsHeader;

            rowHeight = 20f;
            columnIndexForTreeFoldouts = 0;
            showAlternatingRowBackgrounds = true;
            showBorder = true;
            customFoldoutYOffset = (20f - EditorGUIUtility.singleLineHeight) * 0.5f;
            extraSpaceBeforeIconAndLabel = 18f;
            multicolumnHeader.SetSorting(0, true);
            multicolumnHeader.sortingChanged += m => Reload();
            if (resizeToFit) multiColumnHeader.ResizeToFit();

            searchField = new SearchField();
            searchField.downOrUpArrowKeyPressed += SetFocusAndEnsureSelectedItem;
            searchDropdownNames = new string[multicolumnHeader.state.columns.Length];
            searchDropdownNames[0] = EDL.Tr("全部");
            for (int i = 1; i < searchDropdownNames.Length; i++)
            {
                searchDropdownNames[i] = multicolumnHeader.state.columns[i].headerContent.text;
            }
            searchDropdownValues = new int[searchDropdownNames.Length];
            for (int i = 0; i < searchDropdownValues.Length; i++)
            {
                searchDropdownValues[i] = i - 1;
            }
            m_SearchColumn = state.searchColumn;
            var max = searchDropdownValues.Max();
            if (m_SearchColumn > max) m_SearchColumn = max;

            Reload();

            initialized = true;
        }
        #endregion

        #region 绘制
        protected override TreeViewItem BuildRoot() => new TreeViewItem() { id = -1, depth = -1 };
        protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
        {
            visableRowIndices.Clear();
            var rows = new List<TreeViewItem>();
            for (int i = firstRowIsHeader ? 1 : 0; i < m_Data.Count; i++)
            {
                if (!hasSearch || Search(m_Data[i]))
                {
                    rows.Add(new TableRow<T>(i, m_Data[i]));
                    visableRowIndices.Add(i);
                }
            }
            SortIfNeeded(rows);
            SetupParentsAndChildrenFromDepths(root, rows);
            return rows;
        }
        private void SortIfNeeded(List<TreeViewItem> rows)
        {
            if (multiColumnHeader.sortedColumnIndex >= 0)
                rows.Sort((x, y) =>
                {
                    bool ascending = multiColumnHeader.IsSortedAscending(multiColumnHeader.sortedColumnIndex);
                    if (multiColumnHeader.sortedColumnIndex == 0)
                    {
                        if (ascending) return (x as TableRow<T>).id.CompareTo((y as TableRow<T>).id);
                        else return -(x as TableRow<T>).id.CompareTo((y as TableRow<T>).id);
                    }
                    else
                    {
                        if (ascending) return sort(x as TableRow<T>, y as TableRow<T>);
                        else return -sort(x as TableRow<T>, y as TableRow<T>);

                        int sort(TableRow<T> left, TableRow<T> right)
                        {
                            if (m_SortingCallback != null) return m_SortingCallback(multiColumnHeader.sortedColumnIndex - 1, left.data, right.data);
                            else return left.data.ToString().CompareTo(right.data.ToString());
                        }
                    }
                });
        }

        public override void OnGUI(Rect rect)
        {
            if (!dirty)
            {
                CheckErrors();

                if (displaySearchField)
                {
                    var searchRect = new Rect(rect.x, rect.y, rect.width, 20f);
                    searchRectCallback?.Invoke(ref searchRect);
                    var rightWidth = 0;
                    if (displayIgnoreCase) rightWidth += 36;
                    if (displayWholeMatching) rightWidth += 42;
                    var canSearch = CanSearch();
                    EditorGUI.BeginDisabledGroup(!canSearch);
                    if (displaySearchDropdown)
                    {
                        var dropdownRect = new Rect(searchRect.x, searchRect.y - 1, 60, searchRect.height);
                        m_SearchColumn = EditorGUI.IntPopup(dropdownRect, m_SearchColumn, searchDropdownNames, searchDropdownValues);
                        dirty |= oldSearchColumn != m_SearchColumn;
                        searchRect = new Rect(searchRect.x + 62, searchRect.y, searchRect.width - 62, searchRect.height);
                    }
                    var rightRect = new Rect(searchRect.x + searchRect.width - rightWidth + 2, searchRect.y, rightWidth, searchRect.height);
                    if (displayIgnoreCase)
                    {
                        var toggleRect = new Rect(rightRect.x + 2, rightRect.y - 2, 17, rightRect.height);
                        m_IgnoreCase = !EditorGUI.Toggle(toggleRect, !m_IgnoreCase);
                        var labelRect = new Rect(toggleRect.x + 17, toggleRect.y + 1, 17, toggleRect.height);
                        EditorGUI.LabelField(labelRect, new GUIContent("Aa", EDL.Tr("区分大小写")));
                        dirty |= oldIgnoreCase != m_IgnoreCase;
                        rightRect = new Rect(rightRect.x + 36, rightRect.y, rightRect.width - 36, rightRect.height);
                    }
                    if (displayWholeMatching)
                    {
                        var toggleRect = new Rect(rightRect.x + 2, rightRect.y - 2, 17, rightRect.height);
                        m_WholeMatching = EditorGUI.Toggle(toggleRect, m_WholeMatching);
                        var labelRect = new Rect(toggleRect.x + 17, toggleRect.y + 1, 23, toggleRect.height);
                        EditorGUI.LabelField(labelRect, new GUIContent("|Aa|", EDL.Tr("全字匹配")));
                        dirty |= oldWholeMatching != m_WholeMatching;
                    }
                    if (rightWidth > 0) searchRect = new Rect(searchRect.x, searchRect.y, searchRect.width - rightWidth, searchRect.height);
                    var oldString = searchString;
                    if (canSearch) searchString = searchField.OnGUI(searchRect, searchString);
                    else searchField.OnGUI(searchRect, EDL.Tr("没有可用的检索方法"));
                    dirty |= oldString != searchString;
                    EditorGUI.EndDisabledGroup();
                    rect = new Rect(rect.x, rect.y + 20, rect.width, rect.height - 20);
                }
                if (displayFooter)
                {
                    var footerRect = new Rect(rect.x, rect.y + rect.height - 20, rect.width, 20);
                    if (drawFooterCallback == null)
                    {
                        var rowsWdith = displayRowButtons ? 186 : 0;
                        var locateWdith = displayLocateButton ? 92 : 0;
                        var replaceWidth = displayReplaceButtons ? 276 : 0;
                        var colWidth = displayColumnButtons ? 246 : 0;
                        var row = GetRows().Count;
                        var left = footerRect.width;
                        var lineCount = 0;
                        if (displayRowButtons) left -= rowsWdith;
                        if (displayLocateButton) left -= locateWdith;
                        if (displayReplaceButtons)
                        {
                            if (left < replaceWidth + colWidth && left < replaceWidth)
                            {
                                lineCount++;
                                left = footerRect.width;
                            }
                            left -= replaceWidth;
                        }
                        if (displayColumnButtons && left < colWidth) lineCount++;
                        footerRect.Set(footerRect.x, footerRect.y - lineCount * 22f, footerRect.width, 20 + lineCount * 22f);

                        lineCount = 0;
                        left = footerRect.width;

                        if (displayRowButtons)
                        {
                            var tempRect = new Rect(footerRect.x, footerRect.y, 60, 20);
                            var selection = new List<int>();
                            foreach (var s in GetSelection())
                            {
                                if (visableRowIndices.Contains(s))
                                    selection.Add(s);
                            }
                            EditorGUI.BeginDisabledGroup(!inOrder || hasSearch || selection.Count != 1 || row < 1);
                            if (GUI.Button(tempRect, EDL.Tr("插入")))
                            {
                                insertClicked?.Invoke(selection[0]);
                                GUI.FocusControl(null);
                                SetDirty();
                            }
                            EditorGUI.EndDisabledGroup();
                            tempRect = new Rect(footerRect.x + 62, footerRect.y, 60, 20);
                            EditorGUI.BeginDisabledGroup(hasSearch || selection.Count < 1);
                            if (GUI.Button(tempRect, EDL.Tr("删除")))
                            {
                                selection.Sort();
                                deleteClicked?.Invoke(selection);
                                GUI.FocusControl(null);
                                SetDirty();
                            }
                            EditorGUI.EndDisabledGroup();
                            EditorGUI.BeginDisabledGroup(!inOrder || hasSearch);
                            tempRect = new Rect(footerRect.x + 124, footerRect.y, 60, 20);
                            if (GUI.Button(tempRect, EDL.Tr("增加")))
                            {
                                appendClicked?.Invoke();
                                GUI.FocusControl(null);
                                SetDirty();
                            }
                            EditorGUI.EndDisabledGroup();
                            left -= rowsWdith;
                        }
                        if (displayLocateButton)
                        {
                            var tempRect = new Rect(footerRect.x + rowsWdith, footerRect.y, 90, 20);
                            EditorGUI.BeginDisabledGroup(hasSearch || errorRow < 0);
                            if (GUI.Button(tempRect, new GUIContent(EDL.Tr("定位错误行"), EDL.Tr("点击定位到错误行"))))
                            {
                                SetSelected(errorRow);
                                GUI.FocusControl(null);
                            }
                            EditorGUI.EndDisabledGroup();
                            left -= locateWdith;
                        }
                        if (displayReplaceButtons)
                        {
                            if (left < replaceWidth + colWidth && left < replaceWidth)
                            {
                                lineCount++;
                                left = footerRect.width;
                            }
                            var tempRect = new Rect(footerRect.x + footerRect.width - left, footerRect.y + lineCount * 22f, 100, 20);
                            replaceString = EditorGUI.TextField(tempRect, replaceString);
                            EditorGUI.BeginDisabledGroup(row < 1 || !hasSearch || !CanSearch());
                            tempRect = new Rect(tempRect.x + 102, tempRect.y, 90, 20);
                            var visableData = GetRows().Select(x => (x as TableRow<T>).data).ToArray();
                            if (GUI.Button(tempRect, EDL.Tr("替换下一个")))
                            {
                                var replaceRow = replaceClicked?.Invoke(replaceString, visableData, false) ?? -1;
                                if (replaceRow >= 0 && GetRows().Any(x => x.id == replaceRow))
                                {
                                    GUI.FocusControl(null);
                                    SetSelected(replaceRow);
                                }
                            }
                            tempRect = new Rect(tempRect.x + 92, tempRect.y, 80, 20);
                            if (GUI.Button(tempRect, EDL.Tr("替换全部")))
                            {
                                replaceClicked?.Invoke(replaceString, visableData, true);
                                GUI.FocusControl(null);
                            }
                            EditorGUI.EndDisabledGroup();
                            left -= replaceWidth;
                        }
                        if (displayColumnButtons)
                        {
                            if (left < colWidth)
                            {
                                lineCount++;
                                left = footerRect.width;
                            }
                            var column = multiColumnHeader.state.columns.Length - 1;
                            EditorGUI.BeginDisabledGroup(column < 1);
                            var tempRect = new Rect(footerRect.x + (left < colWidth ? 0 : footerRect.width - (colWidth - 2)), footerRect.y + lineCount * 22f, 80, 20);
                            var oldLabelWidth = EditorGUIUtility.labelWidth;
                            var label = EDL.Tr("列号");
                            EditorGUIUtility.labelWidth = Mathf.Max(40, EditorStyles.label.CalcSize(new GUIContent(label)).x);
                            columnToDelete = EditorGUI.IntField(tempRect, new GUIContent(label, EDL.Tr("从0开始，不包含序号列")), columnToDelete);
                            EditorGUIUtility.labelWidth = oldLabelWidth;
                            columnToDelete = Mathf.Clamp(columnToDelete, minColumnCanDelete > 0 ? minColumnCanDelete : 0, column - 1 > 0 ? column - 1 : 0);
                            tempRect = new Rect(tempRect.x + 82, tempRect.y, 80, 20);
                            if (GUI.Button(tempRect, EDL.Tr("删除列")))
                            {
                                deleteColumnClicked?.Invoke(columnToDelete);
                                GUI.FocusControl(null);
                                SetDirty();
                            }
                            EditorGUI.EndDisabledGroup();
                            tempRect = new Rect(tempRect.x + 82, tempRect.y, 80, 20);
                            if (GUI.Button(tempRect, EDL.Tr("增加列")))
                            {
                                addColumnClicked?.Invoke();
                                GUI.FocusControl(null);
                                SetDirty();
                            }
                        }
                    }
                    else drawFooterCallback(ref footerRect);
                    rect = new Rect(rect.x, rect.y, rect.width, rect.height - footerRect.height - 2);
                }

                if (dirty) Reload();

                base.OnGUI(rect);

                state.searchColumn = m_SearchColumn;
                state.ignoreCase = m_IgnoreCase;
                state.wholeMatching = m_WholeMatching;
                oldSearchColumn = m_SearchColumn;
                oldIgnoreCase = m_IgnoreCase;
                oldWholeMatching = m_WholeMatching;
                errorRow = -1;
            }
            else Reload();
            dirty = false;
        }
        protected override void RowGUI(RowGUIArgs args)
        {
            if (dirty) return;
            var item = args.item as TableRow<T>;
            EditorGUI.LabelField(args.GetCellRect(0), item.id.ToString());
            for (int i = 1; i < args.GetNumVisibleColumns(); i++)
            {
                var rect = args.GetCellRect(i);
                CenterRectUsingSingleLineHeight(ref rect);
                if (drawCellCallback != null) drawCellCallback(rect, item.data, i - 1, item.id, args.focused, args.selected);
                else EditorGUI.LabelField(rect, GetColumnData(item.data, i - 1)?.ToString() ?? EDL.Tr("(空或不可读)"));
            }
            rowGUICallback?.Invoke(item.data, item.id, args.focused, args.selected);
        }

        public void SetDirty()
        {
            dirty = true;
        }
        private void CheckErrors()
        {
            var columns = multiColumnHeader.state.columns;
            for (int i = 1; i < columns.Length; i++)
            {
                GUIContent headerContent = columns[i].headerContent;
                if (checkErrorsCallback != null && checkErrorsCallback(i - 1, out var errorString) is int errorRow && errorRow >= 0)
                {
                    headerContent.text = $"{searchDropdownNames[i]}({EDL.Tr("存在错误")})";
                    headerContent.tooltip = $"{EDL.Tr("序号为 {0} 的行存在错误", errorRow)}: {errorString}";
                    headerContent.image = errorTexture;
                    if (this.errorRow < 0) this.errorRow = errorRow;
                }
                else
                {
                    headerContent.text = searchDropdownNames[i];
                    headerContent.tooltip = null;
                    headerContent.image = null;
                }
            }
        }
        protected override bool CanMultiSelect(TreeViewItem item) => multiSelect;
        protected override float GetCustomRowHeight(int row, TreeViewItem item)
        {
            var tr = item as TableRow<T>;
            return rowHeightCallback?.Invoke(tr.id, tr.data) ?? base.GetCustomRowHeight(row, item);
        }
        #endregion

        #region 选中
        public void SetSelected(int id, bool frame = true) =>
            SetSelection(new int[] { id }, frame ? TreeViewSelectionOptions.RevealAndFrame : TreeViewSelectionOptions.None);
        public int GetSelected()
        {
            var selection = GetSelection();
            if (selection.Count > 0) return selection[^1];
            else return -1;
        }
        protected override void SelectionChanged(IList<int> selectedIds)
        {
            selectionChanged?.Invoke(selectedIds.Select(x => (FindItem(x, rootItem) as TableRow<T>).id).ToArray());
        }
        #endregion

        #region 搜索
        private bool Search(T rowData)
        {
            if (!initialized) return true;
            else if (searchCallback != null)
                return searchCallback(searchString, rowData, displaySearchDropdown ? m_SearchColumn : -1);
            else
            {
                var columnData = GetColumnData(rowData, displaySearchDropdown && m_SearchColumn > 0 ? m_SearchColumn : 0);
                if (columnData != null)
                {
                    if (!m_WholeMatching)
                        return columnData.ToString().IndexOf(searchString, m_IgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.CurrentCulture) >= 0;
                    else
                        return columnData.ToString().Equals(searchString, m_IgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.CurrentCulture);
                }
                else return false;
            }
        }
        private bool CanSearch() => searchCallback != null || indexer != null;
        #endregion

        #region 拖拽
        protected override bool CanStartDrag(CanStartDragArgs args) => draggable;
        protected override void SetupDragAndDrop(SetupDragAndDropArgs args)
        {
            if (hasSearch) return;
            if (!inOrder)
            {
                Debug.LogWarning(EDL.Tr("只有按序号的升序排序时才能拖拽"));
                return;
            }
            DragAndDrop.PrepareStartDrag();
            var draggedRows = GetRows().Where(item => args.draggedItemIDs.Contains(item.id)).ToList();
            DragAndDrop.SetGenericData(genericDragID, draggedRows);
            DragAndDrop.objectReferences = new UnityEngine.Object[] { };
            string title = draggedRows.Count == 1 ? draggedRows[0].displayName : "< Multiple >";
            DragAndDrop.StartDrag(title);
        }
        protected override DragAndDropVisualMode HandleDragAndDrop(DragAndDropArgs args)
        {
            if (DragAndDrop.GetGenericData(genericDragID) is not List<TreeViewItem> draggedRows)
                return DragAndDropVisualMode.None;

            switch (args.dragAndDropPosition)
            {
                case DragAndDropPosition.BetweenItems:
                    var rows = GetRows();
                    bool valid = args.insertAtIndex >= 0 && args.insertAtIndex < rows.Count && validDrag(args.parentItem, draggedRows);
                    if (args.performDrop && valid) insert(rows[args.insertAtIndex].id);
                    return valid ? DragAndDropVisualMode.Move : DragAndDropVisualMode.None;
                case DragAndDropPosition.OutsideItems:
                    rows = GetRows();
                    if (args.performDrop) insert(rows[^1].id + 1);
                    return DragAndDropVisualMode.Move;

                    void insert(int insertAtIndex)
                    {
                        var indices = draggedRows.Select(r => (r as TableRow<T>).id).ToArray();
                        if (dropRowsCallback != null)
                        {
                            if (dropRowsCallback(indices, insertAtIndex, out var newIndices) && newIndices.Length > 0)
                            {
                                Reload();
                                SetSelection(newIndices);
                            }
                        }
                        else if (m_Data is not null)
                        {
                            if (Utility.MoveElements(m_Data, indices, insertAtIndex, out var newIndices) && newIndices.Length > 0)
                            {
                                dataModified?.Invoke(m_Data);
                                Reload();
                                SetSelection(newIndices);
                            }
                        }
                    }
                case DragAndDropPosition.UponItem:
                default:
                    return DragAndDropVisualMode.None;
            }

            static bool validDrag(TreeViewItem parent, List<TreeViewItem> draggedItems)
            {
                TreeViewItem currentParent = parent;
                while (currentParent != null)
                {
                    if (draggedItems.Contains(currentParent))
                        return false;
                    currentParent = currentParent.parent;
                }
                return true;
            }
        }
        #endregion

        #region 静态方法
        private static MultiColumnHeaderState GenerateHeaderState(TableColumn[] columns, SortingCallback sortingCallback)
        {
            var _columns = new TableColumn[columns.Length];
            columns.CopyTo(_columns, 0);
            if (sortingCallback == null)
                foreach (var col in _columns)
                {
                    col.canSort = false;
                }
            PrependIndexColumn(ref _columns);
            return new MultiColumnHeaderState(_columns);
        }
        private static MultiColumnHeaderState GenerateHeaderState(int column, int primary, IList<T> data, SortingCallback sortingCallback)
        {
            if (data.Count > 0)
            {
                var row = data[0];
                string[] columns = new string[column];
                for (int i = 0; i < column; i++)
                {
                    columns[i] = GetColumnData(row, i).ToString();
                }
                return GenerateHeaderState(columns, null, primary, sortingCallback);
            }
            throw new NotSupportedException(nameof(data) + ": " + EDL.Tr("没有可用的表头"));
        }
        private static MultiColumnHeaderState GenerateHeaderState(string[] columns, Func<int, string> columnTooltipCallback, int primary, SortingCallback sortingCallback)
        {
            var _columns = new TableColumn[columns.Length];
            for (int i = 0; i < columns.Length; i++)
            {
                _columns[i] = new TableColumn()
                {
                    headerContent = new GUIContent(columns[i], columnTooltipCallback?.Invoke(i)),
                    headerTextAlignment = i == primary ? TextAlignment.Left : TextAlignment.Center,
                    sortedAscending = true,
                    sortingArrowAlignment = i == primary ? TextAlignment.Center : TextAlignment.Left,
                    width = 100,
                    minWidth = 60,
                    autoResize = true,
                    allowToggleVisibility = false,
                    canSort = sortingCallback != null
                };
            }
            PrependIndexColumn(ref _columns);
            return new MultiColumnHeaderState(_columns);
        }
        private static void PrependIndexColumn(ref TableColumn[] columns)
        {
            ArrayUtility.Insert(ref columns, 0, new TableColumn()
            {
                headerContent = new GUIContent(EDL.Tr("序号"), EDL.Tr("表格行结构的原始序号")),
                headerTextAlignment = TextAlignment.Center,
                sortedAscending = true,
                sortingArrowAlignment = TextAlignment.Left,
                width = 40,
                minWidth = 40,
                autoResize = false,
                allowToggleVisibility = false
            });
        }
        private static object GetColumnData(T row, int column) => indexer?.GetValue(row, new object[] { column });
        #endregion
    }

    [Serializable]
    public sealed class TableColumn : MultiColumnHeaderState.Column { }

    public sealed class TableRow<T> : TreeViewItem
    {
        public readonly T data;

        public TableRow(int index, T data) : base(index)
        {
            this.data = data;
        }
    }

    [Serializable]
    public sealed class TableViewState : TreeViewState
    {
        public int searchColumn;
        public bool ignoreCase;
        public bool wholeMatching;
    }
}