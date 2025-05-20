using UnityEngine;

public class IgnoreCollision : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Collider ThisCollider;
    [SerializeField] private Collider[] ColliderToIgnore;

    void Start()
    {
        foreach (Collider collider in ColliderToIgnore)
        {
            Physics.IgnoreCollision(ThisCollider, collider, true);
        }
    }
}
