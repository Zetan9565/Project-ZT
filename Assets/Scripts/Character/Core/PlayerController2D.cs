using UnityEngine;

public class PlayerController2D : MonoBehaviour
{
    [SerializeField]
#if UNITY_EDITOR
    [DisplayName("角色控制器")]
#endif
    private CharacterController2D characterController;
    public CharacterController2D CharacterController
    {
        get
        {
            return characterController;
        }
    }

    [SerializeField]
#if UNITY_EDITOR
    [DisplayName("寻路代理")]
#endif
    private AStarUnit unit;
    public AStarUnit Unit
    {
        get
        {
            return unit;
        }
    }

    public Animator Animator
    {
        get
        {
            if (characterController) return characterController.Animator;
            else return null;
        }
    }


    [SerializeField]
#if UNITY_EDITOR
    [DisplayName("更新方式")]
#endif
    private UpdateMode updateMode;

    public bool controlAble = true;

    private void Awake()
    {
        if (Unit) Unit.moveSpeed = characterController.moveSpeed;
    }

    private bool isTrace;

    void Update()
    {
        if (updateMode == UpdateMode.Update) Control();
        //以下只用于Debug
        if (Input.GetKey(KeyCode.LeftControl) && Unit)
        {
            if (Input.GetMouseButtonDown(1) && Camera.main)
            {
                Unit.IsFollowingTarget = false;
                Unit.ShowPath(true);
                Unit.SetDestination(ZetanUtility.MousePositionAsWorld);
            }
        }
    }

    public void Trace()
    {
        isTrace = Unit ? (Unit.HasPath ? !isTrace : false) : false;
    }

    public void ResetPath()
    {
        if (Unit) Unit.ResetPath();
    }

    private void FixedUpdate()
    {
        if (updateMode == UpdateMode.FixedUpdate) Control();
    }

    private void Control()
    {
        if (!controlAble) return;
        var horizontal = Input.GetAxisRaw("Horizontal");
        var vertical = Input.GetAxisRaw("Vertical");
        var input = new Vector2(horizontal, vertical);
        characterController.Move(input.normalized);
        if (Unit)
        {
            if (input.magnitude > 0 || Unit.IsStop)
            {
                Unit.IsFollowingPath = false;
                Unit.IsFollowingTarget = false;
                isTrace = false;
            }
            if (characterController)
            {
                if (input.magnitude == 0 && isTrace) characterController.Move(Unit.DesiredVelocity.normalized);
                else if (Unit.IsFollowingPath || Unit.IsFollowingTarget) characterController.SetAnima(Unit.DesiredVelocity.normalized);
                Unit.moveSpeed = characterController.moveSpeed;
            }
        }
    }

    public void SetController(CharacterController2D characterController)
    {
        this.characterController = characterController;
    }
}