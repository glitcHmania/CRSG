using Mirror;
using UnityEngine;

public class PlayerState : NetworkBehaviour
{
    public enum Movement
    {
        Idle,
        Walking,
        Running,
        Jumping
    }

    public Movement movementState { get; private set; }
    public bool isAiming { get; private set; }
    public bool isGrounded { get; set; }

    private bool _isRagdoll;
    public bool IsRagdoll => _isRagdoll;

    public bool IsMoving => movementState == Movement.Walking || movementState == Movement.Running;

    public void SetRagdollState(bool state)
    {
        _isRagdoll = state;
    }

    private void Update()
    {
        if (!isLocalPlayer) return;

        HandleMovementInput();
        HandleAimingInput();
    }

    private void HandleMovementInput()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            movementState = Movement.Jumping;
            return;
        }

        bool hasInput = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) ||
                        Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D);

        if (hasInput)
        {
            movementState = Input.GetKey(KeyCode.LeftShift)
                ? Movement.Running
                : Movement.Walking;
        }
        else
        {
            movementState = Movement.Idle;
        }
    }

    private void HandleAimingInput()
    {
        isAiming = Input.GetMouseButton(1);
    }
}
