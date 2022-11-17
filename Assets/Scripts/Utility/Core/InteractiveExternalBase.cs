using System;
using UnityEngine;

namespace ZetanStudio.InteractionSystem
{
    public abstract class InteractiveExternalBase : InteractiveBase
    {
        /**
        * 这个类和不带External那个的主要区别是：
        * 后者是一个完整的可继承并直接生效可用的组件，
        * 而这个则需要自行搭配其它组件并设置相关回调才可正常使用
        * **/

        public override string Name
        {
            get
            {
                return getNameFunc?.Invoke() ?? base.Name;
            }
        }

        public override bool IsInteractive => interactiveFunc != null && interactiveFunc();

        [SerializeField]
        protected Component component;
        [SerializeField, Tooltip("返回值是布尔且不含参")]
        protected string interactMethod;
        [SerializeField, Tooltip("返回值是布尔且不含参")]
        protected string interactiveMethod;
        [SerializeField]
        protected string endInteractionMethod;
        [SerializeField]
        protected string interactableMethod;
        [SerializeField]
        protected string notInteractableMethod;
        [SerializeField, Tooltip("返回值是字符串且不含参")]
        protected string nameMethod;

        public Func<bool> interactFunc;
        public Action endInteractionFunc;
        public Action interactableFunc;
        public Action notInteractableFunc;
        public Func<bool> interactiveFunc;
        public Func<string> getNameFunc;

        /// <summary>
        /// 进行交互
        /// </summary>
        /// <returns>交互是否成功</returns>
        public override bool DoInteract()
        {
            return interactFunc?.Invoke() ?? false;
        }

        protected override void OnEndInteraction()
        {
            endInteractionFunc?.Invoke();
        }

        private void Start()
        {
            if (component)
            {
                var type = component.GetType();
                interactFunc = (Func<bool>)Delegate.CreateDelegate(typeof(Func<bool>), component, type.GetMethod(interactMethod));
                endInteractionFunc = (Action)Delegate.CreateDelegate(typeof(Action), component, type.GetMethod(interactiveMethod));
                interactableFunc = (Action)Delegate.CreateDelegate(typeof(Action), component, type.GetMethod(interactiveMethod));
                notInteractableFunc = (Action)Delegate.CreateDelegate(typeof(Action), component, type.GetMethod(interactiveMethod));
                interactiveFunc = (Func<bool>)Delegate.CreateDelegate(typeof(Func<bool>), component, type.GetMethod(interactiveMethod));
                if (!string.IsNullOrEmpty(nameMethod))
                    getNameFunc = (Func<string>)Delegate.CreateDelegate(typeof(Func<string>), component, type.GetMethod(nameMethod));
            }
        }
    }
}