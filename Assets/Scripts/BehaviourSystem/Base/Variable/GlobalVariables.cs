using System;
using System.Collections.Generic;
using UnityEngine;

namespace ZetanStudio.BehaviourTree
{
    public sealed class GlobalVariables : ScriptableObject, ISharedVariableHandler
    {
        [SerializeReference]
        public List<SharedVariable> variables;
        public Dictionary<string, SharedVariable> Variables { get; private set; }

        public bool IsInstance { get; private set; }

        public GlobalVariables()
        {
            variables = new List<SharedVariable>();
            Variables = new Dictionary<string, SharedVariable>();
        }

        public GlobalVariables GetInstance()
        {
            GlobalVariables global = Instantiate(this);
            global.IsInstance = true;
            foreach (var variable in variables)
            {
                if (!Variables.ContainsKey(variable.name))
                {
                    Variables.Add(variable.name, variable);
                }
            }
            return global;
        }

        public SharedVariable GetVariable(string name)
        {
            if (Variables.TryGetValue(name, out var variable)) return variable;
            else return null;
        }

        public SharedVariable<T> GetVariable<T>(string name)
        {
            if (Variables.TryGetValue(name, out var variable)) return variable as SharedVariable<T>;
            else return null;
        }

        public List<SharedVariable> GetVariables(Type type)
        {
            List<SharedVariable> variables = new List<SharedVariable>();
            foreach (var variable in this.variables)
            {
                if (variable.GetType().Equals(type))
                    variables.Add(variable);
            }
            return variables;
        }

        public List<SharedVariable<T>> GetVariables<T>()
        {
            List<SharedVariable<T>> variables = new List<SharedVariable<T>>();
            foreach (var variable in this.variables)
            {
                if (variable is SharedVariable<T> var)
                    variables.Add(var);
            }
            return variables;
        }


        public bool SetVariable(string name, object value)
        {
            if (!IsInstance)
            {
                Debug.LogError("尝试对未实例化的局部变量赋值");
                return false;
            }
            if (Variables.TryGetValue(name, out var variable))
            {
                variable.SetValue(value);
                return true;
            }
            else return false;
        }

        public bool SetVariable<T>(string name, T value)
        {
            if (!IsInstance)
            {
                Debug.LogError("尝试对未实例化的全局变量赋值");
                return false;
            }
            if (Variables.TryGetValue(name, out var variable))
            {
                if (variable is SharedVariable<T> var)
                {
                    var.SetGenericValue(value);
                    return true;
                }
                else return false;
            }
            else return false;
        }
    }
}