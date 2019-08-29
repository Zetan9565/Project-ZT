using UnityEngine;

public delegate void MoveToPointListener(QuestPoint point);

[DisallowMultipleComponent, RequireComponent(typeof(MapIconHolder))]
public class QuestPoint : MonoBehaviour
{
    [SerializeField]
#if UNITY_EDITOR
    [DisplayName("识别码")]
#endif
    private string _ID;
    public string ID
    {
        get
        {
            return _ID;
        }
    }

    public event MoveToPointListener OnMoveIntoEvent;
    public event MoveToPointListener OnMoveAwayEvent;

    private void Awake()
    {
        if (!GameManager.QuestPoints.ContainsKey(ID)) GameManager.QuestPoints.Add(ID, this);
        else if (!GameManager.QuestPoints[ID] || !GameManager.QuestPoints[ID].gameObject)
        {
            GameManager.QuestPoints.Remove(ID);
            GameManager.QuestPoints.Add(ID, this);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            OnMoveIntoEvent?.Invoke(this);
            QuestManager.Instance.UpdateUI();
        }
    }

    /*private void OnTriggerStay(Collider other)
    {
        //TODO 滞留于任务点时的操作
    }*/

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            OnMoveAwayEvent?.Invoke(this);
            QuestManager.Instance.UpdateUI();
        }
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            OnMoveIntoEvent?.Invoke(this);
            QuestManager.Instance.UpdateUI();
        }
    }

    /*private void OnTriggerStay2D(Collider2D collision)
    {
        //TODO 滞留于任务点时的操作
    }*/

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            OnMoveAwayEvent?.Invoke(this);
            QuestManager.Instance.UpdateUI();
        }
    }
}
