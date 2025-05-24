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
    [SerializeField] private float collisionBuffer = 0.3f;
    [SerializeField] private float sphereCastRadius = 0.5f;

    private float currentYaw = 0f;
    private float currentPitch = 20f;
    private float mouseY;
    private float mouseX;
    private bool locked = false;

    void LateUpdate()
    {
        if (!PlayerSpawner.IsInGameScene) return;

        if (!locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            locked = true;
        }

        if (UIManager.Instance.IsGameFocused)
        {
            mouseX = Input.GetAxis("Mouse X");
            mouseY = Input.GetAxis("Mouse Y");
        }
        else
        {
            mouseX = 0;
            mouseY = 0;
        }

        currentYaw += mouseX * rotationSpeed;
        currentPitch -= mouseY * rotationSpeed;
        currentPitch = Mathf.Clamp(currentPitch, minYAngle, maxYAngle);

        Quaternion rotation = Quaternion.Euler(currentPitch, currentYaw, 0);

        // Update aiming position offset (no FOV)
        if (playerState.IsAiming)
        {
            target.localPosition = Vector3.Lerp(target.localPosition, new Vector3(0.6f, target.localPosition.y, 1.3f), Time.deltaTime * 5f);
        }
        else
        {
            target.localPosition = Vector3.Lerp(target.localPosition, new Vector3(0f, target.localPosition.y, 0f), Time.deltaTime * 5f);
        }

        Vector3 desiredPosition = target.position + rotation * offset;
        Vector3 direction = (desiredPosition - target.position).normalized;
        float distance = Vector3.Distance(target.position, desiredPosition);

        if (Physics.SphereCast(target.position, sphereCastRadius, direction, out RaycastHit hit, distance, collisionMask))
        {
            transform.position = hit.point - direction * collisionBuffer;
        }
        else
        {
            transform.position = desiredPosition;
        }

        transform.LookAt(target.position);
    }
}
