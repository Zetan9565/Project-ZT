namespace ZetanStudio.BehaviourTree
{
    [NodeDescription("标签比较结点：与指定标签进行比较，若标签未设置或比较对象未设置，则向上反馈评估正进行，否则根据比较结果向上反馈评估成败")]
    public class CompareTag : Conditional
    {
        [DisplayName("比较标签"), Tag_BT]
        public SharedString tag;
        [DisplayName("与自身比较")]
        public SharedBool compareThis = true;
        [DisplayName("比较对象"), HideIf_BT("compareThis", true)]
        public SharedGameObject target;

        public override bool IsValid => tag != null && tag.IsValid && !string.IsNullOrEmpty(tag.Value) && target != null && target.IsValid;

        public override bool CheckCondition()
        {
            if (!compareThis) return target.GetGenericValue().CompareTag(tag);
            else return gameObject.CompareTag(tag);
        }
    }
}