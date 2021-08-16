using System;
using UnityEngine;


[DisallowMultipleComponent, RequireComponent(typeof(MapIconHolder))]
public class CheckPoint : MonoBehaviour
{
    public string ID => Data ? Data.Info.ID : string.Empty;

    private dynamic collider;

    public CheckPointData Data { get; private set; }

    public bool IsValid => Data && Data.Info;

    public void Init(CheckPointData data, Vector3 position)
    {
        Data = data;
        ZetanUtility.SetActive(gameObject, data);
        if (!data) return;
        Data = data;
        gameObject.layer = Data.Info.Layer;
        transform.position = position;
        if (collider == null)
        {
            switch (Data.Info.TriggerType)
            {
                case CheckPointTriggerType.Box:
                    collider = gameObject.AddComponent<BoxCollider2D>();
                    (collider as BoxCollider2D).size = Data.Info.Size;
                    break;
                case CheckPointTriggerType.Circle:
                    collider = gameObject.AddComponent<CircleCollider2D>();
                    (collider as CircleCollider2D).radius = Data.Info.Radius;
                    break;
                case CheckPointTriggerType.Capsule:
                    collider = gameObject.AddComponent<CapsuleCollider2D>();
                    (collider as CapsuleCollider2D).size = new Vector2(Data.Info.Radius * 2, Data.Info.Height);
                    break;
            }
            if (collider) (collider as Collider2D).isTrigger = true;
        }
    }

    //private void OnTriggerEnter(Collider other)
    //{
    //    if (other.tag == "Player")
    //    {
    //        OnMoveIntoEvent?.Invoke(this);
    //        QuestManager.Instance.UpdateUI();
    //    }
    //}

    ///*private void OnTriggerStay(Collider other)
    //{
    //}*/

    //private void OnTriggerExit(Collider other)
    //{
    //    if (other.tag == "Player")
    //    {
    //        OnMoveAwayEvent?.Invoke(this);
    //        QuestManager.Instance.UpdateUI();
    //    }
    //}


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsValid) return;
        if (collision.CompareTag(Data.Info.TargetTag))
        {
            Data.MoveInto();
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!IsValid) return;
        if (collision.CompareTag(Data.Info.TargetTag))
        {
            Data.StayInside();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!IsValid) return;
        if (collision.CompareTag(Data.Info.TargetTag))
        {
            Data.LeaveAway();
        }
    }
}
