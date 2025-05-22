using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSpawner : NetworkBehaviour
{
    [SerializeField] private Camera playerCamera;
    private static List<int> usedSpawnIndices = new List<int>(); // server only
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

        HandleCameraForScene(scene.name);

        StartCoroutine(DelayedSpawn());
    }

    private IEnumerator DelayedSpawn()
    {
        yield return null; // wait 1 frame
        TryRequestSpawn();
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
        if (!isOwned) return;

        if (isServer)
        {
            // Host (Server + Client)
            AssignAndBroadcastSpawnPoint(connectionToClient);
        }
        else
        {
            CmdRequestSpawn();
        }
    }

    [Command]
    private void CmdRequestSpawn(NetworkConnectionToClient sender = null)
    {
        AssignAndBroadcastSpawnPoint(sender);
    }

    private void AssignAndBroadcastSpawnPoint(NetworkConnectionToClient conn)
    {
        Transform spawnPoint = GetUniqueSpawnPoint();
        Vector3 pos = spawnPoint.position;
        Quaternion rot = spawnPoint.rotation;

        // Apply server-side
        TeleportInstantlyFromValues(pos, rot);

        // Inform client
        TargetApplySpawn(conn, pos, rot);
    }

    [TargetRpc]
    private void TargetApplySpawn(NetworkConnection conn, Vector3 pos, Quaternion rot)
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

        if (spawnPoints.Length == 0)
        {
            Debug.LogWarning("No spawn points found in scene.");
            return transform;
        }

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (!usedSpawnIndices.Contains(i))
            {
                usedSpawnIndices.Add(i);
                return spawnPoints[i].transform;
            }
        }

        // fallback if all are used
        int fallback = Random.Range(0, spawnPoints.Length);
        Debug.LogWarning("All spawn points used. Assigning random fallback.");
        return spawnPoints[fallback].transform;
    }
}
