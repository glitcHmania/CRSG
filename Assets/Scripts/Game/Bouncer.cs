using UnityEngine;
using Mirror;

[RequireComponent(typeof(AudioSource))]
public class Bouncer : NetworkBehaviour
{
    [Header("Settings")]
    [SerializeField] private float bounceForce = 10f;
    [SerializeField] private float bounceCooldown = 1f;

    [Header("Audio")]
    [SerializeField] private bool randomizePitch = true;

    private AudioSource audioSource;
    private Timer bounceCooldownTimer; 
    private AudioClip bounceClip;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            Debug.LogWarning("Bouncer: No AudioSource found on object.");

        bounceCooldownTimer = new Timer(bounceCooldown, startFinished: true);
        bounceClip = audioSource.clip;
    }

    private void Update()
    {
        bounceCooldownTimer.Update();
    }

    private void OnCollisionEnter(Collision collision)
    {
        GameObject otherObj = collision.gameObject;

        if (otherObj.layer > 12) return;

        var mScript = otherObj.GetComponentInParent<Movement>();
        var psScript = otherObj.GetComponentInParent<PlayerState>();
        var rcScript = otherObj.GetComponentInParent<RagdollController>();

        if (mScript != null && psScript != null && rcScript != null && mScript.isLocalPlayer)
        {
            if (!bounceCooldownTimer.IsFinished)
                return;

            bounceCooldownTimer.Reset();

            Rigidbody rb = psScript.GetComponent<Rigidbody>();
            if (rb != null) rb.velocity = Vector3.zero;

            mScript.AddForceToPlayer(Vector3.up, bounceForce, ForceMode.VelocityChange);
            psScript.IsBouncing = true;
            rcScript.EnableBalance();

            CmdPlayBounceSound();
        }
    }

    [Command(requiresAuthority = false)]
    private void CmdPlayBounceSound()
    {
        RpcPlayBounceSound();
    }

    [ClientRpc]
    private void RpcPlayBounceSound()
    {
        PlayBounceSound();
    }

    private void PlayBounceSound()
    {
        if (audioSource == null || bounceClip == null) return;

        audioSource.pitch = randomizePitch ? 1f + Random.Range(-0.1f, 0.1f) : 1f;
        audioSource.PlayOneShot(bounceClip);
    }
}
