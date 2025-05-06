using Mirror;
using UnityEngine;
public class ObtainableWeapon : NetworkBehaviour
{
    [Header("Prefab Mapping")]
    public WeaponPrefabReference prefabReference;
    public GameObject WeaponPrefab => prefabReference?.weaponPrefab;
    public GameObject SelfPrefab => prefabReference?.obtainableWeaponPrefab;
    [Header("Settings")]
    [SerializeField] private float bobAmplitude = 0.25f;
    [SerializeField] private float bobFrequency = 2f;
    [SerializeField] private float rotationSpeed = 50f;

    private GameObject modelInstance;
    private float startY;
    private Vector3 modelOffset = new Vector3(0f, 0.777f);
    void Start()
    {
        // Only instantiate model if it's NOT already there
        if (modelInstance != null || transform.childCount > 2)
            return;
        if (WeaponPrefab != null)
        {
            Transform model = WeaponPrefab.transform.Find("Model");
            if (model != null)
            {
                modelInstance = Instantiate(model.gameObject, transform);
                foreach (Collider collider in modelInstance.GetComponentsInChildren<Collider>())
                {
                    collider.enabled = false;
                }
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