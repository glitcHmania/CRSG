using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class BootstrapLoader : MonoBehaviour
{
    [Header("Fallback scene to load if none set")]
    [SerializeField] private string defaultScene = "MainMenu";

    [Header("Network Manager Prefab")]
    [SerializeField] private GameObject networkManagerPrefab;

    [Header("Optional: Loading UI")]
    [SerializeField] private GameObject loadingUI;
    [SerializeField] private Slider progressBar;

    public static string SceneToLoad;
    private static bool initialized = false;

    private void Awake()
    {
        Debug.Log("BOOTSTRAP AWAKE: initialized = " + initialized);

        if (!initialized)
        {
            Debug.Log("BOOTSTRAP: Creating NetworkManager from prefab.");

            var instance = Instantiate(networkManagerPrefab);
            DontDestroyOnLoad(instance);

            var steamLobby = instance.GetComponentInChildren<SteamLobby>();
            if (steamLobby != null)
            {
                Debug.Log("✅ SteamLobby found in prefab and assigned.");
                SteamLobby.Instance = steamLobby;
            }
            else
            {
                Debug.LogError("❌ SteamLobby NOT found on the network manager prefab!");
            }

            initialized = true;
        }
    }


    private void Start()
    {
        string targetScene = !string.IsNullOrEmpty(SceneToLoad) ? SceneToLoad : defaultScene;
        SceneToLoad = null;

        if (loadingUI != null)
            loadingUI.SetActive(true);

        StartCoroutine(LoadSceneAsync(targetScene));
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        Debug.Log("Bootstrap: Loading scene asynchronously → " + sceneName);

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;

        while (operation.progress < 0.9f)
        {
            if (progressBar != null)
                progressBar.value = operation.progress;

            yield return null;
        }

        if (progressBar != null)
            progressBar.value = 1f;

        yield return new WaitForSeconds(0.3f);

        operation.allowSceneActivation = true;
    }

    public static void ForceReset()
    {
        initialized = false;
    }
}
