using UnityEngine;

public class PlayerController2D : MonoBehaviour
{
    [SerializeField]
#if UNITY_EDITOR
    [DisplayName("角色控制器")]
#endif
    private CharacterController2D characterController;

    [SerializeField]
#if UNITY_EDITOR
    [DisplayName("更新方式")]
#endif
    private UpdateType updateType;

    void Update()
    {
        if (updateType == UpdateType.Update)
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            Vector2 dir = new Vector2(horizontal, vertical);
            dir.Normalize();
            characterController.Move(dir);
        }
    }

    private void FixedUpdate()
    {
        if(updateType == UpdateType.FixedUpdate)
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            Vector2 dir = new Vector2(horizontal, vertical);
            dir.Normalize();
            characterController.Move(dir);
        }
    }

    public void SetController(CharacterController2D characterController)
    {
        this.characterController = characterController;
    }
}
public enum UpdateType
{
    Update,
    FixedUpdate
}