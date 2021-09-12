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
        private List<SharedVariable> variables;
        public List<SharedVariable> Variables => variables;

        #region 运行时属性
        public bool IsPaused { get; private set; }

        public bool IsStarted => entry.IsStarted;

        public bool IsDone => entry.IsDone;

        public bool IsInstance { get; private set; }

        [SerializeField]
        private bool isRuntime;//要暴露给Unity，要不然每运行一次就会被重置
        public bool IsRuntime => isRuntime;

        public int ExecutionTimes { get; private set; }

        public NodeStates ExecutionState { get; private set; }

        public BehaviourExecutor Executor { get; private set; }
        public Dictionary<string, SharedVariable> KeyedVariables { get; private set; }
        #endregion

        #region 共享变量获取
        public SharedVariable GetVariable(string name)
        {
            if (KeyedVariables.TryGetValue(name, out var variable)) return variable;
            else return null;
        }
        public SharedVariable<T> GetVariable<T>(string name)
        {
            if (KeyedVariables.TryGetValue(name, out var variable)) return variable as SharedVariable<T>;
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
            if (KeyedVariables.TryGetValue(name, out var variable))
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
            if (KeyedVariables.TryGetValue(name, out var variable))
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
            BehaviourTree tree;
            if (isRuntime) tree = this;
            else tree = Instantiate(this);
            tree.entry = entry.GetInstance() as Entry;
            if (!isRuntime)
            {
                tree.nodes = new List<Node>();
                Traverse(tree.entry, n => { tree.nodes.Add(n); });
            }
            else
            {
                for (int i = 0; i < tree.nodes.Count; i++)
                {
                    tree.nodes[i] = tree.nodes[i].GetInstance();
                }
            }
            tree.IsInstance = true;
            return tree;
        }

        public void Init(BehaviourExecutor executor)
        {
            if (!IsInstance)
            {
                Debug.LogError("尝试初始化未实例化的行为树");
                return;
            }
            Executor = executor;
            KeyedVariables = new Dictionary<string, SharedVariable>();
            foreach (var variable in variables)
            {
                if (!KeyedVariables.ContainsKey(variable.name))
                {
                    KeyedVariables.Add(variable.name, variable);
                }
            }
            Traverse(entry, (n) => n.Init(this));
        }

        public void PresetVariables(List<SharedVariable> variables)
        {
            if (!IsInstance)
            {
                Debug.LogError("尝试预设未实例化的行为树");
                return;
            }
            foreach (var variable in variables)
            {
                if (KeyedVariables.TryGetValue(variable.name, out var keyedVar) && keyedVar.GetType() == variable.GetType())
                    keyedVar.SetValue(variable.GetValue());
            }
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

        #region 碰撞器事件
        public void OnCollisionEnter(Collision collision)
        {
            if (IsInstance) Traverse(entry, n => n.OnCollisionEnter(collision));
        }
        public void OnCollisionStay(Collision collision)
        {
            if (IsInstance) Traverse(entry, n => n.OnCollisionStay(collision));
        }
        public void OnCollisionExit(Collision collision)
        {
            if (IsInstance) Traverse(entry, n => n.OnCollisionExit(collision));
        }

        public void OnCollisionEnter2D(Collision2D collision)
        {
            if (IsInstance) Traverse(entry, n => n.OnCollisionEnter2D(collision));
        }
        public void OnCollisionStay2D(Collision2D collision)
        {
            if (IsInstance) Traverse(entry, n => n.OnCollisionStay2D(collision));
        }
        public void OnCollisionExit2D(Collision2D collision)
        {
            if (IsInstance) Traverse(entry, n => n.OnCollisionExit2D(collision));
        }
        #endregion

        #region 触发器事件
        public void OnTriggerEnter(Collider other)
        {
            if (IsInstance) Traverse(entry, n => n.OnTriggerEnter(other));
        }
        public void OnTriggerStay(Collider other)
        {
            if (IsInstance) Traverse(entry, n => n.OnTriggerStay(other));
        }
        public void OnTriggerExit(Collider other)
        {
            if (IsInstance) Traverse(entry, n => n.OnTriggerExit(other));
        }

        public void OnTriggerEnter2D(Collider2D collision)
        {
            if (IsInstance) Traverse(entry, n => n.OnTriggerEnter2D(collision));
        }
        public void OnTriggerStay2D(Collider2D collision)
        {
            if (IsInstance) Traverse(entry, n => n.OnTriggerStay2D(collision));
        }
        public void OnTriggerExit2D(Collider2D collision)
        {
            if (IsInstance) Traverse(entry, n => n.OnTriggerExit2D(collision));
        }
        #endregion

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
                node.GetChildren().ForEach(n => Traverse(n, onAccess));
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
            if (!IsInstance && !IsRuntime)
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
            if (!IsInstance && !IsRuntime)
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
                if (!IsInstance && !IsRuntime)
                {
                    UnityEditor.AssetDatabase.RemoveObjectFromAsset(entry);
                    UnityEditor.AssetDatabase.SaveAssets();
                }
            }
            this.entry = entry;
            nodes.Add(entry);
        }

        /// <summary>
        /// 用于在编辑器中获取运行时行为树，不应在游戏逻辑中使用
        /// </summary>
        /// <returns>运行时行为树</returns>
        public static BehaviourTree GetRuntimeTree()
        {
            BehaviourTree tree = CreateInstance<BehaviourTree>();
            tree.entry = Node.GetRuntimeNode(typeof(Entry)) as Entry;
            tree.entry.name = "(0) Entry(R)";
            tree.entry.guid = UnityEditor.GUID.Generate().ToString();
            tree.nodes.Add(tree.entry);
            tree.isRuntime = true;
            return tree;
        }
        /// <summary>
        /// 用于在编辑器将运行时行为树本地化时做准备，不应在游戏逻辑中使用
        /// </summary>
        /// <param name="runtimeTree">运行时行为树</param>
        /// <returns>准备好本地化的行为树</returns>
        public static BehaviourTree ConvertToLocal(BehaviourTree runtimeTree)
        {
            if (runtimeTree.IsInstance || !runtimeTree.isRuntime) return null;
            BehaviourTree localTree = Instantiate(runtimeTree);
            localTree.isRuntime = false;
            Traverse(localTree.entry, n => localTree.nodes.Remove(n));
            for (int i = 0; i < localTree.nodes.Count; i++)
            {
                localTree.nodes[i] = localTree.nodes[i].ConvertToLocal();
            }
            localTree.entry = localTree.entry.ConvertToLocal() as Entry;
            Traverse(localTree.entry, n => localTree.nodes.Add(n));
            return localTree;
        }
#endif
    }
}