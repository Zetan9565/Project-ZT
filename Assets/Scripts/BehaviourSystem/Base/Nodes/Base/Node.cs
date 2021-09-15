using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace ZetanStudio.BehaviourTree
{
    /// <summary>
    /// 结点基类，所有后续扩展结点都继承于此
    /// </summary>
    public abstract class Node : ScriptableObject
    {
        //为什么使用ScriptableObject：树本身是ScriptableObject生成的asset文件，而Unity保存
        //asset的方式类似于json，所以有时候看着是同样的asset，其实已经是某次反序列化的结果，
        //不是最初的那个了，这会造成Node之间的相互引用丢失，比如父结点失去子结点。都使用Scri-
        //-ptableObject则可以避免这一点。当然，结点之间的关联可以用某种键值定义，每次需要访问
        //子结点都传入树对象并根据这种键值从中找对应的结点，如此复杂，何必多此一举。

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

        protected BehaviourTree owner;
        public BehaviourTree Owner => owner;

        public NodeShortcut Shortcut { get; private set; }

        /// <summary>
        /// 对行为树执行器游戏对象的快捷引用
        /// </summary>
        public virtual GameObject gameObject => Shortcut ? Shortcut.gameObject : null;
        /// <summary>
        /// 对行为树执行器变换组件的快捷引用
        /// </summary>
        public virtual Transform transform => Shortcut ? Shortcut.transform : null;

        public bool IsInstance { get; protected set; }

        [SerializeField]
        protected bool isRuntime;//要暴露给Unity，要不然每运行一次就会被重置
        public bool IsRuntime => isRuntime;
        /// <summary>
        /// 是否是实例
        /// </summary>
        #endregion

        #region 运行时方法
        public virtual Node GetInstance()
        {
            if (isRuntime)
            {
                IsInstance = true;
                return this;
            }
            Node node = Instantiate(this);
            node.IsInstance = true;
            return node;
        }
        protected virtual T GetInstance<T>() where T : Node
        {
            if (isRuntime)
            {
                IsInstance = true;
                return this as T;
            }
            Node node = Instantiate(this);
            node.IsInstance = true;
            return node as T;
        }
        public void Init(BehaviourTree owner)
        {
            if (!IsInstance)
            {
                Debug.LogError("尝试初始化未实例化的结点");
                return;
            }
            this.owner = owner;
            Type type = GetType();
            foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
            {
                if (field.FieldType.IsSubclassOf(typeof(SharedVariable)))
                {
                    SharedVariable variable = field.GetValue(this) as SharedVariable;
                    if (variable.isShared) field.SetValue(this, this.owner.GetVariable(variable.name));
                    else if (variable.isGlobal) field.SetValue(this, BehaviourManager.Instance.GetGlobalVariable(variable.name));
                }
            }
            Shortcut = new NodeShortcut(owner.Executor);
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
                Debug.LogError("尝试评估未实例化的结点");
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
            Owner.EvaluatedNodes.Enqueue(this);
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
        /// 评估开始回调，在第一次OnUpdate()之前调用
        /// </summary>
        protected virtual void OnStart() { }
        /// <summary>
        /// 评估结束回调
        /// </summary>
        protected virtual void OnEnd() { }
        /// <summary>
        /// 结点被暂停时
        /// </summary>
        /// <param name="paused"></param>
        protected virtual void OnPause(bool paused) { }
        /// <summary>
        /// 结点被重置时
        /// </summary>
        protected virtual void OnReset() { }

        public virtual void OnBehaviourStart() { }
        public virtual void OnBehaviourRestart() { }
        public virtual void OnBehaviourEnd() { }

        public void Abort()
        {
            if (State != NodeStates.Inactive)
            {
                isStarted = false;
                State = NodeStates.Failure;
                GetChildren().ForEach(n => n.Abort());
            }
        }

        #region Unity回调
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
        #endregion

        public virtual List<Node> GetChildren() { return new List<Node>(); }

        public static implicit operator bool(Node self)
        {
            return self != null;
        }

        #region EDITOR
#if UNITY_EDITOR
        /// <summary>
        /// 用于标识编辑器结点，不应在游戏逻辑中使用
        /// </summary>
        [HideInInspector] public string guid;
        /// <summary>
        /// 用于设置编辑器结点位置，不应在游戏逻辑中使用
        /// </summary>
        [HideInInspector] public Vector2 position;
        /// <summary>
        /// 用于在编辑器中备注结点功能，不应在游戏逻辑中使用
        /// </summary>
        [TextArea, DisplayName("描述")] public string description;

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
                Node node = CreateInstance(type) as Node;
                node.isRuntime = true;
                return node;
            }
            return null;
        }

        protected T ConvertToLocal<T>() where T : Node
        {
            Node node = Instantiate(this);
            node.name = node.name.Replace("(R)(Clone)", "");
            node.isRuntime = false;
            return node as T;
        }

        /// <summary>
        /// 用于在编辑器中本地化行为树，不应在游戏逻辑中使用
        /// </summary>
        /// <returns></returns>
        public virtual Node ConvertToLocal()
        {
            Node node = Instantiate(this);
            node.name = node.name.Replace("(R)(Clone)", "");
            node.isRuntime = false;
            return node;
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