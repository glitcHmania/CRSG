using Mirror;
using UnityEngine;

public class DropSound : NetworkBehaviour
{
    private AudioSource audioSource;
    [SerializeField] private AudioClip clip;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!isServer) return;

        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            if (audioSource != null && clip != null)
            {
                audioSource.pitch = 1f + Random.Range(0f, 0.4f);
                audioSource.PlayOneShot(clip);
            }

            RpcPlaySound();
        }
    }

    [ClientRpc]
    private void RpcPlaySound()
    {
        if (isServer) return;

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (audioSource == null || clip == null)
        {
            Debug.LogWarning("DropSound: AudioSource or AudioClip is not assigned.");
            return;
        }

        audioSource.pitch = 1f + Random.Range(0f, 0.4f);
        audioSource.PlayOneShot(clip);
    }
}
