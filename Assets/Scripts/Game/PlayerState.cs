using Mirror;
using UnityEngine;

public class PlayerState : NetworkBehaviour
{
    public enum Movement
    {
        Idle,
        Walking,
        Running,
        Jumping,
        Falling,
    }

    [SyncVar] public Movement MovementState;
    [SyncVar] public bool IsAiming;
    [SyncVar] public bool IsGrounded;
    [SyncVar] public bool IsRagdoll;
    [SyncVar] public bool IsUnbalanced;
    [SyncVar] public bool IsArmed;
    [SyncVar] public bool IsBouncing;
    [SyncVar] public bool IsCrouching;
    [SyncVar] public float Numbness;

    public bool IsMoving => (MovementState == Movement.Walking || MovementState == Movement.Running) && MovementState != Movement.Jumping;

    private void Update()
    {
        if (!isLocalPlayer) return;

        HandleMovementInput();
    }

    private void HandleMovementInput()
    {
        if (Application.isFocused && !ChatBehaviour.Instance.IsInputActive)
        {
            if (Input.GetKey(KeyCode.Space))
            {
                MovementState = Movement.Jumping;
                return;
            }

            if (Input.GetKeyDown(KeyCode.LeftControl))
            {
                IsCrouching = true;
            }
            else if (Input.GetKeyUp(KeyCode.LeftControl))
            {
                IsCrouching = false;
            }
        }

        bool hasInput = (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) ||
                        Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D)) && Application.isFocused && !ChatBehaviour.Instance.IsInputActive;

        if (hasInput)
        {
            MovementState = Input.GetKey(KeyCode.LeftShift)
                ? Movement.Running
                : Movement.Walking;
        }
        else if (IsGrounded)
        {
            MovementState = Movement.Idle;
        }
    }
}
