using System;
using System.Collections.Generic;
using System.Reflection;
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

        [SerializeReference]
        private Entry entry;
        public Entry Entry => entry;

        [SerializeReference]
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
        public bool TryGetVariable<T>(string name, out SharedVariable<T> variable)
        {
            variable = null;
            if (KeyedVariables.TryGetValue(name, out var find))
            {
                variable = find as SharedVariable<T>;
                return true;
            }
            else return false;
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
        public bool ReplaceVariable<T>(string name, SharedVariable<T> variable)
        {
            if (!IsInstance)
            {
                Debug.LogError("尝试替换未实例化的局部变量");
                return false;
            }
            if (KeyedVariables.ContainsKey(name))
            {
                Traverse(entry, n =>
                {
                    Type type = n.GetType();
                    foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
                    {
                        if (field.FieldType.Equals(typeof(SharedVariable<T>)))
                        {
                            if (name == (field.GetValue(n) as SharedVariable<T>).name)
                                field.SetValue(n, variable);
                        }
                    }
                });
                return true;
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
                Debug.LogError("尝试初始化未实例化的行为树: " + executor.gameObject.name);
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
            Nodes.ForEach(n => n.Init(this));
            executedComposites = new List<Composite>();
            compositesMap = new HashSet<Composite>();
        }

        public void PresetVariables(List<SharedVariable> variables)
        {
            if (!IsInstance)
            {
                Debug.LogError($"尝试预设未实例化的行为树：{(Executor ? $"{Executor.gameObject.name}." : string.Empty)}{name}");
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
            if (!IsPaused)
            {
                bool abort = false;
                for (int i = 0; i < executedComposites.Count; i++)
                {
                    Composite composite = executedComposites[i];
                    if (composite.CheckConditionalAbort())
                    {
                        executedComposites.RemoveAt(i);
                        compositesMap.Remove(composite);
                        abort = true;
                        break;
                    }
                }
                if (entry.State == NodeStates.Inactive || entry.State == NodeStates.Running || abort)
                {
                    if (!entry.IsStarted) Traverse(entry, n => n.OnBehaviourStart());
                    ExecutionState = entry.Evaluate();
                    switch (ExecutionState)
                    {
                        case NodeStates.Success:
                        case NodeStates.Failure:
                            ExecutionTimes++;
                            Traverse(entry, n => n.OnBehaviourEnd());
                            break;
                    }
                }
            }
            return ExecutionState;
        }

        public void Restart(bool reset = false)
        {
            if (!IsInstance)
            {
                Debug.LogError($"尝试重启未实例化的行为树：{(Executor ? $"{Executor.gameObject.name}." : string.Empty)}{name}");
                return;
            }
            if (!entry)
            {
                Debug.LogError($"尝试重启空的行为树：{(Executor ? $"{Executor.gameObject.name}." : string.Empty)}{name}");
                return;
            }
            if (reset) Reset_();
            else entry.Reset();
            executedComposites = new List<Composite>();
            compositesMap = new HashSet<Composite>();
            Traverse(entry, n => n.OnBehaviourRestart());
            Execute();
        }
        public void Pause(bool paused)
        {
            Traverse(entry, n => n.Pause(paused));
            IsPaused = paused;
        }
        public void Reset_()
        {
            Traverse(entry, n => n.Reset());
        }
        private List<Composite> executedComposites;
        private HashSet<Composite> compositesMap;
        public void OnCompositeEvaluate(Composite composite)
        {
            if (!compositesMap.Contains(composite))
            {
                executedComposites.Add(composite);
                compositesMap.Add(composite);
            }
        }

        #region Unity回调
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

        public void OnDrawGizmos()
        {
            Traverse(entry, n => n.OnDrawGizmos());
        }
        public void OnDrawGizmosSelected()
        {
            Traverse(entry, n => n.OnDrawGizmosSelected());
        }
        #endregion
        #endregion

        #region 结点搜索相关
        public Node FindParent(Node child, bool nonTraverse = false)
        {
            Node parent = null;
            if (nonTraverse) parent = nodes.Find(n => n.GetChildren().Contains(child));
            else Traverse(entry, n =>
            {
                if (n.GetChildren().Contains(child))
                {
                    parent = n;
                    return true;
                }
                return false;
            });
            return parent;
        }
        public Node FindParent(Node child, out int childIndex)
        {
            Node parent = null;
            int index = -1;
            Traverse(entry, n =>
            {
                var children = n.GetChildren();
                for (int i = 0; i < children.Count; i++)
                {
                    if (children[i] == child)
                    {
                        parent = n;
                        index = i;
                        return true;
                    }
                }
                return false;
            });
            childIndex = index;
            return parent;
        }

        public Node FindNode(string name)
        {
            return nodes.Find(x => x.name == name);
        }
        public Node FindNode(Type type)
        {
            return nodes.Find(x => x.GetType() == type);
        }
        public T FindNode<T>() where T : Node
        {
            return nodes.Find(x => x.GetType() == typeof(T)) as T;
        }

        public List<Node> FindNodes(Type type)
        {
            return nodes.FindAll(x => x.GetType() == type);
        }
        public List<T> FindNodes<T>() where T : Node
        {
            return nodes.FindAll(x => x.GetType() == typeof(T)).ConvertAll(x => x as T);
        }
        #endregion

        public BehaviourTree()
        {
            entry = new Entry() { name = "(0) Entry" };
            nodes = new List<Node>() { entry };
            variables = new List<SharedVariable>();
        }

        /// <summary>
        /// 从指定结点开始遍历一遍行为树
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

        /// <summary>
        /// 从指定结点开始遍历行为树
        /// </summary>
        /// <param name="node">指定的结点</param>
        /// <param name="onAccess">带终止条件的结点访问回调(返回值决定是否终止）</param>
        public static void Traverse(Node node, Func<Node, bool> onAccess)
        {
            if (node)
            {
                if (onAccess.Invoke(node)) return;
                node.GetChildren().ForEach(n => Traverse(n, onAccess));
            }
        }

        public bool Reachable(Node node)
        {
            bool reachable = false;
            Traverse(entry, (n) =>
            {
                reachable = n == node;
                return reachable;
            });
            return reachable;
        }
        #region EDITOR
#if UNITY_EDITOR
        /// <summary>
        /// 用于在编辑器中增加结点，不应在游戏逻辑中使用
        /// </summary>
        /// <param name="newNode">增加的结点</param>
        public void AddNode(Node newNode)
        {
            if (nodes.Contains(newNode)) return;
            UnityEditor.Undo.RegisterCompleteObjectUndo(this, "新增结点");
            nodes.Add(newNode);
            if (!IsInstance && !IsRuntime)
            {
                UnityEditor.EditorUtility.SetDirty(this);
            }
        }

        /// <summary>
        /// 用于在编辑器删除结点，不应在游戏逻辑中使用
        /// </summary>
        /// <param name="node"></param>
        public void DeleteNode(Node node)
        {
            UnityEditor.Undo.RegisterCompleteObjectUndo(this, "删除结点");
            nodes.Remove(node);
            if (!IsInstance && !IsRuntime)
            {
                UnityEditor.EditorUtility.SetDirty(this);
            }
        }

        /// <summary>
        /// 用于在编辑器中获取运行时行为树，不应在游戏逻辑中使用
        /// </summary>
        /// <returns>运行时行为树</returns>
        public static BehaviourTree GetRuntimeTree()
        {
            BehaviourTree tree = CreateInstance<BehaviourTree>();
            tree.entry.ConvertToRuntime();
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
            localTree.nodes.ForEach(x => x.ConvertToLocal());
            return localTree;
        }
#endif
        #endregion
    }
}