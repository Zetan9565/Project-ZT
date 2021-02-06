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

    public Character character;

    public void Init()
    {
        playerController = FindObjectOfType<PlayerController2D>();
        character = playerController.GetComponent<Character>();
        if (playerInfo)
        {
            playerInfo = Instantiate(playerInfo);
            character.SetInfo(playerInfo);
            playerController.CharacterController.character = character;
        }
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