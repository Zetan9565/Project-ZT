using UnityEngine;

namespace ZetanStudio.BehaviourTree
{
    [NodeDescription("根据标签查找：使用GameObject.FindGameObjectWithTag查找游戏对象，重复查找时应配合Wait结点进行优化")]
    public class FindWithTag : Action
    {
        [DisplayName("标签"), Tag]
        public SharedString tag;
        [DisplayName("寄存器")]
        public SharedGameObject register;

        public override bool IsValid => tag != null && !string.IsNullOrEmpty(tag.Value);

        protected override NodeStates OnUpdate()
        {
            GameObject find = GameObject.FindGameObjectWithTag(tag);
            if (!find) return NodeStates.Failure;
            register.Value = find;
            return NodeStates.Success;
        }
    }
}