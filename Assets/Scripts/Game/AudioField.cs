using UnityEngine;

public class AudioField : MonoBehaviour
{
    public PlayerState.AudioField type;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerState playerState = other.GetComponentInParent<PlayerState>();
            if (playerState != null)
            {
                playerState.AudioFieldType = type;
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerState playerState = other.GetComponentInParent<PlayerState>();
            if (playerState != null)
            {
                playerState.AudioFieldType = PlayerState.AudioField.Default;
            }
        }
    }
}
