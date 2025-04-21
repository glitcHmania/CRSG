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

    [SyncVar]
    public Movement movementState;

    [SyncVar]
    public bool isAiming;

    [SyncVar]
    public bool isGrounded;

    [SyncVar]
    public bool isRagdoll;

    [SyncVar]
    public bool isArmed;

    [SyncVar]
    public float Numbness;

    public bool IsMoving => (movementState == Movement.Walking || movementState == Movement.Running) && movementState != Movement.Jumping;

    private void Update()
    {
        if (!isLocalPlayer) return;
        HandleMovementInput();
        Debug.Log($"Movement State: {movementState}");
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
        else if(isGrounded)
        {
            movementState = Movement.Idle;
        }
    }
}
