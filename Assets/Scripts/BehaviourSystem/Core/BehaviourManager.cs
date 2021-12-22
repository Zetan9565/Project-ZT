using System;
using System.Collections.Generic;
using UnityEngine;

namespace ZetanStudio.BehaviourTree
{
    [DefaultExecutionOrder(-1)]
    public class BehaviourManager : SingletonMonoBehaviour<BehaviourManager>
    {
        public List<BehaviourExecutor> Executors { get; } = new List<BehaviourExecutor>();
        public HashSet<BehaviourExecutor> executorsMap = new HashSet<BehaviourExecutor>();

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

        public void Remove(BehaviourExecutor behaviourExecutor)
        {
            Executors.Remove(behaviourExecutor);
            executorsMap.Remove(behaviourExecutor);
        }

        public void Add(BehaviourExecutor behaviourExecutor)
        {
            if (executorsMap.Contains(behaviourExecutor)) return;
            Executors.Add(behaviourExecutor);
            executorsMap.Add(behaviourExecutor);
        }

        public SharedVariable GetVariable(string name)
        {
            return globalVariables.GetVariable(name);
        }
        public bool TryGetVariable<T>(string name, out SharedVariable<T> variable)
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

        private void OnValidate()
        {
            if (FindObjectsOfType<BehaviourManager>().Length > 1)
                Debug.LogError("存在多个激活的BehaviourManager，请删除或失活其它");
        }
#endif
    }
}