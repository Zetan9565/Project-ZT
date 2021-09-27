using System;
using System.Collections.Generic;
using UnityEngine;

namespace ZetanStudio.BehaviourTree
{
    public class BehaviourExecutor : MonoBehaviour
    {
        [SerializeField]
        protected BehaviourTree behaviour;
        public BehaviourTree Behaviour => behaviour;

        public Frequency frequency = Frequency.PerFrame;
        public float interval = 0.02f;
        public bool startOnStart = true;
        public bool restartOnComplete;
        public bool resetOnRestart;

        protected bool isRuntimeMode;

        private float time;

        [SerializeReference]
        private List<SharedVariable> presetVariables = new List<SharedVariable>();

        private void Prepare(bool execute)
        {
            if (!behaviour.IsInstance) behaviour = behaviour.GetInstance();
            behaviour.Init(this);
            if (!isRuntimeMode) behaviour.PresetVariables(presetVariables);
            if (execute) behaviour.Execute();
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

        public SharedVariable GetVariable(string name)
        {
            if (!behaviour) return null;
            return behaviour.GetVariable(name);
        }
        public bool TryGetVariable<T>(string name, out SharedVariable<T> variable)
        {
            variable = null;
            if (!behaviour) return false;
            return behaviour.TryGetVariable<T>(name, out variable);
        }

        public List<SharedVariable> GetVariables(Type type)
        {
            if (!behaviour) return null;
            return behaviour.GetVariables(type);
        }
        public List<SharedVariable<T>> GetVariables<T>()
        {
            if (!behaviour) return null;
            return behaviour.GetVariables<T>();
        }

        public bool SetVariable(string name, object value)
        {
            if (!behaviour) return false;
            return behaviour.SetVariable(name, value);
        }
        public bool SetVariable<T>(string name, T value)
        {
            if (!behaviour) return false;
            return behaviour.SetVariable(name, value);
        }

        #region Unity回调
        private void Awake()
        {
            if (BehaviourManager.Instance) BehaviourManager.Instance.Add(this);
        }
        private void Start()
        {
            if (behaviour) Prepare(startOnStart);
        }

        private void Update()
        {
            if (frequency == Frequency.PerFrame && behaviour) behaviour.Execute();
            if (frequency == Frequency.FixedTime)
            {
                time += Time.deltaTime;
                if (time >= interval)
                {
                    time = 0;
                    if (behaviour) behaviour.Execute();
                }
            }
            if (restartOnComplete && behaviour.IsDone) behaviour.Restart(resetOnRestart);
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

        private void OnDrawGizmos()
        {
            if (behaviour) behaviour.OnDrawGizmos();
        }
        private void OnDrawGizmosSelected()
        {
            if (behaviour) behaviour.OnDrawGizmosSelected();
        }

        private void OnDestroy()
        {
            if (BehaviourManager.Instance) BehaviourManager.Instance.Remove(this);
        }
        #endregion

        public enum Frequency
        {
            [InspectorName("每帧")]
            PerFrame,
            [InspectorName("固定间隔")]
            FixedTime,
        }

#if UNITY_EDITOR
        public Type GetPresetVariableTypeAtIndex(int index)
        {
            if (index < 0 || index > presetVariables.Count) return null;
            return presetVariables[index].GetType();
        }
#endif
    }
}