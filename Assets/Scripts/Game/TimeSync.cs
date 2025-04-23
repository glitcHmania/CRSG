using Mirror;
using UnityEngine;

public class TimeSync : NetworkBehaviour
{
    [SyncVar] public float serverStartTime;

    public override void OnStartServer()
    {
        serverStartTime = Time.time;
    }
}
