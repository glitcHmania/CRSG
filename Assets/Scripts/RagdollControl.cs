using UnityEngine;

public class RagdollControl : MonoBehaviour
{
    public bool ragdollActive = false;
    public ConfigurableJoint hipJoint;
    public ConfigurableJoint mainJoint;
    public ConfigurableJoint[] otherJoints;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            if (ragdollActive)
            {
                ragdollActive = false;
                DeactivateRagdoll();
            }
            else
            {
                ragdollActive = true;
                ActivateRagdoll();
            }
        }
    }

    private void ActivateRagdoll()
    {
        SetPositionDamper(mainJoint, 0f);

        foreach (ConfigurableJoint joint in otherJoints)
        {
            SetPositionSpring(joint, 30f);
        }

        FreeHipAxis();
    }

    private void DeactivateRagdoll()
    {
        SetPositionDamper(mainJoint, 1000f);

        foreach (ConfigurableJoint joint in otherJoints)
        {
            SetPositionSpring(joint, 300f);
        }

        LockHipAxis();
    }

    private void SetPositionDamper(ConfigurableJoint joint, float damper)
    {
        JointDrive slerpDrive = joint.slerpDrive;
        slerpDrive.positionDamper = damper;
        joint.slerpDrive = slerpDrive;
    }

    private void SetPositionSpring(ConfigurableJoint joint, float spring)
    {
        JointDrive slerpDrive = joint.slerpDrive;
        slerpDrive.positionSpring = spring;
        joint.slerpDrive = slerpDrive;
    }

    private void FreeHipAxis()
    {
        hipJoint.angularXMotion = ConfigurableJointMotion.Free;
        hipJoint.angularYMotion = ConfigurableJointMotion.Free;
        hipJoint.angularZMotion = ConfigurableJointMotion.Free;
        hipJoint.xMotion = ConfigurableJointMotion.Free;
        hipJoint.yMotion = ConfigurableJointMotion.Free;
        hipJoint.zMotion = ConfigurableJointMotion.Free;

        //mainJoint.angularXMotion = ConfigurableJointMotion.Free;
        //mainJoint.angularZMotion = ConfigurableJointMotion.Free;
    }

    private void LockHipAxis()
    {
        hipJoint.angularXMotion = ConfigurableJointMotion.Locked;
        hipJoint.angularYMotion = ConfigurableJointMotion.Locked;
        hipJoint.angularZMotion = ConfigurableJointMotion.Locked;
        hipJoint.xMotion = ConfigurableJointMotion.Locked;
        hipJoint.yMotion = ConfigurableJointMotion.Locked;
        hipJoint.zMotion = ConfigurableJointMotion.Locked;

        //mainJoint.angularXMotion = ConfigurableJointMotion.Locked;
        //mainJoint.angularZMotion = ConfigurableJointMotion.Locked;
    }
}