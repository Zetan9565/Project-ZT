using UnityEngine;

namespace ZetanStudio.BehaviourTree
{
    [System.Serializable]
    public sealed class SharedVector2 : SharedVariable<Vector2>
    {
        /// <summary>
        /// 运行过程中赋值请使用<see cref="SharedVariable{T}.Value"/>属性或<see cref="SharedVariable{T}.SetValue(object)"/>方法，
        /// 如果直接把泛型值赋值给该对象，它会被覆盖，泛型值赋值只是为了方便声明变量的初始值
        /// </summary>
        /// <param name="value">泛型值</param>
        public static implicit operator SharedVector2(Vector2 value)
        {
            return new SharedVector2() { value = value };
        }
    }
}