using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace ZetanStudio.BehaviourTree
{
    [Serializable]
    /// <summary>
    /// 结点基类，所有后续扩展结点都继承于此
    /// </summary>
    public abstract class Node
    {
        public string name;

        public int priority;

        /// <summary>
        /// 结点是否有效
        /// </summary>
        public abstract bool IsValid { get; }

        #region 运行时属性
        /// <summary>
        /// 结点当前评估状态
        /// </summary>
        public NodeStates State { get; protected set; }

        protected bool isStarted;
        public bool IsStarted => isStarted;

        public bool IsPaused { get; protected set; }

        public bool IsDone => State == NodeStates.Success || State == NodeStates.Failure;

        public BehaviourTree Tree { get; private set; }

        public NodeShortcut Shortcut { get; private set; }

        /// <summary>
        /// 对行为树执行器游戏对象的快捷引用
        /// </summary>
        public virtual GameObject gameObject => Shortcut ? Shortcut.gameObject : null;
        /// <summary>
        /// 对行为树执行器变换组件的快捷引用
        /// </summary>
        public virtual Transform transform => Shortcut ? Shortcut.transform : null;

        /// <summary>
        /// 是否是实例
        /// </summary>
        public bool IsInstance { get; protected set; }

        [SerializeField]
        protected bool isRuntime;//要暴露给Unity，要不然每运行一次就会被重置
        public bool IsRuntime => isRuntime;

        #endregion

        #region 运行时方法
        public void Instantiate()
        {
            IsInstance = true;
        }
        public void Init(BehaviourTree tree)
        {
            if (!IsInstance)
            {
                Debug.LogError("尝试初始化未实例化的结点: " + name);
                return;
            }
            Tree = tree;
            foreach (var field in GetType().GetFields(ZetanUtility.CommonBindingFlags).Where(field => field.FieldType.IsSubclassOf(typeof(SharedVariable))))
            {
                var hideAttr = field.GetCustomAttribute<HideIf_BTAttribute>();
                if (hideAttr != null && ZetanUtility.TryGetMemberValue(hideAttr.path, this, out var value, out _) && Equals(value, hideAttr.value))
                    continue;
                SharedVariable variable = field.GetValue(this) as SharedVariable;
                //if (variable.isShared) field.SetValue(this, this.owner.GetVariable(variable.name));
                //else if (variable.isGlobal) field.SetValue(this, BehaviourManager.Instance.GetVariable(variable.name));
                if (variable.isShared) variable.Link(Tree.GetVariable(variable.name));
                else if (variable.isGlobal) variable.Link(BehaviourManager.Instance.GetVariable(variable.name));
            }
            Shortcut = new NodeShortcut(Tree.Executor);
            OnAwake();
#if false
            void TryLinkSharedVariable(object onwer, FieldInfo field)
            {
                if (field.FieldType.IsSubclassOf(typeof(SharedVariable)))
                {
                    SharedVariable variable = field.GetValue(onwer) as SharedVariable;
                    field.SetValue(this, this.owner.GetVariable(variable.name));
                }
                else if (field.FieldType.IsArray)//因为Unity只能显示数组和列表，只对这两种特殊情况特殊处理
                {
                    var array = field.GetValue(owner) as SharedVariable[];
                    if (array.Length < 1) return;
                    bool typeMatch = false;
                    var eType = array[0].GetType();
                    var fieldInfos = eType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                    if (eType.IsSubclassOf(typeof(SharedVariable))) typeMatch = true;
                    for (int i = 0; i < array.Length; i++)
                    {
                        if (typeMatch) array[i] = this.owner.GetVariable(array[i].name);
                        else foreach (var _field in fieldInfos)
                            {
                                TryLinkSharedVariable(array[i], _field);
                            }
                    }
                }
                else if (typeof(IList).IsAssignableFrom(field.FieldType))
                {
                    var list = field.GetValue(owner) as IList;
                    if (list.Count < 1) return;
                    bool typeMatch = false;
                    var eType = list[0].GetType();
                    var fieldInfos = eType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                    if (eType.IsSubclassOf(typeof(SharedVariable))) typeMatch = true;
                    for (int i = 0; i < list.Count; i++)
                    {
                        if (typeMatch) list[i] = this.owner.GetVariable((list[i] as SharedVariable).name);
                        else foreach (var _field in fieldInfos)
                            {
                                TryLinkSharedVariable(list[i], _field);
                            }
                    }
                }
            }
#endif
        }
        /// <summary>
        /// 对此结点进行评估
        /// </summary>
        /// <returns></returns>
        public NodeStates Evaluate()
        {
            if (!IsInstance)
            {
                Debug.LogError("尝试评估未实例化的结点: " + name);
                return NodeStates.Failure;
            }
            if (!isStarted)
            {
                OnStart();
                isStarted = true;
            }
            if (isStarted)
            {
                if (!IsPaused) State = OnUpdate();
                if (IsDone)
                {
                    OnEnd();
                    isStarted = false;
                }
            }
            Tree.OnNodeEvaluated(this);
            return State;
        }
        /// <summary>
        /// 暂停此结点
        /// </summary>
        /// <param name="paused"></param>
        public void Pause(bool paused)
        {
            OnPause(paused);
            IsPaused = paused;
        }
        /// <summary>
        /// 重置此结点
        /// </summary>
        public void Reset()
        {
            State = NodeStates.Inactive;
            isStarted = false;
            OnReset();
        }

        /// <summary>
        /// 初始化时调用一次
        /// </summary>
        protected virtual void OnAwake() { }
        /// <summary>
        /// 评估进行时回调，是结点的逻辑核心
        /// </summary>
        /// <returns></returns>
        protected abstract NodeStates OnUpdate();
        /// <summary>
        /// 评估开始回调，在第一次OnUpdate()之前调用，默认为空
        /// </summary>
        protected virtual void OnStart() { }
        /// <summary>
        /// 评估结束回调，在最后一次OnUpdate()之后调用，默认为空
        /// </summary>
        protected virtual void OnEnd() { }
        /// <summary>
        /// 结点被暂停时，默认为空
        /// </summary>
        /// <param name="paused"></param>
        protected virtual void OnPause(bool paused) { }
        /// <summary>
        /// 结点被重置时，默认为空
        /// </summary>
        protected virtual void OnReset() { }

        public virtual void OnBehaviourStart() { }
        public virtual void OnBehaviourRestart() { }
        public virtual void OnBehaviourEnd() { }
        /// <summary>
        /// 中断此节点
        /// </summary>
        public void Abort()
        {
            if (State != NodeStates.Inactive)
            {
                isStarted = false;
                State = NodeStates.Failure;
                if(this is ParentNode parent) parent.GetChildren().ForEach(n => n.Abort());
                OnEnd();
            }
        }
        /// <summary>
        /// 失活此节点
        /// </summary>
        public void Inactivate()
        {
            isStarted = false;
            State = NodeStates.Inactive;
            if(this is ParentNode parent) parent.GetChildren().ForEach(n => n.Inactivate());
        }
        #endregion

        #region Unity回调
        #region 快捷方式
        public Component GetComponent(Type type)
        {
            if (!gameObject) return null;
            return gameObject.GetComponent(type);
        }
        public T GetComponent<T>()
        {
            if (!gameObject) return default;
            return gameObject.GetComponent<T>();
        }
        public Component[] GetComponents(Type type)
        {
            if (!gameObject) return null;
            return gameObject.GetComponents(type);
        }
        public T[] GetComponents<T>()
        {
            if (!gameObject) return null;
            return gameObject.GetComponents<T>();
        }
        public Component GetComponentInParent(Type type, bool includeInactive = false)
        {
            if (!gameObject) return null;
            return gameObject.GetComponentInParent(type, includeInactive);
        }
        public T GetComponentInParent<T>(bool includeInactive = false)
        {
            if (!gameObject) return default;
            return gameObject.GetComponentInParent<T>(includeInactive);
        }
        public Component[] GetComponentsInParent(Type type, bool includeInactive = false)
        {
            if (!gameObject) return null;
            return gameObject.GetComponentsInParent(type, includeInactive);
        }
        public T[] GetComponentsInParent<T>(bool includeInactive = false)
        {
            if (!gameObject) return null;
            return gameObject.GetComponentsInParent<T>(includeInactive);
        }
        public Component GetComponentInChildren(Type type, bool includeInactive = false)
        {
            if (!gameObject) return null;
            return gameObject.GetComponentInChildren(type, includeInactive);
        }
        public T GetComponentInChildren<T>(bool includeInactive = false)
        {
            if (!gameObject) return default;
            return gameObject.GetComponentInChildren<T>(includeInactive);
        }
        public Component[] GetComponentsInChildren(Type type, bool includeInactive = false)
        {
            if (!gameObject) return null;
            return gameObject.GetComponentsInChildren(type, includeInactive);
        }
        public T[] GetComponentsInChildren<T>(bool includeInactive = false)
        {
            if (!gameObject) return null;
            return gameObject.GetComponentsInChildren<T>(includeInactive);
        }
        public Coroutine StartCoroutine(IEnumerator routine)
        {
            if (!Tree || !Tree.Executor) return null;
            return Tree.Executor.StartCoroutine(routine);
        }
        public void StopCoroutine(Coroutine routine)
        {
            if (Tree && Tree.Executor)
                Tree.Executor.StopCoroutine(routine);
        }
        public void StopAllCoroutines()
        {
            if (Tree && Tree.Executor)
                Tree.Executor.StopAllCoroutines();
        }
        #endregion

        #region 碰撞器事件
        public virtual void OnCollisionEnter(Collision collision) { }
        public virtual void OnCollisionStay(Collision collision) { }
        public virtual void OnCollisionExit(Collision collision) { }

        public virtual void OnCollisionEnter2D(Collision2D collision) { }

        public virtual void OnCollisionStay2D(Collision2D collision) { }
        public virtual void OnCollisionExit2D(Collision2D collision) { }
        #endregion

        #region 触发器事件
        public virtual void OnTriggerEnter(Collider other) { }
        public virtual void OnTriggerStay(Collider other) { }
        public virtual void OnTriggerExit(Collider other) { }

        public virtual void OnTriggerEnter2D(Collider2D collision) { }
        public virtual void OnTriggerStay2D(Collider2D collision) { }
        public virtual void OnTriggerExit2D(Collider2D collision) { }
        #endregion

        public virtual void OnDrawGizmos() { }
        public virtual void OnDrawGizmosSelected() { }
        #endregion

        public static implicit operator bool(Node self)
        {
            return self != null;
        }

        public class Comparer : IComparer<Node>
        {
            public static Comparer Default = new Comparer();

            public int Compare(Node x, Node y)
            {
                if (x.priority < y.priority) return -1;
                else if (x.priority > y.priority) return 1;
                else return 0;
            }
        }

        #region EDITOR
#if UNITY_EDITOR
        /// <summary>
        /// 用于在编辑器中标识结点，不应在游戏逻辑中使用
        /// </summary>
        [HideInInspector] public string guid;
        /// <summary>
        /// 用于在编辑器中设置结点位置，不应在游戏逻辑中使用
        /// </summary>
        [HideInInspector] public Vector2 _position;
        /// <summary>
        /// 用于在编辑器中备注结点，不应在游戏逻辑中使用
        /// </summary>
        [TextArea(1, 3), DisplayName("结点描述")] public string _description;

        /// <summary>
        /// 用于在编辑器中连接子结点，不应在游戏逻辑中使用
        /// </summary>
        /// <param name="child"></param>
        public virtual void AddChild(Node child) { }
        /// <summary>
        /// 用于在编辑器中断开子结点，不应在游戏逻辑中使用
        /// </summary>
        /// <param name="child"></param>
        public virtual void RemoveChild(Node child) { }

        /// <summary>
        /// 用于在编辑器中获取运行时结点，不应在游戏逻辑中使用
        /// </summary>
        /// <returns>运行时结点</returns>
        public static Node GetRuntimeNode(Type type)
        {
            if (type.IsSubclassOf(typeof(Node)))
            {
                Node node = Activator.CreateInstance(type) as Node;
                node.isRuntime = true;
                return node;
            }
            return null;
        }

        /// <summary>
        /// 用于在编辑器中本地化行为树，不应在游戏逻辑中使用
        /// </summary>
        /// <returns></returns>
        public void PrepareLocalization()
        {
            name = name.Replace("(R)", "");
            isRuntime = false;
        }

        /// <summary>
        /// 用于在编辑器中复制结点，不应在游戏逻辑中使用
        /// </summary>
        /// <returns>克隆的结点</returns>
        public virtual Node Copy()
        {
            return MemberwiseClone() as Node;
        }
#endif
        #endregion
    }

    public class NodeShortcut
    {
        public GameObject gameObject;
        public Transform transform;
        public Animator animator;
        public MeshRenderer meshRenderer;
        public SpriteRenderer spriteRenderer;
        public Rigidbody rigidbody;
        public Rigidbody2D rigidbody2D;

        public NodeShortcut(BehaviourExecutor executor)
        {
            gameObject = executor.gameObject;
            transform = executor.transform;
            animator = executor.GetComponent<Animator>();
            meshRenderer = executor.GetComponent<MeshRenderer>();
            spriteRenderer = executor.GetComponent<SpriteRenderer>();
            rigidbody = executor.GetComponent<Rigidbody>();
            rigidbody2D = executor.GetComponent<Rigidbody2D>();
        }

        public static implicit operator bool(NodeShortcut self)
        {
            return self != null;
        }
    }

    public enum NodeStates
    {
        [InspectorName("未评估")]
        Inactive,
        [InspectorName("成功")]
        Success,
        [InspectorName("失败")]
        Failure,
        [InspectorName("评估中")]
        Running
    }
}