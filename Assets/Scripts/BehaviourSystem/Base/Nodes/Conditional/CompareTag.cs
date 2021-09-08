namespace ZetanStudio.BehaviourTree
{
    [NodeDescription("标签比较结点：与指定标签进行比较，若标签未设置或比较对象未设置，则向上反馈评估正进行，否则根据比较结果向上反馈评估成败")]
    public class CompareTag : Conditional
    {
        [DisplayName("与自身比较")]
        public SharedBoolean compareThis = true;
        [DisplayName("比较标签")]
        public SharedString tag;
        [DisplayName("比较对象")]
        public SharedGameObject target;

        public override bool IsValid => tag != null && target != null;

        protected override bool CheckCondition()
        {
            if (!compareThis) return target.GetGenericValue().CompareTag(tag);
            else return gameObject.CompareTag(tag);
        }

        protected override bool ShouldKeepRunning()
        {
            if (!compareThis) return target.GetGenericValue() == null;
            else return string.IsNullOrEmpty(tag.GetGenericValue());
        }
    }
}