using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ZetanStudio.DialogueSystem
{
    using System.Collections;
    using UI;

    [Serializable]
    public abstract class DialogueNode
    {
        [field: SerializeField, TextArea, HideInNode]
        public string ID { get; protected set; } = "CON-" + Guid.NewGuid().ToString("N");

        [SerializeField, HideInNode]
        protected DialogueOption[] options = { };
        public ReadOnlyCollection<DialogueOption> Options => new ReadOnlyCollection<DialogueOption>(options);

        [field: SerializeField, HideInNode]
        public bool ExitHere { get; protected set; }

        public bool Exitable => Dialogue.Traverse(this, n => n.ExitHere);

        public abstract bool IsValid { get; }

        public DialogueOption this[int optionIndex] => optionIndex >= 0 && optionIndex < options.Length ? options[optionIndex] : null;

        public string GetName() => GetName(GetType());

        public virtual bool OnEnter() => true;

        public virtual bool IsManual() => false;

        public virtual void DoManual(DialogueWindow window) { }

        public static string GetGroup(Type type) => type.GetCustomAttribute<GroupAttribute>(true)?.group ?? string.Empty;
        public static string GetName(Type type) => type.GetCustomAttribute<NameAttribute>()?.name ?? type.Name;
        public static string GetDescription(Type type) => type.GetCustomAttribute<DescriptionAttribute>()?.desc ?? string.Empty;

        public static implicit operator bool(DialogueNode obj) => obj != null;

        #region 特性
        [AttributeUsage(AttributeTargets.Class)]
        protected sealed class MainOptionsCountAttribute : Attribute
        {
            public readonly int min;
            public readonly int max;

            public MainOptionsCountAttribute(int min, int max)
            {
                this.min = min < 0 ? 0 : min;
                this.max = max < 0 ? 0 : max;
            }
        }
        [AttributeUsage(AttributeTargets.Class)]
        protected sealed class NameAttribute : Attribute
        {
            public readonly string name;

            public NameAttribute(string name)
            {
                this.name = name;
            }
        }
        [AttributeUsage(AttributeTargets.Class)]
        protected sealed class GroupAttribute : Attribute
        {
            public readonly string group;

            public GroupAttribute(string group)
            {
                this.group = group;
            }
        }
        [AttributeUsage(AttributeTargets.Class)]
        protected sealed class DescriptionAttribute : Attribute
        {
            public readonly string desc;

            public DescriptionAttribute(string desc)
            {
                this.desc = desc;
            }
        }
        [AttributeUsage(AttributeTargets.Class)]
        protected sealed class WidthAttribute : Attribute
        {
            public readonly float width;

            public WidthAttribute(float width)
            {
                this.width = width;
            }
        }
        [AttributeUsage(AttributeTargets.Field)]
        protected sealed class HideInNodeAttribute : Attribute { }
        #endregion

#if UNITY_EDITOR
        /// <summary>
        /// 用于在编辑器中设置结点位置，不应在游戏逻辑中使用
        /// </summary>
        [HideInInspector] public Vector2 _position;
        /// <summary>
        /// 用于在编辑器中记录上一次退出点，不应在游戏逻辑中使用
        /// </summary>
        [HideInInspector] public bool lastExitHere;

        /// <summary>
        /// 用于在编辑器中筛选端口，不应在游戏逻辑中使用
        /// </summary>
        public virtual bool CanLinkFrom(DialogueNode from, DialogueOption option) => true;

        public HashSet<string> GetHiddenFields()
        {
            HashSet<string> fields = new HashSet<string>();
            Type type = GetType();
            while (type != null)
            {
                collect(type);
                type = type.BaseType;
            }
            return fields;

            void collect(Type type)
            {
                foreach (var field in type.GetFields(Utility.CommonBindingFlags))
                {
                    if (field.GetCustomAttribute<HideInNodeAttribute>() is not null)
                        fields.Add(field.Name);
                }
            }
        }

        /// <summary>
        /// 用于在编辑器中复制结点，不应在游戏逻辑中使用
        /// </summary>
        public DialogueNode Copy()
        {
            var type = GetType();
            var copy = Activator.CreateInstance(type) as DialogueNode;
            EditorUtility.CopySerializedManagedFieldsOnly(this, copy);
            copy.ID = "CON-" + Guid.NewGuid().ToString("N");
            copy.ExitHere = false;
            copy.lastExitHere = false;
            for (int i = 0; i < options.Length; i++)
            {
                DialogueOption.Editor.SetNext(copy.options[i], null);
            }
            foreach (var field in type.GetFields(Utility.CommonBindingFlags))
            {
                if (typeof(ICopiable).IsAssignableFrom(field.FieldType))
                {
                    var value = field.GetValue(this) as ICopiable;
                    field.SetValue(this, value.Copy());
                }
                else if (field.FieldType.IsArray)
                {
                    var eType = field.FieldType.GetElementType();
                    if (typeof(ICopiable).IsAssignableFrom(eType))
                    {
                        var array = field.GetValue(this) as IList;
                        for (int i = 0; i < array.Count; i++)
                        {
                            if (array[i] != null)
                                array[i] = (array[i] as ICopiable).Copy();
                        }
                    }
                }
                else if (field.FieldType.IsGenericType && typeof(List<>) == field.FieldType.GetGenericTypeDefinition())
                {
                    var eType = field.FieldType.GetGenericArguments()[0];
                    if (typeof(ICopiable).IsAssignableFrom(eType))
                    {
                        var array = field.GetValue(this) as IList;
                        for (int i = 0; i < array.Count; i++)
                        {
                            array[i] = (array[i] as ICopiable).Copy();
                        }
                    }
                }
            }
            return copy;
        }

        public static class Editor
        {
            public static float GetWidth(Type type) => type.GetCustomAttribute<WidthAttribute>()?.width ?? 0f;

            public static DialogueOption AddOption(DialogueNode node, bool main, string title = null)
            {
                DialogueOption option = new DialogueOption(main, title);
                ArrayUtility.Add(ref node.options, option);
                return option;
            }
            public static void RemoveOption(DialogueNode node, DialogueOption option)
            {
                ArrayUtility.Remove(ref node.options, option);
            }

            public static void MoveOptionUpward(DialogueNode node, int index)
            {
                if (index < 1) return;
                (node.options[index], node.options[index - 1]) = (node.options[index - 1], node.options[index]);
            }
            public static void MoveOptionDownward(DialogueNode node, int index)
            {
                if (index >= node.options.Length - 1) return;
                (node.options[index], node.options[index + 1]) = (node.options[index + 1], node.options[index]);
            }

            public static void SetAsExit(DialogueNode node, bool exit = true)
            {
                if (node.options.Length > 1) return;
                if (exit)
                    if (node.options.Length == 1) DialogueOption.Editor.SetNext(node.options[0], null);
                    else AddOption(node, true);
                node.ExitHere = exit;
            }
        }
#endif
    }

    #region 接口
    public interface ISoloMainOption { }
    public interface IEventNode
    {
        public ReadOnlyCollection<DialogueEvent> Events { get; }
    }
    #endregion

    [Serializable]
    public sealed class DialogueOption
    {
        [field: SerializeField]
        public string Title { get; private set; }

        [field: SerializeReference]
        public DialogueNode Next { get; private set; }

        [field: SerializeField]
        public bool IsMain { get; private set; }

        public DialogueOption() { }

        public DialogueOption(bool main, string title)
        {
            IsMain = main;
            Title = title;
        }

        public static implicit operator bool(DialogueOption obj) => obj != null;

#if UNITY_EDITOR
        public static class Editor
        {
            public static void SetNext(DialogueOption option, DialogueNode node)
            {
                if (node is EntryNode or ExitNode) return;
                option.Next = node;
            }

            public static void SetIsMain(DialogueOption option, bool main) => option.IsMain = main;
        }
#endif
    }

#if UNITY_EDITOR
    /// <summary>
    /// 用于在编辑器中设置分组，不应在游戏逻辑中使用
    /// </summary>
    [Serializable]
    public sealed class DialogueGroupData
    {
        public string name;
        public List<string> nodes = new List<string>();
        public Vector2 position;

        public DialogueGroupData(string name, Vector2 position)
        {
            this.name = name;
            this.position = position;
        }
    }
#endif
}