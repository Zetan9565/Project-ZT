using System;
using System.Collections.Generic;
using UnityEngine;

namespace ZetanStudio.BehaviourTree
{
    [DisallowMultipleComponent]
    public class BehaviourExecutor : MonoBehaviour
    {
        [SerializeField, ObjectDropDown(typeof(BehaviourTree))]
        protected BehaviourTree behaviour;
        public BehaviourTree Behaviour => behaviour;

        public bool startOnStart = true;
        public bool restartOnComplete;
        public bool resetOnRestart;

        protected bool isRuntimeMode;

        [SerializeReference]
        private List<SharedVariable> presetVariables = new List<SharedVariable>();

        private void Start()
        {
            if (behaviour) Prepare(startOnStart);
        }

        private void Prepare(bool execute)
        {
            if (!behaviour.IsInstance) behaviour = behaviour.GetInstance();
            behaviour.Init(this);
            if (!isRuntimeMode) behaviour.PresetVariables(presetVariables);
            if (execute) behaviour.Execute();
        }

        private void Update()
        {
            if (behaviour) behaviour.Execute();
            if (restartOnComplete && behaviour.IsDone) behaviour.Restart(resetOnRestart);
        }

        public virtual void SetBehaviour(BehaviourTree tree, bool executeImmediate)
        {
            behaviour = tree;
            if (executeImmediate && behaviour) Prepare(true);
        }

        public void Restart()
        {
            if (behaviour) behaviour.Restart(resetOnRestart);
        }

        #region 碰撞器事件
        public void OnCollisionEnter(Collision collision)
        {
            if (behaviour && behaviour.IsInstance) behaviour.OnCollisionEnter(collision);
        }
        public void OnCollisionStay(Collision collision)
        {
            if (behaviour && behaviour.IsInstance) behaviour.OnCollisionStay(collision);
        }
        public void OnCollisionExit(Collision collision)
        {
            if (behaviour && behaviour.IsInstance) behaviour.OnCollisionExit(collision);
        }

        public void OnCollisionEnter2D(Collision2D collision)
        {
            if (behaviour && behaviour.IsInstance) behaviour.OnCollisionEnter2D(collision);
        }
        public void OnCollisionStay2D(Collision2D collision)
        {
            if (behaviour && behaviour.IsInstance) behaviour.OnCollisionStay2D(collision);
        }
        public void OnCollisionExit2D(Collision2D collision)
        {
            if (behaviour && behaviour.IsInstance) behaviour.OnCollisionExit2D(collision);
        }
        #endregion

        #region 触发器事件
        public void OnTriggerEnter(Collider other)
        {
            if (behaviour && behaviour.IsInstance) behaviour.OnTriggerEnter(other);
        }
        public void OnTriggerStay(Collider other)
        {
            if (behaviour && behaviour.IsInstance) behaviour.OnTriggerStay(other);
        }
        public void OnTriggerExit(Collider other)
        {
            if (behaviour && behaviour.IsInstance) behaviour.OnTriggerExit(other);
        }

        public void OnTriggerEnter2D(Collider2D collision)
        {
            if (behaviour && behaviour.IsInstance) behaviour.OnTriggerEnter2D(collision);
        }
        public void OnTriggerStay2D(Collider2D collision)
        {
            if (behaviour && behaviour.IsInstance) behaviour.OnTriggerStay2D(collision);
        }
        public void OnTriggerExit2D(Collider2D collision)
        {
            if (behaviour && behaviour.IsInstance) behaviour.OnTriggerExit2D(collision);
        }
        #endregion

        private void OnDestroy()
        {
            if (BehaviourManager.Instance) BehaviourManager.Instance.Remove(this);
        }

#if UNITY_EDITOR
        public Type GetVariableTypeAtIndex(int index)
        {
            if (index < 0 || index > presetVariables.Count) return null;
            return presetVariables[index].GetType();
        }
#endif
    }
}