using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text.RegularExpressions;
using ZetanStudio.Math;
using ZetanStudio.Serialization;
using Newtonsoft.Json;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace ZetanStudio
{
    using System.Text;
    using ZetanStudio.ItemSystem;
    using ZetanStudio.ItemSystem.Module;

    [CustomEditor(typeof(CodeRreplacer))]
    public class CodeReplacerInspector : UnityEditor.Editor
    {
        //private string formula = "2-(1-5)";

        //private Vector2Int IntRange;
        //private AnimationCurve curve = new AnimationCurve();
        private string format;
        private string value;
        private string replace;

        private MemoryStream stream;
        [System.NonSerialized]
        private SaveData saveData;
        [System.NonSerialized]
        private GenericData items;

        [System.Serializable]
        public class Container
        {
            [JsonConverter(typeof(PloyListConverter<TestBase>))]
            public List<TestBase> tests = new List<TestBase>();

            public Dictionary<string, string> dict = new Dictionary<string, string>() { { "key1", "value1" }, { "key2", "value2" } };
        }

        [System.Serializable]
        public class TestBase
        {
            public string testname;
        }

        [System.Serializable]
        public class Test1 : TestBase
        {
            public string test1name;
        }
        [System.Serializable]
        public class Test2 : TestBase
        {
            public string test2name;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            //formula = EditorGUILayout.TextField(formula);
            //IntRange = EditorGUILayout.Vector2IntField("范围", IntRange);
            //curve = EditorGUILayout.CurveField(curve);
            value = EditorGUILayout.TextField(value);
            format = EditorGUILayout.TextField(format);
            replace = EditorGUILayout.TextField(replace);
            if (GUILayout.Button("测试"))
            {
                //var match = Regex.Match(value, @format);
                //if (match.Success) Debug.Log(match.Value);
                Debug.Log(Regex.Replace(value, format, m =>
                {
                    return m.Groups[1].Value + replace + m.Groups[3].Value;
                }));
            }
        }
    }
}
