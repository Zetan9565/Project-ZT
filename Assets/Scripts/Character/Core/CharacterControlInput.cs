using UnityEngine;

[DisallowMultipleComponent]
public class CharacterControlInput : MonoBehaviour
{
    public float actionInputInterval = 0.2f;

    [SerializeField,ReadOnly]
    private Vector2 moveInput;
    public Vector2 MoveInput => moveInput;

    [SerializeField,ReadOnly]
    private Vector2 validMoveInput;
    public Vector2 ValidMoveInput => validMoveInput;

    [SerializeField, ReadOnly]
    private bool rollInput;
    public bool RollInput => rollInput;

    [SerializeField, ReadOnly]
    private bool dashInput;
    public bool DashInput => dashInput;

    public virtual void SetMoveInput(Vector2 input)
    {
        moveInput = input.normalized;
        if (moveInput.x != 0 || moveInput.y != 0)
            validMoveInput = moveInput;
    }

    public virtual void SetRollInput(bool input)
    {
        rollInput = input;
    }

    public virtual void SetDashInput(bool input)
    {
        dashInput = input;
    }

    public void UseRollInput()
    {
        rollInput = false;
    }
    public void UseDashInput()
    {
        dashInput = false;
    }

    public void UseActionInputs()
    {
        UseRollInput();
        UseDashInput();
    }
}