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
        protected bool errorBef;

        public ModuleBlock(SerializedProperty property, ItemModule module)
        {
            serializedObject = property.serializedObject;
            shouldCheckError = property.serializedObject.targetObject is Item;
            this.property = property;
            userData = module;
            value = property.isExpanded;
            this.Q<Toggle>().RegisterValueChangedCallback(evt => property.isExpanded = evt.newValue);
            if (property.hasVisibleChildren)
            {
                IMGUIContainer inspector = new IMGUIContainer(() =>
                {
                    if (serializedObject.targetObject)
                    {
                        if (errorBef != HasError())
                        {
                            errorBef = !errorBef;
                            RefreshTitle();
                        }
                        EditorGUI.BeginChangeCheck();
                        serializedObject.UpdateIfRequiredOrScript();
                        OnInspectorGUI();
                        if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
                    }
                });
                inspector.style.flexGrow = 1;
                Add(inspector);
            }
            else
            {
                this.Q<Toggle>().Q("unity-checkmark").visible = false;
                contentContainer.style.paddingBottom = default;
            }
            errorBef = HasError();
            RefreshTitle();
        }

        private void RefreshTitle()
        {
            text = ItemModule.GetName(userData.GetType());
            this.Q<Toggle>().tooltip = null;
            if (errorBef) text += "(存在错误)";
            MarkDirtyRepaint();
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
        protected virtual bool HasError()
        {
            return userData is ItemModule module && shouldCheckError && !module.IsValid;
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
        protected sealed class CustomMuduleDrawerAttribute : Attribute
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