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
    void Start()
    {
        foreach (Collider collider in ColliderToIgnore)
        {
            Physics.IgnoreCollision(ThisCollider, collider, true);
        }
    }
}
