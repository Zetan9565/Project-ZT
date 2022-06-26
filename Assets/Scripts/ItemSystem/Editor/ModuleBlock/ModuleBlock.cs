using System;
using System.Reflection;
using UnityEditor;
using UnityEngine.UIElements;
using ZetanStudio.ItemSystem.Module;

namespace ZetanStudio.ItemSystem.Editor
{
    public class ModuleBlock : ItemInspectorBlock
    {
        protected readonly SerializedObject serializedObject;
        public readonly SerializedProperty property;
        protected bool shouldCheckError;
        protected bool errorBef;
        public ItemModule Module => userData as ItemModule;

        public ModuleBlock(SerializedProperty property, ItemModule module)
        {
            if (module)
            {
                serializedObject = property.serializedObject;
                shouldCheckError = property.serializedObject.targetObject is Item;
                this.property = property;
                userData = module;
                value = property.isExpanded;
                this.RegisterValueChangedCallback(evt => property.isExpanded = evt.newValue);
                if (property.hasVisibleChildren)
                {
                    IMGUIContainer inspector = new IMGUIContainer(() =>
                    {
                        if (serializedObject.targetObject)
                        {
                            if (errorBef != HasError())
                            {
                                errorBef = !errorBef;
                                EditorApplication.delayCall += RefreshTitle;
                            }
                            EditorGUI.BeginChangeCheck();
                            serializedObject.UpdateIfRequiredOrScript();
                            OnInspectorGUI();
                            if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
                        }
                    });
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
            else
            {
                text = Tr("(失效的模块)");
                HelpBox helpBox = new HelpBox(Tr("模块已失效"), HelpBoxMessageType.Warning);
                Add(helpBox);
                Button fix = new Button(click) { text = Tr("尝试修复") };
                Add(fix);

                static void click()
                {
                    ReferencesFixing.CreateWindow((c) =>
                    {
                        if (c > 0 && EditorWindow.HasOpenInstances<ItemEditor>())
                            EditorWindow.GetWindow<ItemEditor>().RefreshModules();
                    });
                }
            }
        }

        private void RefreshTitle()
        {
            EditorApplication.delayCall -= RefreshTitle;
            text = ItemModule.GetName(userData.GetType()) + (errorBef ? $"({Tr("存在错误")})" : string.Empty);
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
            return shouldCheckError && userData is ItemModule module && !module.IsValid;
        }

        public static ModuleBlock Create(SerializedProperty property, ItemModule module)
        {
            if (module)
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