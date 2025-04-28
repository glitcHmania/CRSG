using Mirror;
using UnityEngine;

public class ObtainableWeapon : NetworkBehaviour
{
    [Header("References")]
    public GameObject WeaponPrefab;

    [Header("Settings")]
    [SerializeField] private Vector3 modelOffset = Vector3.zero;
    [SerializeField] private float bobAmplitude = 0.25f;
    [SerializeField] private float bobFrequency = 2f;
    [SerializeField] private float rotationSpeed = 50f;

    private GameObject modelInstance;
    private float startY;

    void Start()
    {
        if (WeaponPrefab != null)
        {
            Transform model = WeaponPrefab.transform.Find("Model");
            if (model != null)
            {
                modelInstance = Instantiate(model.gameObject, transform);
                modelInstance.transform.localPosition = modelOffset;
                modelInstance.transform.localRotation = Quaternion.identity;
                startY = modelInstance.transform.localPosition.y;
            }
            else
            {
                Debug.LogWarning("Weapon prefab is missing a 'Model' child!");
            }
        }
    }

    void Update()
    {
        if (modelInstance != null)
        {
            float bobOffset = Mathf.Sin(Time.time * bobFrequency) * bobAmplitude;
            Vector3 pos = modelOffset;
            pos.y = startY + bobOffset;
            modelInstance.transform.localPosition = pos;

            modelInstance.transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponentInParent<WeaponHolder>()?.TryPickupWeapon(this);
        }
    }
}
