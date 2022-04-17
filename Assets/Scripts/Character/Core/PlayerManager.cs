using System.Collections.Generic;
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
    public bool CheckIsIdleWithAlert()
    {
        if (Player.MachineState is not CharacterIdleState)
        {
            MessageManager.Instance.New("待机状态才可以进行此操作");
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
    private void Awake()
    {
        RegisterNotify();
    }
    private void RegisterNotify()
    {
        NotifyCenter.AddListener(NotifyCenter.CommonKeys.PlayerStateChanged, OnPlayerStateChanged, this);
        NotifyCenter.AddListener(PlayerBusyWithUI, OnPlayerBusyWithUI, this);
    }
    private void UnregisterNotify()
    {
        NotifyCenter.RemoveListener(this);
    }
    private void OnPlayerStateChanged(params object[] msg)
    {

    }
    private HashSet<object> busyUI = new HashSet<object>();
    private void OnPlayerBusyWithUI(params object[] msg)
    {
        if (msg.Length > 0 && msg[0] is not null && msg[1] is bool busy)
            if (busy)
            {
                if (!busyUI.Contains(msg[0]) && Player.GetMainState(out var state) && state == CharacterStates.Normal)
                {
                    busyUI.Add(msg[0]);
                    Player.SetMachineAndCharacterState<PlayerMakingState>(CharacterStates.Busy, CharacterBusyStates.UI);
                }
            }
            else if (busyUI.Remove(msg[0]) && busyUI.Count < 1 && Player.GetState(out var state, out var sub) && state == CharacterStates.Busy && (CharacterBusyStates)sub == CharacterBusyStates.UI)
            {
                Player.SetMachineAndCharacterState<CharacterIdleState>(CharacterStates.Normal, CharacterNormalStates.Idle);
            }
    }
    #region 消息
    public const string PlayerBusyWithUI = "PlayerBusyWithUI";

    #endregion
}