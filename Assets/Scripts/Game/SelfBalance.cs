using UnityEngine;

public class SelfBalance : MonoBehaviour
{
    public GameObject[] PlayerParts;
    public ConfigurableJoint[] JointParts;
    Vector3 COM;
    public float TouchForce, TimeStep, LegsHeight, FallFactor;
    float Step_R_Time, Step_L_Time;
    public bool StepR, StepL, WalkF, WalkB, Falling, Fall, StandUp;
    bool flag, Flag_Leg_R, Flag_Leg_L;
    Quaternion StartLegR1, StartLegR2, StartLegL1, StartLegL2;
    JointDrive Spring0, Spring150, Spring300, Spring320;

    private void Awake()
    {
        Physics.IgnoreCollision(PlayerParts[2].GetComponent<Collider>(), PlayerParts[4].GetComponent<Collider>(), true);
        Physics.IgnoreCollision(PlayerParts[3].GetComponent<Collider>(), PlayerParts[7].GetComponent<Collider>(), true);
        StartLegR1 = PlayerParts[4].GetComponent<ConfigurableJoint>().targetRotation;
        StartLegR2 = PlayerParts[5].GetComponent<ConfigurableJoint>().targetRotation;
        StartLegL1 = PlayerParts[7].GetComponent<ConfigurableJoint>().targetRotation;
        StartLegL2 = PlayerParts[8].GetComponent<ConfigurableJoint>().targetRotation;

        Spring0 = new JointDrive();
        Spring0.positionSpring = 0;
        Spring0.positionDamper = 0;
        Spring0.maximumForce = Mathf.Infinity;

        Spring150 = new JointDrive();
        Spring150.positionSpring = 150;
        Spring150.positionDamper = 0;
        Spring150.maximumForce = Mathf.Infinity;

        Spring300 = new JointDrive();
        Spring300.positionSpring = 300;
        Spring300.positionDamper = 100;
        Spring300.maximumForce = Mathf.Infinity;

        Spring320 = new JointDrive();
        Spring320.positionSpring = 320;
        Spring320.positionDamper = 0;
        Spring320.maximumForce = Mathf.Infinity;
    }

    private void Update()
    {

        #region Input
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            PlayerParts[0].GetComponent<Rigidbody>().AddForce(Vector3.back * TouchForce, ForceMode.Impulse);
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            PlayerParts[0].GetComponent<Rigidbody>().AddForce(Vector3.forward * TouchForce, ForceMode.Impulse);
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            if (Time.timeScale == 1)
                Time.timeScale = 0.4f;
            else
                Time.timeScale = 1;
        }

        #endregion

        Calculate_COM();

        PlayerParts[10].transform.position = COM;

        Balance();

