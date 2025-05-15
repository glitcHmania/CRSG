using UnityEngine;
using Mirror;

public class PlayerAudioPlayer : NetworkBehaviour
{
    [SerializeField] private AudioSource vocalAudioSource;
    private AudioSource audioSource;
    private PlayerState playerState;

    [Header("Player")]
    [SerializeField] private AudioClip[] launchSounds;
    [SerializeField] private AudioClip[] playerJumpStartSounds;
    [SerializeField] private AudioClip[] ragdollSounds;
    [SerializeField] private AudioClip[] handSounds;
    [SerializeField] private AudioClip[] breathSounds;

    [Header("Default")]
    [SerializeField] private AudioClip[] defaultWalkingSounds;
    [SerializeField] private AudioClip[] defaultRunningSounds;
    [SerializeField] private AudioClip[] defaultJumpStartSounds;
    [SerializeField] private AudioClip[] defaultJumpEndSounds;

    [Header("Water")]
    [SerializeField] private AudioClip[] waterWalkingSounds;
    [SerializeField] private AudioClip[] waterRunningSounds;
    [SerializeField] private AudioClip[] waterJumpStartSounds;
    [SerializeField] private AudioClip[] waterJumpEndSounds;

    [Header("Grass")]
    [SerializeField] private AudioClip[] grassWalkingSounds;
    [SerializeField] private AudioClip[] grassRunningSounds;
    [SerializeField] private AudioClip[] grassJumpStartSounds;
    [SerializeField] private AudioClip[] grassJumpEndSounds;

    [Header("Dirt")]
    [SerializeField] private AudioClip[] dirtWalkingSounds;
    [SerializeField] private AudioClip[] dirtRunningSounds;
    [SerializeField] private AudioClip[] dirtJumpStartSounds;
    [SerializeField] private AudioClip[] dirtJumpEndSounds;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        playerState = GetComponent<PlayerState>();
    }

    public void PlayBreathSound()
    {
        CmdPlayBreathSound();
    }

    public void PlayHandSound()
    {
        CmdPlayHandSound();
    }

    public void PlayRagdollSound()
    {
        CmdPlayRagdollSound();
    }

    public void PlayLaunchSound()
    {
        CmdPlayLaunchSound();
    }

    public void PlayFootStepSound()
    {
        CmdPlayFootstep((int)playerState.AudioFieldType, (int)playerState.MovementState, playerState.IsMoving);
    }

    public void PlayJumpStartSound(bool playVocal)
    {
        CmdPlayJumpStart((int)playerState.AudioFieldType, playVocal);
    }

    public void PlayJumpEndSound()
    {
        CmdPlayJumpEnd((int)playerState.AudioFieldType);
    }

    [Command]
    void CmdPlayBreathSound()
    {
        RpcPlayBreathSound();
    }

    [Command]
    void CmdPlayHandSound()
    {
        RpcPlayHandSound();
    }

    [Command]
    void CmdPlayRagdollSound()
    {
        RpcPlayRagdollSound();
    }

    [Command]
    void CmdPlayLaunchSound()
    {
        RpcPlayLaunchSound();
    }

    [Command]
    void CmdPlayFootstep(int fieldType, int movementType, bool isMoving)
    {
        RpcPlayFootstep(fieldType, movementType, isMoving);
    }

    [Command]
    void CmdPlayJumpStart(int fieldType, bool playVocal)
    {
        RpcPlayJumpStart(fieldType, playVocal);
    }

    [Command]
    void CmdPlayJumpEnd(int fieldType)
    {
        RpcPlayJumpEnd(fieldType);
    }

    [ClientRpc]
    void RpcPlayBreathSound()
    {
        PlayRandomVocalSound(breathSounds);
    }

    [ClientRpc]
    void RpcPlayHandSound()
    {
        PlayRandomSound(handSounds, 0.02f, true, 0.4f, 0f);
    }

    [ClientRpc]
    void RpcPlayRagdollSound()
    {
        PlayRandomVocalSound(ragdollSounds);
    }

    [ClientRpc]
    void RpcPlayLaunchSound()
    {
        PlayRandomVocalSound(launchSounds);
    }

    [ClientRpc]
    void RpcPlayFootstep(int fieldType, int movementType, bool isMoving)
    {
        if (!isMoving) return;

        AudioClip[] walkingSounds = defaultWalkingSounds;
        AudioClip[] runningSounds = defaultRunningSounds;

        switch ((PlayerState.AudioField)fieldType)
        {
            case PlayerState.AudioField.Water:
                walkingSounds = waterWalkingSounds;
                runningSounds = waterRunningSounds;
                break;
            case PlayerState.AudioField.Grass:
                walkingSounds = grassWalkingSounds;
                runningSounds = grassRunningSounds;
                break;
            case PlayerState.AudioField.Dirt:
                walkingSounds = dirtWalkingSounds;
                runningSounds = dirtRunningSounds;
                break;
        }

        if ((PlayerState.Movement)movementType == PlayerState.Movement.Walking)
            PlayRandomSound(walkingSounds, 0.15f, true);
        else if ((PlayerState.Movement)movementType == PlayerState.Movement.Running)
            PlayRandomSound(runningSounds, 0.15f, true);
    }

    [ClientRpc]
    void RpcPlayJumpStart(int fieldType, bool playVocal)
    {
        AudioClip[] jumpStartSounds = defaultJumpStartSounds;
        switch ((PlayerState.AudioField)fieldType)
        {
            case PlayerState.AudioField.Water: jumpStartSounds = waterJumpStartSounds; break;
            case PlayerState.AudioField.Grass: jumpStartSounds = grassJumpStartSounds; break;
            case PlayerState.AudioField.Dirt: jumpStartSounds = dirtJumpStartSounds; break;
        }

        PlayRandomSound(jumpStartSounds, randomizeMore: true);
        if (playVocal)
        {
            PlayRandomVocalSound(playerJumpStartSounds);
        }
    }

    [ClientRpc]
    void RpcPlayJumpEnd(int fieldType)
    {
        AudioClip[] jumpEndSounds = defaultJumpEndSounds;
        switch ((PlayerState.AudioField)fieldType)
        {
            case PlayerState.AudioField.Water: jumpEndSounds = waterJumpEndSounds; break;
            case PlayerState.AudioField.Grass: jumpEndSounds = grassJumpEndSounds; break;
            case PlayerState.AudioField.Dirt: jumpEndSounds = dirtJumpEndSounds; break;
        }

        PlayRandomSound(jumpEndSounds, 0.3f);
    }

    private void PlayRandomSound(AudioClip[] sounds, float volume = 1f, bool randomizeMore = false, float maxPitch = 0.2f, float minPitch = -0.2f)
    {
        if (sounds == null || sounds.Length == 0) return;

        if (randomizeMore)
            audioSource.pitch = 1f + Random.Range(minPitch, maxPitch);

        int randomIndex = Random.Range(0, sounds.Length);
        audioSource.volume = volume;
        audioSource.PlayOneShot(sounds[randomIndex]);
    }

    private void PlayRandomVocalSound(AudioClip[] sounds, float volume = 0.15f)
    {
        if (sounds == null || sounds.Length == 0) return;

        int randomIndex = Random.Range(0, sounds.Length);
        vocalAudioSource.volume = volume;
        vocalAudioSource.PlayOneShot(sounds[randomIndex]);
    }
}
