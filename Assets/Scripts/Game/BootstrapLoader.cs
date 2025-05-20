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
        if (!initialized)
        {
            Debug.Log("Bootstrap: Creating NetworkManager");
            DontDestroyOnLoad(Instantiate(networkManagerPrefab));
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

        // Final fill
        if (progressBar != null)
            progressBar.value = 1f;

        // Optional: small delay to show 100%
        yield return new WaitForSeconds(0.3f);

        operation.allowSceneActivation = true;
    }
}
