using TMPro;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.UI;

namespace ZetanStudio
{
    public class CodeRreplacer : MonoBehaviour
    {
        public MonoScript replace;

        public TextMeshProUGUI text;

        public void Replace()
        {
            List<string> names = new List<string>();
            foreach (var field in GetType().GetFields(Utility.CommonBindingFlags))
            {
                if (field.FieldType == typeof(Text))
                    names.Add(field.Name);
            }
            string scrPath = AssetDatabase.GetAssetPath(MonoScript.FromMonoBehaviour(this));
            var lines = File.ReadAllLines(scrPath);
            for (int i = 0; i < lines.Length; i++)
            {
                foreach (var name in names)
                {
                    if (lines[i].Contains($" Text {name}"))
                        lines[i] = lines[i].Replace($" Text {name}", $" TextMeshProUGUI {name}");
                }
            }
            ArrayUtility.Insert(ref lines, 0, "using TMPro;");
            File.WriteAllLines(scrPath, lines);
            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(MonoScript.FromMonoBehaviour(text), out var guid, out long localId);
            var tguid = guid;
            var tlid = localId;
            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(replace, out guid, out localId);
            lines = File.ReadAllLines(gameObject.scene.path);
            var toID = GlobalObjectId.GetGlobalObjectIdSlow(text).targetObjectId;
            bool start = false;
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].EndsWith($"&{toID}"))
                    start = true;
                if (start == true && lines[i].Contains("m_Script: {fileID"))
                {
                    lines[i] = lines[i].Replace(tguid, guid).Replace(tlid.ToString(), localId.ToString());
                    //AssetDatabase.Refresh();
                    for (int j = i + 1; j < lines.Length; j++)
                    {
                        if (lines[j].Contains("m_Text:"))
                        {
                            lines[j] = lines[j].Replace("m_Text:", "m_text:");
                            break;
                        }
                    }
                    break;
                }
            }
            File.WriteAllLines(gameObject.scene.path, lines);
            AssetDatabase.ImportAsset(scrPath);
        }
    }
}
