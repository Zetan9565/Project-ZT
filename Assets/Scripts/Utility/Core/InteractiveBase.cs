using UnityEngine;

namespace ZetanStudio.InteractionSystem
{
    using UI;

    public abstract class InteractiveBase : MonoBehaviour, IInteractive
    {
        public bool activated = true;

        [SerializeField]
        private string defaultName = "可交互对象";
        public virtual string Name
        {
            get
            {
                return defaultName;
            }
        }

        [SerializeField, SpriteSelector]
        private Sprite defaultIcon;
        public virtual Sprite Icon => defaultIcon;

        /// <summary>
        /// 可否交互
        /// </summary>
        public abstract bool IsInteractive { get; }

        /// <summary>
        /// 是否处于可交互状态
        /// </summary>
        private bool interactable;

        public abstract bool DoInteract();

        public void EndInteraction()
        {
            interactable = false;
            OnEndInteraction();
        }

        protected void OnDestroy()
        {
            if (InteractionPanel.Instance) InteractionPanel.Instance.Remove(this);
            OnDestroy_();
        }

        protected virtual void OnDestroy_()
        {
            if (InteractionPanel.Instance) InteractionPanel.Instance.Remove(this);
        }

        protected void Insert()
        {
            if (activated && !interactable && IsInteractive)
            {
                InteractionPanel.Instance.Insert(this);
                interactable = true;
                OnInteractable();
            }
        }
        protected void Remove()
        {
            InteractionPanel.Instance.Remove(this);
            interactable = false;
            OnNotInteractable();
        }

        protected virtual void OnEndInteraction() { }
        protected virtual void OnNotInteractable() { }
        protected virtual void OnInteractable() { }
    }
    public interface IInteractive
    {
        string Name { get; }
        Sprite Icon { get; }
        bool IsInteractive { get; }

        bool DoInteract();
        void EndInteraction();
    }
}