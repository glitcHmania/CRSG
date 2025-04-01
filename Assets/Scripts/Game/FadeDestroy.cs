using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeDestroy : MonoBehaviour
{
    public GameObject fadeTarget;
    public float lifeTime = 3.0f;

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

        Color color = rendererComponent.material.color;
        color.a = 1f - timer.GetRatio();
        rendererComponent.material.color = color;
    }
}
