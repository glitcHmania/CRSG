using UnityEngine;

public class HandController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private KeyCode controlkey;
    [SerializeField] private Rigidbody foreArm;
    [SerializeField] private PlayerState playerState;

    [Header("Settings")]
    [SerializeField] private bool disableOnArmed = false;

    private FixedJoint fixedJoint;
    private GameObject collidedObject = null;
    private bool isColliding = false;
    private bool isCarrying = false;
    private bool isHolding = false;

    void Update()
    {
        if (disableOnArmed && playerState.IsArmed) return;

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
                }
            }
        }
        if (Input.GetKeyUp(controlkey))
        {
            if (fixedJoint != null)
            {
                if (isCarrying)
                {
                    Destroy(fixedJoint);
                    isCarrying = false;
                }
                else
                {
                    Destroy(fixedJoint);
                    fixedJoint = null;
                    isHolding = false;
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(disableOnArmed && playerState.IsArmed) return;

        if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            isColliding = true;
            collidedObject = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (disableOnArmed && playerState.IsArmed) return;

        if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            isColliding = false;
            collidedObject = null;
        }
    }
}
