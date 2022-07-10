using System;
using System.Collections.Generic;
//using System.Reflection;
using UnityEngine;
using ZetanStudio.BehaviourTree.Nodes;

namespace ZetanStudio.BehaviourTree
{
    public sealed class BehaviourTree : ScriptableObject, ISharedVariableHandler
    {
        [SerializeField]
        private string _name;
        /// <summary>
        /// 行为树名称，不同于name，后者是行为树的资源文件名称
        /// </summary>
        public string Name => string.IsNullOrEmpty(_name) ? "(未命名)" : _name;

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

        #region 运行时变量
        public bool IsPaused { get; private set; }

        public bool IsStarted => entry.IsStarted;

        public bool IsDone => entry.IsDone;

        public bool IsInstance { get; private set; }

        [SerializeField]
        private bool scenceOnly;//要暴露给Unity，要不然每运行一次就会被重置
        public bool ScenceOnly => scenceOnly;

        public int ExecutionTimes { get; private set; }

        public NodeStates ExecutionState { get; private set; }

        public BehaviourTreeExecutor Executor { get; private set; }

        private readonly SortedSet<Composite> evaluatedComposites = new SortedSet<Composite>(Node.Comparer.Default);
        #endregion

        #region 共享变量获取
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
        #endregion

        #region 运行时方法
        public bool SetVariable(string name, object value)
        {
            if (!IsInstance)
            {
                Debug.LogError("尝试对未实例化的局部变量赋值");
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
                Debug.LogError("尝试对未实例化的局部变量赋值");
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
        public bool ReplaceVariable<T>(string name, SharedVariable<T> variable)
        {
            if (!IsInstance)
            {
                Debug.LogError("尝试替换未实例化的局部变量");
                return false;
            }
            if (Variables.Exists(x => x.name == name))
            {
                Traverse(entry, n =>
                {
                    Type type = n.GetType();
                    foreach (var field in type.GetFields(ZetanUtility.CommonBindingFlags))
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
            if (!Application.isPlaying)
            {
                Debug.LogError("尝试在编辑模式实例化行为树：" + Name);
                return null;
            }
            BehaviourTree tree;
            if (scenceOnly) tree = this;
            else tree = Instantiate(this);
            //Traverse(tree.entry, n => n.Instantiate());
            tree.nodes.ForEach(n => n.Instantiate());
            tree.IsInstance = true;
            return tree;
        }

        public void Init(BehaviourTreeExecutor executor)
        {
            if (!IsInstance)
            {
                Debug.LogError("尝试初始化未实例化的行为树: " + executor.gameObject.name);
                return;
            }
            Executor = executor;
            nodes.ForEach(InitNode);
            SortPriority();
            evaluatedComposites.Clear();
        }
        public void InitNode(Node node)
        {
            node.Init(this);
            node.OnEvaluated += OnNodeEvaluated;
        }
        public void SortPriority()
        {
            int i = 0;
            Traverse(entry, n => n.priority = i++);
        }

        public void PresetVariables(List<SharedVariable> variables)
        {
            if (!IsInstance)
            {
                Debug.LogError($"尝试预设未实例化的行为树：{(Executor ? $"{Executor.gameObject.name}." : string.Empty)}{name}");
                return;
            }
            foreach (var preVar in variables)
            {
                SharedVariable variable = Variables.Find(x => x.name == preVar.name);
                if (variable != null) variable.SetValue(preVar.GetValue());
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
                if (CheckConditionalAbort() || entry.State == NodeStates.Inactive || entry.State == NodeStates.Running)
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
            evaluatedComposites.Clear();
            nodes.ForEach(n => n.OnBehaviourRestart());
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

        private void OnNodeEvaluated(Node node)
        {
            if (node is Composite composite) evaluatedComposites.Add(composite);
        }
        private bool CheckConditionalAbort()
        {
            foreach (var composite in evaluatedComposites)
            {
                if (composite.CheckConditionalAbort() is Conditional conditional)
                {
                    PostConditionalAbort(conditional);
                    evaluatedComposites.Clear();
                    return true;
                }
            }
            return false;
        }
        private void PostConditionalAbort(Conditional conditional)
        {
            using var cEnum = evaluatedComposites.GetEnumerator();
            //已进入评估的Composite优先级更高才有可能包含此Conditional并接收它发起的ConditionalAbort，
            //换句话说，此Conditional已进入评估，但包含它的Composite未进入评估，是不可能发生的。
            while (cEnum.MoveNext() && cEnum.Current && cEnum.Current.ComparePriority(conditional))
            {
                if (cEnum.Current.ReciveConditionalAbort(conditional))
                    if (cEnum.Current.AbortLowerPriority)//接收了Abort，而且是中止低优先级，则向更先进入评估的Composite发起低优先中止
                        PostLowerPriorityAbort(cEnum.Current);
            }
        }
        private void PostLowerPriorityAbort(Composite composite)
        {
            using var cEunm = evaluatedComposites.GetEnumerator();
            //已进入评估的Composite优先级更高才有可能包含此Composite并接收它发起的LowerPriorityAbort
            while (cEunm.MoveNext() && cEunm.Current && cEunm.Current.ComparePriority(composite))
            {
                cEunm.Current.ReciveLowerPriorityAbort(composite);
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
        /// <summary>
        /// 在此树中查找某结点的父结点
        /// </summary>
        /// <param name="child">结点</param>
        /// <param name="nonTraverse">遍历结点列表而不是树</param>
        /// <returns>找到的父结点</returns>
        public ParentNode FindParent(Node child, bool nonTraverse = false)
        {
            ParentNode parent = null;
            if (nonTraverse) parent = nodes.Find(n => n is ParentNode p && p.GetChildren().Contains(child)) as ParentNode;
            else Traverse(entry, n =>
            {
                if (n is ParentNode p && p.GetChildren().Contains(child))
                {
                    parent = p;
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
                if (n is ParentNode p)
                {
                    var children = p.GetChildren();
                    for (int i = 0; i < children.Count; i++)
                    {
                        if (children[i] == child)
                        {
                            parent = n;
                            index = i;
                            return true;
                        }
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
                if (node is ParentNode parent) parent.GetChildren().ForEach(n => Traverse(n, onAccess));
            }
        }

        /// <summary>
        /// 从指定结点开始遍历行为树
        /// </summary>
        /// <param name="node">指定的结点</param>
        /// <param name="onAccess">带终止条件的结点访问回调(返回值决定是否终止）</param>
        /// <returns>是否在遍历时产生终止</returns>
        public static bool Traverse(Node node, Func<Node, bool> onAccess)
        {
            if (onAccess != null && node)
            {
                if (onAccess(node)) return true;
                if (node is ParentNode parent)
                    foreach (Node n in parent.GetChildren())
                        if (Traverse(n, onAccess))
                            return true;
            }
            return false;
        }

        public bool Reachable(Node node)
        {
            return Reachable(entry, node);
        }
        public static bool Reachable(Node from, Node to)
        {
            bool reachable = false;
            Traverse(from, n =>
            {
                reachable = n == to;
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
            nodes.Add(newNode);
            if (!IsInstance && !ScenceOnly)
            {
                UnityEditor.EditorUtility.SetDirty(this);
                UnityEditor.AssetDatabase.SaveAssetIfDirty(this);
            }
            else SortPriority();
        }

        /// <summary>
        /// 用于在编辑器删除结点，不应在游戏逻辑中使用
        /// </summary>
        /// <param name="node"></param>
        public void DeleteNode(Node node)
        {
            nodes.Remove(node);
            if (!IsInstance && !ScenceOnly)
            {
                UnityEditor.EditorUtility.SetDirty(this);
                UnityEditor.AssetDatabase.SaveAssetIfDirty(this);
            }
            else
            {
                if (node is Composite conditional) evaluatedComposites.Remove(conditional);
                SortPriority();
            }
        }

        /// <summary>
        /// 用于在编辑器中获取运行时行为树，不应在游戏逻辑中使用
        /// </summary>
        /// <returns>场景型行为树</returns>
        public static BehaviourTree GetSceneOnlyTree()
        {
            BehaviourTree tree = CreateInstance<BehaviourTree>();
            tree.entry.ConvertToRuntime();
            tree.scenceOnly = true;
            return tree;
        }
        /// <summary>
        /// 用于在编辑器将场景型行为树本地化时做准备，不应在游戏逻辑中使用
        /// </summary>
        /// <param name="scenedTree">场景型行为树</param>
        /// <returns>准备好本地化的行为树</returns>
        public static BehaviourTree PrepareLocalization(BehaviourTree scenedTree)
        {
            if (scenedTree.IsInstance || !scenedTree.scenceOnly) return null;
            BehaviourTree localTree = Instantiate(scenedTree);
            localTree.scenceOnly = false;
            localTree.nodes.ForEach(x => x.PrepareLocalization());
            return localTree;
        }

        public SortedSet<Composite> GetEvaluatedComposites()
        {
            return evaluatedComposites;
        }
#endif
        #endregion
    }
}