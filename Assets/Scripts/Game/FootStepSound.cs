using UnityEngine;

public class FootStepSound : MonoBehaviour
{
    [SerializeField] private PlayerAudioPlayer audioPlayer;
    [SerializeField] private PlayerState playerState;

    private Timer soundTimer;

    private void Start()
    {
        soundTimer = new Timer(0.4f);
    }

    private void Update()
    {
        soundTimer.Update();
    }

    void OnCollisionEnter(Collision collision)
    {
        //check the layer for ground
        if (soundTimer.IsFinished && collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            audioPlayer.PlayFootStepSound();
        }
    }
}
