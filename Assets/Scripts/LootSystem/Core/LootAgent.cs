using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootAgent : Interactive2D
{
    [SerializeField]
    private float disappearTime;

    private Coroutine recycleRoutine;

    [HideInInspector]
    public List<CountedItem> lootItems = new List<CountedItem>();

    public override bool IsInteractive
    {
        get
        {
            return lootItems.Count > 0 && !WindowsManager.IsWindowOpen<LootWindow>();
        }
    }

    public void Init(List<CountedItem> lootItems, Vector3 position)
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

    protected override void OnNotInteractable()
    {
        if (WindowsManager.IsWindowOpen<LootWindow>(out var window) && window.Target == this)
            window.Interrupt();
    }

    public override bool DoInteract()
    {
        return WindowsManager.OpenWindowBy<LootWindow>(this);
    }
}