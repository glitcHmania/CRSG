using UnityEngine;

public class FadeDestroy : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject fadeTarget;

    [Header("Settings")]
    [SerializeField] private float lifeTime = 3.0f;
    [SerializeField] private bool fadeOut = true;

    private Timer timer;
    private Renderer rendererComponent;

    void Start()
    {
        timer = new Timer(lifeTime, () => Destroy(gameObject));
        rendererComponent = fadeTarget.GetComponent<Renderer>();
    }

    void Update()
    {
        timer.Update();

        if (fadeOut)
        {
            Color color = rendererComponent.material.color;
            color.a = 1f - timer.GetRatio();
            rendererComponent.material.color = color;
        }
    }
}
