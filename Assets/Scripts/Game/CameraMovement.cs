using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 0f, -4f);
    public float rotationSpeed = 5f;
    public float minYAngle = -30f;
    public float maxYAngle = 60f;

    private float currentYaw = 0f;
    private float currentPitch = 20f;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        if (target == null) return;

        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        currentYaw += mouseX * rotationSpeed;
        currentPitch -= mouseY * rotationSpeed;
        currentPitch = Mathf.Clamp(currentPitch, minYAngle, maxYAngle);

        // Combine pitch and yaw rotations
        Quaternion rotation = Quaternion.Euler(currentPitch, currentYaw, 0);
        Vector3 desiredPosition = target.position + rotation * offset;

        transform.position = desiredPosition;

        // Optional: smooth movement
        // transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * smoothSpeed);

        transform.LookAt(target.position + Vector3.up * 1.5f);
    }
}
