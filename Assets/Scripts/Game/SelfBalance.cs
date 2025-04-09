using UnityEngine;

public class SelfBalance : MonoBehaviour
{
    [Header("Player Parts")]
    public GameObject spine;
    public GameObject hips;
    public GameObject rightArm;
    public GameObject leftArm;
    public GameObject rightUpLeg;
    public GameObject rightLeg;
    public GameObject rightFoot;
    public GameObject leftUpLeg;
    public GameObject leftLeg;
    public GameObject leftFoot;
    public GameObject head;
    public GameObject centerOfMass;

    [Header("Settings")]
    public float spineMovingSpeed = 6f;
    public float touchForce = 1f;
    public float timeStep = 0.2f;
    public float legsHeight = 1f;
    public float fallFactor = 0.4f;

    ConfigurableJoint spineJoint;
    ConfigurableJoint hipsJoint;
    ConfigurableJoint rightArmJoint;
    ConfigurableJoint leftArmJoint;
    ConfigurableJoint rightUpLegJoint;
    ConfigurableJoint rightLegJoint;
    ConfigurableJoint rightFootJoint;
    ConfigurableJoint leftUpLegJoint;
    ConfigurableJoint leftLegJoint;
    ConfigurableJoint leftFootJoint;

    private Vector3 COM;
    float rightStepTime, leftStepTime;
    public bool StepR, StepL, WalkF, WalkB, Falling, Fall, StandUp;
    bool flag, Flag_Leg_R, Flag_Leg_L;
    Quaternion StartLegR1, StartLegR2, StartLegL1, StartLegL2;
    JointDrive Spring0, Spring150, Spring300, Spring320;

    private void Awake()
    {
        //Physics.IgnoreCollision(rightArm.GetComponent<Collider>(), rightUpLeg.GetComponent<Collider>(), true);
        //Physics.IgnoreCollision(leftArm.GetComponent<Collider>(), leftUpLeg.GetComponent<Collider>(), true);
        StartLegR1 = rightUpLeg.GetComponent<ConfigurableJoint>().targetRotation;
        StartLegR2 = rightLeg.GetComponent<ConfigurableJoint>().targetRotation;
        StartLegL1 = leftUpLeg.GetComponent<ConfigurableJoint>().targetRotation;
        StartLegL2 = leftLeg.GetComponent<ConfigurableJoint>().targetRotation;

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

        spineJoint = spine.GetComponent<ConfigurableJoint>();
        hipsJoint = hips.GetComponent<ConfigurableJoint>();
        rightArmJoint = rightArm.GetComponent<ConfigurableJoint>();
        leftArmJoint = leftArm.GetComponent<ConfigurableJoint>();
        rightUpLegJoint = rightUpLeg.GetComponent<ConfigurableJoint>();
        rightLegJoint = rightLeg.GetComponent<ConfigurableJoint>();
        rightFootJoint = rightFoot.GetComponent<ConfigurableJoint>();
        leftUpLegJoint = leftUpLeg.GetComponent<ConfigurableJoint>();
        leftLegJoint = leftLeg.GetComponent<ConfigurableJoint>();
        leftFootJoint = leftFoot.GetComponent<ConfigurableJoint>();
    }

    private void Update()
    {

        #region Input
        if (Input.GetKey(KeyCode.UpArrow))
        {
            head.GetComponent<Rigidbody>().AddForce(Vector3.back * touchForce, ForceMode.Impulse);
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            head.GetComponent<Rigidbody>().AddForce(Vector3.forward * touchForce, ForceMode.Impulse);
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            if (Time.timeScale == 1)
                Time.timeScale = 0.4f;
            else
                Time.timeScale = 1;
        }

        #endregion

        CalculateCenterOfMass();

        centerOfMass.transform.position = COM;

        Balance();

        if (!WalkF && !WalkB)
        {
            StepR = false;
            StepL = false;
            rightStepTime = 0;
            leftStepTime = 0;
            Flag_Leg_R = false;
            Flag_Leg_L = false;
            spineJoint.targetRotation = Quaternion.Lerp(spineJoint.targetRotation, new Quaternion(-0.1f, spineJoint.targetRotation.y, spineJoint.targetRotation.z, spineJoint.targetRotation.w), spineMovingSpeed * Time.fixedDeltaTime);
        }
    }

