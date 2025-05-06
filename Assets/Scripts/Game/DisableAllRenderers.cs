using UnityEngine;

public class DisableAllRenderers : MonoBehaviour
{
    private void Start()
    {
        // Disable all renderers in the object and its children
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            renderer.enabled = false;
        }
    }
}
