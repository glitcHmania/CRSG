using UnityEngine;

public class HandController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private KeyCode controlkey;
    [SerializeField] private Rigidbody foreArm;
    [SerializeField] private PlayerState playerState;

    [Header("Settings")]
    [SerializeField] private bool isLeftHand = false;
    [Header("Right hand reference for left hand only")]
    [SerializeField] private HandController rightHandReference;

    private FixedJoint fixedJoint;
    private GameObject collidedObject = null;
    private bool isColliding = false;
    private bool isCarrying = false;
    private bool isHolding = false;

    void Update()
    {
        if (!isLeftHand && playerState.IsArmed)
        {
            if (isHolding || isCarrying)
            {
                if (fixedJoint != null)
                {
                    Destroy(fixedJoint);
                    fixedJoint = null;
                    isHolding = false;
                    isCarrying = false;
                }
                return;
            }
        }

        if (!isHolding && !isCarrying && Input.GetKey(controlkey))
        {
            if (isColliding && collidedObject != null)
            {
                if (collidedObject.GetComponent<Rigidbody>())
                {
                    fixedJoint = foreArm.gameObject.AddComponent<FixedJoint>();
                    fixedJoint.connectedBody = collidedObject.GetComponent<Rigidbody>();
                    isCarrying = true;
                }
                else
                {
                    fixedJoint = foreArm.gameObject.AddComponent<FixedJoint>();
                    fixedJoint.connectedAnchor = collidedObject.transform.position;
                    isHolding = true;
                    playerState.IsClimbing = true;
                }
            }
        }
        if (Input.GetKeyUp(controlkey) || Input.GetKeyUp(KeyCode.Space))
        {
            if (fixedJoint != null)
            {
                Destroy(fixedJoint);
                fixedJoint = null;

                if (isCarrying)
                {
                    isCarrying = false;
                }
                else
                {
                    isHolding = false;
                }
            }
        }

        if (isLeftHand && !isHolding && !rightHandReference.isHolding)
        {
            playerState.IsClimbing = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isLeftHand && playerState.IsArmed) return;

        if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            isColliding = true;
            collidedObject = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!isLeftHand && playerState.IsArmed) return;

        if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            isColliding = false;
            collidedObject = null;
        }
    }
}
