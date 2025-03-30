using UnityEngine;

public class RagdollControl : MonoBehaviour
{
    public PlayerState playerState;
    public float ragdollStiffness;
    public Rigidbody hipsRigidbody;
    public Rigidbody spineRigidbody;

    [SerializeField]
    ConfigurableJoint hipJoint;
    [SerializeField]
    ConfigurableJoint[] legJoints;
    [SerializeField]
    ConfigurableJoint[] otherJoints;

    private float initialXPositionSpring;
    private float initialYZPositionDamper;


    private void Start()
    {
        initialXPositionSpring = hipJoint.angularXDrive.positionSpring;
        initialYZPositionDamper = hipJoint.angularYZDrive.positionSpring;
    }

    private void Update()
    {
        //if g is pressed, toggle ragdoll
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
    }

    public void ActivateRagdoll()
    {
        hipsRigidbody.mass = 0.3f;
        spineRigidbody.mass = 0.3f;

        playerState.isRagdoll = true;
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
        hipsRigidbody.mass = 2f;
        spineRigidbody.mass = 1f;

        playerState.isRagdoll = false;
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