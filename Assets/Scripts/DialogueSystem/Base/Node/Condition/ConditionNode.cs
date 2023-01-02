namespace ZetanStudio.DialogueSystem
{
    using UI;

    [Group("条件显示")]
    public abstract class ConditionNode : DialogueNode, ISoloMainOption
    {
        public sealed override bool OnEnter() => true;
        public sealed override bool IsManual() => false;
        public sealed override void DoManual(DialogueWindow window) { }

        public ConditionNode() => options = new DialogueOption[] { new DialogueOption(true, null) };

        /// <summary>
        /// 检查此分支是否显示
        /// </summary>
        /// <returns><i>true</i> = 显示</returns>
        public bool Check(DialogueData entryData)
        {
            DialogueNode temp = this;
            while (temp is ConditionNode condition)
            {
                if (!condition.CheckCondition(entryData)) return false;
                temp = temp[0]?.Next;
            }
            return true;
        }
        protected abstract bool CheckCondition(DialogueData entryData);

#if UNITY_EDITOR
        public override bool CanLinkFrom(DialogueNode from, DialogueOption option) => from is ConditionNode or BranchNode || !option.IsMain;
#endif
    }
}