using System.Collections.Generic;
using UnityEngine;


[DisallowMultipleComponent, RequireComponent(typeof(MapIconHolder))]
public class QuestPoint :MonoBehaviour, IManageAble
{
    [SerializeField]
#if UNITY_EDITOR
    [DisplayName("识别码")]
#endif
    private string _ID;
    public string ID => _ID;

    public bool IsInit
    {
        get
        {
            throw new System.NotImplementedException();
        }

        set
        {
            throw new System.NotImplementedException();
        }
    }

    public delegate void MoveToPointListener(QuestPoint point);
    public event MoveToPointListener OnMoveIntoEvent;
    public event MoveToPointListener OnMoveAwayEvent;

    public bool Init()
    {
        if (!GameManager.QuestPoints.ContainsKey(ID)) GameManager.QuestPoints.Add(ID, new List<QuestPoint>() { this });
        else if (!GameManager.QuestPoints[ID].Contains(this)) GameManager.QuestPoints[ID].Add(this);
        return true;
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
        if (collision.CompareTag("Player"))
        {
            OnMoveIntoEvent?.Invoke(this);
            QuestManager.Instance.UpdateUI();
        }
    }

    /*private void OnTriggerStay2D(Collider2D collision)
    {
    }*/

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            OnMoveAwayEvent?.Invoke(this);
            QuestManager.Instance.UpdateUI();
        }
    }

    public bool Reset()
    {
        throw new System.NotImplementedException();
    }

    public bool OnSaveGame(SaveData data)
    {
        throw new System.NotImplementedException();
    }

    public bool OnLoadGame(SaveData data)
    {
        throw new System.NotImplementedException();
    }
}
