using System;

namespace ZetanStudio.DialogueSystem
{
    using UI;

    [Serializable, Group("条件拦截")]
    public abstract class BlockerNode : DialogueNode, ISoloMainOption
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

        public BlockerNode() => options = new DialogueOption[] { new DialogueOption(true, null) };
    }
}