using Mirror;
using UnityEngine;

public class LegMovement : NetworkBehaviour
{
    public float speed;
    public float maxAngle;
    public float angleMultiplier = 1.0f;

    public GameObject leftUpLeg;
    public GameObject leftLeg;
    public GameObject rightUpLeg;
    public GameObject rightLeg;

    private ConfigurableJoint leftUpLegJoint;
    private ConfigurableJoint rightUpLegJoint;
    private ConfigurableJoint rightLegJoint;
    private ConfigurableJoint leftLegJoint;

    public override void OnStartClient()
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
                break;
            case "Jump":
                Jump();
                break;
            case "Idle":
                ResetLegs();
                break;
        }
    }

    private void MoveForward()
    {
        float t = Mathf.PingPong(Time.time * speed, 1);
        float angle = Mathf.Lerp(-maxAngle, maxAngle, t);
        leftUpLegJoint.targetRotation = Quaternion.Euler(angle * angleMultiplier, 0f, 0f);
        leftLegJoint.targetRotation = Quaternion.Euler(Mathf.Max(-angle, 0f), 0f, 0f);
        rightUpLegJoint.targetRotation = Quaternion.Euler(-angle * angleMultiplier, 0f, 0f);
        rightLegJoint.targetRotation = Quaternion.Euler(Mathf.Max(angle, 0f), 0f, 0f);
    }

    private void MoveBackward()
    {
        float t = Mathf.PingPong(Time.time * speed, 1);
        float angle = Mathf.Lerp(-maxAngle, maxAngle, t);
        leftUpLegJoint.targetRotation = Quaternion.Euler(Mathf.Max(angle * 0.5f * angleMultiplier, 0f), 0f, 0f);
        leftLegJoint.targetRotation = Quaternion.Euler(Mathf.Max(angle, 0f), 0f, 0f);
        rightUpLegJoint.targetRotation = Quaternion.Euler(Mathf.Max(-angle * 0.5f * angleMultiplier, 0f), 0f, 0f);
        rightLegJoint.targetRotation = Quaternion.Euler(Mathf.Max(-angle, 0f), 0f, 0f);
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
