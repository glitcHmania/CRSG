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
        // Let the SERVER handle the sound logic
        if (!isServer) return;

        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            RpcPlaySound(); // Broadcast to all clients

            // Play the sound on server (host player too)
            audioSource.pitch = 1f + Random.Range(0f, 0.4f);
            audioSource.PlayOneShot(clip);
        }
    }

    [ClientRpc]
    private void RpcPlaySound()
    {
        // Play on remote clients
        if (isServer) return; // Skip if we're also the server

        audioSource.pitch = 1f + Random.Range(0f, 0.4f);
        audioSource.PlayOneShot(clip);
    }
}
