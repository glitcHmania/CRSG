using UnityEngine;

public class RagdollController : MonoBehaviour
{
    [Header("References")]
    public PlayerState playerState;
    public Rigidbody hipsRigidbody;
    public Rigidbody spineRigidbody;
    public ConfigurableJoint hipJoint;
    public ConfigurableJoint[] legJoints;
    public ConfigurableJoint[] otherJoints;

    [Header("Settings")]
    public float ragdollDuration;
    public float ragdollStiffness;

    private float initialHipSpring;
    private float initialLegSpring;
    private Timer ragdollTimer;

    private void Start()
    {
        playerState.isRagdoll = false;

        initialHipSpring = hipJoint.angularXDrive.positionSpring;
        initialLegSpring = legJoints[0].angularXDrive.positionSpring;

        ragdollTimer = new Timer(ragdollDuration, DisableRagdoll);
    }

    private void Update()
    {
        if (ChatBehaviour.Instance.IsInputActive) return;

        if (playerState.isRagdoll && playerState.isGrounded)
        {
            ragdollTimer.Update();
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            if (playerState.isRagdoll)
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
        playerState.isRagdoll = true;
        playerState.isUnbalanced = true;

        //hipsRigidbody.mass = 0.3f;
        //spineRigidbody.mass = 0.3f;

        DisableBalance();
        SetRagdollStiffness(ragdollStiffness);
    }

    public void DisableRagdoll()
    {
        playerState.isRagdoll = false;
        playerState.isUnbalanced = false;

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
        playerState.isUnbalanced = true;
        hipJoint.angularXDrive = new JointDrive { positionSpring = 0, maximumForce = 3.402823e+38f };
        hipJoint.angularYZDrive = new JointDrive { positionSpring = 0, maximumForce = 3.402823e+38f };
    }

    public void EnableBalance()
    {
        playerState.isUnbalanced = false;
        hipJoint.angularXDrive = new JointDrive { positionSpring = initialHipSpring, maximumForce = 3.402823e+38f };
        hipJoint.angularYZDrive = new JointDrive { positionSpring = initialHipSpring, maximumForce = 3.402823e+38f };
    }
}