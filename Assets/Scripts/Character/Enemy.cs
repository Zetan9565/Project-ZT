using UnityEngine;

public delegate void EnermyDeathListener();

[DisallowMultipleComponent]
public class Enemy : MonoBehaviour {

    [SerializeField]
    private EnemyInfomation info;
    public EnemyInfomation Info
    {
        get
        {
            return info;
        }
    }

    public event EnermyDeathListener OnDeathEvent;

    public void Death()
    {
        OnDeathEvent?.Invoke();
        QuestManager.Instance.UpdateObjectivesUI();
    }
}