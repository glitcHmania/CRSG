using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [Header("Scene Music Map")]
    public List<SceneMusic> sceneMusicList = new List<SceneMusic>();

    private Dictionary<string, AudioClip> sceneMusicMap = new Dictionary<string, AudioClip>();

    private AudioSource audioSource;

    [Header("Settings")]
    public float fadeDuration = 1f;
    public float volume = 0.7f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.volume = 0;

        foreach (var entry in sceneMusicList)
        {
            if (!sceneMusicMap.ContainsKey(entry.sceneName))
                sceneMusicMap.Add(entry.sceneName, entry.musicClip);
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (sceneMusicMap.TryGetValue(scene.name, out AudioClip newClip))
        {
            StartCoroutine(SwitchMusic(newClip));
        }
        else
        {
            StartCoroutine(FadeOutAndStop());
        }
    }

    private IEnumerator SwitchMusic(AudioClip newClip)
    {
        if (audioSource.isPlaying && audioSource.clip == newClip)
            yield break;

        yield return StartCoroutine(FadeOutAndStop());

        audioSource.clip = newClip;
        audioSource.Play();

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(0, volume, t / fadeDuration);
            yield return null;
        }

        audioSource.volume = volume;
    }

    private IEnumerator FadeOutAndStop()
    {
        float startVol = audioSource.volume;
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVol, 0, t / fadeDuration);
            yield return null;
        }

        audioSource.Stop();
        audioSource.clip = null;
        audioSource.volume = 0;
    }
}

[System.Serializable]
public class SceneMusic
{
    public string sceneName;
    public AudioClip musicClip;
}
