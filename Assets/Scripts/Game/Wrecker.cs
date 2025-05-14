using Mirror;
using UnityEngine;

public class Wrecker : NetworkBehaviour
{
    private AudioSource audioSource;
    private AudioClip audioClip;
    private Timer cooldownTimer;

    private void Start()
    {
        audioSource = GetComponentInParent<AudioSource>();
        audioClip = audioSource.clip;
        cooldownTimer = new Timer(0.5f, startFinished: true);
    }

    private void Update()
    {
        cooldownTimer.Update();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isServer) return; // Let the server handle all logic

        if (cooldownTimer.IsFinished &&
            (other.gameObject.layer == LayerMask.NameToLayer("PlayerSpine") ||
             other.gameObject.layer == LayerMask.NameToLayer("PlayerHip")))
        {
            var ragdoll = other.GetComponentInParent<RagdollController>();
            if (ragdoll != null)
            {
                ragdoll.EnableRagdoll();
            }

            cooldownTimer.Reset();

            // Play sound locally on server
            audioSource.PlayOneShot(audioClip);

            // Play sound on clients
            RpcPlaySound();
        }
    }

    [ClientRpc]
    private void RpcPlaySound()
    {
        if (isServer) return; // Prevent server from playing twice

        audioSource.PlayOneShot(audioClip);
    }
}
