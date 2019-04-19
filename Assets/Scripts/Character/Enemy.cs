using UnityEngine;

public delegate void EnermyDeathListener();

[DisallowMultipleComponent]
public class Enemy : MonoBehaviour
{
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
        Debug.Log("One" + info.Name + "was killed");
        OnDeathEvent?.Invoke();
        QuestManager.Instance.UpdateUI();
    }
}