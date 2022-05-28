using System.Collections.Generic;
using UnityEngine;

namespace ZetanStudio.BehaviourTree.Nodes
{
    public class SelectVector3 : Action
    {
        [Label("随机选取")]
        public SharedBool random;
        [Label("结果池")]
        public SharedVector3List list = new List<Vector3>();
        [Label("结果寄存器")]
        public SharedVector3 rigester;

        private int currentIndex;

        public override bool IsValid => random != null && list != null && rigester != null && random.IsValid && list.IsValid && rigester.IsValid;

        protected override NodeStates OnUpdate()
        {
            if (list.Value.Count > 0)
            {
                if (random)
                {
                    if (list.Value.Count == 1)
                        currentIndex = 0;
                    else
                    {
                        int indexBef = currentIndex;
                        while (currentIndex == indexBef)
                        {
                            currentIndex = Random.Range(0, list.Value.Count);
                        }
                    }
                }
                else currentIndex = (currentIndex + 1) % list.Value.Count;
                if (currentIndex >= 0 && currentIndex < list.Value.Count)
                    rigester.Value = list.Value[currentIndex];
                return NodeStates.Success;
            }
            return NodeStates.Failure;
        }

        protected override void OnReset()
        {
            currentIndex = 0;
        }
    }
}