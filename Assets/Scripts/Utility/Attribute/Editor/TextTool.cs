using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UI;
using ZetanExtends;
using Debug = UnityEngine.Debug;

public class TextTool : EditorWindow
{
    [MenuItem("Zetan Studio/Text工具")]
    private static void CreateWindow()
    {
        TextTool window = GetWindowWithRect<TextTool>(new Rect(0, 0, 450, 720), false, "Text工具");
        window.Show();
    }

    private ConvertType type = ConvertType.Legacy;
    private bool onlyOpenScene = false;

    private string path = "Assets/Resources/Prefabs";
    private List<Text> texts;
    private List<Text> textsPaged;
    private List<TextMeshProUGUI> textMeshs;
    private List<TextMeshProUGUI> textMeshsPaged;
    private int funcIndex;
    private ReorderableList textList;
    private Font font;
    private TMP_FontAsset fontAsset;
    private Vector2 scrollPos;
    //private GameObject temp;
    protected int pageEach = 20;
    protected int page = 1;
    protected int maxPage = 1;

    private void OnEnable()
    {
        texts = new List<Text>();
        textsPaged = new List<Text>();
        textMeshs = new List<TextMeshProUGUI>();
        textMeshsPaged = new List<TextMeshProUGUI>();
    }

    private void OnGUI()
    {
        scrollPos = GUILayout.BeginScrollView(scrollPos);
        int funcBef = funcIndex;
        funcIndex = GUILayout.Toolbar(funcIndex, new string[] { "字体替换", "组件替换" });
        if (funcBef != funcIndex)
        {
            textList = null;
            texts.Clear();
            textsPaged.Clear();
            textMeshs.Clear();
            textMeshsPaged.Clear();
            page = 1;
            maxPage = 1;
        }
        onlyOpenScene = EditorGUILayout.Toggle("仅打开的场景", onlyOpenScene);
        if (!onlyOpenScene) path = EditorGUILayout.TextField("预制件路径", path);
        switch (funcIndex)
        {
            case 0:
                type = (ConvertType)EditorGUILayout.EnumPopup("替换对象", type);
                switch (type)
                {
                    case ConvertType.Legacy:
                        textList = new ReorderableList(textsPaged, typeof(Text), true, true, false, false)
                        {
                            drawElementCallback = (rect, index, isActive, isFocused) =>
                            {
                                EditorGUI.ObjectField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), textsPaged[index], typeof(Text), false);
                                string path = textsPaged[index].gameObject.GetPath();
                                EditorGUI.LabelField(new Rect(rect.x, rect.y + EditorGUIUtility.singleLineHeight, rect.width, EditorGUIUtility.singleLineHeight), new GUIContent(path, path));
                            },
                            elementHeightCallback = (index) =>
                            {
                                return EditorGUIUtility.singleLineHeight * 2;
                            },
                            drawNoneElementCallback = (rect) =>
                            {
                                EditorGUI.LabelField(rect, "暂无");
                            },
                            drawHeaderCallback = (rect) =>
                            {
                                EditorGUI.LabelField(rect, "收集到的Text");
                                GUIStyle style = new GUIStyle() { alignment = TextAnchor.MiddleRight };
                                style.normal.textColor = GUI.contentColor;
                                EditorGUI.LabelField(new Rect(rect.x + rect.width - 150, rect.y, 30, rect.height), $"{page}/{maxPage}", style);
                                if (GUI.Button(new Rect(rect.x + rect.width - 120, rect.y, 60, rect.height), "上一页"))
                                    if (page > 1)
                                    {
                                        page--;
                                        Refresh();
                                    }
                                if (GUI.Button(new Rect(rect.x + rect.width - 60, rect.y, 60, rect.height), "下一页"))
                                    if (page * pageEach <= texts.Count)
                                    {
                                        page++;
                                        Refresh();
                                    }
                            },
                        };
                        font = EditorGUILayout.ObjectField("字体", font, typeof(Font), false) as Font;
                        maxPage = Mathf.CeilToInt(texts.Count * 1.0f / pageEach);
                        break;
                    case ConvertType.TMP:

                        textList = new ReorderableList(textMeshsPaged, typeof(TextMeshProUGUI), true, true, false, false)
                        {
                            drawElementCallback = (rect, index, isActive, isFocused) =>
                            {
                                EditorGUI.ObjectField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), textMeshsPaged[index], typeof(TextMeshProUGUI), false);
                                string path = textMeshsPaged[index].gameObject.GetPath();
                                EditorGUI.LabelField(new Rect(rect.x, rect.y + EditorGUIUtility.singleLineHeight, rect.width, EditorGUIUtility.singleLineHeight), new GUIContent(path, path));
                            },
                            elementHeightCallback = (index) =>
                            {
                                return EditorGUIUtility.singleLineHeight * 2;
                            },
                            drawNoneElementCallback = (rect) =>
                            {
                                EditorGUI.LabelField(rect, "暂无");
                            },
                            drawHeaderCallback = (rect) =>
                            {
                                EditorGUI.LabelField(rect, "收集到的TMP");
                                GUIStyle style = new GUIStyle() { alignment = TextAnchor.MiddleRight };
                                style.normal.textColor = GUI.contentColor;
                                EditorGUI.LabelField(new Rect(rect.x + rect.width - 150, rect.y, 30, rect.height), $"{page}/{maxPage}", style);
                                if (GUI.Button(new Rect(rect.x + rect.width - 120, rect.y, 60, rect.height), "上一页"))
                                    if (page > 1)
                                    {
                                        page--;
                                        Refresh();
                                    }
                                if (GUI.Button(new Rect(rect.x + rect.width - 60, rect.y, 60, rect.height), "下一页"))
                                    if (page * pageEach <= textMeshs.Count)
                                    {
                                        page++;
                                        Refresh();
                                    }
                            },
                        };
                        fontAsset = EditorGUILayout.ObjectField("字体资源", fontAsset, typeof(TMP_FontAsset), false) as TMP_FontAsset;
                        maxPage = Mathf.CeilToInt(textMeshs.Count * 1.0f / pageEach);
                        break;
                    default:
                        break;
                }
                if (GUILayout.Button("一键收集"))
                {
                    Stopwatch sw = new Stopwatch();
                    sw.Start();
                    switch (type)
                    {
                        case ConvertType.Legacy:
                            texts.Clear();
                            if (onlyOpenScene && ZetanUtility.ActiveScene == null)
                                Debug.LogWarning("没有打开的场景");
                            else if (ZetanUtility.ActiveScene != null)
                            {
                                foreach (var root in ZetanUtility.ActiveScene.GetRootGameObjects())
                                {
                                    foreach (var text in root.GetComponentsInChildren<Text>())
                                    {
                                        texts.Add(text);
                                    }
                                }
                            }
                            if (!onlyOpenScene)
                            {
                                foreach (var go in ZetanUtility.Editor.LoadAssets<GameObject>(path))
                                {
                                    foreach (var text in go.GetComponentsInChildren<Text>())
                                    {
                                        texts.Add(text);
                                    }
                                }
                            }
                            break;
                        case ConvertType.TMP:
                            textMeshs.Clear();
                            if (onlyOpenScene && ZetanUtility.ActiveScene == null)
                                Debug.LogWarning("没有打开的场景");
                            else if (ZetanUtility.ActiveScene != null)
                            {
                                foreach (var root in ZetanUtility.ActiveScene.GetRootGameObjects())
                                {
                                    foreach (var text in root.GetComponentsInChildren<TextMeshProUGUI>())
                                    {
                                        textMeshs.Add(text);
                                    }
                                }
                            }
                            if (!onlyOpenScene)
                            {
                                foreach (var go in ZetanUtility.Editor.LoadAssets<GameObject>(path))
                                {
                                    foreach (var text in go.GetComponentsInChildren<TextMeshProUGUI>())
                                    {
                                        textMeshs.Add(text);
                                    }
                                }
                            }
                            break;
                        default:
                            break;
                    }
                    Refresh();
                    Debug.Log($"收集完成，耗时 {sw.ElapsedMilliseconds} ms");
                }
                if (GUILayout.Button("一键替换")) Replace();
                break;
            case 1:
                type = (ConvertType)EditorGUILayout.EnumPopup("替换对象", type);
                EditorGUILayout.HelpBox("敬请期待", MessageType.Info);
                break;
            default:
                break;
        }
        textList?.DoLayoutList();
        GUILayout.EndScrollView();
    }

    //private void OnDestroy()
    //{
    //    if (temp) DestroyImmediate(temp);
    //}

    private void Refresh()
    {
        switch (type)
        {
            case ConvertType.Legacy:
                maxPage = Mathf.CeilToInt(texts.Count * 1.0f / pageEach);
                while ((page - 1) * pageEach > texts.Count && page > 1)
                {
                    page--;
                }
                textsPaged.Clear();
                for (int i = (page - 1) * pageEach; i < page * pageEach && i < texts.Count; i++)
                {
                    textsPaged.Add(texts[i]);
                }
                break;
            case ConvertType.TMP:
                maxPage = Mathf.CeilToInt(textMeshs.Count * 1.0f / pageEach);
                while ((page - 1) * pageEach > textMeshs.Count && page > 1)
                {
                    page--;
                }
                textMeshsPaged.Clear();
                for (int i = (page - 1) * pageEach; i < page * pageEach && i < textMeshs.Count; i++)
                {
                    textMeshsPaged.Add(textMeshs[i]);
                }
                break;
            default:
                break;
        }

    }

    private void Replace()
    {
        switch (funcIndex)
        {
            case 0:
                switch (type)
                {
                    case ConvertType.Legacy:
                        if (font)
                        {
                            Stopwatch sw = new Stopwatch();
                            sw.Start();
                            int count = 0;
                            foreach (var text in texts)
                            {
                                if (text.font != font)
                                {
                                    text.font = font;
                                    count++;
                                }
                            }
                            sw.Stop();
                            Debug.Log($"替换完成，共替换 {count} 个，耗时 {sw.ElapsedMilliseconds} ms");
                        }
                        else EditorUtility.DisplayDialog("错误", "尚未指定字体文件", "确定");
                        break;
                    case ConvertType.TMP:
                        if (fontAsset)
                        {
                            Stopwatch sw = new Stopwatch();
                            sw.Start();
                            int count = 0;
                            foreach (var text in textMeshs)
                            {
                                if (text.font != fontAsset)
                                {
                                    text.font = fontAsset;
                                    text.UpdateFontAsset();
                                    count++;
                                }
                            }
                            sw.Stop();
                            Debug.Log($"替换完成，共替换 {count} 个，耗时 {sw.ElapsedMilliseconds} ms");
                        }
                        else EditorUtility.DisplayDialog("错误", "尚未指定字体资源文件", "确定");
                        break;
                    default:
                        break;
                }
                break;
            case 1:
                break;
            default:
                break;
        }
    }

    //private void Replace(Text text)
    //{
    //    if (!temp) temp = new GameObject("temp");
    //    var newText = temp.AddComponent<TextMeshProUGUI>();
    //    newText.text = text.text;
    //    newText.fontSize = text.fontSize;
    //    newText.enableAutoSizing = text.resizeTextForBestFit;
    //    newText.fontSizeMin = text.resizeTextMinSize;
    //    newText.fontSizeMax = text.resizeTextMaxSize;
    //    switch (text.alignment)
    //    {
    //        case TextAnchor.UpperLeft:
    //            newText.alignment = TextAlignmentOptions.TopLeft;
    //            break;
    //        case TextAnchor.UpperCenter:
    //            newText.alignment = TextAlignmentOptions.Top;
    //            break;
    //        case TextAnchor.UpperRight:
    //            newText.alignment = TextAlignmentOptions.TopRight;
    //            break;
    //        case TextAnchor.MiddleLeft:
    //            newText.alignment = TextAlignmentOptions.MidlineLeft;
    //            break;
    //        case TextAnchor.MiddleCenter:
    //            newText.alignment = TextAlignmentOptions.Midline;
    //            break;
    //        case TextAnchor.MiddleRight:
    //            newText.alignment = TextAlignmentOptions.MidlineRight;
    //            break;
    //        case TextAnchor.LowerLeft:
    //            newText.alignment = TextAlignmentOptions.BottomLeft;
    //            break;
    //        case TextAnchor.LowerCenter:
    //            newText.alignment = TextAlignmentOptions.Bottom;
    //            break;
    //        case TextAnchor.LowerRight:
    //            newText.alignment = TextAlignmentOptions.BottomRight;
    //            break;
    //        default:
    //            break;
    //    }
    //    var go = text.gameObject;
    //    DestroyImmediate(text);
    //    EditorUtility.CopySerialized(temp, go.AddComponent<TextMeshPro>());
    //    DestroyImmediate(newText);
    //}
    //private void Replace(TextMeshPro text)
    //{

    //}

    private enum ConvertType
    {
        [InspectorName("Text")]
        Legacy,
        [InspectorName("TMP")]
        TMP
    }
}