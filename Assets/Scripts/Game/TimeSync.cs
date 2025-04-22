using Mirror;
using UnityEngine;

public class TimeSync : NetworkBehaviour
{
    [SyncVar] public float serverStartTime;

    public override void OnStartServer()
    {
        serverStartTime = Time.time;
    }
    void Update()
    {
        Debug.Log($"server Time: {serverStartTime} local Time: {NetworkTime.time}");
    }

}
