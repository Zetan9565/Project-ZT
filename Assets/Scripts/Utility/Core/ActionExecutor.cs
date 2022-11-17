using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ZetanStudio
{
    using SavingSystem;

    [AddComponentMenu("Zetan Studio/事件/行为执行器")]
    public class ActionExecutor : MonoBehaviour
    {
        [SerializeField]
        private string _ID;
        public string ID => _ID;
        [SerializeField]
        private float endDelayTime;
        public float EndDelayTime => endDelayTime;

        [SerializeField]
        private UnityEvent onBegin = new UnityEvent();
        [System.Serializable]
        private class ExecutingEvent : UnityEvent<float> { }
        [SerializeField]
        private ExecutingEvent onExecuting = new ExecutingEvent();
        [SerializeField]
        private UnityEvent onEnd = new UnityEvent();
        [SerializeField]
        private UnityEvent onUndoBegin = new UnityEvent();
        [SerializeField]
        private UnityEvent onUndo = new UnityEvent();

        public bool IsExecuting { get; private set; }

        private bool isDone;
        public bool IsDone
        {
            get
            {
                return isDone;
            }
            set
            {
                if (value && !isDone) End();
                else if (!value && isDone) Undo();
                isDone = value;
            }
        }

        public float ExecutionTime { get; private set; }

        public void Execute(float endDelay = -1, float startTime = 0)
        {
            if (IsDone) return;
            //Debug.Log($"end: {endDelay}, start: {startTime}");
            onBegin?.Invoke();
            if (endDelay <= 0 || endDelay <= startTime) End();
            else
            {
                endDelayTime = endDelay;
                ExecutionTime = startTime;
                IsExecuting = true;
            }
        }

        public void End()
        {
            if (IsDone) return;
            onEnd?.Invoke();
            ExecutionTime = endDelayTime;
            IsExecuting = false;
            isDone = true;
        }

        public void UndoEnd()
        {
            if (!IsDone) return;
            onUndo?.Invoke();
            ExecutionTime = 0;
            IsExecuting = false;
            isDone = false;
        }

        public void Undo(float undoneDelay = -1, float startTime = 0)
        {
            if (!IsDone) return;
            onUndoBegin?.Invoke();
            if (undoneDelay <= 0 || undoneDelay <= startTime) UndoEnd();
            else
            {
                IsExecuting = true;
                ExecutionTime = undoneDelay - startTime;
            }
            isDone = false;
        }

        private void Update()
        {
            if (IsExecuting && !IsDone)
            {
                ExecutionTime += Time.deltaTime;
                onExecuting?.Invoke(ExecutionTime);
                if (ExecutionTime >= endDelayTime)
                {
                    End();
                }
            }
            if (IsExecuting && IsDone)
            {
                ExecutionTime -= Time.deltaTime;
                onExecuting?.Invoke(ExecutionTime);
                if (ExecutionTime <= 0)
                {
                    UndoEnd();
                }
            }
        }
    }
    public class ActionStack
    {
        private readonly static Stack<ActionStackData> actionStacks = new Stack<ActionStackData>();

        public static void Push(ActionExecutor executor, float delay = -1, float startTime = 0)
        {
            executor.Execute(delay, startTime);
            actionStacks.Push(new ActionStackData(executor, ActionType.Execute));
            //Debug.Log("Push: " + executor.ID);
        }

        public static void Push(ActionExecutor executor, ActionType actionType, float delay = -1, float startTime = 0)
        {
            if (actionType == ActionType.Execute) executor.Execute(delay, startTime);
            else executor.Undo(delay, startTime);
            actionStacks.Push(new ActionStackData(executor, actionType));
            //Debug.Log("Push: " + executor.ID);
        }

        public static ActionExecutor Pop()
        {
            var ase = actionStacks.Pop();
            if (ase.actionType == ActionType.Execute) ase.executor.Undo(ase.executor.ExecutionTime);
            else ase.executor.Execute(ase.executor.ExecutionTime);
            //Debug.Log("Pop: " + ase.executor.ID);
            return ase.executor;
        }

        public static ActionStackData[] ToArray()
        {
            return actionStacks.ToArray();
        }

        public static void Clear(bool popAll = false)
        {
            if (!popAll) actionStacks.Clear();
            else while (actionStacks.Count > 0)
                    Pop();
        }

        [SaveMethod]
        public static void SaveData(SaveData saveData)
        {
            var data = saveData.Write("actionData", new GenericData());
            var array = ToArray();
            for (int i = array.Length - 1; i >= 0; i--)
            {
                var stackElement = array[i];
                var ad = new GenericData();
                ad["ID"] = stackElement.executor.ID;
                ad["isExecuting"] = stackElement.executor.IsExecuting;
                ad["executionTime"] = stackElement.executor.ExecutionTime;
                ad["endDelayTime"] = stackElement.executor.EndDelayTime;
                ad["isDone"] = stackElement.executor.IsDone;
                ad["actionType"] = (int)stackElement.actionType;
                data["ID"] = ad;
            }
        }

        [LoadMethod]
        public static void LoadData(SaveData saveData)
        {
            var actions = Object.FindObjectsOfType<ActionExecutor>();
            foreach (var ad in saveData.ReadData("actionData").ReadDataDict())
                foreach (var action in actions)
                    if (action.ID == ad.Key)
                        Push(action, (ActionType)ad.Value.ReadInt("actionType"), ad.Value.ReadFloat("endDelayTime"), ad.Value.ReadFloat("executionTime"));
        }
    }

    public class ActionStackData
    {
        public ActionExecutor executor;
        public ActionType actionType;

        public ActionStackData(ActionExecutor executor, ActionType actionType)
        {
            this.executor = executor;
            this.actionType = actionType;
        }
    }

    public enum ActionType
    {
        Execute,
        Undo
    }
}