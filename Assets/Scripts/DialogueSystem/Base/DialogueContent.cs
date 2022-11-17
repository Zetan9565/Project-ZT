using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ZetanStudio.DialogueSystem
{
    using ConditionSystem;
    using InventorySystem;
    using InventorySystem.UI;
    using ItemSystem;
    using ItemSystem.UI;
    using UI;

    [Serializable]
    public abstract class DialogueContent
    {
        [field: SerializeField, TextArea]
        public string ID { get; protected set; } = "CON-" + Guid.NewGuid().ToString("N");

        [SerializeReference, PolymorphismList("GetName")]
        protected DialogueEvent[] events = { };
        public ReadOnlyCollection<DialogueEvent> Events => new ReadOnlyCollection<DialogueEvent>(events);

        [SerializeField]
        protected DialogueOption[] options = { };
        public ReadOnlyCollection<DialogueOption> Options => new ReadOnlyCollection<DialogueOption>(options);

        [field: SerializeField]
        public bool ExitHere { get; protected set; }

        public bool Exitable => Dialogue.Traverse(this, n => n.ExitHere);

        public abstract bool IsValid { get; }

        public DialogueOption this[int optionIndex] => optionIndex >= 0 && optionIndex < options.Length ? options[optionIndex] : null;

        public string GetName() => GetName(GetType());

        public virtual bool OnEnter() => true;

        public virtual bool IsManual() => false;

        public virtual void DoManual(DialogueWindow window) { }

        public static string GetGroup(Type type) => type.GetCustomAttribute<GroupAttribute>(true)?.group ?? string.Empty;
        public static string GetName(Type type) => type.GetCustomAttribute<NameAttribute>(true)?.name ?? type.Name;
        public static bool IsNormal(DialogueContent content) => content is not null && content is not DecoratorContent && content is not SuffixContent;

        public static implicit operator bool(DialogueContent self) => self != null;

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

        /// <summary>
        /// 用于在编辑器中复制结点，不应在游戏逻辑中使用
        /// </summary>
        public DialogueContent Copy()
        {
            var clone = MemberwiseClone() as DialogueContent;
            if (clone)
            {
                clone.ID = "CON-" + Guid.NewGuid().ToString("N");
                clone.options = new DialogueOption[options.Length];
                clone.ExitHere = false;
                for (int i = 0; i < options.Length; i++)
                {
                    clone.options[i] = new DialogueOption(options[i].IsMain, options[i].Title);
                }
                var copy = Activator.CreateInstance(GetType()) as DialogueContent;
                EditorUtility.CopySerializedManagedFieldsOnly(clone, copy);
                return copy;
            }
            else return null;
        }

        public static class Editor
        {
            public static float GetWidth(Type type) => type.GetCustomAttribute<WidthAttribute>()?.width ?? 0f;

            public static DialogueOption AddOption(DialogueContent content, bool main, string title = null)
            {
                DialogueOption option = new DialogueOption(main, title);
                ArrayUtility.Add(ref content.options, option);
                return option;
            }
            public static void RemoveOption(DialogueContent content, DialogueOption option)
            {
                ArrayUtility.Remove(ref content.options, option);
            }

            public static void MoveOptionUpward(DialogueContent content, int index)
            {
                if (index < 1) return;
                (content.options[index], content.options[index - 1]) = (content.options[index - 1], content.options[index]);
            }
            public static void MoveOptionDownward(DialogueContent content, int index)
            {
                if (index >= content.options.Length - 1) return;
                (content.options[index], content.options[index - 1]) = (content.options[index - 1], content.options[index]);
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
    [Serializable, Group("选项修饰器")]
    public abstract class DecoratorContent : DialogueContent, INonEvent, IMainOptionOnly
    {
        public DecoratorContent() => options = new DialogueOption[] { new DialogueOption(true, null) };

        public override bool IsValid => true;

        public sealed override bool IsManual() => false;
        public sealed override void DoManual(DialogueWindow window) { }

        public abstract void Decorate(DialogueData data, ref string title);

#if UNITY_EDITOR
        public override bool CanLinkFrom(DialogueContent from, DialogueOption option) => from is DecoratorContent || !option.IsMain;
#endif
    }

    [Serializable, Name("染色"), Width(50f)]
    public sealed class ColorfulDecorator : DecoratorContent
    {
        [field: SerializeField]
        public Color Color { get; private set; } = Color.black;

        public override void Decorate(DialogueData data, ref string title)
        {
            title = Utility.ColorText(title, Color);
        }
    }
    [Serializable, Name("粗体")]
    public sealed class BoldDecorator : DecoratorContent
    {
        public override void Decorate(DialogueData data, ref string title)
        {
            title = Utility.BoldText(title);
        }
    }
    [Serializable, Name("斜体")]
    public sealed class ItalicDecorator : DecoratorContent
    {
        public override void Decorate(DialogueData data, ref string title)
        {
            title = Utility.ItalicText(title);
        }
    }
    #endregion

    #region 条件
    public abstract class ConditionContent : DialogueContent, IMainOptionOnly
    {
        public sealed override bool OnEnter() => true;
        public sealed override bool IsManual() => false;
        public sealed override void DoManual(DialogueWindow window) { }

        public ConditionContent() => options = new DialogueOption[] { new DialogueOption(true, null) };

        public bool Check(DialogueData entryData)
        {
            DialogueContent temp = this;
            while (temp is ConditionContent condition)
            {
                if (!condition.CheckCondition(entryData)) return false;
                temp = temp[0]?.Content;
            }
            return true;
        }
        protected abstract bool CheckCondition(DialogueData entryData);

#if UNITY_EDITOR
        public override bool CanLinkFrom(DialogueContent from, DialogueOption option) => from is ConditionContent or BranchContent || !option.IsMain;
#endif
    }
    [Serializable, Name("按条件显示"), Width(246f)]
    public class NormalCondition : ConditionContent
    {
        [field: SerializeField]
        public ConditionGroup Condition { get; private set; } = new ConditionGroup();

        public override bool IsValid => Condition.IsValid;

        protected override bool CheckCondition(DialogueData entryData) => Condition.IsMeet();
    }
    [Serializable, Name("完成前显示"), Width(50f)]
    public class DeleteOnDoneCondition : ConditionContent
    {
        public override bool IsValid => true;

        protected override bool CheckCondition(DialogueData entryData) => !entryData[ID].IsDone;
    }
    #endregion

    #region 文本
    [Serializable, Group("文本")]
    public abstract class TextContent : DialogueContent
    {
        [field: SerializeField]
        public string Talker { get; protected set; }

        [field: SerializeField, TextArea]
        public string Text { get; protected set; }

        //[field: SerializeField]
        //public Sprite Portrait { get; protected set; }

        [field: SerializeField]
        public AnimationCurve UtterInterval { get; protected set; } = new AnimationCurve(new Keyframe(0, 0.02f), new Keyframe(1, 0.02f));

        public override bool IsValid => !string.IsNullOrEmpty(Talker) && !string.IsNullOrEmpty(Text);

#if UNITY_EDITOR
        public string Preview()
        {
            string result = string.Empty;
            string talker = $"[{(string.IsNullOrEmpty(Talker) ? "(未定义)" : Keyword.Editor.HandleKeywords(Talker))}]说：";
            talker = System.Text.RegularExpressions.Regex.Replace(talker, @"{\[NPC\]}", "(交互对象)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            talker = System.Text.RegularExpressions.Regex.Replace(talker, @"{\[PLAYER\]}", "(玩家)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            result += talker;
            string text = $"{(string.IsNullOrEmpty(Text) ? "(无内容)" : Keyword.Editor.HandleKeywords(Text))}";
            text = System.Text.RegularExpressions.Regex.Replace(text, @"{\[NPC\]}", "[交互对象]", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            text = System.Text.RegularExpressions.Regex.Replace(text, @"{\[PLAYER\]}", "[玩家]", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            result += text;
            return Utility.RemoveTags(result);
        }
#endif
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

        public EntryContent(string id, string talker, string content) : this(talker, content)
        {
            ID = id;
        }

#if UNITY_EDITOR
        public override bool CanLinkFrom(DialogueContent from, DialogueOption option) => false;
#endif
    }

    [Serializable, Name("语句")]
    public sealed class WordsContent : TextContent
    {
        public WordsContent() { }

        public WordsContent(string talker, string content)
        {
            Talker = talker;
            Text = content;
        }
    }

    [Serializable, Name("提交道具")]
    public class SubmitItemContent : TextContent
    {
        [SerializeField]
        private ItemInfo[] itemsToSubmit = { };
        public ReadOnlyCollection<ItemInfo> ItemsToSubmit => new ReadOnlyCollection<ItemInfo>(itemsToSubmit);

        public override bool IsValid => base.IsValid && itemsToSubmit.Length > 0 && itemsToSubmit.All(x => x.IsValid);

        public SubmitItemContent() { }

        public SubmitItemContent(string talker, string content, params ItemInfo[] items)
        {
            Talker = talker;
            Text = content;
            itemsToSubmit = items;
        }

        public override bool IsManual() => true;

        public override void DoManual(DialogueWindow window)
        {
            if (InventoryWindow.OpenSelectionWindow<BackpackWindow>(ItemSelectionType.SelectNum, confirm, cancel: cancel, amountLimit: amount, selectCondition: condition))
                WindowsManager.HideWindow(window, true);

            bool confirm(IEnumerable<CountedItem> items)
            {
                foreach (var info in itemsToSubmit)
                {
                    if (items.FirstOrDefault(x => info.Item == x.source.Model) is not CountedItem find || info.Amount != find.amount)
                    {
                        MessageManager.Instance.New(LM.Tr(typeof(Dialogue).Name, "数量不正确"));
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

    [Serializable, Name("交换道具")]
    public sealed class SubmitAndGetItemContent : SubmitItemContent
    {
        [SerializeField]
        private ItemInfo[] itemsCanGet = { };
        public ReadOnlyCollection<ItemInfo> ItemsCanGet => new ReadOnlyCollection<ItemInfo>(itemsCanGet);

        public override bool IsValid => base.IsValid && itemsCanGet.Length > 0 && itemsCanGet.All(x => x.IsValid);

        public SubmitAndGetItemContent() { }

        public SubmitAndGetItemContent(string talker, string content, ItemInfo[] itemsToSubmit, ItemInfo[] itemsCanGet) : base(talker, content, itemsToSubmit)
        {
            Talker = talker;
            Text = content;
            this.itemsCanGet = itemsCanGet;
        }

        public override void DoManual(DialogueWindow window)
        {
            if (InventoryWindow.OpenSelectionWindow<BackpackWindow>(ItemSelectionType.SelectNum, confirm, cancel: cancel, amountLimit: amount, selectCondition: condition))
                WindowsManager.HideWindow(window, true);

            bool confirm(IEnumerable<CountedItem> items)
            {
                foreach (var info in ItemsToSubmit)
                {
                    if (items.FirstOrDefault(x => info.Item == x.source.Model) is not CountedItem find || info.Amount != find.amount)
                    {
                        MessageManager.Instance.New(LM.Tr(typeof(Dialogue).Name, "数量不正确"));
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

    [Serializable, Name("获得道具")]
    public sealed class GetItemContent : TextContent
    {
        [SerializeField]
        private ItemInfo[] itemsCanGet = { };
        public ReadOnlyCollection<ItemInfo> ItemsCanGet => new ReadOnlyCollection<ItemInfo>(itemsCanGet);

        public override bool IsValid => base.IsValid && itemsCanGet.Length > 0 && itemsCanGet.All(i => i.IsValid);

        public GetItemContent() { }

        public GetItemContent(string talker, string content, params ItemInfo[] items)
        {
            Talker = talker;
            Text = content;
            itemsCanGet = items;
        }

        public override bool OnEnter()
        {
            return BackpackManager.Instance.Get(itemsCanGet);
        }
    }
    #endregion

    #region 后缀
    [Serializable]
    public abstract class SuffixContent : DialogueContent, INonEvent
    {

    }
    [Serializable, Name("递归"), Width(100f)]
    public sealed class RecursionSuffix : SuffixContent
    {
        [field: SerializeField, Min(2)]
        public int Depth { get; private set; } = 2;

        public override bool IsValid => Depth > 1;

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
                if (!Dialogue.Traverse(entry, c =>
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
        public override void DoManual(DialogueWindow window)
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

    #region 拦截
    [Serializable]
    public abstract class BlockerContent : DialogueContent, IMainOptionOnly
    {
        public sealed override bool OnEnter()
        {
            var result = CheckCondition();
            var notification = GetNotification(result);
            if (!string.IsNullOrEmpty(notification)) MessageManager.Instance.New(notification);
            return result;
        }
        protected abstract bool CheckCondition();
        protected virtual string GetNotification(bool result) => string.Empty;

        public sealed override bool IsManual() => false;
        public sealed override void DoManual(DialogueWindow window) { }

        public BlockerContent() => options = new DialogueOption[] { new DialogueOption(true, null) };
    }
    [Name("道具条件")]
    public class ItemBlocker : BlockerContent
    {
        [field: SerializeField]
        public Item Item { get; private set; }

        [field: SerializeField]
        public bool Have { get; private set; } = true;

        public override bool IsValid => Item;

        protected override bool CheckCondition() => !(BackpackManager.Instance.HasItem(Item) ^ Have);

        protected override string GetNotification(bool result)
        {
            if (!result && Have) return $"未持有[{ItemFactory.GetColorName(Item)}]时无法继续";
            else if (!result && !Have) return $"持有时[{ItemFactory.GetColorName(Item)}]无法继续";
            else return string.Empty;
        }
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

        public override bool IsValid => true;

        public override bool CanLinkFrom(DialogueContent from, DialogueOption option) => option.IsMain && from.Options.Count == 1 && from is not DecoratorContent && from is not BlockerContent;
    }
#endif

    [Serializable, Name("分支"), Width(60f)]
    public sealed class BranchContent : DialogueContent
    {
        public override bool IsValid => options.Length > 0 && options.All(x => x.IsMain);

        public DialogueContent GetBranch(DialogueData entryData)
        {
            foreach (var option in options)
            {
                if (option?.Content is ConditionContent condition && condition.Check(entryData))
                {
                    var temp = condition.Options[0]?.Content;
                    while (temp is ConditionContent)
                    {
                        temp = temp[0]?.Content;
                    }
                    return temp;
                }
            }
            return null;
        }
    }
    [Serializable, Name("其它对话")]
    public sealed class OtherDialogueContent : DialogueContent
    {
        [field: SerializeField]
        public Dialogue Dialogue { get; private set; }

        public override bool IsValid => Dialogue;

        public override bool OnEnter() => Dialogue;

        public override bool IsManual() => true;

        public override void DoManual(DialogueWindow window)
        {
            window.CurrentEntryData[this].Access();
            window.PushContinuance(this);
            window.StartWith(Dialogue);
        }
    }

    [Serializable, Group("外置选项")]
    public abstract class ExternalOptionsContent : DialogueContent
    {
        public abstract ReadOnlyCollection<DialogueOption> GetOptions(DialogueData entryData, DialogueContent owner);

#if UNITY_EDITOR
        public override bool CanLinkFrom(DialogueContent from, DialogueOption option)
        {
            return from is not DecoratorContent and not ExternalOptionsContent && option.IsMain;
        }
#endif
    }

    [Serializable, Name("随机选项顺序"), Width(50f)]
    public sealed class RandomOptions : ExternalOptionsContent
    {
        public override bool IsValid => true;

        [field: SerializeField]
        public bool Always { get; private set; }

        public override ReadOnlyCollection<DialogueOption> GetOptions(DialogueData entryData, DialogueContent owner)
        {
            var order = getOptionOrder(entryData);
            var options = new DialogueOption[order.Count];
            for (int i = 0; i < order.Count; i++)
            {
                options[i] = this.options[order[i]];
            }
            return new ReadOnlyCollection<DialogueOption>(options);

            IList<int> getOptionOrder(DialogueData entryData)
            {
                if (Always) return Utility.RandomOrder(getIndices());
                var data = entryData[this];
                if (!data.Accessed) return data.AdditionalData.Write("order", new GenericData()).WriteAll(Utility.RandomOrder(getIndices()));
                else return data.AdditionalData?.ReadData("order")?.ReadIntList() as IList<int> ?? getIndices();

                int[] getIndices()
                {
                    int[] indices = new int[this.options.Length];
                    for (int i = 0; i < this.options.Length; i++)
                    {
                        indices[i] = i;
                    }
                    return indices;
                }
            }
        }
    }
    #endregion

    #region 接口
    public interface IMainOptionOnly { }
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

        public static implicit operator bool(DialogueOption self) => self != null;

#if UNITY_EDITOR
        public static class Editor
        {
            public static void SetContent(DialogueOption option, DialogueContent content)
            {
                if (content is EntryContent or ExitContent) return;
                option.Content = content;
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
    public sealed class DialogueContentGroup
    {
        public string name;
        public List<string> contents = new List<string>();
        public Vector2 position;

        public DialogueContentGroup(string name, Vector2 position)
        {
            this.name = name;
            this.position = position;
        }
    }
#endif
}