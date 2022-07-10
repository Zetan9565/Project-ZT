using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace ZetanStudio.DialogueSystem
{
    using ItemSystem;
    using UI;

    [Serializable]
    public abstract class DialogueContent
    {
        [field: SerializeField, TextArea]
        public string ID { get; protected set; } = "CON-" + Guid.NewGuid().ToString("N");

        [SerializeField, NonReorderable]
        protected WordsEvent[] events = { };
        public ReadOnlyCollection<WordsEvent> Events => new ReadOnlyCollection<WordsEvent>(events);

        [SerializeField]
        protected DialogueOption[] options = { };
        public ReadOnlyCollection<DialogueOption> Options => new ReadOnlyCollection<DialogueOption>(options);

        public DialogueOption FirstOption => options.Length > 0 ? options[0] : null;

        [field: SerializeField]
        public bool ExitHere { get; protected set; }

        public bool Exitable => NewDialogue.Traverse(this, n => n.ExitHere);

        public string GetName() => GetName(GetType());

        public virtual bool Enter() => true;

        public virtual bool IsManual() => false;

        public virtual void Manual(NewDialogueWindow window) { }

        public static string GetGroup(Type type) => type.GetCustomAttribute<GroupAttribute>()?.group ?? string.Empty;
        public static string GetName(Type type) => type.GetCustomAttribute<NameAttribute>()?.name ?? type.Name;
        public static bool IsNormal(DialogueContent content)
        {
            return content is not null && content is not DecoratorContent && content is not SuffixContent;
        }

        public static implicit operator bool(DialogueContent self)
        {
            return self != null;
        }

        #region 特性
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
        protected sealed class WidthAttribute : Attribute
        {
            public readonly float width;

            public WidthAttribute(float width)
            {
                this.width = width;
            }
        }
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
        public virtual bool CanLinkFrom(DialogueContent from, DialogueOption option) => true;

        public static class Editor
        {
            public static float GetWidth(Type type) => type.GetCustomAttribute<WidthAttribute>()?.width ?? 0f;

            public static DialogueOption AddOption(DialogueContent content, bool main, string title = null)
            {
                if (main && content.options.Length > 1) return null;
                DialogueOption option = new DialogueOption(main, title);
                UnityEditor.ArrayUtility.Add(ref content.options, option);
                return option;
            }
            public static void RemoveOption(DialogueContent content, DialogueOption option)
            {
                UnityEditor.ArrayUtility.Remove(ref content.options, option);
            }
            public static void SetAsExit(DialogueContent content, bool exit = true)
            {
                if (content.options.Length > 1) return;
                if (exit)
                    if (content.options.Length == 1) DialogueOption.Editor.SetContent(content.options[0], null);
                    else AddOption(content, true);
                content.ExitHere = exit;
            }
        }
#endif
    }

    #region 选项修饰器
    [Serializable]
    public abstract class DecoratorContent : DialogueContent, INonEvent
    {
        public DecoratorContent() => options = new DialogueOption[] { new DialogueOption(true, null) };

        public sealed override bool IsManual() => false;
        public sealed override void Manual(NewDialogueWindow window) { }

        /// <returns><i>false</i> 该选项会被剔除</returns>
        public abstract bool Decorate(DialogueContentData data, DialogueContent owner, DialogueOption option, out string title);

#if UNITY_EDITOR
        public override bool CanLinkFrom(DialogueContent from, DialogueOption option) => !option.IsMain;
#endif
    }

    [Serializable, Name("完成删除"), Group("选项修饰器"), Width(50f)]
    public sealed class DeleteOnDoneDecorator : DecoratorContent
    {
        public override bool Decorate(DialogueContentData data, DialogueContent owner, DialogueOption option, out string title)
        {
            title = option.Title;
            if (owner.FirstOption == option && owner.Options.Where(x => x != option).All(opt =>
            {
                var temp = opt.Content;
                while (temp is DecoratorContent decorator)
                {
                    if (decorator is DeleteOnDoneDecorator delete)
                    {
                        if (findTarget(delete) is DialogueContent target && data[target].IsDone)
                            return true;
                    }
                    else if (!decorator.Decorate(data, owner, opt, out _)) return true;
                    temp = temp.FirstOption?.Content;
                }
                return false;
            })) return true;
            if (findTarget(this) is not DialogueContent target) return true;
            else return !data[target].IsDone;

            static DialogueContent findTarget(DeleteOnDoneDecorator delete)
            {
                DialogueContent target = null;
                DialogueContent temp = delete;
                while (temp is DecoratorContent)
                {
                    temp = temp.FirstOption?.Content;
                    if (temp is not DecoratorContent)
                        target = temp;
                }
                return target;
            }
        }
    }
    [Serializable, Name("条件"), Group("选项修饰器")]
    public sealed class ConditionDecorator : DecoratorContent
    {
        public override bool Decorate(DialogueContentData data, DialogueContent owner, DialogueOption option, out string title)
        {
            throw new NotImplementedException();
        }
    }
    [Serializable, Name("染色"), Group("选项修饰器")]
    public sealed class ColorfulDecorator : DecoratorContent
    {
        [field: SerializeField]
        public Color Color { get; private set; }

        public override bool Decorate(DialogueContentData data, DialogueContent owner, DialogueOption option, out string title)
        {
            title = ZetanUtility.ColorText(option.Title, Color);
            return true;
        }
    }
    #endregion

    #region 文本
    [Serializable]
    public abstract class TextContent : DialogueContent
    {
        [field: SerializeField]
        public string Talker { get; protected set; }

        [field: SerializeField, TextArea]
        public string Text { get; protected set; }
    }

    [Serializable, Name("开始")]
    public sealed class EntryContent : TextContent
    {
        public EntryContent()
        {
            ID = "DLG" + Guid.NewGuid().ToString("N");
            options = new DialogueOption[] { new DialogueOption(true, null) };
            ExitHere = true;
        }

        public EntryContent(string talker, string content) : this()
        {
            Talker = talker;
            Text = content;
        }

        public EntryContent(string id, string talker, string content) : this()
        {
            ID = id;
            Talker = talker;
            Text = content;
        }

#if UNITY_EDITOR
        public override bool CanLinkFrom(DialogueContent from, DialogueOption option) => false;
#endif
    }

    [Serializable, Name("语句"), Group("文本")]
    public sealed class WordsContent : TextContent
    {
        public WordsContent(string talker, string content)
        {
            Talker = talker;
            Text = content;
        }
    }

    [Serializable, Name("提交道具"), Group("文本")]
    public class SubmitItemContent : TextContent
    {
        [SerializeField]
        private ItemInfo[] itemsToSubmit = { };
        public ReadOnlyCollection<ItemInfo> ItemsToSubmit => new ReadOnlyCollection<ItemInfo>(itemsToSubmit);

        public SubmitItemContent() { }

        public SubmitItemContent(string talker, string content, params ItemInfo[] items)
        {
            Talker = talker;
            Text = content;
            itemsToSubmit = items;
        }

        public override bool IsManual() => true;

        public override void Manual(NewDialogueWindow window)
        {
            if (InventoryWindow.OpenSelectionWindow<BackpackWindow>(ItemSelectionType.SelectNum, confirm, cancel: cancel, amountLimit: amount, selectCondition: condition))
                WindowsManager.HideWindow(window, true);

            bool confirm(IEnumerable<CountedItem> items)
            {
                foreach (var info in itemsToSubmit)
                {
                    if (items.FirstOrDefault(x => info.Item == x.source.Model) is not CountedItem find || info.Amount != find.amount)
                    {
                        MessageManager.Instance.New(LM.Tr(typeof(NewDialogue).Name, "数量不正确"));
                        return false;
                    }
                }
                if (BackpackManager.Instance.Lose(items)) window.ContinueWith(this);
                WindowsManager.HideWindow(window, false);
                return true;
            }
            bool condition(ItemData item) => itemsToSubmit.Any(x => x.Item == item?.Model);
            int amount(ItemData item) => itemsToSubmit.FirstOrDefault(x => x.Item == item.Model)?.Amount ?? 0;
            void cancel() => WindowsManager.HideWindow(window, false);
        }
    }

    [Serializable, Name("提交并获得道具"), Group("文本")]
    public sealed class SubmitAndGetItemContent : SubmitItemContent
    {
        [SerializeField]
        private ItemInfo[] itemsCanGet = { };
        public ReadOnlyCollection<ItemInfo> ItemsCanGet => new ReadOnlyCollection<ItemInfo>(itemsCanGet);

        public SubmitAndGetItemContent(string talker, string content, ItemInfo[] itemsToSubmit, ItemInfo[] itemsCanGet) : base(talker, content, itemsToSubmit)
        {
            Talker = talker;
            Text = content;
            this.itemsCanGet = itemsCanGet;
        }

        public override void Manual(NewDialogueWindow window)
        {
            if (InventoryWindow.OpenSelectionWindow<BackpackWindow>(ItemSelectionType.SelectNum, confirm, cancel: cancel, amountLimit: amount, selectCondition: condition))
                WindowsManager.HideWindow(window, true);

            bool confirm(IEnumerable<CountedItem> items)
            {
                foreach (var info in ItemsToSubmit)
                {
                    if (items.FirstOrDefault(x => info.Item == x.source.Model) is not CountedItem find || info.Amount != find.amount)
                    {
                        MessageManager.Instance.New(LM.Tr(typeof(NewDialogue).Name, "数量不正确"));
                        return false;
                    }
                }
                if (BackpackManager.Instance.Lose(items, CountedItem.Convert(itemsCanGet))) window.ContinueWith(this);
                WindowsManager.HideWindow(window, false);
                return true;
            }
            bool condition(ItemData item) => ItemsToSubmit.Any(x => x.Item == item?.Model);
            int amount(ItemData item) => ItemsToSubmit.FirstOrDefault(x => x.Item == item.Model)?.Amount ?? 0;
            void cancel() => WindowsManager.HideWindow(window, false);
        }
    }

    [Serializable, Name("获得道具"), Group("文本")]
    public sealed class GetItemContent : TextContent
    {
        [SerializeField]
        private ItemInfo[] itemsCanGet = { };
        public ReadOnlyCollection<ItemInfo> ItemsCanGet => new ReadOnlyCollection<ItemInfo>(itemsCanGet);

        public GetItemContent() { }

        public GetItemContent(string talker, string content, params ItemInfo[] items)
        {
            Talker = talker;
            Text = content;
            itemsCanGet = items;
        }

        public override bool Enter()
        {
            return BackpackManager.Instance.Get(itemsCanGet);
        }
    }
    #endregion

    #region 后缀
    [Serializable]
    public abstract class SuffixContent : DialogueContent, INonOption, INonEvent
    {

    }
    [Serializable, Name("递归"), Width(100f)]
    public sealed class RecursionSuffix : SuffixContent
    {
        [field: SerializeField, Min(2)]
        public int Depth { get; private set; } = 2;

        public RecursionSuffix() { }

        public RecursionSuffix(int depth) => Depth = depth;

        public DialogueContent FindRecursionPoint(EntryContent entry)
        {
            DialogueContent find = null;
            if (!entry) return find;
            int depth = 0;
            DialogueContent temp = this;
            while (temp && depth < Depth)
            {
                if (!NewDialogue.Traverse(entry, c =>
                {
                    if (c.Options.Any(x => x.Content == temp))
                    {
                        temp = c;
                        if (temp is not DecoratorContent) depth++;
                        return true;
                    }
                    return false;
                })) temp = null;
                if (temp == entry) break;
            }
            if (depth == Depth || temp == entry) find = temp;
            return find;
        }

        public override bool IsManual() => true;
        public override void Manual(NewDialogueWindow window)
        {
            window.CurrentEntryData[this].Access();
            window.ContinueWith(FindRecursionPoint(window.CurrentEntry));
        }

#if UNITY_EDITOR
        public override bool CanLinkFrom(DialogueContent from, DialogueOption option)
        {
            return from is not EntryContent;
        }
#endif
    }
    #endregion

    #region 特殊
#if UNITY_EDITOR
    /// <summary>
    /// 用于在编辑器中设置退出点，不应在游戏逻辑中使用
    /// </summary>
    [Serializable, Name("结束"), Width(100f)]
    public sealed class ExitContent : SuffixContent
    {
        public ExitContent() => _position = new Vector2(360, 0);

        public override bool CanLinkFrom(DialogueContent from, DialogueOption option) => option.IsMain && from.Options.Count == 1 && from is not DecoratorContent;
    }
#endif

    [Serializable, Name("分支")]
    public sealed class BranchContent : DialogueContent
    {
        [field: SerializeField]
        public NewDialogue Dialogue { get; private set; }

        public override bool Enter()
        {
            return Dialogue;
        }

        public override bool IsManual() => true;

        public override void Manual(NewDialogueWindow window)
        {
            window.CurrentEntryData[this].Access();
            window.PushContinuance(this);
            window.StartWith(Dialogue);
        }
    }
    #endregion

    #region 接口
    public interface INonOption { }
    public interface INonEvent { }
    #endregion

    [Serializable]
    public sealed class DialogueOption
    {
        [field: SerializeField]
        public string Title { get; private set; }

        [field: SerializeReference]
        public DialogueContent Content { get; private set; }

        [field: SerializeField]
        public bool IsMain { get; private set; }

        public DialogueOption() { }

        public DialogueOption(bool main, string title)
        {
            IsMain = main;
            Title = title;
        }

#if UNITY_EDITOR
        public static class Editor
        {
            public static void SetContent(DialogueOption option, DialogueContent content)
            {
                if (content is EntryContent || content is ExitContent) return;
                option.Content = content;
            }

            public static void SetIsMain(DialogueOption option, bool main) => option.IsMain = main;
        }
#endif
    }
}