    private void FixedUpdate()
    {
        LegsMoving();
    }

    void Balance()
    {
        if (centerOfMass.transform.position.z < rightFoot.transform.position.z && centerOfMass.transform.position.z < leftFoot.transform.position.z)
        {
            WalkB = true;
            spineJoint.targetRotation = Quaternion.Lerp(spineJoint.targetRotation, new Quaternion(-0.1f, spineJoint.targetRotation.y, spineJoint.targetRotation.z, spineJoint.targetRotation.w), spineMovingSpeed * Time.fixedDeltaTime);
        }
        else
        {
            WalkB = false;
        }

        if (centerOfMass.transform.position.z > rightFoot.transform.position.z && centerOfMass.transform.position.z > leftFoot.transform.position.z)
        {
            WalkF = true;
            spineJoint.targetRotation = Quaternion.Lerp(spineJoint.targetRotation, new Quaternion(0, spineJoint.targetRotation.y, spineJoint.targetRotation.z, spineJoint.targetRotation.w), spineMovingSpeed * Time.fixedDeltaTime);
        }
        else
        {
            WalkF = false;
        }

        if (centerOfMass.transform.position.z > rightFoot.transform.position.z + fallFactor &&
           centerOfMass.transform.position.z > leftFoot.transform.position.z + fallFactor ||
           centerOfMass.transform.position.z < rightFoot.transform.position.z - (fallFactor + 0.2f) &&
           centerOfMass.transform.position.z < leftFoot.transform.position.z - (fallFactor + 0.2f))
        {
            Falling = true;
        }
        else
        {
            Falling = false;
        }

        if (Falling)
        {
            hipsJoint.angularXDrive = Spring0;
            hipsJoint.angularYZDrive = Spring0;
            legsHeight = 5;
        }
        else
        {
            hipsJoint.angularXDrive = Spring300;
            hipsJoint.angularYZDrive = Spring300;
            legsHeight = 1;
            rightArmJoint.targetRotation = Quaternion.Lerp(rightArmJoint.targetRotation, new Quaternion(0, rightArmJoint.targetRotation.y, rightArmJoint.targetRotation.z, rightArmJoint.targetRotation.w), 6 * Time.fixedDeltaTime);
            leftArmJoint.targetRotation = Quaternion.Lerp(leftArmJoint.targetRotation, new Quaternion(0, leftArmJoint.targetRotation.y, leftArmJoint.targetRotation.z, leftArmJoint.targetRotation.w), 6 * Time.fixedDeltaTime);
            rightArmJoint.angularXDrive = Spring0;
            rightArmJoint.angularYZDrive = Spring150;
            leftArmJoint.angularXDrive = Spring0;
            leftArmJoint.angularYZDrive = Spring150;
        }

        if (spine.transform.position.y - 0.1f <= hips.transform.position.y)
        {
            Fall = true;
        }
        else
        {
            Fall = false;
        }

        if (Fall)
        {
            hipsJoint.angularXDrive = Spring0;
            hipsJoint.angularYZDrive = Spring0;
            StandUping();
        }
    }

