using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

public class JointController : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerState playerState;
    [SerializeField] private GameObject hips;
    [SerializeField] private Camera cam;

    [Header("Speed Settings")]
    [SerializeField] private float walkSpeed;
    [SerializeField] private float sprintSpeed;

    [Header("Step Settings")]
    [SerializeField] private float stepHeight;

    [Header("Arm Settings")]
    [SerializeField] private float armSwingHeight;

    [Header("Leg Joints")]
    [SerializeField] private ConfigurableJoint leftUpLegJoint;
    [SerializeField] private ConfigurableJoint leftLegJoint;
    [SerializeField] private ConfigurableJoint leftFootJoint;
    [SerializeField] private ConfigurableJoint rightUpLegJoint;
    [SerializeField] private ConfigurableJoint rightLegJoint;
    [SerializeField] private ConfigurableJoint rightFootJoint;

    [Header("Arm Joints")]
    [SerializeField] private ConfigurableJoint leftArmJoint;
    [SerializeField] private ConfigurableJoint leftForeArmJoint;
    [SerializeField] private ConfigurableJoint rightArmJoint;
    [SerializeField] private ConfigurableJoint rightForeArmJoint;

    [Header("Other Joints")]
    [SerializeField] private ConfigurableJoint spineJoint;
    [SerializeField] private ConfigurableJoint hipJoint;
    [SerializeField] private ConfigurableJoint headJoint;

    private float currentSpeed;
    private float currentSlope;
    private float currentStepHeight;
    private Quaternion defaultSpineTargetRotation;
    private Chronometer stepChronometer;
    private Quaternion initialHeadLocalRotation;
    private Movement movement;

    private void Start()
    {
        defaultSpineTargetRotation = spineJoint.targetRotation;
        stepChronometer = new Chronometer();
        initialHeadLocalRotation = headJoint.transform.localRotation;

        movement = GetComponent<Movement>();
    }

    void Update()
    {
        if (!isLocalPlayer || !PlayerSpawner.IsInGameScene) return;

        RaycastHit hit;
        if (Physics.Raycast(hips.transform.position, -hips.transform.up, out hit, 10f, LayerMask.GetMask("Ground")))
        {
            currentSlope = Mathf.Min( Vector3.Angle(hips.transform.up, hit.transform.up), 40f);
        }

        //if (!playerState.IsRagdoll && !playerState.IsUnbalanced && playerState.MovementState != PlayerState.Movement.Falling && playerState.MovementState != PlayerState.Movement.Jumping)
        //{
        //    //rotating the head to the direction of camera
        //    Quaternion localTargetRotation = Quaternion.Inverse(headJoint.transform.parent.rotation) * Quaternion.LookRotation(cam.transform.forward, Vector3.up);
        //    headJoint.targetRotation = Quaternion.Inverse(localTargetRotation) * initialHeadLocalRotation;
        //}

        if (playerState.MovementState == PlayerState.Movement.Running)
        {
            currentSpeed = sprintSpeed;
            currentStepHeight = stepHeight + 20f;
            hipJoint.targetRotation = Quaternion.Euler(-15f, 0, 0); // reset hip rotation when running
        }
        else
        {
            currentSpeed = walkSpeed;
            currentStepHeight = stepHeight;
            hipJoint.targetRotation = Quaternion.Euler(0, 0, 0); // reset hip rotation when running
        }

        if (playerState.IsAiming)
        {
            spineJoint.targetRotation = Quaternion.Euler(-cam.transform.eulerAngles.x, 0f, 0f); // rotate spine to aim
            if (playerState.IsArmed)
            {
                RotateArmToAim(rightArmJoint, rightForeArmJoint);
            }
        }
        else if (!playerState.IsUnbalanced && spineJoint.targetRotation != defaultSpineTargetRotation)
        {
            spineJoint.targetRotation = defaultSpineTargetRotation; // reset spine rotation if not aiming
        }

        if (!playerState.IsUnbalanced && playerState.IsMoving)
        {
            stepChronometer.Update();

            if (!playerState.IsAiming)
            {
                MoveForward();
                SwingArms();
                ResetUpperArms();
            }
            else if (Application.isFocused && !ChatBehaviour.Instance.IsInputActive)
            {
                #region Input
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
                #endregion
            }

        }
        else if (movement.CanJump && playerState.MovementState == PlayerState.Movement.Jumping)
        {
            BendLeg(leftUpLegJoint, leftLegJoint);
            BendLeg(rightUpLegJoint, rightLegJoint);
        }
        else if (playerState.MovementState == PlayerState.Movement.Falling || playerState.MovementState == PlayerState.Movement.Climbing || playerState.IsRagdoll)
        {
            BendLeg(leftUpLegJoint, leftLegJoint);
            BendLeg(rightUpLegJoint, rightLegJoint);
            if (!playerState.IsAiming)
            {
                RaiseArms(leftArmJoint, rightArmJoint);
                ResetLowerArms();
            }
            else
            {
                ResetUpperArms();
                ResetLeftLowerArm();
            }
        }
        else
        {
            ResetLegs();

            ResetUpperArms();
            if (!playerState.IsAiming)
            {
                ResetLowerArms();
            }

            stepChronometer.Reset();
        }
    }

    private void MoveForward()
    {
        var legSwing = Mathf.Sin(stepChronometer.Elapsed * currentSpeed) * (currentStepHeight + currentSlope * 2f);

        if(playerState.CanCrouch && playerState.IsCrouching)
        {
            leftUpLegJoint.targetRotation = Quaternion.Euler(Mathf.Min(legSwing, 0) - 70, 0, 0);
            rightUpLegJoint.targetRotation = Quaternion.Euler(Mathf.Min(-legSwing, 0) - 70, 0, 0);

            if (legSwing < 0)
            {
                leftLegJoint.targetRotation = Quaternion.Euler(-legSwing + 100, 0, 0);
            }
            else
            {
                rightLegJoint.targetRotation = Quaternion.Euler(legSwing + 100, 0, 0);
            }
        }
        else
        {
            leftUpLegJoint.targetRotation = Quaternion.Euler(Mathf.Min(legSwing, 0), 0, 0);
            rightUpLegJoint.targetRotation = Quaternion.Euler(Mathf.Min(-legSwing, 0), 0, 0);

            if (legSwing < 0)
            {
                leftLegJoint.targetRotation = Quaternion.Euler(-legSwing, 0, 0);
            }
            else
            {
                rightLegJoint.targetRotation = Quaternion.Euler(legSwing, 0, 0);
            }
        }
    }

    private void MoveBackward()
    {
        var legSwing = Mathf.Sin(stepChronometer.Elapsed * currentSpeed) * (currentStepHeight + currentSlope * 2f);

        leftUpLegJoint.targetRotation = Quaternion.Euler(Mathf.Max(-legSwing * 0.4f, 0), 0, 0);
        rightUpLegJoint.targetRotation = Quaternion.Euler(Mathf.Max(legSwing * 0.4f, 0), 0, 0);

        if (legSwing < 0)
        {
            leftLegJoint.targetRotation = Quaternion.Euler(-legSwing * 1.5f, 0, 0);
        }
        else
        {
            rightLegJoint.targetRotation = Quaternion.Euler(legSwing * 1.5f, 0, 0);
        }
    }

    private void MoveLeft()
    {
        var legSwing = Mathf.Sin(stepChronometer.Elapsed * currentSpeed) * (currentStepHeight * 0.8f + currentSlope * 2f);
        leftUpLegJoint.targetRotation = Quaternion.Euler(0, 0, Mathf.Max(legSwing, 0));

        if (legSwing < 0)
        {
            rightUpLegJoint.targetRotation = Quaternion.Euler(legSwing, 0, 0);
            rightLegJoint.targetRotation = Quaternion.Euler(-legSwing * 1.5f, 0, 0);
        }
        else
        {
            leftLegJoint.targetRotation = Quaternion.Euler(legSwing, 0, 0);
        }
    }

    private void MoveRight()
    {
        var legSwing = Mathf.Sin(stepChronometer.Elapsed * currentSpeed) * (currentStepHeight * 0.8f + currentSlope * 2f);
        rightUpLegJoint.targetRotation = Quaternion.Euler(0, 0, Mathf.Min(-legSwing, 0));

        if (legSwing < 0)
        {
            leftUpLegJoint.targetRotation = Quaternion.Euler(legSwing, 0, 0);
            leftLegJoint.targetRotation = Quaternion.Euler(-legSwing * 1.5f, 0, 0);
        }
        else
        {
            rightLegJoint.targetRotation = Quaternion.Euler(legSwing, 0, 0);
        }
    }

    private void SwingArms()
    {
        float armSwing = (Mathf.Sin(stepChronometer.Elapsed * currentSpeed) + 1f) * 0.5f * armSwingHeight;

        leftForeArmJoint.targetRotation = Quaternion.Euler(0, 0, 30f + armSwing);
        rightForeArmJoint.targetRotation = Quaternion.Euler(0, 0, 330f - (armSwingHeight - armSwing));
    }

    private void RotateArmToAim(ConfigurableJoint arm, ConfigurableJoint forearm)
    {
        arm.targetRotation = Quaternion.Euler(285f, 0f, 0f);
        forearm.targetRotation = Quaternion.Euler(8f, 0f, 260f);
    }

    private void BendLeg(ConfigurableJoint leftupleg, ConfigurableJoint leftleg)
    {
        leftupleg.targetRotation = Quaternion.Euler(285, 0, 0);
        leftleg.targetRotation = Quaternion.Euler(120, 0, 0);
    }

    private void BendLowerLeg(ConfigurableJoint leftleg)
    {
        leftleg.targetRotation = Quaternion.Euler(60, 0, 0);
    }

    private void RaiseArms(ConfigurableJoint leftArm, ConfigurableJoint rightArm)
    {
        leftArm.targetRotation = Quaternion.Euler(40, 285, 25);
        rightArm.targetRotation = Quaternion.Euler(40, -285, -25);
    }

    private void ResetLowerArms()
    {
        leftForeArmJoint.targetRotation = Quaternion.Euler(0, 0, 30);
        rightForeArmJoint.targetRotation = Quaternion.Euler(0, 0, 330);
    }

    private void ResetLeftLowerArm()
    {
        leftForeArmJoint.targetRotation = Quaternion.Euler(0, 0, 30);
    }

    private void ResetUpperArms()
    {
        rightArmJoint.targetRotation = Quaternion.Euler(300, 0, 0);
        leftArmJoint.targetRotation = Quaternion.Euler(300, 0, 0);
    }

    private void ResetLeftLeg()
    {
        leftUpLegJoint.targetRotation = Quaternion.Euler(0, 0, 0);
        leftLegJoint.targetRotation = Quaternion.Euler(0, 0, 0);
    }

    private void ResetRightLeg()
    {
        rightUpLegJoint.targetRotation = Quaternion.Euler(0, 0, 0);
        rightLegJoint.targetRotation = Quaternion.Euler(0, 0, 0);
    }

    private void ResetLegs()
    {
        if(playerState.CanCrouch && playerState.IsCrouching)
        {
            leftUpLegJoint.targetRotation = Quaternion.Euler(-70, 0, 0);
            leftLegJoint.targetRotation = Quaternion.Euler(120, 0, 0);
            rightUpLegJoint.targetRotation = Quaternion.Euler(-70, 0, 0);
            rightLegJoint.targetRotation = Quaternion.Euler(120, 0, 0);
        }
        else
        {
            leftUpLegJoint.targetRotation = Quaternion.Euler(0, 0, 0);
            leftLegJoint.targetRotation = Quaternion.Euler(0, 0, 0);
            rightUpLegJoint.targetRotation = Quaternion.Euler(0, 0, 0);
            rightLegJoint.targetRotation = Quaternion.Euler(0, 0, 0);
        }
    }
}
