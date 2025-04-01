using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class ScaleParticles : MonoBehaviour
{
    void Update()
    {
        var particleSystem = GetComponent<ParticleSystem>();
        var mainModule = particleSystem.main;
        mainModule.startSize = transform.lossyScale.magnitude;
    }
}
