using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IgnoreCollision : NetworkBehaviour
{
    [SerializeField]
    Collider ThisCollider;

    [SerializeField]
    Collider[] ColliderToIgnore;
    // Start is called before the first frame update
    void Start()
    {
        foreach (Collider collider in ColliderToIgnore)
        {
            Physics.IgnoreCollision(ThisCollider, collider, true);
        }
    }
}
