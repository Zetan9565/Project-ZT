using System;
using System.Reflection;
using UnityEditor;
using UnityEngine.UIElements;
using ZetanStudio.Item.Module;

namespace ZetanStudio.Item.Editor
{
    public class ModuleBlock : ItemInspectorBlock
    {
        protected readonly SerializedObject serializedObject;
        protected readonly SerializedProperty property;
        protected bool shouldCheckError;

        public ModuleBlock(SerializedProperty property, ItemModule module)
        {
            serializedObject = property.serializedObject;
            shouldCheckError = property.serializedObject.targetObject is ItemNew;
            this.property = property;
            userData = module;
            value = property.isExpanded;
            this.Q<Toggle>().RegisterValueChangedCallback(new EventCallback<ChangeEvent<bool>>(evt => property.isExpanded = evt.newValue));
            if (property.hasVisibleChildren)
            {
                IMGUIContainer inspector = new IMGUIContainer(() =>
                {
                    if (serializedObject.targetObject)
                    {
                        EditorGUI.BeginChangeCheck();
                        serializedObject.UpdateIfRequiredOrScript();
                        OnInspectorGUI();
                        if (EditorGUI.EndChangeCheck())
                        {
                            serializedObject.ApplyModifiedProperties();
                            CheckError();
                        }
                    }
                });
                inspector.style.flexGrow = 1;
                Add(inspector);
            }
            else Add(new Label("(无可用参数)"));
            CheckError();
        }
        public void AddManipulator(IManipulator manipulator)
        {
            this.Q<Toggle>().AddManipulator(manipulator);
        }
        protected virtual void OnInspectorGUI()
        {
            using var copy = property.Copy();
            SerializedProperty end = copy.GetEndProperty();
            bool enter = true;
            while (copy.NextVisible(enter) && !SerializedProperty.EqualContents(copy, end))
            {
                EditorGUILayout.PropertyField(copy, true);
                enter = false;
            }
        }
        protected virtual void CheckError()
        {
            if (userData is ItemModule module)
            {
                text = module.GetName();
                if (shouldCheckError && !module.IsValid) text += "(存在错误)";
            }
        }

        public static ModuleBlock Create(SerializedProperty property, ItemModule module)
        {
            foreach (var type in TypeCache.GetTypesWithAttribute<CustomMuduleDrawerAttribute>())
            {
                if (typeof(ModuleBlock).IsAssignableFrom(type))
                {
                    var attr = type.GetCustomAttribute<CustomMuduleDrawerAttribute>();
                    if (attr.useForChildren ? attr.type.IsAssignableFrom(module.GetType()) : (attr.type == module.GetType()))
                        return Activator.CreateInstance(type, property, module) as ModuleBlock;
                }
            }
            return new ModuleBlock(property, module);
        }

        [AttributeUsage(AttributeTargets.Class)]
        protected class CustomMuduleDrawerAttribute : Attribute
        {
            public readonly Type type;
            public readonly bool useForChildren;

            public CustomMuduleDrawerAttribute(Type type, bool useForChildren = false)
            {
                this.type = type;
                this.useForChildren = useForChildren;
            }
        }
    }
}