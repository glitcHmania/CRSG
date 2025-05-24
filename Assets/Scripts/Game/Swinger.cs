using Mirror;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Swinger : NetworkBehaviour
{
    [Header("Settings")]
    [SerializeField] private float swingSpeed = 10f;
    [SerializeField] private float swingAmount = 30f;
    [SerializeField] private Vector3 swingAxis = Vector3.up;
    [SerializeField] private float rotationLerpSpeed = 5f;

    [SerializeField] private AudioClip swingSound;
    [SerializeField] private float swingSoundVolume = 0.1f;


    [SyncVar] private Quaternion syncedStartRotation;
    private Rigidbody rb;
    private TimeSync timeSync;
    private AudioSource audioSource;
    private Quaternion targetRotation;
    private float previousSwingAngle = 0f;

    public override void OnStartServer()
    {
        syncedStartRotation = transform.rotation;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        timeSync = FindObjectOfType<TimeSync>();
        targetRotation = transform.rotation;
        audioSource = gameObject.GetComponent<AudioSource>();
    }

    void FixedUpdate()
    {
        if (timeSync == null || timeSync.ServerStartTime <= 0f)
            return;

        float elapsed = (float)(NetworkTime.time - timeSync.ServerStartTime);
        float angle = swingSpeed * elapsed;

        float swingAngle = Mathf.Sin(angle) * swingAmount;

        // Detect zero crossing
        if (Mathf.Sign(previousSwingAngle) != Mathf.Sign(swingAngle))
        {
            // Only the server should call RPC to maintain sync
            if (isServer)
            {
                RpcPlaySound();
            }
        }

        previousSwingAngle = swingAngle;

        Quaternion swingRotation = Quaternion.AngleAxis(swingAngle, swingAxis.normalized);
        targetRotation = syncedStartRotation * swingRotation;
        Quaternion smoothedRotation = Quaternion.Lerp(rb.rotation, targetRotation, rotationLerpSpeed * Time.fixedDeltaTime);
        rb.MoveRotation(smoothedRotation);
    }


    [ClientRpc]
    private void RpcPlaySound()
    {
        PlaySound();
    }

    private void PlaySound()
    {
        if (audioSource == null || swingSound == null)
            return;
        // Clamp pitch to avoid extreme values
        float pitch = swingSpeed * 0.5f;
        audioSource.pitch = pitch;

        audioSource.PlayOneShot(swingSound, swingSoundVolume);
    }

}
