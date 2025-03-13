using UnityEngine;

public class LegMovement : MonoBehaviour
{
    enum BalanceState
    {
        Behind,
        Front,
        Balanced
    }

    [SerializeField]
    float speed = 1.0f;
    [SerializeField]
    float maxAngle = 90.0f;

    public GameObject leftUpLeg;
    public GameObject leftLeg;
    public GameObject rightUpLeg;
    public GameObject rightLeg;

    private ConfigurableJoint leftUpLegJoint;
    private ConfigurableJoint rightUpLegJoint;
    private ConfigurableJoint rightLegJoint;
    private ConfigurableJoint leftLegJoint;

    // Start is called before the first frame update
    void Start()
    {
        leftUpLegJoint = leftUpLeg.GetComponent<ConfigurableJoint>();
        rightUpLegJoint = rightUpLeg.GetComponent<ConfigurableJoint>();
        rightLegJoint = rightLeg.GetComponent<ConfigurableJoint>();
        leftLegJoint = leftLeg.GetComponent<ConfigurableJoint>();

    }

    // Update is called once per frame
    void Update()
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
        else if (Input.GetKey(KeyCode.Space))
        {
            float t = Mathf.PingPong(Time.time * speed, 1);
            float angle = Mathf.Lerp(0, maxAngle, t);
            leftUpLegJoint.targetRotation = Quaternion.Euler(-angle, 0f, 0f);
            leftLegJoint.targetRotation = Quaternion.Euler(Mathf.Max(angle, 0f), 0f, 0f);
            rightUpLegJoint.targetRotation = Quaternion.Euler(-angle, 0f, 0f);
            rightLegJoint.targetRotation = Quaternion.Euler(Mathf.Max(angle, 0f), 0f, 0f);
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
        float t = Mathf.PingPong(Time.time * speed, 1);
        float angle = Mathf.Lerp(-maxAngle, maxAngle, t);
        leftUpLegJoint.targetRotation = Quaternion.Euler(angle, 0f, 0f);
        leftLegJoint.targetRotation = Quaternion.Euler(Mathf.Max(-angle, 0f), 0f, 0f);
        rightUpLegJoint.targetRotation = Quaternion.Euler(-angle, 0f, 0f);
        rightLegJoint.targetRotation = Quaternion.Euler(Mathf.Max(angle, 0f), 0f, 0f);
    }

    private void MoveBackward()
    {
        float t = Mathf.PingPong(Time.time * speed, 1);
        float angle = Mathf.Lerp(-maxAngle, maxAngle, t);
        leftUpLegJoint.targetRotation = Quaternion.Euler(Mathf.Max(angle * 0.5f, 0f), 0f, 0f);
        leftLegJoint.targetRotation = Quaternion.Euler(Mathf.Max(angle, 0f), 0f, 0f);
        rightUpLegJoint.targetRotation = Quaternion.Euler(Mathf.Max(-angle * 0.5f, 0f), 0f, 0f);
        rightLegJoint.targetRotation = Quaternion.Euler(Mathf.Max(-angle, 0f), 0f, 0f);
    }

    private void MoveLeft()
    {
        float t = Mathf.PingPong(Time.time * speed, 1);
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
        float t = Mathf.PingPong(Time.time * speed, 1);
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

    private BalanceState CheckBalance()
    {
        var hipPosition = transform.position;
        var leftLegPosition = leftLeg.transform.position;
        var rightLegPosition = rightLeg.transform.position;
        var faceDirection = transform.forward;
        var hipRotation = transform.rotation;

        //direction vector from hip to left leg
        var leftLegDirection = leftLegPosition - hipPosition;
        //direction vector from hip to right leg
        var rightLegDirection = rightLegPosition - hipPosition;

        //dot product between face direction and left leg direction
        var leftLegDot = Vector3.Dot(faceDirection, leftLegDirection);

        //dot product between face direction and right leg direction
        var rightLegDot = Vector3.Dot(faceDirection, rightLegDirection);

        //if both dot products are negative not balanced
        if (leftLegDot < 0 && rightLegDot < 0)
        {
            Debug.Log("LEGS ARE BEHIND THE HIP");
            return BalanceState.Behind;
        }
        else if (leftLegDot > 0 && rightLegDot > 0)
        {
            Debug.Log("LEGS ARE IN FRONT OF THE HIP");
            return BalanceState.Front;
        }
        else
        {
            Debug.Log("+");
            return BalanceState.Balanced;
        }

    }
}