    void LegsMoving()
    {
        if (WalkF)
        {
            if (rightFoot.transform.position.z < leftFoot.transform.position.z && !StepL && !Flag_Leg_R)
            {
                StepR = true;
                Flag_Leg_R = true;
                Flag_Leg_L = true;
            }
            if (rightFoot.transform.position.z > leftFoot.transform.position.z && !StepR && !Flag_Leg_L)
            {
                StepL = true;
                Flag_Leg_L = true;
                Flag_Leg_R = true;
            }
        }

        if (WalkB)
        {
            if (rightFoot.transform.position.z > leftFoot.transform.position.z && !StepL && !Flag_Leg_R)
            {
                StepR = true;
                Flag_Leg_R = true;
                Flag_Leg_L = true;
            }
            if (rightFoot.transform.position.z < leftFoot.transform.position.z && !StepR && !Flag_Leg_L)
            {
                StepL = true;
                Flag_Leg_L = true;
                Flag_Leg_R = true;
            }
        }

        if (StepR)
        {
            rightStepTime += Time.fixedDeltaTime;

            if (WalkF)
            {
                rightUpLegJoint.targetRotation = new Quaternion(rightUpLegJoint.targetRotation.x + 0.07f * legsHeight, rightUpLegJoint.targetRotation.y, rightUpLegJoint.targetRotation.z, rightUpLegJoint.targetRotation.w);
                rightLegJoint.targetRotation = new Quaternion(rightLegJoint.targetRotation.x - 0.04f * legsHeight * 2, rightLegJoint.targetRotation.y, rightLegJoint.targetRotation.z, rightLegJoint.targetRotation.w);

                leftUpLegJoint.targetRotation = new Quaternion(leftUpLegJoint.targetRotation.x - 0.02f * legsHeight / 2, leftUpLegJoint.targetRotation.y, leftUpLegJoint.targetRotation.z, leftUpLegJoint.targetRotation.w);
            }

            if (WalkB)
            {
                rightUpLegJoint.targetRotation = new Quaternion(rightUpLegJoint.targetRotation.x - 0.00f * legsHeight, rightUpLegJoint.targetRotation.y, rightUpLegJoint.targetRotation.z, rightUpLegJoint.targetRotation.w);
                rightLegJoint.targetRotation = new Quaternion(rightLegJoint.targetRotation.x - 0.06f * legsHeight * 2, rightLegJoint.targetRotation.y, rightLegJoint.targetRotation.z, rightLegJoint.targetRotation.w);

                leftUpLegJoint.targetRotation = new Quaternion(leftUpLegJoint.targetRotation.x + 0.02f * legsHeight / 2, leftUpLegJoint.targetRotation.y, leftUpLegJoint.targetRotation.z, leftUpLegJoint.targetRotation.w);
            }

            if (rightStepTime > timeStep)
            {
                rightStepTime = 0;
                StepR = false;

                if (WalkB || WalkF)
                {
                    StepL = true;
                }
            }
        }
        else
        {
            rightUpLegJoint.targetRotation = Quaternion.Lerp(rightUpLegJoint.targetRotation, StartLegR1, (8f) * Time.fixedDeltaTime);
            rightLegJoint.targetRotation = Quaternion.Lerp(rightLegJoint.targetRotation, StartLegR2, (17f) * Time.fixedDeltaTime);
        }

        if (StepL)
        {
            leftStepTime += Time.fixedDeltaTime;

            if (WalkF)
            {
                leftUpLegJoint.targetRotation = new Quaternion(leftUpLegJoint.targetRotation.x + 0.07f * legsHeight, leftUpLegJoint.targetRotation.y, leftUpLegJoint.targetRotation.z, leftUpLegJoint.targetRotation.w);
                leftLegJoint.targetRotation = new Quaternion(leftLegJoint.targetRotation.x - 0.04f * legsHeight * 2, leftLegJoint.targetRotation.y, leftLegJoint.targetRotation.z, leftLegJoint.targetRotation.w);

                rightUpLegJoint.targetRotation = new Quaternion(rightUpLegJoint.targetRotation.x - 0.02f * legsHeight / 2, rightUpLegJoint.targetRotation.y, rightUpLegJoint.targetRotation.z, rightUpLegJoint.targetRotation.w);
            }

            if (WalkB)
            {
                leftUpLegJoint.targetRotation = new Quaternion(leftUpLegJoint.targetRotation.x - 0.00f * legsHeight, leftUpLegJoint.targetRotation.y, leftUpLegJoint.targetRotation.z, leftUpLegJoint.targetRotation.w);
                leftLegJoint.targetRotation = new Quaternion(leftLegJoint.targetRotation.x - 0.06f * legsHeight * 2, leftLegJoint.targetRotation.y, leftLegJoint.targetRotation.z, leftLegJoint.targetRotation.w);

                rightUpLegJoint.targetRotation = new Quaternion(rightUpLegJoint.targetRotation.x + 0.02f * legsHeight / 2, rightUpLegJoint.targetRotation.y, rightUpLegJoint.targetRotation.z, rightUpLegJoint.targetRotation.w);
            }

            if (leftStepTime > timeStep)
            {
                leftStepTime = 0;
                StepL = false;

                if (WalkB || WalkF)
                {
                    StepR = true;
                }
            }
        }
        else
        {
            leftUpLegJoint.targetRotation = Quaternion.Lerp(leftUpLegJoint.targetRotation, StartLegL1, (8) * Time.fixedDeltaTime);
            leftLegJoint.targetRotation = Quaternion.Lerp(leftLegJoint.targetRotation, StartLegL2, (17) * Time.fixedDeltaTime);
        }
    }

