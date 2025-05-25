using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIAudioManager : MonoBehaviour
{
    public AudioClip clickSound;
    public AudioClip hoverSound;
    public AudioClip menuOpenSound;
    public AudioClip menuCloseSound;

    private AudioSource audioSource;

    private static UIAudioManager instance;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(this.gameObject);

        audioSource = gameObject.GetComponent<AudioSource>();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        AttachSoundsToAllButtons();

        // Look for UIManager and subscribe to the toggle event
        UIManager uiManager = FindObjectOfType<UIManager>();
        if (uiManager != null)
        {
            uiManager.OnEscMenuToggle += HandleEscMenuToggle;
        }
    }

    private void HandleEscMenuToggle(bool isActive)
    {
        PlaySound(isActive ? menuOpenSound : menuCloseSound, 0.02f);
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        UIManager uiManager = FindObjectOfType<UIManager>();
        if (uiManager != null)
        {
            uiManager.OnEscMenuToggle -= HandleEscMenuToggle;
        }
    }



    void AttachSoundsToAllButtons()
    {
        Button[] buttons = FindObjectsOfType<Button>(true); // include inactive buttons
        foreach (Button btn in buttons)
        {
            btn.onClick.AddListener(() => PlaySound(clickSound, 0.05f));

            EventTrigger trigger = btn.GetComponent<EventTrigger>();
            if (trigger == null)
                trigger = btn.gameObject.AddComponent<EventTrigger>();

            EventTrigger.Entry entry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerEnter
            };
            entry.callback.AddListener((eventData) =>
            {
                //if button is interactable
                if (btn.interactable)
                {
                    PlaySound(hoverSound, 0.05f);
                }
            });
            trigger.triggers.Add(entry);
        }
    }

    void PlaySound(AudioClip clip, float volume)
    {
        if (clip != null)
        {
            audioSource.PlayOneShot(clip, volume);
        }
    }
}
