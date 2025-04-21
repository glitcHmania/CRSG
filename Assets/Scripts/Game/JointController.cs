using UnityEngine;

public class JointController : MonoBehaviour
{
    [Header("References")]
    public PlayerState playerState;
    public GameObject hips;
    public Camera cam;

    [Header("Speed Settings")]
    public float walkSpeed;
    public float sprintSpeed;

    [Header("Step Settings")]
    public float stepHeight;

    [Header("Arm Settings")]
    public float armSwingHeight;

    [Header("Leg Joints")]
    public ConfigurableJoint leftUpLegJoint;
    public ConfigurableJoint leftLegJoint;
    public ConfigurableJoint leftFootJoint;
    public ConfigurableJoint rightUpLegJoint;
    public ConfigurableJoint rightLegJoint;
    public ConfigurableJoint rightFootJoint;

    [Header("Arm Joints")]
    public ConfigurableJoint leftArmJoint;
    public ConfigurableJoint leftForeArmJoint;
    public ConfigurableJoint rightArmJoint;
    public ConfigurableJoint rightForeArmJoint;

    [Header("Spine Joint")]
    public ConfigurableJoint spineJoint;

    private float currentSpeed;
    private float currentSlope;
    private float currentStepHeight;
    private Quaternion defaultSpineTargetRotation;
    private Chronometer stepChronometer;
    public RagdollController ragdollController;

    private void Start()
    {
        defaultSpineTargetRotation = spineJoint.targetRotation;
        stepChronometer = new Chronometer();
    }

    void Update()
    {
        RaycastHit hit;
        if (Physics.Raycast(hips.transform.position, -hips.transform.up, out hit, 10f, LayerMask.GetMask("Ground")))
        {
            currentSlope = Vector3.Angle(hips.transform.up, hit.transform.up); // calculate slope angle for step height adjustment
        }

        if (playerState.movementState == PlayerState.Movement.Running)
        {
            currentSpeed = sprintSpeed;
            currentStepHeight = stepHeight + 20f;
        }
        else
        {
            currentSpeed = walkSpeed;
            currentStepHeight = stepHeight;
        }

        if (playerState.isAiming)
        {
            spineJoint.targetRotation = Quaternion.Euler(-cam.transform.eulerAngles.x, 0f, 0f); // rotate spine to aim
            if (playerState.isArmed)
            {
                RotateArmToAim(rightArmJoint, rightForeArmJoint);
            }
        }
        else if (spineJoint.targetRotation != defaultSpineTargetRotation)
        {
            spineJoint.targetRotation = defaultSpineTargetRotation; // reset spine rotation if not aiming
        }

        if (playerState.IsMoving && !ChatBehaviour.Instance.IsInputActive)
        {
            stepChronometer.Update();

            if (!playerState.isAiming)
            {
                MoveForward();
                SwingArms();
                ResetArms();
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
            BendLeg(leftUpLegJoint, leftLegJoint);
            BendLeg(rightUpLegJoint, rightLegJoint);
        }
        else if (playerState.movementState == PlayerState.Movement.Falling)
        {
            MoveForward();
            RaiseArms(leftArmJoint, rightArmJoint);
        }
        else
        {
            ResetLegs();

            if (!playerState.isAiming)
            {
                ResetArms();
            }

            stepChronometer.Reset();
        }
    }

    private void MoveForward()
    {
        var legSwing = Mathf.Sin(stepChronometer.Elapsed * currentSpeed) * (currentStepHeight + currentSlope * 2f);

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

    private void MoveBackward()
    {
        var legSwing = Mathf.Sin(stepChronometer.Elapsed * currentSpeed) * (currentStepHeight * 0.6f + currentSlope * 2f);

        leftUpLegJoint.targetRotation = Quaternion.Euler(Mathf.Max(-legSwing, 0), 0, 0);
        rightUpLegJoint.targetRotation = Quaternion.Euler(Mathf.Max(legSwing, 0), 0, 0);

        if (legSwing < 0)
        {
            leftLegJoint.targetRotation = Quaternion.Euler(-legSwing, 0, 0);
        }
        else
        {
            rightLegJoint.targetRotation = Quaternion.Euler(legSwing, 0, 0);
        }
    }

    private void MoveLeft()
    {
        var legSwing = Mathf.Sin(stepChronometer.Elapsed * currentSpeed) * (currentStepHeight + currentSlope * 2f);
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
        var legSwing = Mathf.Sin(stepChronometer.Elapsed * currentSpeed) * (currentStepHeight + currentSlope * 2f);
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
        forearm.targetRotation = Quaternion.Euler(15f, 0f, 250f);
    }

    private void BendLeg(ConfigurableJoint leftupleg, ConfigurableJoint leftleg)
    {
        leftupleg.targetRotation = Quaternion.Euler(285, 0, 0);
        leftleg.targetRotation = Quaternion.Euler(120, 0, 0);
    }

    private void RaiseArms(ConfigurableJoint leftArm, ConfigurableJoint rightArm)
    {
        leftArm.targetRotation = Quaternion.Euler(60, 250, 0);
        rightArm.targetRotation = Quaternion.Euler(60, 110, 0);
    }

    private void ResetLeftArm()
    {
        leftArmJoint.targetRotation = Quaternion.Euler(300, 0, 0);
        leftForeArmJoint.targetRotation = Quaternion.Euler(0, 0, 30);
    }

    private void ResetRightArm()
    {
        rightArmJoint.targetRotation = Quaternion.Euler(300, 0, 0);
        rightForeArmJoint.targetRotation = Quaternion.Euler(0, 0, 330);
    }

    private void ResetArms()
    {
        ResetLeftArm();
        ResetRightArm();
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
        ResetLeftLeg();
        ResetRightLeg();
    }
}
