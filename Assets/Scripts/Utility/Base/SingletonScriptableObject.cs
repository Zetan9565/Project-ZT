using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ZetanStudio
{
    public abstract class SingletonScriptableObject : ScriptableObject
    {
#if UNITY_EDITOR
        [InitializeOnLoadMethod]
        private static void FindEditorInstance()
        {
            foreach (var type in TypeCache.GetTypesDerivedFrom<SingletonScriptableObject>())
            {
                if (type.BaseType.IsGenericType
                    && typeof(SingletonScriptableObject<>).IsAssignableFrom(type.BaseType.GetGenericTypeDefinition())
                    && !type.IsAbstract
                    && !type.IsGenericType
                    && !type.IsGenericTypeDefinition)
                    type.BaseType.GetField("instance", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic).SetValue(null, instance(type));
            }

            static SingletonScriptableObject instance(System.Type type)
            {
                SingletonScriptableObject instance = Utility.LoadResource(type) as SingletonScriptableObject;
                if (!instance) instance = Utility.Editor.LoadAsset(type) as SingletonScriptableObject;
                return instance;
            }
        }
#endif
    }

    public abstract class SingletonScriptableObject<T> : SingletonScriptableObject where T : SingletonScriptableObject<T>
    {
        private static T instance;
        public static T Instance
        {
            get
            {
                if (!instance) instance = Utility.LoadResource<T>();
#if UNITY_EDITOR
                if (!instance) instance = Utility.Editor.LoadAsset<T>();
#endif
                if (Application.isPlaying && !instance) instance = CreateInstance<T>();
                return instance;
            }
        }

        public SingletonScriptableObject()
        {
            instance = this as T;
        }
        private void OnDestroy()
        {
            if (instance == this) instance = null;
            instance = Utility.LoadResource<T>();
#if UNITY_EDITOR
            if (!instance) instance = Utility.Editor.LoadAsset<T>();
#endif
        }

#if UNITY_EDITOR
        public static T GetOrCreate()
        {
            if (!instance) instance = Utility.LoadResource<T>();
            if (!instance) instance = Utility.Editor.LoadAsset<T>();
            if (!instance)
            {
                instance = CreateInstance<T>();
                AssetDatabase.CreateAsset(instance, AssetDatabase.GenerateUniqueAssetPath($"Assets/Resources/New {ObjectNames.NicifyVariableName(typeof(T).Name)}.asset"));
            }
            return instance;
        }

        protected static void CreateSingleton()
        {
            if (typeof(T).IsAbstract || typeof(T).IsGenericType || typeof(T).IsGenericTypeDefinition || typeof(T).IsGenericParameter) return;
            if (Utility.Editor.LoadAsset<T>() is T instance)
            {
                Debug.LogWarning($"创建{typeof(T).Name}失败, 因为已存在单例, 路径: {AssetDatabase.GetAssetPath(instance)}");
                EditorGUIUtility.PingObject(instance);
                return;
            }
            var path = AssetDatabase.GetAssetPath(Selection.activeInstanceID);
            if (!AssetDatabase.IsValidFolder(path)) path = System.IO.Path.GetDirectoryName(path).Replace('\\', '/');
            if (AssetDatabase.IsValidFolder(path))
            {
                instance = CreateInstance<T>();
                AssetDatabase.CreateAsset(instance, AssetDatabase.GenerateUniqueAssetPath(path + $"/New {ObjectNames.NicifyVariableName(typeof(T).Name)}.asset"));
                Selection.activeObject = instance;
            }
        }
#endif
    }
}