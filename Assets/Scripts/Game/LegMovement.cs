using Mirror;
using UnityEngine;

public class LegMovement : NetworkBehaviour
{
    enum BalanceState
    {
        Behind,
        Front,
        Balanced
    }

    private float currentSpeed;
    [SerializeField]
    private float walkSpeed;
    [SerializeField]
    private float sprintSpeed;
    [SerializeField]
    private float maxAngle;
    [SerializeField]
    private float lowerLegAngleMultiplier;

    public GameObject leftUpLeg;
    public GameObject leftLeg;
    public GameObject rightUpLeg;
    public GameObject rightLeg;

    private ConfigurableJoint leftUpLegJoint;
    private ConfigurableJoint rightUpLegJoint;
    private ConfigurableJoint rightLegJoint;
    private ConfigurableJoint leftLegJoint;

    private float jumpAngle = 0f;
    [SerializeField]
    private float jumpSpeed = 60f;


    void Start()
    {
        leftUpLegJoint = leftUpLeg.GetComponent<ConfigurableJoint>();
        rightUpLegJoint = rightUpLeg.GetComponent<ConfigurableJoint>();
        rightLegJoint = rightLeg.GetComponent<ConfigurableJoint>();
        leftLegJoint = leftLeg.GetComponent<ConfigurableJoint>();

    }

    void Update()
    {
        if (isLocalPlayer)
        {
            if( Input.GetKey(KeyCode.LeftShift))
            {
                currentSpeed = sprintSpeed;
            }
            else
            {
                currentSpeed = walkSpeed;
            }

            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D))
            {
                MoveForward();
            }
            else if (Input.GetKey(KeyCode.Space))
            {
                jumpAngle = Mathf.MoveTowards(jumpAngle, maxAngle, Time.deltaTime * jumpSpeed);
                Debug.Log(jumpAngle);

                float upperLegAngle = -jumpAngle;
                float lowerLegAngle = Mathf.Max(jumpAngle, 0f) * 2f;

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
                jumpAngle = 0f;
            }
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
