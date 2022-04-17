using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootAgent : Interactive2D
{
    [SerializeField]
    private float disappearTime;

    private Coroutine recycleRoutine;

    [HideInInspector]
    public List<ItemWithAmount> lootItems = new List<ItemWithAmount>();

    public override bool IsInteractive
    {
        get
        {
            return lootItems.Count > 0 && !NewWindowsManager.IsWindowOpen<LootWindow>();
        }
    }

    public void Init(List<ItemWithAmount> lootItems, Vector3 position)
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
        return NewWindowsManager.OpenWindowBy<LootWindow>(this);
    }
}