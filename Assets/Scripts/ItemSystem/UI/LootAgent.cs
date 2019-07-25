using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootAgent : MonoBehaviour
{
    [SerializeField]
    private string _name;
    public new string name
    {
        get
        {
            return _name;
        }
    }

    [SerializeField]
    private float disappearTime;

    private Coroutine recycleRoutine;

    [HideInInspector]
    public List<ItemInfo> lootItems = new List<ItemInfo>();


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
        ObjectPool.Instance.Put(gameObject);
        if (recycleRoutine != null) StopCoroutine(recycleRoutine);
        recycleRoutine = null;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !LootManager.Instance.IsPicking)
            LootManager.Instance.CanPick(this);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !LootManager.Instance.IsPicking)
            LootManager.Instance.CanPick(this);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && LootManager.Instance.LootAgent == this)
            LootManager.Instance.CannotPick();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !LootManager.Instance.IsPicking)
            LootManager.Instance.CanPick(this);
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && !LootManager.Instance.IsPicking)
            LootManager.Instance.CanPick(this);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && LootManager.Instance.LootAgent == this)
            LootManager.Instance.CannotPick();
    }
}
