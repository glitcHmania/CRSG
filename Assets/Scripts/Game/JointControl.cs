using UnityEngine;

public class JointControl : MonoBehaviour
{
    [Header("References")]
    public PlayerState playerState;
    public GameObject hips;
    public Camera cam;

    [Header("Speed Settings")]
    public float walkSpeed;
    public float sprintSpeed;

    [Header("Step Settings")]
    public float stepTime;
    public float stepHeight;
    public float stepSpeed;

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
    public ConfigurableJoint leftForeArmJoint;
    public ConfigurableJoint rightForeArmJoint;


    [Header("Spine Joint")]
    public ConfigurableJoint spineJoint;

    private float currentSpeed;
    private float currentSlope;
    private float currentStepHeight;
    private Quaternion defaultSpineTargetRotation;

    private void Start()
    {
        defaultSpineTargetRotation = spineJoint.targetRotation;
    }

    void Update()
    {
        RaycastHit hit;
        if (Physics.Raycast(hips.transform.position, -hips.transform.up, out hit, 10f, LayerMask.GetMask("Ground")))
        {
            currentSlope = Vector3.Angle(hips.transform.up, hit.transform.up);
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
            spineJoint.targetRotation = Quaternion.Euler(-cam.transform.eulerAngles.x, 0f, 0f);
        }
        else if (spineJoint.targetRotation != defaultSpineTargetRotation)
        {
            spineJoint.targetRotation = defaultSpineTargetRotation;
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
        else
        {
            leftUpLegJoint.targetRotation = Quaternion.Euler(0, 0, 0);
            rightUpLegJoint.targetRotation = Quaternion.Euler(0, 0, 0);
            rightLegJoint.targetRotation = Quaternion.Euler(0, 0, 0);
            leftLegJoint.targetRotation = Quaternion.Euler(0, 0, 0);
            leftForeArmJoint.targetRotation = Quaternion.Euler(0, 0, 30);
            rightForeArmJoint.targetRotation = Quaternion.Euler(0, 0, 330);
        }
    }

    private void MoveForward()
    {
        SwingArms();
        //calculate the slope to determine the step height

        Debug.Log("Angle: " + currentSlope);

        var legSwing = Mathf.Sin(Time.time * currentSpeed) * (currentStepHeight + currentSlope * 2f);

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

    }

    private void MoveLeft()
    {

    }

    private void MoveRight()
    {

    }

    private void SwingArms()
    {
        float armSwing = (Mathf.Sin(Time.time * currentSpeed) + 1f) / 2f * armSwingHeight;

        leftForeArmJoint.targetRotation = Quaternion.Euler(0, 0, 30f + armSwing);
        rightForeArmJoint.targetRotation = Quaternion.Euler(0, 0, 330f - (armSwingHeight - armSwing));
    }
}
