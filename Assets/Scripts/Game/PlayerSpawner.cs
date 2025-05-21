using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSpawner : NetworkBehaviour
{
    [SerializeField] private Camera playerCamera;
    private static HashSet<int> usedSpawnIndices = new HashSet<int>();
    private static GameObject[] spawnPoints;

    public static bool IsInGameScene { get; private set; }
    private void Awake()
    {
        if (playerCamera == null)
            playerCamera = GetComponentInChildren<Camera>();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        IsInGameScene = SceneManager.GetActiveScene().name == "Game";
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        IsInGameScene = scene.name == "Game";
        Debug.Log($"{scene.name} scene loaded");


        TryRequestSpawn();

        HandleCameraForScene(scene.name);
    }

    private void HandleCameraForScene(string sceneName)
    {
        if (!isOwned) return;  // ← Important!

        if (sceneName == "Game")
        {
            EnablePlayerCamera(true);
        }
        else
        {
            EnablePlayerCamera(false);
        }

        if (sceneName == "MainMenu")
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }


    private void EnablePlayerCamera(bool enable)
    {
        if (playerCamera != null)
            playerCamera.enabled = enable;

        var sceneCam = Camera.main;
        if (sceneCam != null && sceneCam != playerCamera)
            sceneCam.enabled = !enable;
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        Debug.Log("OnStartLocalPlayer called (client has authority)");

        IsInGameScene = SceneManager.GetActiveScene().name == "Game";

        HandleCameraForScene(SceneManager.GetActiveScene().name);


        TryRequestSpawn();
    }


    private void TryRequestSpawn()
    {
        Transform spawnPoint = GetUniqueSpawnPoint();

        // Immediate correction to avoid 1-frame flash
        TeleportInstantly(spawnPoint);

        if (isServer && isClient)
        {
            //spawn request on host (server + client)
            StartCoroutine(SafeRagdollSpawn(spawnPoint));
        }
        else if (isServer)
        {
            //spawn request on server (for remote client)
            RpcSpawnPlayerAtPoint_Internal(spawnPoint.position, spawnPoint.rotation);
        }
        else
        {
            //spawn request on client
            CmdRequestSpawn(spawnPoint.position, spawnPoint.rotation);
        }
    }

    [Command]
    private void CmdRequestSpawn(Vector3 pos, Quaternion rot)
    {
        TeleportInstantlyFromValues(pos, rot);
        RpcSpawnPlayerAtPoint_Internal(pos, rot);
    }

    [ClientRpc]
    private void RpcSpawnPlayerAtPoint_Internal(Vector3 pos, Quaternion rot)
    {
        TeleportInstantlyFromValues(pos, rot);
        StartCoroutine(SafeRagdollSpawnFromValues(pos, rot));
    }

    private void TeleportInstantlyFromValues(Vector3 pos, Quaternion rot)
    {
        Rigidbody root = GetComponent<Rigidbody>();
        if (root != null)
        {
            root.position = pos;
            root.rotation = rot;
        }
        else
        {
            transform.position = pos;
            transform.rotation = rot;
        }
    }

    private IEnumerator SafeRagdollSpawnFromValues(Vector3 pos, Quaternion rot)
    {
        Rigidbody[] rbs = GetComponentsInChildren<Rigidbody>();

        foreach (var rb in rbs)
        {
            if (rb == null) continue;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        yield return new WaitForFixedUpdate();

        Rigidbody root = GetComponent<Rigidbody>();
        if (root != null)
        {
            root.position = pos;
            root.rotation = rot;
        }
        else
        {
            transform.position = pos;
            transform.rotation = rot;
        }

        yield return new WaitForFixedUpdate();

        foreach (var rb in rbs)
        {
            if (rb == null) continue;
            rb.isKinematic = false;
        }
    }

    private Transform GetUniqueSpawnPoint()
    {
        spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");
        Debug.Log($"Found {spawnPoints.Length} spawn points in scene.");

        if (spawnPoints.Length == 0)
        {
            Debug.LogWarning("No spawn points found in scene.");
            return transform;
        }

        Transform chosen = null;

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (!usedSpawnIndices.Contains(i))
            {
                usedSpawnIndices.Add(i);
                chosen = spawnPoints[i].transform;
                break;
            }
        }

        if (chosen == null)
        {
            int fallback = Random.Range(0, spawnPoints.Length);
            Debug.LogWarning("All spawn points used. Assigning random fallback.");
            chosen = spawnPoints[fallback].transform;
        }

        return chosen;
    }

    private IEnumerator SafeRagdollSpawn(Transform spawnPoint)
    {
        Debug.Log($"Spawning player at {spawnPoint.name}");
        Vector3 pos = spawnPoint.position;
        Quaternion rot = spawnPoint.rotation;

        Rigidbody[] rbs = GetComponentsInChildren<Rigidbody>();

        foreach (var rb in rbs)
        {
            if (rb == null) continue;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        yield return new WaitForFixedUpdate();

        Rigidbody root = GetComponent<Rigidbody>();
        if (root != null)
        {
            root.position = pos;
            root.rotation = rot;
        }
        else
        {
            transform.position = pos;
            transform.rotation = rot;
        }

        yield return new WaitForFixedUpdate();

        foreach (var rb in rbs)
        {
            if (rb == null) continue;
            rb.isKinematic = false;
        }
    }
    private void TeleportInstantly(Transform spawnPoint)
    {
        Vector3 pos = spawnPoint.position;
        Quaternion rot = spawnPoint.rotation;

        Rigidbody root = GetComponent<Rigidbody>();
        if (root != null)
        {
            root.position = pos;
            root.rotation = rot;
        }
        else
        {
            transform.position = pos;
            transform.rotation = rot;
        }
    }

}
