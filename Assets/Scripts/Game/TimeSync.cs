using Mirror;
using UnityEngine;

public class TimeSync : NetworkBehaviour
{
    [SyncVar] public float ServerStartTime;

    public override void OnStartServer()
    {
        ServerStartTime = Time.time;
    }
}
