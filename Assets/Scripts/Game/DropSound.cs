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

    void OnCollisionEnter(Collision collision)
    {
        //check the layer for ground
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            CmdPlaySound();
            if(isServer)
            {
                //randomize the pitch
                audioSource.pitch = 1f + Random.Range(0f, 0.4f);
                audioSource.PlayOneShot(clip);
            }
        }
    }

    //play the sound on all clients
    [Command]
    public void CmdPlaySound()
    {
        RpcPlaySound();
    }

    [ClientRpc]
    public void RpcPlaySound()
    {
        audioSource.pitch = 1f + Random.Range(0f, 0.4f);
        audioSource.PlayOneShot(clip);
    }
}
