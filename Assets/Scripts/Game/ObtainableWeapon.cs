using Mirror;
using UnityEngine;

public class ObtainableWeapon : NetworkBehaviour
{
    [Header("References")]
    public GameObject weaponPrefab;
    public GameObject itemEffect;

    [Header("Settings")]
    public Vector3 modelOffset = Vector3.zero;
    public float bobAmplitude = 0.25f;
    public float bobFrequency = 2f;
    public float rotationSpeed = 50f;

    private GameObject modelInstance;
    private float startY;

    void Start()
    {
        Instantiate(itemEffect, transform.position + new Vector3(0f, -0.3f), Quaternion.identity, gameObject.transform);

        if (weaponPrefab != null)
        {
            Transform model = weaponPrefab.transform.Find("Model");
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
            WeaponHolder holder = other.GetComponentInParent<WeaponHolder>();
            if (holder != null)
            {
                holder.TryPickupWeapon(this);
            }
        }
    }
}
