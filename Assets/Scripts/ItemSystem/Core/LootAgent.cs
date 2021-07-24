using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootAgent : InteractiveObject
{
    [SerializeField]
    private float disappearTime;

    private Coroutine recycleRoutine;

    [HideInInspector]
    public List<ItemInfo> lootItems = new List<ItemInfo>();

    public override bool Interactive
    {
        get
        {
            return base.Interactive && !LootManager.Instance.IsPicking;
        }

        protected set
        {
            base.Interactive = value;
        }
    }

    public void Init(List<ItemInfo> lootItems, Vector3 position)
    {
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
        return LootManager.Instance.Pick(this);
    }
}