using UnityEngine;

public class JointControl : MonoBehaviour
{
    [Header("References")]
    public PlayerState playerState;
    public GameObject hips;
    public Camera cam;

    [Header("Speed Settings")]
    public float currentSpeed;
    public float walkSpeed;
    public float sprintSpeed;

    [Header("Step Settings")]
    public float stepTime;
    public float stepHeight;
    public float stepSpeed;

    private bool stepFlag = false;
    private Timer stepTimer;

    [Header("Leg Joints")]
    public ConfigurableJoint leftUpLegJoint;
    public ConfigurableJoint leftLegJoint;
    public ConfigurableJoint leftFootJoint;
    public ConfigurableJoint rightUpLegJoint;
    public ConfigurableJoint rightLegJoint;
    public ConfigurableJoint rightFootJoint;

    [Header("Spine Joint")]
    public ConfigurableJoint spineJoint;

    private Quaternion defaultSpineTargetRotation;

    private void Start()
    {
        defaultSpineTargetRotation = spineJoint.targetRotation;
        stepTimer = new Timer(stepTime);
    }

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
        }
    }

    private void MoveForward()
    {
        //calculate the slope to determine the step height
        float angle = 0;
        RaycastHit hit;
        if (Physics.Raycast(hips.transform.position, -hips.transform.up, out hit, 10f, LayerMask.GetMask("Ground")))
        {
            angle = Vector3.Angle(hips.transform.up, hit.transform.up);
        }

        Debug.Log("Angle: " + angle);

        var legSwing = Mathf.Sin(Time.time * stepSpeed) * (stepHeight + angle * 2f);

        leftUpLegJoint.targetRotation = Quaternion.Euler(Mathf.Min(legSwing, 0), 0, 0);
        rightUpLegJoint.targetRotation = Quaternion.Euler(Mathf.Min(-legSwing, 0), 0, 0);
        if(legSwing < 0)
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
}
