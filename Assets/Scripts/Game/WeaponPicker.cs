using UnityEngine;
using Mirror; // optional if using Mirror to spawn

public class WeaponPicker : MonoBehaviour
{
    public GameObject weaponPrefab; // Reference to the full weapon prefab
    public Vector3 modelOffset = Vector3.zero;
    public float bobAmplitude = 0.25f;
    public float bobFrequency = 2f;
    public float rotationSpeed = 50f;

    private GameObject modelInstance;
    private float startY;

    void Start()
    {
        // Get the model child from the weapon prefab and instantiate it for visuals
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
            // Bobbing
            float bobOffset = Mathf.Sin(Time.time * bobFrequency) * bobAmplitude;
            Vector3 pos = modelOffset;
            pos.y = startY + bobOffset;
            modelInstance.transform.localPosition = pos;

            // Rotation
            modelInstance.transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entered trigger!");
            var weaponHolder = other.GetComponentInParent<WeaponHolder>();
            if (weaponHolder != null)
            {
                // Give the player the weapon
                GameObject newWeapon = Instantiate(weaponPrefab);
                weaponHolder.EquipWeapon(newWeapon);

                // Optionally: Network spawn if using Mirror
                // NetworkServer.Spawn(newWeapon, other.connectionToClient);

                // Destroy the pickup
                Destroy(gameObject);
            }
        }
    }
}
