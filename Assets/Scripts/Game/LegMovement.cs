using Mirror;
using UnityEngine;

public class LegMovement : NetworkBehaviour
{
    public float speed;
    public float maxAngle;
    public float angleMultiplier = 1.0f;

    [SerializeField]
    float speed = 1.0f;
    [SerializeField]
    float maxAngle = 90.0f;
    [SerializeField]
    float lowerLegAngleMultiplier = 0.6f;

    public GameObject leftUpLeg;
    public GameObject leftLeg;
    public GameObject rightUpLeg;
    public GameObject rightLeg;

    private ConfigurableJoint leftUpLegJoint;
    private ConfigurableJoint rightUpLegJoint;
    private ConfigurableJoint rightLegJoint;
    private ConfigurableJoint leftLegJoint;

    private float jumpAngle = 0f;
    public float jumpSpeed = 60f;

    // Start is called before the first frame update
    void Start()
    {
        base.OnStartClient();
        InitializeJoints();
    }

    private void InitializeJoints()
    {
        if (leftUpLeg != null) leftUpLegJoint = leftUpLeg.GetComponent<ConfigurableJoint>();
        if (rightUpLeg != null) rightUpLegJoint = rightUpLeg.GetComponent<ConfigurableJoint>();
        if (rightLeg != null) rightLegJoint = rightLeg.GetComponent<ConfigurableJoint>();
        if (leftLeg != null) leftLegJoint = leftLeg.GetComponent<ConfigurableJoint>();

        if (leftUpLegJoint == null || rightUpLegJoint == null || rightLegJoint == null || leftLegJoint == null)
        {
            Debug.LogError("One or more ConfigurableJoint components are missing!", this);
        }
    }

    private void Update()
    {
        if (!isLocalPlayer) return; // only the local player processes input

        if (Input.GetKey(KeyCode.W))
            CmdMove("Forward");
        else if (Input.GetKey(KeyCode.S))
            CmdMove("Backward");
        else if (Input.GetKey(KeyCode.A))
            CmdMove("Left");
        else if (Input.GetKey(KeyCode.D))
            CmdMove("Right");
        else if (Input.GetKey(KeyCode.Space))
            CmdMove("Jump");
        else
            CmdMove("Idle");

        if (Input.GetKey(KeyCode.LeftShift))
        {
            angleMultiplier = 1.4f;
        }
        else
        {
            angleMultiplier = 1.0f;
        }
    }

    [Command]
    private void CmdMove(string direction)
    {
        RpcMove(direction);
    }

    [ClientRpc]
    private void RpcMove(string direction)
    {
        switch (direction)
        {
            case "Forward":
                MoveForward();
                break;
            case "Backward":
                MoveBackward();
                break;
            case "Left":
                MoveLeft();
                break;
            case "Right":
                MoveRight();
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
        float stepDuration = 1f / speed;
        float cycleTime = Time.time % (stepDuration * 2f);
        bool isRightLegMoving = cycleTime < stepDuration;

        float t = Mathf.Clamp01((cycleTime % stepDuration) / stepDuration);
        float easedT = Mathf.Sin(t * Mathf.PI);
        float angle = easedT * maxAngle;
        float lowerLegAngle = Mathf.Max(angle * lowerLegAngleMultiplier, 0f);

        // Reset both legs to default position
        leftUpLegJoint.targetRotation = Quaternion.identity;
        leftLegJoint.targetRotation = Quaternion.identity;
        rightUpLegJoint.targetRotation = Quaternion.identity;
        rightLegJoint.targetRotation = Quaternion.identity;

        // Apply movement to the active leg
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
        float t = Mathf.PingPong(Time.time * speed, 1);
        float angle = Mathf.Lerp(-maxAngle, maxAngle, t);
        leftUpLegJoint.targetRotation = Quaternion.Euler(Mathf.Max(angle * 0.5f, 0f), 0f, 0f);
        leftLegJoint.targetRotation = Quaternion.Euler(Mathf.Max(angle, 0f) * 0.5f, 0f, 0f);
        rightUpLegJoint.targetRotation = Quaternion.Euler(Mathf.Max(-angle * 0.5f, 0f), 0f, 0f);
        rightLegJoint.targetRotation = Quaternion.Euler(Mathf.Max(-angle, 0f) * 0.5f, 0f, 0f);
    }


    private void MoveLeft()
    {
        float t = Mathf.PingPong(Time.time * speed, 1);
        float angle = Mathf.Lerp(-maxAngle, maxAngle, t);
        if (angle >= 0)
        {
            leftUpLegJoint.targetRotation = Quaternion.Euler(0f, 0f, angle * 0.5f * angleMultiplier);
            leftLegJoint.targetRotation = Quaternion.Euler(Mathf.Max(angle, 0f), 0f, 0f);
        }
        else
        {
            rightLegJoint.targetRotation = Quaternion.Euler(Mathf.Max(-angle, 0f), 0f, 0f);
        }
    }

    private void MoveRight()
    {
        float t = Mathf.PingPong(Time.time * speed, 1);
        float angle = Mathf.Lerp(-maxAngle, maxAngle, t);
        if (angle >= 0)
        {
            rightUpLegJoint.targetRotation = Quaternion.Euler(0f, 0f, -angle * 0.5f * angleMultiplier);
            rightLegJoint.targetRotation = Quaternion.Euler(Mathf.Max(angle, 0f), 0f, 0f);
        }
        else
        {
            leftLegJoint.targetRotation = Quaternion.Euler(Mathf.Max(-angle * 1.5f, 0f), 0f, 0f);
        }
    }

    private void Jump()
    {
        float t = Mathf.PingPong(Time.time * speed, 1);
        float angle = Mathf.Lerp(0, maxAngle, t);
        leftUpLegJoint.targetRotation = Quaternion.Euler(-angle, 0f, 0f);
        leftLegJoint.targetRotation = Quaternion.Euler(Mathf.Max(angle, 0f), 0f, 0f);
        rightUpLegJoint.targetRotation = Quaternion.Euler(-angle, 0f, 0f);
        rightLegJoint.targetRotation = Quaternion.Euler(Mathf.Max(angle, 0f), 0f, 0f);
    }

    private void ResetLegs()
    {
        leftUpLegJoint.targetRotation = Quaternion.Euler(0, 0, 0);
        rightUpLegJoint.targetRotation = Quaternion.Euler(0, 0, 0);
        rightLegJoint.targetRotation = Quaternion.Euler(0, 0, 0);
        leftLegJoint.targetRotation = Quaternion.Euler(0, 0, 0);
    }
}
