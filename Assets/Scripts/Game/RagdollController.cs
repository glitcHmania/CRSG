using UnityEngine;

public class RagdollController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerState playerState;
    [SerializeField] private Rigidbody hipsRigidbody;
    [SerializeField] private Rigidbody spineRigidbody;
    [SerializeField] private ConfigurableJoint hipJoint;
    [SerializeField] private ConfigurableJoint[] legJoints;
    [SerializeField] private ConfigurableJoint[] otherJoints;

    [Header("Settings")]
    [SerializeField] private float ragdollDuration;
    [SerializeField] private float ragdollStiffness;

    private float initialHipSpring;
    private float initialLegSpring;
    private Timer ragdollTimer;

    private void Start()
    {
        playerState.IsRagdoll = false;

        initialHipSpring = hipJoint.angularXDrive.positionSpring;
        initialLegSpring = legJoints[0].angularXDrive.positionSpring;

        ragdollTimer = new Timer(ragdollDuration, DisableRagdoll);
    }

    private void Update()
    {
        if (ChatBehaviour.Instance.IsInputActive) return;

        if (playerState.IsRagdoll && playerState.IsGrounded)
        {
            ragdollTimer.Update();
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            if (playerState.IsRagdoll)
            {
                DisableRagdoll();
            }
            else
            {
                EnableRagdoll();
            }
        }

        if (playerState.Numbness > 0)
        {
            playerState.Numbness -= Time.deltaTime * 5f;
        }
        else
        {
            playerState.Numbness = 0;
        }

        if (playerState.Numbness > 100f)
        {
            EnableRagdoll();
            playerState.Numbness = 0;
        }
    }

    public void EnableRagdoll()
    {
        playerState.IsRagdoll = true;
        playerState.IsUnbalanced = true;

        //hipsRigidbody.mass = 0.3f;
        //spineRigidbody.mass = 0.3f;

        DisableBalance();
        SetRagdollStiffness(ragdollStiffness);
    }

    public void DisableRagdoll()
    {
        playerState.IsRagdoll = false;
        playerState.IsUnbalanced = false;

        //hipsRigidbody.mass = 2f;
        //spineRigidbody.mass = 1f;

        EnableBalance();
        ResetRagdollStifness();
        ragdollTimer.Reset();
    }

    public void SetRagdollStiffness(float stiffness)
    {
        foreach (var joint in otherJoints)
        {
            joint.angularXDrive = new JointDrive { positionSpring = stiffness, maximumForce = 3.402823e+38f };
            joint.angularYZDrive = new JointDrive { positionSpring = stiffness, maximumForce = 3.402823e+38f };
        }

        foreach (var joint in legJoints)
        {
            joint.angularXDrive = new JointDrive { positionSpring = stiffness, maximumForce = 3.402823e+38f };
            joint.angularYZDrive = new JointDrive { positionSpring = stiffness, maximumForce = 3.402823e+38f };
        }
    }

    public void ResetRagdollStifness()
    {
        foreach (var joint in otherJoints)
        {
            joint.angularXDrive = new JointDrive { positionSpring = 100, maximumForce = 3.402823e+38f };
            joint.angularYZDrive = new JointDrive { positionSpring = 100, maximumForce = 3.402823e+38f };
        }

        foreach (var joint in legJoints)
        {
            joint.angularXDrive = new JointDrive { positionSpring = initialLegSpring, maximumForce = 3.402823e+38f };
            joint.angularYZDrive = new JointDrive { positionSpring = initialLegSpring, maximumForce = 3.402823e+38f };
        }
    }

    public void DisableBalance()
    {
        playerState.IsUnbalanced = true;
        hipJoint.angularXDrive = new JointDrive { positionSpring = 0, maximumForce = 3.402823e+38f };
        hipJoint.angularYZDrive = new JointDrive { positionSpring = 0, maximumForce = 3.402823e+38f };
    }

    public void EnableBalance()
    {
        playerState.IsUnbalanced = false;
        hipJoint.angularXDrive = new JointDrive { positionSpring = initialHipSpring, maximumForce = 3.402823e+38f };
        hipJoint.angularYZDrive = new JointDrive { positionSpring = initialHipSpring, maximumForce = 3.402823e+38f };
    }
}