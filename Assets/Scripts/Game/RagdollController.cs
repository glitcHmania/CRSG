using System.Collections.Generic;
using UnityEngine;

public class RagdollController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerState playerState;
    [SerializeField] private Rigidbody hipsRigidbody;
    [SerializeField] private Rigidbody spineRigidbody;
    [SerializeField] private ConfigurableJoint[] joints;

    [Header("Settings")]
    [SerializeField] private float ragdollDuration;
    [SerializeField] private float ragdollStiffness;

    private List<float> initialSpringValues;
    private List<float> initialDamperValues;
    private Timer ragdollTimer;

    private void Start()
    {
        initialSpringValues = new List<float>();
        initialDamperValues = new List<float>();

        playerState.IsRagdoll = false;

        foreach (var joint in joints)
        {
            initialSpringValues.Add(joint.angularXDrive.positionSpring);
        }

        foreach (var joint in joints)
        {
            initialDamperValues.Add(joint.angularXDrive.positionDamper);
        }

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
        foreach (var joint in joints)
        {
            joint.angularXDrive = new JointDrive { positionSpring = stiffness, maximumForce = 3.402823e+38f };
            joint.angularYZDrive = new JointDrive { positionSpring = stiffness, maximumForce = 3.402823e+38f };
        }
    }

    public void ResetRagdollStifness()
    {
        for (int i = 0; i < joints.Length; i++)
        {
            joints[i].angularXDrive = new JointDrive { positionSpring = initialSpringValues[i], positionDamper = initialDamperValues[i], maximumForce = 3.402823e+38f };
            joints[i].angularYZDrive = new JointDrive { positionSpring = initialSpringValues[i], positionDamper = initialDamperValues[i], maximumForce = 3.402823e+38f };
        }
    }

    public void DisableBalance()
    {
        playerState.IsUnbalanced = true;
        joints[0].angularXDrive = new JointDrive { positionSpring = 0, maximumForce = 3.402823e+38f };
        joints[0].angularYZDrive = new JointDrive { positionSpring = 0, maximumForce = 3.402823e+38f };
    }

    public void EnableBalance()
    {
        playerState.IsUnbalanced = false;
        joints[0].angularXDrive = new JointDrive { positionSpring = initialSpringValues[0], positionDamper = initialDamperValues[0], maximumForce = 3.402823e+38f };
        joints[0].angularYZDrive = new JointDrive { positionSpring = initialSpringValues[0], positionDamper = initialDamperValues[0], maximumForce = 3.402823e+38f };
    }
}