using UnityEngine;

public class JointControl : MonoBehaviour
{
    public PlayerState playerState;

    private float currentSpeed;
    [SerializeField]
    private float walkSpeed;
    [SerializeField]
    private float sprintSpeed;
    [SerializeField]
    private float maxAngle;
    [SerializeField]
    private float lowerLegAngleMultiplier;
    [SerializeField]
    private float jumpSpeed = 60f;

    public Camera cam;

    [Header("Leg Joints")]
    public ConfigurableJoint leftUpLegJoint;
    public ConfigurableJoint leftLegJoint;
    public ConfigurableJoint rightUpLegJoint;
    public ConfigurableJoint rightLegJoint;

    [Header("Spine Joint")]
    public ConfigurableJoint spineJoint;

    private Quaternion defaultSpineTargetRotation = Quaternion.identity;
    private float currentJumpAngle = 0f;

    void Update()
    {
        if (playerState.movementState == PlayerState.Movement.Running)
        {
            currentSpeed = sprintSpeed;
        }
        else
        {
            currentSpeed = walkSpeed;
        }

        if (playerState.isAiming)
        {
            spineJoint.targetRotation = Quaternion.Euler(-cam.transform.eulerAngles.x, 0f, 0f);
        }
        else if (spineJoint.targetRotation != defaultSpineTargetRotation)
        {
            spineJoint.targetRotation = Quaternion.identity;
        }


        if (playerState.IsMoving)
        {
            if (!playerState.isAiming)
            {
                MoveForward();
            }
            else
            {
                if (Input.GetKey(KeyCode.W))
                {
                    MoveForward();
                }
                else if (Input.GetKey(KeyCode.S))
                {
                    MoveBackward();
                }
                else if (Input.GetKey(KeyCode.A))
                {
                    MoveLeft();
                }
                else if (Input.GetKey(KeyCode.D))
                {
                    MoveRight();
                }
            }

        }
        else if (playerState.movementState == PlayerState.Movement.Jumping)
        {
            currentJumpAngle = Mathf.MoveTowards(currentJumpAngle, maxAngle, Time.deltaTime * jumpSpeed);

            float upperLegAngle = -currentJumpAngle;
            float lowerLegAngle = Mathf.Max(currentJumpAngle, 0f) * 2f;

            leftUpLegJoint.targetRotation = Quaternion.Euler(upperLegAngle, 0f, 0f);
            leftLegJoint.targetRotation = Quaternion.Euler(lowerLegAngle, 0f, 0f);
            rightUpLegJoint.targetRotation = Quaternion.Euler(upperLegAngle, 0f, 0f);
            rightLegJoint.targetRotation = Quaternion.Euler(lowerLegAngle, 0f, 0f);
        }
        else
        {
            leftUpLegJoint.targetRotation = Quaternion.Euler(0, 0, 0);
            rightUpLegJoint.targetRotation = Quaternion.Euler(0, 0, 0);
            rightLegJoint.targetRotation = Quaternion.Euler(0, 0, 0);
            leftLegJoint.targetRotation = Quaternion.Euler(0, 0, 0);
            currentJumpAngle = 0f;
        }
    }

    private void MoveForward()
    {
        float stepDuration = 1f / currentSpeed;
        float cycleTime = Time.time % (stepDuration * 2f);
        bool isRightLegMoving = cycleTime < stepDuration;

        float t = Mathf.Clamp01((cycleTime % stepDuration) / stepDuration);
        float easedT = Mathf.Sin(t * Mathf.PI);
        float angle = easedT * maxAngle;
        float lowerLegAngle = Mathf.Max(angle * lowerLegAngleMultiplier, 0f);

        if (isRightLegMoving)
        {
            rightUpLegJoint.targetRotation = Quaternion.Euler(-angle, 0f, 0f);
            rightLegJoint.targetRotation = Quaternion.Euler(lowerLegAngle, 0f, 0f);
        }
        else
        {
            leftUpLegJoint.targetRotation = Quaternion.Euler(-angle, 0f, 0f);
            leftLegJoint.targetRotation = Quaternion.Euler(lowerLegAngle, 0f, 0f);
        }
    }

    private void MoveBackward()
    {
        float t = Mathf.PingPong(Time.time * currentSpeed, 1);
        float angle = Mathf.Lerp(-maxAngle, maxAngle, t);
        leftUpLegJoint.targetRotation = Quaternion.Euler(Mathf.Max(angle * 0.5f, 0f), 0f, 0f);
        leftLegJoint.targetRotation = Quaternion.Euler(Mathf.Max(angle, 0f) * 0.5f, 0f, 0f);
        rightUpLegJoint.targetRotation = Quaternion.Euler(Mathf.Max(-angle * 0.5f, 0f), 0f, 0f);
        rightLegJoint.targetRotation = Quaternion.Euler(Mathf.Max(-angle, 0f) * 0.5f, 0f, 0f);
    }


    private void MoveLeft()
    {
        float t = Mathf.PingPong(Time.time * currentSpeed, 1);
        float angle = Mathf.Lerp(-maxAngle, maxAngle, t);
        if (angle >= 0)
        {
            leftUpLegJoint.targetRotation = Quaternion.Euler(0f, 0f, angle * 0.5f);
            leftLegJoint.targetRotation = Quaternion.Euler(Mathf.Max(angle, 0f), 0f, 0f);
        }
        else
        {
            rightLegJoint.targetRotation = Quaternion.Euler(Mathf.Max(-angle, 0f), 0f, 0f);
        }
    }

    private void MoveRight()
    {
        float t = Mathf.PingPong(Time.time * currentSpeed, 1);
        float angle = Mathf.Lerp(-maxAngle, maxAngle, t);
        if (angle >= 0)
        {
            rightUpLegJoint.targetRotation = Quaternion.Euler(0f, 0f, -angle * 0.5f);
            rightLegJoint.targetRotation = Quaternion.Euler(Mathf.Max(angle, 0f), 0f, 0f);
        }
        else
        {
            leftLegJoint.targetRotation = Quaternion.Euler(Mathf.Max(-angle * 1.5f, 0f), 0f, 0f);
        }

    }
}
