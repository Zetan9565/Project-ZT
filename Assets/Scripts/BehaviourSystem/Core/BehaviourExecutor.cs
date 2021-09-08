using UnityEngine;

namespace ZetanStudio.BehaviourTree
{
    public class BehaviourExecutor : MonoBehaviour
    {
        public BehaviourTree behaviour;

        public bool startOnStart;
        public bool restartOnComplete;
        public bool resetOnRestart;

        private void Start()
        {
            if (behaviour)
            {
                if (!behaviour.IsInstance) behaviour = behaviour.GetInstance();
                behaviour.Init(this);
                if (startOnStart) behaviour.Execute();
            }
        }

        public void Update()
        {
            if (behaviour && behaviour.IsStarted) behaviour.Execute();
            if (restartOnComplete && (behaviour.ExecutionState == NodeStates.Success || behaviour.ExecutionState == NodeStates.Failure))
                behaviour.Restart(resetOnRestart);
        }

        public void SetBehaviour(BehaviourTree tree)
        {
            behaviour = tree;
            if (startOnStart) Start();
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {

        }

        private void OnCollisionStay(Collision collision)
        {

        }

        private void OnCollisionExit(Collision collision)
        {

        }

        private void OnDestroy()
        {
            if (BehaviourManager.Instance) BehaviourManager.Instance.Remove(this);
        }
    }
}