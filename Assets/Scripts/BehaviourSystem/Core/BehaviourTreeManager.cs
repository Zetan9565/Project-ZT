using System;
using System.Collections.Generic;
using UnityEngine;

namespace ZetanStudio.BehaviourTree
{
    [DefaultExecutionOrder(-1), DisallowMultipleComponent]
    public class BehaviourTreeManager : SingletonMonoBehaviour<BehaviourTreeManager>
    {
        public List<BehaviourTreeExecutor> Executors { get; } = new List<BehaviourTreeExecutor>();
        public HashSet<BehaviourTreeExecutor> executorsMap = new HashSet<BehaviourTreeExecutor>();

        [SerializeField]
        private GlobalVariables globalVariables;
        public GlobalVariables GlobalVariables => globalVariables;

        [SerializeReference]
        private List<SharedVariable> presetVariables = new List<SharedVariable>();

        private void Awake()
        {
            if (globalVariables) globalVariables = globalVariables.GetInstance();
            else globalVariables = ScriptableObject.CreateInstance<GlobalVariables>().GetInstance();
            globalVariables.PresetVariables(presetVariables);
        }

        public void Remove(BehaviourTreeExecutor behaviourExecutor)
        {
            Executors.Remove(behaviourExecutor);
            executorsMap.Remove(behaviourExecutor);
        }

        public void Add(BehaviourTreeExecutor behaviourExecutor)
        {
            if (executorsMap.Contains(behaviourExecutor)) return;
            Executors.Add(behaviourExecutor);
            executorsMap.Add(behaviourExecutor);
        }

        public SharedVariable GetVariable(string name)
        {
            return globalVariables.GetVariable(name);
        }
        public bool TryGetVariable(string name, out SharedVariable variable)
        {
            return globalVariables.TryGetVariable(name, out variable);
        }

        public List<SharedVariable> GetVariables(Type type)
        {
            return globalVariables.GetVariables(type);
        }
        public List<SharedVariable<T>> GetVariables<T>()
        {
            return globalVariables.GetVariables<T>();
        }

        public bool SetVariable(string name, object value)
        {
            return globalVariables.SetVariable(name, value);
        }
        public bool SetVariable<T>(string name, T value)
        {
            return globalVariables.SetVariable(name, value);
        }

#if UNITY_EDITOR
        public Type GetPresetVariableTypeAtIndex(int index)
        {
            if (index < 0 || index > presetVariables.Count) return null;
            else return presetVariables[index].GetType();
        }
#endif
    }
}