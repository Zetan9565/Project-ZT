using System.Collections.Generic;
using UnityEngine;

namespace ZetanStudio.BehaviourTree
{
    public class SharedGameObjectList : SharedVariable<List<GameObject>>
    {
        /// <summary>
        /// 运行过程中赋值请使用Value属性或SetValue方法，如果直接把泛型值赋值给该对象，它会被覆盖，泛型值赋值只是为了方便声明变量的初始值
        /// </summary>
        /// <param name="value">泛型值</param>
        public static implicit operator SharedGameObjectList(List<GameObject> value)
        {
            return new SharedGameObjectList() { value = value };
        }
    }
}