using System;
using System.Collections.Generic;
using UnityEngine;

namespace ZetanStudio.BehaviourTree
{
    public class BehaviourManager : SingletonMonoBehaviour<BehaviourManager>
    {
        public List<BehaviourExecutor> Executors { get; } = new List<BehaviourExecutor>();
        public HashSet<BehaviourExecutor> executorsMap = new HashSet<BehaviourExecutor>();

        [SerializeField, ObjectDropDown(typeof(GlobalVariables))]
        private GlobalVariables globalVariables;
        public GlobalVariables GlobalVariables => globalVariables;

        [SerializeReference]
        private List<SharedVariable> presetVariables = new List<SharedVariable>();

        private void Awake()
        {
            foreach (var exe in FindObjectsOfType<BehaviourExecutor>())
            {
                Add(exe);
            }
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

        public SharedVariable GetGlobalVariable(string name)
        {
            return globalVariables.GetVariable(name);
        }

        public SharedVariable<T> GetGlobalVariable<T>(string name)
        {
            return globalVariables.GetVariable<T>(name);
        }

        public List<SharedVariable<T>> GetGlobalVariables<T>()
        {
            return globalVariables.GetVariables<T>();
        }

        public bool SetGlobalVariable<T>(string name, T value)
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