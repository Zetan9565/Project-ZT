using System.Collections;
using UnityEngine;

namespace ZetanStudio.GatheringSystem
{
    using InteractionSystem;
    using ZetanStudio.LootSystem;

    public class Resource : Interactive2D
    {
        [SerializeField]
        protected ResourceInformation resourceInfo;
        public ResourceInformation ResourceInfo => resourceInfo;

        public override string Name { get => resourceInfo.Name; }

        [SerializeField]
        protected bool hideOnGathered;

        protected bool isRefresh = true;
        public override bool IsInteractive
        {
            get
            {
                return isRefresh && resourceInfo && resourceInfo.ProductItems.Count > 0 && GatherManager.Resource != this;
            }
        }

        public virtual float LeftRefreshTime { get; protected set; }

        protected Coroutine refreshCoroutine;

        [HideInInspector]
        public UnityEngine.Events.UnityEvent onGatherFinish = new UnityEngine.Events.UnityEvent();

        public virtual void GatherSuccess()
        {
            onGatherFinish?.Invoke();
            if (hideOnGathered) GetComponent<Renderer>().enabled = false;
            if (ResourceInfo.ProductItems.Count > 0)
            {
                var lootItems = DropItemInfo.Drop(ResourceInfo.ProductItems);
                if (lootItems.Count > 0)
                {
                    LootAgent la = ObjectPool.Get(ResourceInfo.LootPrefab).GetComponent<LootAgent>();
                    la.Init(lootItems, transform.position);
                }
            }
            if (refreshCoroutine != null) StopCoroutine(refreshCoroutine);
            refreshCoroutine = StartCoroutine(UpdateTime());
        }

        protected virtual IEnumerator UpdateTime()
        {
            if (ResourceInfo.RefreshTime < 0) yield break;

            LeftRefreshTime = ResourceInfo.RefreshTime;
            while (!isRefresh && ResourceInfo)
            {
                LeftRefreshTime -= Time.deltaTime;
                if (LeftRefreshTime <= 0)
                {
                    LeftRefreshTime = ResourceInfo.RefreshTime;
                    Refresh();
                    yield break;
                }
                yield return null;
            }
        }

        public virtual void Refresh()
        {
            if (hideOnGathered) GetComponent<Renderer>().enabled = true;
        }

        public override bool DoInteract()
        {
            if (GatherManager.Gather(this))
            {
                return true;
            }
            return false;
        }

        protected override void OnNotInteractable()
        {
            if (GatherManager.Resource == this)
                GatherManager.Cancel();
        }
    }
}