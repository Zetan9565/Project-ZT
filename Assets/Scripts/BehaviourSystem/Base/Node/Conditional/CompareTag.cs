namespace ZetanStudio.BehaviourTree.Nodes
{
    [Description("标签比较结点：与指定标签进行比较，若标签未设置或比较对象未设置，则向上反馈评估正进行，否则根据比较结果向上反馈评估成败")]
    public class CompareTag : Conditional
    {
        [Label("比较标签"), Tag]
        public SharedString tag;
        [Label("与自身比较")]
        public SharedBool compareThis = true;
        [Label("比较对象"), HideIf("compareThis", true)]
        public SharedGameObject target;

        public override bool IsValid => tag != null && tag.IsValid && !string.IsNullOrEmpty(tag.Value) && tag.Value != "Untagged" && target != null && target.IsValid;

        public override bool CheckCondition()
        {
            if (!compareThis) return target.GetGenericValue().CompareTag(tag);
            else return gameObject.CompareTag(tag);
        }
    }
}