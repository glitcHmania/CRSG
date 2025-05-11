using UnityEngine;
using Mirror;

public class FloatingOrigin : NetworkBehaviour
{
    // Optional
    [SerializeField] private Transform worldRoot;

    // Threshold to trigger shifting (in units)
    public float shiftThreshold = 100f;

    private void LateUpdate()
    {
        if (!isLocalPlayer) return;

        Vector3 playerPosition = transform.position;

        // Only shift if the player is far enough from origin
        if (playerPosition.magnitude > shiftThreshold)
        {
            Vector3 offset = -playerPosition;

            // Shift worldRoot visuals if assigned
            if (worldRoot != null)
            {
                worldRoot.position += offset;
            }

            // Shift camera
            if (Camera.main != null)
            {
                Camera.main.transform.position += offset;
            }

            // Optionally, shift any other purely visual objects here too
        }
    }
}