    void StandUping()
    {
        if (WalkF)
        {
            rightArmJoint.angularXDrive = Spring320;
            rightArmJoint.angularYZDrive = Spring320;
            leftArmJoint.angularXDrive = Spring320;
            leftArmJoint.angularYZDrive = Spring320;
            spineJoint.targetRotation = Quaternion.Lerp(spineJoint.targetRotation, new Quaternion(-0.1f, spineJoint.targetRotation.y,
                spineJoint.targetRotation.z, spineJoint.targetRotation.w), 6 * Time.fixedDeltaTime);

            if (rightArmJoint.targetRotation.x < 1.7f)
            {
                rightArmJoint.targetRotation = new Quaternion(rightArmJoint.targetRotation.x + 0.07f, rightArmJoint.targetRotation.y,
                    rightArmJoint.targetRotation.z, rightArmJoint.targetRotation.w);
            }

            if (leftArmJoint.targetRotation.x < 1.7f)
            {
                leftArmJoint.targetRotation = new Quaternion(leftArmJoint.targetRotation.x + 0.07f, leftArmJoint.targetRotation.y,
                    leftArmJoint.targetRotation.z, leftArmJoint.targetRotation.w);
            }
        }

        if (WalkB)
        {
            rightArmJoint.angularXDrive = Spring320;
            rightArmJoint.angularYZDrive = Spring320;
            leftArmJoint.angularXDrive = Spring320;
            leftArmJoint.angularYZDrive = Spring320;

            if (rightArmJoint.targetRotation.x > -1.7f)
            {
                rightArmJoint.targetRotation = new Quaternion(rightArmJoint.targetRotation.x - 0.09f, rightArmJoint.targetRotation.y,
                    rightArmJoint.targetRotation.z, rightArmJoint.targetRotation.w);
            }

            if (leftArmJoint.targetRotation.x > -1.7f)
            {
                leftArmJoint.targetRotation = new Quaternion(leftArmJoint.targetRotation.x - 0.09f, leftArmJoint.targetRotation.y,
                    leftArmJoint.targetRotation.z, leftArmJoint.targetRotation.w);
            }
        }
    }

    void CalculateCenterOfMass()
    {
        COM = (spineJoint.GetComponent<Rigidbody>().mass * spineJoint.transform.position +
            hipsJoint.GetComponent<Rigidbody>().mass * hipsJoint.transform.position +
            rightArmJoint.GetComponent<Rigidbody>().mass * rightArmJoint.transform.position +
            leftArmJoint.GetComponent<Rigidbody>().mass * leftArmJoint.transform.position +
            rightUpLegJoint.GetComponent<Rigidbody>().mass * rightUpLegJoint.transform.position +
            rightLegJoint.GetComponent<Rigidbody>().mass * rightLegJoint.transform.position +
            rightFootJoint.GetComponent<Rigidbody>().mass * rightFootJoint.transform.position +
            leftUpLegJoint.GetComponent<Rigidbody>().mass * leftUpLegJoint.transform.position +
            leftLegJoint.GetComponent<Rigidbody>().mass * leftLegJoint.transform.position +
            leftFootJoint.GetComponent<Rigidbody>().mass * leftFootJoint.transform.position) /
            (spineJoint.GetComponent<Rigidbody>().mass + hipsJoint.GetComponent<Rigidbody>().mass +
            rightArmJoint.GetComponent<Rigidbody>().mass + leftArmJoint.GetComponent<Rigidbody>().mass +
            rightUpLegJoint.GetComponent<Rigidbody>().mass + rightLegJoint.GetComponent<Rigidbody>().mass +
            rightFootJoint.GetComponent<Rigidbody>().mass + leftUpLegJoint.GetComponent<Rigidbody>().mass +
            leftLegJoint.GetComponent<Rigidbody>().mass + leftFootJoint.GetComponent<Rigidbody>().mass);
    }
}
