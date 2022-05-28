using UnityEditor;
using UnityEngine;

namespace ZetanStudio
{
    [CustomPropertyDrawer(typeof(FolderAttribute))]
    public class FolderDrawer : EnhancedAttributeDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType == SerializedPropertyType.String)
            {
                float buttonWidth = GUI.skin.button.CalcSize(new GUIContent(L10n.Tr("Select"))).x;
                EditorGUI.PropertyField(new Rect(position.x, position.y, position.width - buttonWidth - 2, position.height), property, label);
                Rect buttonRect = new Rect(position.x + position.width - buttonWidth, position.y, buttonWidth, position.height);
                if (GUI.Button(buttonRect, L10n.Tr("Select")))
                {
                    string path;
                    var attr = attribute as FolderAttribute;
                    if (attr.external) path = System.IO.File.Exists(property.stringValue) ? property.stringValue : "Assets";
                    else path = property.stringValue.StartsWith("Assets/") && AssetDatabase.IsValidFolder(property.stringValue) ? property.stringValue : "Assets";
                    while (true)
                    {
                        path = EditorUtility.SaveFolderPanel(label.text, path, null);
                        if (!attr.external)
                        {
                            if (!string.IsNullOrEmpty(path) && !ZetanUtility.Editor.IsValidFolder(path))
                                if (!EditorUtility.DisplayDialog("路径错误", $"请选择Assets范围内的路径", "确定", "取消"))
                                {
                                    GUIUtility.ExitGUI();
                                    return;
                                }
                                else continue;
                            path = ZetanUtility.Editor.ConvertToAssetsPath(path);
                            if (!string.IsNullOrEmpty(attr.root) && !string.IsNullOrEmpty(path) && !path.StartsWith($"Assets/{attr.root}"))
                                if (!EditorUtility.DisplayDialog("路径错误", $"请选择Assets/{attr.root}范围内的路径", "确定", "取消"))
                                {
                                    GUIUtility.ExitGUI();
                                    return;
                                }
                                else continue;
                        }
                        if (!string.IsNullOrEmpty(path))
                        {
                            property.stringValue = path;
                            property.serializedObject.ApplyModifiedProperties();
                        }
                        GUIUtility.ExitGUI();
                        return;
                    }
                }
            }
            else EditorGUI.PropertyField(position, property, label, property.hasVisibleChildren);
        }
    }
}
