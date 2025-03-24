using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Rotator : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private Vector3 rotationAxis = Vector3.up;

    private Rigidbody rb;
    private Quaternion startRotation;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        startRotation = transform.rotation;
    }

    void FixedUpdate()
    {
        float angle = speed * Time.time;
        Quaternion targetRotation = Quaternion.AngleAxis(angle, rotationAxis.normalized);
        rb.MoveRotation(startRotation * targetRotation);
    }
}
