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

    public Movement movementState { get; set; }
    public bool isAiming { get; set; }
    public bool isGrounded { get; set; }
    public bool isRagdoll { get; set; }
    public bool isArmed { get; set; }

    public bool IsMoving => (movementState == Movement.Walking || movementState == Movement.Running) && movementState != Movement.Jumping;

    private void Update()
    {
        if (!isLocalPlayer) return;
        HandleMovementInput();
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
}
