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
        if (cooldownTimer.IsFinished &&
            (other.gameObject.layer == LayerMask.NameToLayer("PlayerSpine") ||
             other.gameObject.layer == LayerMask.NameToLayer("PlayerHip")) ||
             other.gameObject.layer == LayerMask.NameToLayer("PlayerHead"))
        {
            var ragdoll = other.GetComponentInParent<RagdollController>();
            if (ragdoll != null)
            {
                ragdoll.EnableRagdoll();
            }

            cooldownTimer.Reset();

            if (isServer)
            {
                audioSource.PlayOneShot(audioClip);
            }
            RpcPlaySound();
        }
    }

    [ClientRpc]
    private void RpcPlaySound()
    {
        audioSource.PlayOneShot(audioClip);
    }
}
