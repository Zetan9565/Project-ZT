using UnityEngine;
using System.Collections;

public class PlayerInfoManager : MonoBehaviour
{
    private static PlayerInfoManager instance;
    public static PlayerInfoManager Instance
    {
        get
        {
            if (!instance || !instance.gameObject)
                instance = FindObjectOfType<PlayerInfoManager>();
            return instance;
        }
    }

    [SerializeField]
    private PlayerInfomation playerInfo;
    public PlayerInfomation PlayerInfo
    {
        get
        {
            return playerInfo;
        }

        private set
        {
            playerInfo = value;
        }
    }

    public Backpack Backpack { get { return PlayerInfo.backpack; } }

    private void Start()
    {
        Init();
    }

    public void Init()
    {
        if (playerInfo)
        {
            playerInfo = Instantiate(playerInfo);
            BackpackManager.Instance.Init();
        }
    }
}