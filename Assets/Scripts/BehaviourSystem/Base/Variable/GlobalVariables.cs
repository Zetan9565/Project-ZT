using System;
using System.Collections.Generic;
using UnityEngine;

namespace ZetanStudio.BehaviourTree
{
    public sealed class GlobalVariables : ScriptableObject, ISharedVariableHandler
    {
        [SerializeReference]
        private List<SharedVariable> variables;
        public List<SharedVariable> Variables => variables;

        public bool IsInstance { get; private set; }

        public GlobalVariables()
        {
            variables = new List<SharedVariable>();
        }

        public GlobalVariables GetInstance()
        {
            GlobalVariables global = Instantiate(this);
            global.IsInstance = true;
            return global;
        }

        public SharedVariable GetVariable(string name)
        {
            return Variables.Find(x => x.name == name);
        }
        public bool TryGetVariable(string name, out SharedVariable value)
        {
            value = GetVariable(name);
            return value != null;
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
                Debug.LogError("尝试对未实例化的全局变量赋值");
                return false;
            }
            var variable = Variables.Find(x => x.name == name);
            if (variable != null)
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
            var variable = Variables.Find(x => x.name == name);
            if (variable != null)
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

        public void PresetVariables(List<SharedVariable> variables)
        {
            foreach (var preVar in variables)
            {
                SharedVariable variable = Variables.Find(x => x.name == preVar.name);
                if (variable != null) variable.SetValue(preVar.GetValue());
            }
        }
    }
}