using UnityEngine;

[DisallowMultipleComponent]
[AddComponentMenu("ZetanStudio/管理器/玩家角色管理器")]
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

    [SerializeField]
    private PlayerController2D playerController;
    public PlayerController2D PlayerController
    {
        get => playerController;
        private set => playerController = value;
    }

    public Transform PlayerTransform => playerController.CharacterController.transform;

    public Backpack Backpack { get { return PlayerInfo.backpack; } }

    public void Init()
    {
        if (playerInfo)
        {
            playerInfo = Instantiate(playerInfo);
        }
        playerController = FindObjectOfType<PlayerController2D>();
    }

    public void SetPlayerInfo(PlayerInformation playerInfo)
    {
        this.playerInfo = playerInfo;
        Init();
    }

    private void Update()
    {
        if (InputManager.IsTyping) playerController.controlAble = false;
    }
}