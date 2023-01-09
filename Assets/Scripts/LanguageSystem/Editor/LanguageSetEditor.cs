using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace ZetanStudio.LanguageSystem.Editor
{
    using Extension.Editor;
    using ZetanStudio.Editor;

    public class LanguageSetEditor : EditorWindow
    {
        [MenuItem("Window/Zetan Studio/语言包编辑器")]
        private static void CreateWindow()
        {
            var wnd = GetWindow<LanguageSetEditor>("语言包编辑器", true);
            wnd.minSize = new Vector2(960, 540);
            wnd.Show();
        }

        private static void CreateWindow(LanguageSet language)
        {
            var wnd = GetWindow<LanguageSetEditor>("语言包编辑器", true);
            wnd.minSize = new Vector2(960, 540);
            wnd.language = language;
            wnd.RefreshSerializedObject();
            wnd.Show();
        }

        [OnOpenAsset]
#pragma warning disable IDE0060
        public static bool OnOpenAsset(int instanceID, int line)
#pragma warning restore IDE0060
        {
            if (EditorUtility.InstanceIDToObject(instanceID) is LanguageSet lang)
            {
                CreateWindow(lang);
                return true;
            }
            return false;
        }

        [SerializeField] private LanguageSet language;
        private SerializedObject serializedObject;
        private SerializedProperty serializedMaps;

        [SerializeField] private TableViewState viewState;
        private TableView<int> table;
        private int row;
        private int langCount;

        private Action delayCall;

        #region Unity 回调
        private void OnSelectionChange()
        {
            if (Selection.objects.Length == 1 && Selection.activeObject is LanguageSet language)
            {
                this.language = language;
                RefreshSerializedObject();
                Repaint();
            }
        }

        private void OnEnable()
        {
            RefreshSerializedObject();
            Undo.undoRedoPerformed -= Repaint;
            Undo.undoRedoPerformed += Repaint;
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= Repaint;
        }

        private void OnGUI()
        {
            serializedObject?.UpdateIfRequiredOrScript();

            var oldLang = language;
            language = EditorGUILayout.ObjectField(EDL.Tr("语言包"), language, typeof(LanguageSet), false) as LanguageSet;
            if (oldLang != language) RefreshSerializedObject();

            if (serializedObject != null)
            {
                var maxLangCount = language.Maps.Count > 0 ? language.Maps.Max(x => x.Values.Count) : 0;
                if (langCount != maxLangCount)
                {
                    langCount = maxLangCount;
                    RefreshTable();
                }
                else if (serializedMaps != null && row != serializedMaps.arraySize) RefreshTable();

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_name"), new GUIContent(EDL.Tr("语言包名称")));
                table?.OnGUI(new Rect(5, 44, position.width - 10, position.height - 49));
                if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
            }
        }
        #endregion

        private void RefreshSerializedObject()
        {
            if (language)
            {
                serializedObject = new SerializedObject(language);
                serializedMaps = serializedObject.FindProperty("maps");
                row = serializedMaps.arraySize;
                langCount = language.Maps.Count > 0 ? language.Maps.Max(x => x.Values.Count) : 0;
            }
            else
            {
                serializedObject?.Dispose();
                serializedObject = null;
                serializedMaps = null;
            }
            RefreshTable();
        }
        private void RefreshTable()
        {
            if (serializedObject != null && serializedMaps != null)
            {
                row = serializedMaps.arraySize;

                var indices = new List<int>();
                for (int i = 0; i < serializedMaps.arraySize; i++)
                {
                    indices.Add(i);
                }
                TableColumn[] columns = new TableColumn[langCount + 1];
                columns[0] = new TableColumn()
                {
                    headerContent = new GUIContent(EDL.Tr("索引")),
                    headerTextAlignment = TextAlignment.Left,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Center,
                    width = 80,
                    minWidth = 60,
                    autoResize = true,
                    allowToggleVisibility = true,
                    canSort = true
                };
                for (int i = 1; i < columns.Length; i++)
                {
                    string lang = EDL.Tr("语言{0}", i);
                    columns[i] = new TableColumn()
                    {
                        headerContent = new GUIContent(lang),
                        headerTextAlignment = TextAlignment.Center,
                        sortedAscending = true,
                        sortingArrowAlignment = TextAlignment.Left,
                        width = 100,
                        minWidth = 60,
                        autoResize = true,
                        allowToggleVisibility = true,
                        canSort = false
                    };
                }
                viewState ??= new TableViewState();
                viewState.searchString = null;
                table = new TableView<int>(viewState, indices, columns, DrawRow)
                {
                    searchCallback = Search,
                    insertClicked = InsertRow,
                    deleteClicked = DeleteRows,
                    appendClicked = AppendRow,
                    replaceClicked = Replace,
                    deleteColumnClicked = DeleteLanguage,
                    addColumnClicked = AddLanguage,
                    checkErrorsCallback = CheckErrors,
                    dropRowsCallback = DropRows,
                    draggable = true,
                    displayFooter = true,
                    minColumnCanDelete = 1,
                };
                table.multiColumnHeader.ResizeToFit();
                delayCall?.Invoke();
                delayCall = null;
            }
            else table = null;
        }
        private SerializedProperty GetValuesProperty(int i)
        {
            return serializedMaps.GetArrayElementAtIndex(i).FindPropertyRelative("values");
        }

        private void DrawRow(Rect rect, int index, int column, int row, bool focused, bool selected)
        {
            SerializedProperty map = serializedMaps.GetArrayElementAtIndex(index);
            if (column == 0)
            {
                var key = map.FindAutoProperty("Key");
                key.stringValue = EditorGUI.TextField(rect, key.stringValue);
            }
            else
            {
                var values = map.FindPropertyRelative("values");
                if (column > values.arraySize)
                {
                    if (GUI.Button(rect, EDL.Tr("插入内容")))
                    {
                        values.arraySize++;
                    }
                }
                else
                {
                    var value = values.GetArrayElementAtIndex(column - 1);
                    value.stringValue = EditorGUI.TextField(rect, value.stringValue);
                }
            }
        }
        private int CheckErrors(int column, out string error)
        {
            error = null;
            if (column != 0) return -1;
            int keyEmptyRow = -1;
            int keyDuplicateRow = -1;
            var maps = language.Maps;
            string duplicatedKey = null;
            var exits = new HashSet<string>();
            for (int i = 0; i < maps.Count; i++)
            {
                var map = maps[i];
                if (string.IsNullOrEmpty(map.Key))
                {
                    keyEmptyRow = i;
                    break;
                }
                else if (!exits.Contains(map.Key)) exits.Add(map.Key);
                else
                {
                    duplicatedKey = map.Key;
                    keyDuplicateRow = i;
                    break;
                }
            }
            int errorRow;
            if (duplicatedKey != null)
            {
                error = EDL.Tr("存在相同的键");
                errorRow = keyDuplicateRow;
            }
            else if (keyEmptyRow > 0)
            {
                error = EDL.Tr("存在空的键");
                errorRow = keyEmptyRow;
            }
            else errorRow = -1;

            return errorRow;
        }
        private bool DropRows(int[] indices, int insert, out int[] newIndices)
        {
            bool result = serializedMaps.MoveArrayElements(indices, insert - 1, out newIndices);
            if (result) serializedObject.ApplyModifiedProperties();
            return result;
        }

        #region 表格按钮点击回调
        private void InsertRow(int index)
        {
            serializedMaps.InsertArrayElementAtIndex(index);
            delayCall += () => table.SetSelected(index + 1);
        }
        private void DeleteRows(IList<int> indices)
        {
            for (int i = 0; i < indices.Count; i++)
            {
                int delete = indices[i];
                if (delete > 0)
                {
                    serializedMaps.DeleteArrayElementAtIndex(delete);
                    for (int j = i + 1; j < indices.Count; j++)
                    {
                        if (indices[j] > delete) indices[j] = indices[j] - 1;
                    }
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
        private void AppendRow()
        {
            serializedMaps.arraySize++;
            serializedObject.ApplyModifiedProperties();
            delayCall += () => table.SetSelected(serializedMaps.arraySize - 1);
        }

        private bool Search(string keywords, int index, int column)
        {
            if (serializedMaps != null)
            {
                var row = serializedMaps.GetArrayElementAtIndex(index);
                if (column == 0) return match(row.FindAutoProperty("Key").stringValue);
                else if (column != -1)
                {
                    var values = row.FindPropertyRelative("values");
                    if (column - 1 < values.arraySize)
                    {
                        var value = values.GetArrayElementAtIndex(column - 1);
                        return match(value.stringValue);
                    }
                }
                else
                {
                    if (match(row.FindAutoProperty("Key").stringValue))
                        return true;
                    var values = row.FindPropertyRelative("values");
                    for (int i = 0; i < values.arraySize; i++)
                    {
                        var value = values.GetArrayElementAtIndex(i);
                        if (match(value.stringValue))
                            return true;
                    }
                }

                bool match(string stringValue)
                {
                    if (!table.wholeMatching)
                        return stringValue.IndexOf(keywords, table.ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.CurrentCulture) >= 0;
                    else
                        return stringValue.Equals(keywords, table.ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.CurrentCulture);
                }
            }
            return false;
        }
        private int Replace(string replaceString, int[] data, bool all)
        {
            if (!all)
            {
                for (int i = 0; i < data.Length; i++)
                {
                    var row = serializedMaps.GetArrayElementAtIndex(data[i]);
                    if (replace(row, false))
                    {
                        serializedObject.ApplyModifiedProperties();
                        return data[i];
                    }
                }
            }
            else
            {
                for (int i = 0; i < data.Length; i++)
                {
                    var row = serializedMaps.GetArrayElementAtIndex(data[i]);
                    replace(row, true);
                }
                serializedObject.ApplyModifiedProperties();
            }
            return -1;

            bool replace(SerializedProperty row, bool all)
            {
                var key = row.FindAutoProperty("Key");
                var values = row.FindPropertyRelative("values");
                if (table.searchColumn == -1)
                {
                    if (key.stringValue.IndexOf(table.searchString) >= 0)
                    {
                        key.stringValue = new Regex(table.searchString).Replace(key.stringValue, replaceString, all ? -1 : 1);
                        if (!all) return true;
                    }
                    for (int j = 0; j < values.arraySize; j++)
                    {
                        var value = values.GetArrayElementAtIndex(j);
                        if (value.stringValue.IndexOf(table.searchString) >= 0)
                        {
                            value.stringValue = new Regex(table.searchString).Replace(value.stringValue, replaceString, all ? -1 : 1);
                            if (!all) return true;
                        }
                    }
                }
                else if (table.searchColumn == 0)
                {
                    if (key.stringValue.IndexOf(table.searchString) >= 0)
                    {
                        key.stringValue = new Regex(table.searchString).Replace(key.stringValue, replaceString, all ? -1 : 1);
                        if (!all) return true;
                    }
                }
                else if (values.arraySize > table.searchColumn)
                {
                    var value = values.GetArrayElementAtIndex(table.searchColumn - 1);
                    if (value.stringValue.IndexOf(table.searchString) >= 0)
                    {
                        value.stringValue = new Regex(table.searchString).Replace(value.stringValue, replaceString, all ? -1 : 1);
                        if (!all) return true;
                    }
                }
                return false;
            }
        }

        private void DeleteLanguage(int columnToDelete)
        {
            if (EditorUtility.DisplayDialog(EDL.Tr("删除"), EDL.Tr("确定删除第{0}个语言吗", columnToDelete), EDL.Tr("确定"), EDL.Tr("取消")))
            {
                for (int i = 0; i < serializedMaps.arraySize; i++)
                {
                    var values = GetValuesProperty(i);
                    if (columnToDelete - 1 < values.arraySize)
                    {
                        values.DeleteArrayElementAtIndex(columnToDelete - 1);
                    }
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
        private void AddLanguage()
        {
            langCount++;
            for (int i = 0; i < serializedMaps.arraySize; i++)
            {
                var values = GetValuesProperty(i);
                while (values.arraySize < langCount)
                {
                    values.arraySize++;
                }
            }
            serializedObject.ApplyModifiedProperties();
            table.SetDirty();
            RefreshTable();
        }
        #endregion
    }
}