        if (!WalkF && !WalkB)
        {
            StepR = false;
            StepL = false;
            Step_R_Time = 0;
            Step_L_Time = 0;
            Flag_Leg_R = false;
            Flag_Leg_L = false;
            JointParts[0].targetRotation = Quaternion.Lerp(JointParts[0].targetRotation, new Quaternion(-0.1f, JointParts[0].targetRotation.y, JointParts[0].targetRotation.z, JointParts[0].targetRotation.w), 6 * Time.fixedDeltaTime);
        }
    }

    private void FixedUpdate()
    {
        LegsMoving();
    }

    void Balance()
    {
        if (PlayerParts[10].transform.position.z < PlayerParts[6].transform.position.z && PlayerParts[10].transform.position.z < PlayerParts[9].transform.position.z)
        {
            WalkB = true;
            JointParts[0].targetRotation = Quaternion.Lerp(JointParts[0].targetRotation, new Quaternion(-0.1f, JointParts[0].targetRotation.y, JointParts[0].targetRotation.z, JointParts[0].targetRotation.w), 6 * Time.fixedDeltaTime);
        }
        else
        {
            WalkB = false;
        }

        if (PlayerParts[10].transform.position.z > PlayerParts[6].transform.position.z && PlayerParts[10].transform.position.z > PlayerParts[9].transform.position.z)
        {
            WalkF = true;
            JointParts[0].targetRotation = Quaternion.Lerp(JointParts[0].targetRotation, new Quaternion(0, JointParts[0].targetRotation.y, JointParts[0].targetRotation.z, JointParts[0].targetRotation.w), 6 * Time.fixedDeltaTime);
        }
        else
        {
            WalkF = false;
        }

        if (PlayerParts[10].transform.position.z > PlayerParts[6].transform.position.z + FallFactor &&
           PlayerParts[10].transform.position.z > PlayerParts[9].transform.position.z + FallFactor ||
           PlayerParts[10].transform.position.z < PlayerParts[6].transform.position.z - (FallFactor + 0.2f) &&
           PlayerParts[10].transform.position.z < PlayerParts[9].transform.position.z - (FallFactor + 0.2f))
        {
            Falling = true;
        }
        else
        {
            Falling = false;
        }

        if (Falling)
        {
            JointParts[1].angularXDrive = Spring0;
            JointParts[1].angularYZDrive = Spring0;
            LegsHeight = 5;
        }
        else
        {
            JointParts[1].angularXDrive = Spring300;
            JointParts[1].angularYZDrive = Spring300;
            LegsHeight = 1;
            JointParts[2].targetRotation = Quaternion.Lerp(JointParts[2].targetRotation, new Quaternion(0, JointParts[2].targetRotation.y, JointParts[2].targetRotation.z, JointParts[2].targetRotation.w), 6 * Time.fixedDeltaTime);
            JointParts[3].targetRotation = Quaternion.Lerp(JointParts[3].targetRotation, new Quaternion(0, JointParts[3].targetRotation.y, JointParts[3].targetRotation.z, JointParts[3].targetRotation.w), 6 * Time.fixedDeltaTime);
            JointParts[2].angularXDrive = Spring0;
            JointParts[2].angularYZDrive = Spring150;
            JointParts[3].angularXDrive = Spring0;
            JointParts[3].angularYZDrive = Spring150;
        }

        if (PlayerParts[0].transform.position.y - 0.1f <= PlayerParts[1].transform.position.y)
        {
            Fall = true;
        }
        else
        {
            Fall = false;
        }

        if (Fall)
        {
            JointParts[1].angularXDrive = Spring0;
            JointParts[1].angularYZDrive = Spring0;
            StandUping();
        }
    }

    void LegsMoving()
    {
        if (WalkF)
        {
            if (PlayerParts[6].transform.position.z < PlayerParts[9].transform.position.z && !StepL && !Flag_Leg_R)
            {
                StepR = true;
                Flag_Leg_R = true;
                Flag_Leg_L = true;
            }
            if (PlayerParts[6].transform.position.z > PlayerParts[9].transform.position.z && !StepR && !Flag_Leg_L)
            {
                StepL = true;
                Flag_Leg_L = true;
                Flag_Leg_R = true;
            }
        }

        if (WalkB)
        {
            if (PlayerParts[6].transform.position.z > PlayerParts[9].transform.position.z && !StepL && !Flag_Leg_R)
            {
                StepR = true;
                Flag_Leg_R = true;
                Flag_Leg_L = true;
            }
            if (PlayerParts[6].transform.position.z < PlayerParts[9].transform.position.z && !StepR && !Flag_Leg_L)
            {
                StepL = true;
                Flag_Leg_L = true;
                Flag_Leg_R = true;
            }
        }

        if (StepR)
        {
            Step_R_Time += Time.fixedDeltaTime;

            if (WalkF)
            {
                JointParts[4].targetRotation = new Quaternion(JointParts[4].targetRotation.x + 0.07f * LegsHeight, JointParts[4].targetRotation.y, JointParts[4].targetRotation.z, JointParts[4].targetRotation.w);
                JointParts[5].targetRotation = new Quaternion(JointParts[5].targetRotation.x - 0.04f * LegsHeight * 2, JointParts[5].targetRotation.y, JointParts[5].targetRotation.z, JointParts[5].targetRotation.w);

                JointParts[7].targetRotation = new Quaternion(JointParts[7].targetRotation.x - 0.02f * LegsHeight / 2, JointParts[7].targetRotation.y, JointParts[7].targetRotation.z, JointParts[7].targetRotation.w);
            }

            if (WalkB)
            {
                JointParts[4].targetRotation = new Quaternion(JointParts[4].targetRotation.x - 0.00f * LegsHeight, JointParts[4].targetRotation.y, JointParts[4].targetRotation.z, JointParts[4].targetRotation.w);
                JointParts[5].targetRotation = new Quaternion(JointParts[5].targetRotation.x - 0.06f * LegsHeight * 2, JointParts[5].targetRotation.y, JointParts[5].targetRotation.z, JointParts[5].targetRotation.w);

                JointParts[7].targetRotation = new Quaternion(JointParts[7].targetRotation.x + 0.02f * LegsHeight / 2, JointParts[7].targetRotation.y, JointParts[7].targetRotation.z, JointParts[7].targetRotation.w);
            }

            if (Step_R_Time > TimeStep)
            {
                Step_R_Time = 0;
                StepR = false;

                if (WalkB || WalkF)
                {
                    StepL = true;
                }
            }
        }
        else
        {
            JointParts[4].targetRotation = Quaternion.Lerp(JointParts[4].targetRotation, StartLegR1, (8f) * Time.fixedDeltaTime);
            JointParts[5].targetRotation = Quaternion.Lerp(JointParts[5].targetRotation, StartLegR2, (17f) * Time.fixedDeltaTime);
        }

        if (StepL)
        {
            Step_L_Time += Time.fixedDeltaTime;

            if (WalkF)
            {
                JointParts[7].targetRotation = new Quaternion(JointParts[7].targetRotation.x + 0.07f * LegsHeight, JointParts[7].targetRotation.y, JointParts[7].targetRotation.z, JointParts[7].targetRotation.w);
                JointParts[8].targetRotation = new Quaternion(JointParts[8].targetRotation.x - 0.04f * LegsHeight * 2, JointParts[8].targetRotation.y, JointParts[8].targetRotation.z, JointParts[8].targetRotation.w);

                JointParts[4].targetRotation = new Quaternion(JointParts[4].targetRotation.x - 0.02f * LegsHeight / 2, JointParts[4].targetRotation.y, JointParts[4].targetRotation.z, JointParts[4].targetRotation.w);
            }

            if (WalkB)
            {
                JointParts[7].targetRotation = new Quaternion(JointParts[7].targetRotation.x - 0.00f * LegsHeight, JointParts[7].targetRotation.y, JointParts[7].targetRotation.z, JointParts[7].targetRotation.w);
                JointParts[8].targetRotation = new Quaternion(JointParts[8].targetRotation.x - 0.06f * LegsHeight * 2, JointParts[8].targetRotation.y, JointParts[8].targetRotation.z, JointParts[8].targetRotation.w);

                JointParts[4].targetRotation = new Quaternion(JointParts[4].targetRotation.x + 0.02f * LegsHeight / 2, JointParts[4].targetRotation.y, JointParts[4].targetRotation.z, JointParts[4].targetRotation.w);
            }

            if (Step_L_Time > TimeStep)
            {
                Step_L_Time = 0;
                StepL = false;

                if (WalkB || WalkF)
                {
                    StepR = true;
                }
            }
        }
        else
        {
            JointParts[7].targetRotation = Quaternion.Lerp(JointParts[7].targetRotation, StartLegL1, (8) * Time.fixedDeltaTime);
            JointParts[8].targetRotation = Quaternion.Lerp(JointParts[8].targetRotation, StartLegL2, (17) * Time.fixedDeltaTime);
        }
    }

    void StandUping()
    {
        if (WalkF)
        {
            JointParts[2].angularXDrive = Spring320;
            JointParts[2].angularYZDrive = Spring320;
            JointParts[3].angularXDrive = Spring320;
            JointParts[3].angularYZDrive = Spring320;
            JointParts[0].targetRotation = Quaternion.Lerp(JointParts[0].targetRotation, new Quaternion(-0.1f, JointParts[0].targetRotation.y,
                JointParts[0].targetRotation.z, JointParts[0].targetRotation.w), 6 * Time.fixedDeltaTime);

            if (JointParts[2].targetRotation.x < 1.7f)
            {
                JointParts[2].targetRotation = new Quaternion(JointParts[2].targetRotation.x + 0.07f, JointParts[2].targetRotation.y,
                    JointParts[2].targetRotation.z, JointParts[2].targetRotation.w);
            }

            if (JointParts[3].targetRotation.x < 1.7f)
            {
                JointParts[3].targetRotation = new Quaternion(JointParts[3].targetRotation.x + 0.07f, JointParts[3].targetRotation.y,
                    JointParts[3].targetRotation.z, JointParts[3].targetRotation.w);
            }
        }

        if (WalkB)
        {
            JointParts[2].angularXDrive = Spring320;
            JointParts[2].angularYZDrive = Spring320;
            JointParts[3].angularXDrive = Spring320;
            JointParts[3].angularYZDrive = Spring320;

            if (JointParts[2].targetRotation.x > -1.7f)
            {
                JointParts[2].targetRotation = new Quaternion(JointParts[2].targetRotation.x - 0.09f, JointParts[2].targetRotation.y,
                    JointParts[2].targetRotation.z, JointParts[2].targetRotation.w);
            }

            if (JointParts[3].targetRotation.x > -1.7f)
            {
                JointParts[3].targetRotation = new Quaternion(JointParts[3].targetRotation.x - 0.09f, JointParts[3].targetRotation.y,
                    JointParts[3].targetRotation.z, JointParts[3].targetRotation.w);
            }
        }
    }

    void Calculate_COM()
    {
        COM = (JointParts[0].GetComponent<Rigidbody>().mass * JointParts[0].transform.position +
            JointParts[1].GetComponent<Rigidbody>().mass * JointParts[1].transform.position +
            JointParts[2].GetComponent<Rigidbody>().mass * JointParts[2].transform.position +
            JointParts[3].GetComponent<Rigidbody>().mass * JointParts[3].transform.position +
            JointParts[4].GetComponent<Rigidbody>().mass * JointParts[4].transform.position +
            JointParts[5].GetComponent<Rigidbody>().mass * JointParts[5].transform.position +
            JointParts[6].GetComponent<Rigidbody>().mass * JointParts[6].transform.position +
            JointParts[7].GetComponent<Rigidbody>().mass * JointParts[7].transform.position +
            JointParts[8].GetComponent<Rigidbody>().mass * JointParts[8].transform.position +
            JointParts[9].GetComponent<Rigidbody>().mass * JointParts[9].transform.position) /
            (JointParts[0].GetComponent<Rigidbody>().mass + JointParts[1].GetComponent<Rigidbody>().mass +
            JointParts[2].GetComponent<Rigidbody>().mass + JointParts[3].GetComponent<Rigidbody>().mass +
            JointParts[4].GetComponent<Rigidbody>().mass + JointParts[5].GetComponent<Rigidbody>().mass +
            JointParts[6].GetComponent<Rigidbody>().mass + JointParts[7].GetComponent<Rigidbody>().mass +
            JointParts[8].GetComponent<Rigidbody>().mass + JointParts[9].GetComponent<Rigidbody>().mass);
    }
}
