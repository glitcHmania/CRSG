using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform target;
    [SerializeField] private LayerMask collisionMask;

    [Header("Settings")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -4f);
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float minYAngle = -30f;
    [SerializeField] private float maxYAngle = 60f;

    private float currentYaw = 0f;
    private float currentPitch = 20f;
    private float mouseY;
    private float mouseX;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        if (target == null) return;

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

        transform.position = desiredPosition;
        transform.LookAt(target.position + Vector3.up * 1.5f);

        Physics.Raycast(target.position + new Vector3(0f, 1f), (desiredPosition - target.position).normalized, out RaycastHit hit, Vector3.Distance(target.position, transform.position), collisionMask);
        if (hit.collider != null)
        {
            transform.position = hit.point;
        }

    }
}
