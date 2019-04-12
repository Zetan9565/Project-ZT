using UnityEngine;

public delegate void MoveToPointListener(QuestPoint point);

[DisallowMultipleComponent]
public class QuestPoint : MonoBehaviour {

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

    /*private void OnTriggerEnter(Collider other)
    {
        OnMoveIntoEvent?.Invoke(this);
        QuestManager.Instance.UpdateObjectivesUI();
    }

    private void OnTriggerStay(Collider other)
    {
        //TODO
    }

    private void OnTriggerExit(Collider other)
    {
        OnMoveAwayEvent?.Invoke(this);
    }*/


    private void OnTriggerEnter2D(Collider2D collision)
    {
        OnMoveIntoEvent?.Invoke(this);
        QuestManager.Instance.UpdateObjectivesUI();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        //TODO
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        OnMoveAwayEvent?.Invoke(this);
    }
}
