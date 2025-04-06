using Mirror;
using UnityEngine;

public class IgnoreCollision : NetworkBehaviour
{
    [SerializeField]
    Collider ThisCollider;

    [SerializeField]
    Collider[] ColliderToIgnore;
    void Start()
    {
        foreach (Collider collider in ColliderToIgnore)
        {
            Physics.IgnoreCollision(ThisCollider, collider, true);
        }
    }
}
