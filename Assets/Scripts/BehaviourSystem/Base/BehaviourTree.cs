using System;
using System.Collections.Generic;
using UnityEngine;

namespace ZetanStudio.BehaviourTree
{
    public sealed class BehaviourTree : ScriptableObject, ISharedVariableHandler
    {
        [SerializeField]
        private string _name;
        /// <summary>
        /// 行为树名称，不同于name，后者是行为树的资源文件名称
        /// </summary>
        public string Name => _name;

        [SerializeField, TextArea]
        private string description;
        public string Descriotion => description;

        [SerializeField]
        private Entry entry;
        public Entry Entry => entry;

        [SerializeField]
        private List<Node> nodes;
        public List<Node> Nodes => nodes;

        [SerializeReference]
        public List<SharedVariable> variables;

        #region 运行时属性
        public bool IsPaused { get; private set; }

        public bool IsStarted => entry.IsStarted;

        public bool IsInstance { get; private set; }

        public int ExecutionTimes { get; private set; }

        public NodeStates ExecutionState { get; private set; }

        public BehaviourExecutor Executor { get; private set; }
        public Dictionary<string, SharedVariable> Variables { get; private set; }
        #endregion

        #region 共享变量获取
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
        #endregion

        #region 运行时方法
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
                Debug.LogError("尝试对未实例化的局部变量赋值");
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

        public BehaviourTree GetInstance()
        {
            BehaviourTree tree = Instantiate(this);
            tree.entry = entry.GetInstance() as Entry;
            tree.nodes = new List<Node>();
            Traverse(tree.entry, n => { tree.nodes.Add(n); });
            tree.IsInstance = true;
            return tree;
        }

        public void Init(BehaviourExecutor executor)
        {
            Executor = executor;
            Variables = new Dictionary<string, SharedVariable>();
            foreach (var variable in variables)
            {
                if (!Variables.ContainsKey(variable.name))
                {
                    Variables.Add(variable.name, variable);
                }
            }
            Traverse(entry, (n) => n.Init(this));
        }

        public NodeStates Execute()
        {
            if (!IsInstance)
            {
                Debug.LogError($"尝试执行未实例化的行为树：{(Executor ? $"{Executor.gameObject.name}." : string.Empty)}{name}");
                return NodeStates.Inactive;
            }
            if (!entry)
            {
                Debug.LogError($"尝试执行空的行为树：{(Executor ? $"{Executor.gameObject.name}." : string.Empty)}{name}");
                return NodeStates.Inactive;
            }
            if (!IsPaused && (entry.State == NodeStates.Inactive || entry.State == NodeStates.Running))
            {
                ExecutionState = entry.Evaluate();
                switch (ExecutionState)
                {
                    case NodeStates.Inactive:
                        break;
                    case NodeStates.Success:
                        ExecutionTimes++;
                        break;
                    case NodeStates.Failure:
                        ExecutionTimes++;
                        break;
                    case NodeStates.Running:
                        break;
                    default:
                        break;
                }
            }
            return ExecutionState;
        }

        public void Restart(bool reset)
        {
            if (reset) Reset();
            else entry.Reset();
            Execute();
        }
        public void Pause(bool paused)
        {
            Traverse(entry, n => n.Pause(paused));
            IsPaused = paused;
        }
        public void Reset()
        {
            Traverse(entry, n => n.Reset());
        }
        #endregion

        public BehaviourTree()
        {
            nodes = new List<Node>();
            variables = new List<SharedVariable>();
        }

        public Node FindParent(Node child)
        {
            return nodes.Find(x => x.GetChildren().Contains(child));
        }

        /// <summary>
        /// 从指定结点开始遍历行为树
        /// </summary>
        /// <param name="node">指定的结点</param>
        /// <param name="onAccess">结点访问回调</param>
        public static void Traverse(Node node, Action<Node> onAccess)
        {
            if (node)
            {
                onAccess?.Invoke(node);
                node.GetChildren().ForEach((n) => Traverse(n, onAccess));
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// 用于在编辑器中增加结点，不应在游戏逻辑中使用
        /// </summary>
        /// <param name="newNode">增加的结点</param>
        public void AddNode(Node newNode)
        {
            if (newNode is Entry entry) CreateEntry(entry);
            else nodes.Add(newNode);
            if (!IsInstance)
            {
                UnityEditor.AssetDatabase.AddObjectToAsset(newNode, this);
                UnityEditor.AssetDatabase.SaveAssets();
            }
        }

        /// <summary>
        /// 用于在编辑器删除结点，不应在游戏逻辑中使用
        /// </summary>
        /// <param name="node"></param>
        public void DeleteNode(Node node)
        {
            nodes.Remove(node);
            if (!IsInstance)
            {
                UnityEditor.AssetDatabase.RemoveObjectFromAsset(node);
                UnityEditor.AssetDatabase.SaveAssets();
            }
        }

        private void CreateEntry(Entry entry)
        {
            if (this.entry)
            {
                if (this.entry.GetChildren().Count > 0)
                    entry.AddChild(this.entry.GetChildren()[0]);
                if (!IsInstance)
                {
                    UnityEditor.AssetDatabase.RemoveObjectFromAsset(entry);
                    UnityEditor.AssetDatabase.SaveAssets();
                }
            }
            this.entry = entry;
            nodes.Add(entry);
        }
#endif
    }
}