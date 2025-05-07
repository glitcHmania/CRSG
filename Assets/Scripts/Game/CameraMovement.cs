using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform target;
    [SerializeField] private PlayerState playerState;
    [SerializeField] private LayerMask collisionMask;

    [Header("Settings")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -4f);
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float minYAngle = -30f;
    [SerializeField] private float maxYAngle = 60f;
    [SerializeField] private float collisionBuffer = 0.3f; // Distance from wall to prevent clipping
    [SerializeField] private float sphereCastRadius = 0.5f; // Thickness of the cast

    private float currentYaw = 0f;
    private float currentPitch = 20f;
    private float mouseY;
    private float mouseX;
    private bool fovDirtyFlag = false;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        if (target == null) return;

        if (playerState.IsAiming)
        {
            GetComponent<Camera>().fieldOfView = Mathf.Lerp(GetComponent<Camera>().fieldOfView, 50f, Time.deltaTime * 5f);
            target.localPosition = Vector3.Lerp(target.localPosition, new Vector3(0.3f, target.localPosition.y, target.localPosition.z), Time.deltaTime * 5f);

            fovDirtyFlag = true;
        }
        else if (fovDirtyFlag)
        {
            GetComponent<Camera>().fieldOfView = Mathf.Lerp(GetComponent<Camera>().fieldOfView, 90f, Time.deltaTime * 5f);
            target.localPosition = Vector3.Lerp(target.localPosition, new Vector3(0f, target.localPosition.y, target.localPosition.z), Time.deltaTime * 5f);

            if (Mathf.Abs(GetComponent<Camera>().fieldOfView - 90f) < 1f)
            {
                fovDirtyFlag = false;
            }
        }

        if (!ChatBehaviour.Instance.IsInputActive)
        {
            mouseX = Input.GetAxis("Mouse X");
            mouseY = Input.GetAxis("Mouse Y");
        }

        currentYaw += mouseX * rotationSpeed;
        currentPitch -= mouseY * rotationSpeed;
        currentPitch = Mathf.Clamp(currentPitch, minYAngle, maxYAngle);

        Quaternion rotation = Quaternion.Euler(currentPitch, currentYaw, 0);
        Vector3 desiredPosition = target.position + rotation * offset;

        Vector3 direction = (desiredPosition - target.position).normalized;
        float distance = Vector3.Distance(target.position, desiredPosition);

        // SphereCast instead of Raycast
        if (Physics.SphereCast(target.position, sphereCastRadius, direction, out RaycastHit hit, distance, collisionMask))
        {
            // Move camera slightly before collision
            transform.position = hit.point - direction * collisionBuffer;
        }
        else
        {
            transform.position = desiredPosition;
        }

        transform.LookAt(target.position);
    }
}
