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

    public PlayerController2D Controller { get; private set; }

    public Transform PlayerTransform => Controller.Motion.transform;

    public Backpack Backpack { get { return PlayerInfo.backpack; } }

    public Character Character { get; private set; }

    public bool CheckIsNormalWithAlert()
    {
        if (Character.Data.mainState != CharacterState.Normal)
        {
            MessageManager.Instance.New("当前状态无法进行此操作");
            return false;
        }
        return true;
    }

    public void Init()
    {
        Controller = FindObjectOfType<PlayerController2D>();
        Character = Controller.Character;
        if (playerInfo)
        {
            playerInfo = Instantiate(playerInfo);
            Character.Init(new CharacterData(playerInfo));
        }
    }

    public void SetPlayerInfo(PlayerInformation playerInfo)
    {
        this.playerInfo = playerInfo;
        Init();
    }

    public void SetPlayerState(CharacterState state, dynamic subState)
    {
        Character.SetState(state, subState);
    }

    public void Trace()
    {
        Controller.Trace();
    }

    public void ResetPath()
    {
        Controller.ResetPath();
    }
}