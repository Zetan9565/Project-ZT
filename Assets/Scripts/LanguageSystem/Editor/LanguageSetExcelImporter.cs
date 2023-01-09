using Excel;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace ZetanStudio.LanguageSystem.Editor
{
    using ZetanStudio.Editor;

    public class LanguageSetExcelImporter : EditorWindow
    {
        [MenuItem("Window/Zetan Studio/从Excel导入语言包")]
        private static void CreateWindow()
        {
            var wnd = GetWindow<LanguageSetExcelImporter>(EDL.Tr("Excel语言包导入工具"), true);
            wnd.minSize = new Vector2(960, 540);
            wnd.Show();
        }

        [SerializeField] private string path;
        private DataSet excel;
        private string[] sheetNames;
        private int[] sheetIndices;
        private int sheetIndex;
        private TableView<DataRow> table;
        private TableViewState treeViewState;

        private bool onEnable;

        private void OnEnable()
        {
            onEnable = true;
            LoadExcel();
            onEnable = false;
        }

        private void OnGUI()
        {
            var rect = EditorGUILayout.GetControlRect();
            var tempRect = new Rect(rect.x, rect.y, 40, rect.height);
            EditorGUI.LabelField(tempRect, EDL.Tr("路径"));
            tempRect = new Rect(rect.x + 42, rect.y, rect.width - 206, rect.height);
            EditorGUI.BeginDisabledGroup(true);
            EditorGUI.TextField(tempRect, path);
            EditorGUI.EndDisabledGroup();
            tempRect = new Rect(rect.x + rect.width - 162, rect.y, 80, rect.height);
            if (GUI.Button(tempRect, new GUIContent(EDL.Tr("打开"))))
            {
                var temp = EditorUtility.OpenFilePanel(EDL.Tr("选择Excel文件"), Utility.GetFileDirectory(path), "xlsx,xls");
                if (!string.IsNullOrEmpty(temp)) LoadExcel(temp);
            }
            EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(path));
            tempRect = new Rect(rect.x + rect.width - 80, rect.y, 80, rect.height);
            if (GUI.Button(tempRect, new GUIContent(EDL.Tr("刷新")))) LoadExcel();
            EditorGUI.EndDisabledGroup();
            if (excel != null)
            {
                rect = EditorGUILayout.GetControlRect();
                tempRect = new Rect(rect.x, rect.y, 40, rect.height);
                EditorGUI.LabelField(tempRect, EDL.Tr("工作簿"));
                tempRect = new Rect(rect.x + 42, rect.y, rect.width - 206, rect.height);
                var oldIndex = sheetIndex;
                sheetIndex = EditorGUI.IntPopup(tempRect, sheetIndex, sheetNames, sheetIndices);
                if (oldIndex != sheetIndex) RefreshTableView();
                tempRect = new Rect(rect.x + rect.width - 162, rect.y, 80, rect.height);
                if (GUI.Button(tempRect, new GUIContent(EDL.Tr("导入"))))
                {
                    var sheet = excel.Tables[sheetIndex];
                    Utility.Editor.SaveFilePanel(CreateInstance<LanguageSet>, lang =>
                    {
                        import(lang, sheet);
                    }, ping: true);
                }
                tempRect = new Rect(rect.x + rect.width - 80, rect.y, 80, rect.height);
                if (GUI.Button(tempRect, new GUIContent(EDL.Tr("全部导入"))))
                {
                    Utility.Editor.SaveFolderPanel(path =>
                    {
                        var langs = new List<LanguageSet>();
                        foreach (DataTable sheet in excel.Tables)
                        {
                            var lang = CreateInstance<LanguageSet>();
                            import(lang, sheet);
                            AssetDatabase.CreateAsset(lang, AssetDatabase.GenerateUniqueAssetPath(path + "/new " +
                                Regex.Replace(typeof(LanguageSet).Name, "([a-z])([A-Z])", "$1 $2").ToLower() + ".asset"));
                            langs.Add(lang);
                        }
                        foreach (var lang in langs)
                        {
                            EditorGUIUtility.PingObject(lang);
                        }
                    });
                }
                table?.OnGUI(new Rect(5, 42, position.width - 10, position.height - 47));
            }

            static void import(LanguageSet lang, DataTable sheet)
            {
                LanguageSet.Editor.SetName(lang, sheet.TableName);
                var column = sheet.Columns.Count;
                var maps = new List<LanguageMap>();
                for (int i = 1; i < sheet.Rows.Count; i++)
                {
                    var values = new string[column - 1];
                    for (int j = 1; j < column; j++)
                    {
                        values[j - 1] = sheet.Rows[i][j].ToString();
                    }
                    maps.Add(new LanguageMap(sheet.Rows[i][0].ToString(), values));
                }
                LanguageSet.Editor.SetMaps(lang, maps.ToArray());
            }
        }

        private void LoadExcel(string path = null)
        {
            path ??= this.path;
            if (!string.IsNullOrEmpty(path))
            {
                if (path.EndsWith(".xlsx") || path.EndsWith(".xls"))
                {
                    using var stream = Utility.OpenFile(path, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                    var reader = path.EndsWith(".xlsx") ? ExcelReaderFactory.CreateOpenXmlReader(stream) : ExcelReaderFactory.CreateBinaryReader(stream);
                    excel = reader.AsDataSet();
                    sheetNames = new string[excel.Tables.Count];
                    for (int i = 0; i < excel.Tables.Count; i++)
                    {
                        sheetNames[i] = excel.Tables[i].TableName;
                    }
                    sheetIndices = new int[excel.Tables.Count];
                    for (int i = 0; i < excel.Tables.Count; i++)
                    {
                        sheetIndices[i] = i;
                    }
                    while (sheetIndex > excel.Tables.Count && sheetIndex > 1)
                    {
                        sheetIndex--;
                    }
                    this.path = path;
                }
            }
            else excel = null;
            RefreshTableView();
        }

        private void RefreshTableView()
        {
            if (excel == null)
            {
                table = null;
                return;
            }
            try
            {
                var columns = new string[excel.Tables[sheetIndex].Columns.Count];
                for (int i = 0; i < excel.Tables[sheetIndex].Columns.Count; i++)
                {
                    columns[i] = excel.Tables[sheetIndex].Rows[0][i].ToString();
                }
                List<DataRow> rows = new List<DataRow>();
                for (int i = 0; i < excel.Tables[sheetIndex].Rows.Count; i++)
                {
                    rows.Add(excel.Tables[sheetIndex].Rows[i]);
                }
                table = new TableView<DataRow>(treeViewState ??= new TableViewState(), rows, excel.Tables[sheetIndex].Columns.Count, 0, drawCell, sort);
            }
            catch// (System.Exception ex)
            {
                //Debug.LogException(ex);
                if (onEnable) Debug.LogError(EDL.Tr("读取失败，请检查所选Excel表的格式和数据！"));
                else if (EditorUtility.DisplayDialog(EDL.Tr("失败"), EDL.Tr("读取失败，请检查所选Excel表的格式和数据！"), EDL.Tr("编辑"), EDL.Tr("取消")))
                    EditorUtility.OpenWithDefaultApp(path);
            }

            static void drawCell(Rect rect, DataRow data, int column, int row, bool focused, bool selected)
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUI.TextField(rect, data[column].ToString());
                EditorGUI.EndDisabledGroup();
            }
            static int sort(int column, DataRow left, DataRow right)
            {
                return left[column].ToString().CompareTo(right[column].ToString());
            }
        }
    }
}