using UnityEngine;

public class RagdollControl : MonoBehaviour
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

    private float initialXPositionSpring;
    private float initialYZPositionDamper;
    private Timer ragdollTimer;


    private void Start()
    {
        playerState.isRagdoll = false;

        initialXPositionSpring = hipJoint.angularXDrive.positionSpring;
        initialYZPositionDamper = hipJoint.angularYZDrive.positionSpring;

        ragdollTimer = new Timer(ragdollDuration, DeactivateRagdoll);
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
                DeactivateRagdoll();
            }
            else
            {
                ActivateRagdoll();
            }
        }

        if (playerState.Numbness > 0)
        {
            playerState.Numbness -= Time.deltaTime * 1f;
        }
        else
        {
            playerState.Numbness = 0;
        }

        if (playerState.Numbness > 100f)
        {
            ActivateRagdoll();
            playerState.Numbness = 0;
        }
    }

    public void ActivateRagdoll()
    {
        playerState.isRagdoll = true;

        //hipsRigidbody.mass = 0.3f;
        //spineRigidbody.mass = 0.3f;

        hipJoint.angularXDrive = new JointDrive { positionSpring = 0, maximumForce = 3.402823e+38f };
        hipJoint.angularYZDrive = new JointDrive { positionSpring = 0, maximumForce = 3.402823e+38f };

        foreach (var joint in otherJoints)
        {
            joint.angularXDrive = new JointDrive { positionSpring = ragdollStiffness, maximumForce = 3.402823e+38f };
            joint.angularYZDrive = new JointDrive { positionSpring = ragdollStiffness, maximumForce = 3.402823e+38f };
        }

        foreach (var joint in legJoints)
        {
            joint.angularXDrive = new JointDrive { positionSpring = ragdollStiffness, maximumForce = 3.402823e+38f };
            joint.angularYZDrive = new JointDrive { positionSpring = ragdollStiffness, maximumForce = 3.402823e+38f };
        }
    }

    public void DeactivateRagdoll()
    {
        playerState.isRagdoll = false;

        //hipsRigidbody.mass = 2f;
        //spineRigidbody.mass = 1f;

        ragdollTimer.Reset();

        ResetRagdollStifness();
    }

    public void SetRagdollStiffness(float stiffness)
    {
        hipJoint.angularXDrive = new JointDrive { positionSpring = stiffness, maximumForce = 3.402823e+38f };
        hipJoint.angularYZDrive = new JointDrive { positionSpring = stiffness, maximumForce = 3.402823e+38f };

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
        hipJoint.angularXDrive = new JointDrive { positionSpring = initialXPositionSpring, maximumForce = 3.402823e+38f };
        hipJoint.angularYZDrive = new JointDrive { positionSpring = initialYZPositionDamper, maximumForce = 3.402823e+38f };

        foreach (var joint in otherJoints)
        {
            joint.angularXDrive = new JointDrive { positionSpring = 100, maximumForce = 3.402823e+38f };
            joint.angularYZDrive = new JointDrive { positionSpring = 100, maximumForce = 3.402823e+38f };
        }

        foreach (var joint in legJoints)
        {
            joint.angularXDrive = new JointDrive { positionSpring = 1000, maximumForce = 3.402823e+38f };
            joint.angularYZDrive = new JointDrive { positionSpring = 1000, maximumForce = 3.402823e+38f };
        }
    }
}