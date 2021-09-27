using UnityEngine;

[DisallowMultipleComponent]
[AddComponentMenu("Zetan Studio/管理器/玩家角色管理器")]
public class PlayerManager : SingletonMonoBehaviour<PlayerManager>
{
    [SerializeField]
    private PlayerInformation playerInfo;
    public PlayerInformation PlayerInfo
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

    public PlayerControlInput Controller { get; private set; }

    public Transform PlayerTransform => Controller.transform;

    public Backpack Backpack { get { return PlayerInfo.backpack; } }

    public Player Player { get; private set; }

    public bool CheckIsNormalWithAlert()
    {
        if (Player.GetData().mainState != CharacterStates.Normal)
        {
            MessageManager.Instance.New("当前状态无法进行此操作");
            return false;
        }
        return true;
    }

    public void Init()
    {
        Controller = FindObjectOfType<PlayerControlInput>();
        Player = Controller.GetComponent<Player>();
        if (playerInfo)
        {
            playerInfo = Instantiate(playerInfo);
            Player.Init(new PlayerData(playerInfo));
        }
    }

    public void SetPlayerInfo(PlayerInformation playerInfo)
    {
        this.playerInfo = playerInfo;
        Init();
    }

    public void SetPlayerState(CharacterStates state, dynamic subState)
    {
        Player.SetState(state, subState);
    }

    public void Trace()
    {
        //Controller.Trace();
    }

    public void ResetPath()
    {
        //Controller.ResetPath();
    }
}