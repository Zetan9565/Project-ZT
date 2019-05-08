using UnityEngine;

public delegate void EnermyDeathListener();

[DisallowMultipleComponent]
public class Enemy : MonoBehaviour
{
    public string EnemyID
    {
        get { return Info.ID; }
    }

    public string EnemyName
    {
        get { return info.Name; }
    }

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

    private void Awake()
    {
        if (!GameManager.Enermies.ContainsKey(EnemyID)) GameManager.Enermies.Add(EnemyID, new System.Collections.Generic.List<Enemy>() { this });
        else if (!GameManager.Enermies[EnemyID].Contains(this)) GameManager.Enermies[EnemyID].Add(this);
        else if (GameManager.Enermies[EnemyID].Exists(x => !x.gameObject)) GameManager.Enermies[EnemyID].RemoveAll(x => !x.gameObject);
    }

    public void Death()
    {
        //Debug.Log("One [" + info.Name + "] was killed");
        OnDeathEvent?.Invoke();
        QuestManager.Instance.UpdateUI();
    }
}