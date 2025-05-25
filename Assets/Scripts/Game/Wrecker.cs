using Mirror;
using UnityEngine;

public class Wrecker : NetworkBehaviour
{
    [SerializeField] private float hitSoundVolume = 0.1f;
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
        int layer = other.gameObject.layer;
        if (!cooldownTimer.IsFinished) return;

        if (layer == LayerMask.NameToLayer("PlayerSpine") ||
            layer == LayerMask.NameToLayer("PlayerHip") ||
            layer == LayerMask.NameToLayer("PlayerHead"))
        {
            var ragdoll = other.GetComponentInParent<RagdollController>();
            if (ragdoll != null)
            {
                ragdoll.EnableRagdoll();
            }

            cooldownTimer.Reset();

            // 1. Local play (no network latency)
            PlaySound();

            // 2. Tell server to play it globally
            if (NetworkClient.active)
            {
                CmdPlaySoundOnAllClients();
            }
        }
    }

    [Command(requiresAuthority = false)]
    private void CmdPlaySoundOnAllClients()
    {
        RpcPlaySound();
    }

    [ClientRpc]
    private void RpcPlaySound()
    {
        PlaySound();
    }

    private void PlaySound()
    {
        if (audioSource != null && audioClip != null && !audioSource.isPlaying)
        {
            audioSource.PlayOneShot(audioClip, hitSoundVolume);
        }
    }
}
