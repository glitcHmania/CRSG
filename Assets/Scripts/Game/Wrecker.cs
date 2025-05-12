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
        if (cooldownTimer.IsFinished && (other.gameObject.layer == LayerMask.NameToLayer("PlayerSpine") || other.gameObject.layer == LayerMask.NameToLayer("PlayerHip")))
        {
            other.gameObject.GetComponentInParent<RagdollController>().EnableRagdoll();
            cooldownTimer.Reset();
            CmdPlaySound();
            if (isServer)
            {
                audioSource.PlayOneShot(audioClip);
            }
        }
    }

    [Command]
    public void CmdPlaySound()
    {
        RpcPlaySound();
    }

    [ClientRpc]
    public void RpcPlaySound()
    {
        audioSource.PlayOneShot(audioClip);
    }
}
