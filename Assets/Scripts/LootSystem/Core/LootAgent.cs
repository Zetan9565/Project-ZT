using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootAgent : InteractiveObject
{
    [SerializeField]
    private float disappearTime;

    private Coroutine recycleRoutine;

    [HideInInspector]
    public List<ItemInfoBase> lootItems = new List<ItemInfoBase>();

    public override bool IsInteractive
    {
        get
        {
            return base.IsInteractive && !LootManager.Instance.IsPicking;
        }

        protected set
        {
            base.IsInteractive = value;
        }
    }

    public void Init(List<ItemInfoBase> lootItems, Vector3 position)
    {
        if (lootItems == null)
        {
            Recycle();
            return;
        }
        this.lootItems = lootItems;
        transform.position = position;
        if (this.lootItems.Count < 1) Recycle();//没有产出，直接消失
        else recycleRoutine = StartCoroutine(RecycleDelay());//有产出，延迟消失
    }

    private IEnumerator RecycleDelay()
    {
        yield return new WaitForSeconds(disappearTime);
        Recycle();
    }

    public void Recycle()
    {
        lootItems.Clear();
        ObjectPool.Put(gameObject);
        if (recycleRoutine != null) StopCoroutine(recycleRoutine);
        recycleRoutine = null;
    }

    public override bool DoInteract()
    {
        if (LootManager.Instance.Pick(this))
            return base.DoInteract();
        return false;
    }
}