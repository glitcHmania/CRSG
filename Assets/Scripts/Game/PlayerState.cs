using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class PlayerState : NetworkBehaviour
{
    public enum Movement
    {
        Idle,
        Walking,
        Running,
        Jumping,
        Falling,
        Climbing
    }

    public enum AudioField
    {
        Default,
        Water,
        Grass,
        Dirt,
    }

    [SerializeField] private Camera playerCamera; // Assign in inspector

    [SyncVar] public Movement MovementState;
    [SyncVar] public AudioField AudioFieldType;
    [SyncVar] public bool IsAiming;
    [SyncVar] public bool IsGrounded;
    [SyncVar] public bool IsRagdoll;
    [SyncVar] public bool IsUnbalanced;
    [SyncVar] public bool IsArmed;
    [SyncVar] public bool IsBouncing;
    [SyncVar] public bool IsCrouching;
    [SyncVar] public bool IsClimbing;
    [SyncVar] public float Numbness;

    public bool IsMoving => (MovementState == Movement.Walking || MovementState == Movement.Running) && MovementState != Movement.Jumping;

    public static bool IsInGameScene { get; private set; }

    private static HashSet<int> usedSpawnIndices = new HashSet<int>();

    public override void OnStartLocalPlayer()
    {
        if (SceneManager.GetActiveScene().name == "Lobby")
        {
            TrySpawnAtAvailablePoint();
            EnableSceneCamera(true);
        }
        else if (SceneManager.GetActiveScene().name == "Game")
        {
            TrySpawnAtAvailablePoint();
            EnablePlayerCamera();
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        IsInGameScene = scene.name == "Game";

        if (!isLocalPlayer) return;

        if (scene.name == "Game")
        {
            TrySpawnAtAvailablePoint();
            EnablePlayerCamera();
            EnableSceneCamera(false);
        }
        else if (scene.name == "Lobby")
        {
            EnableSceneCamera(true);
        }
        else if(scene.name == "MainMenu")
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private void EnablePlayerCamera()
    {
        if (playerCamera != null)
        {
            playerCamera.enabled = true;
        }
    }

    private void EnableSceneCamera(bool enable)
    {
        var sceneCam = Camera.main;
        if (sceneCam != null && sceneCam != playerCamera)
        {
            sceneCam.enabled = enable;
        }
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

    private void TrySpawnAtAvailablePoint()
    {
        GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");

        if (spawnPoints.Length == 0)
        {
            Debug.LogWarning("No spawn points found in scene.");
            return;
        }

        Transform chosenPoint = null;

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (!usedSpawnIndices.Contains(i))
            {
                usedSpawnIndices.Add(i);
                chosenPoint = spawnPoints[i].transform;
                break;
            }
        }

        if (chosenPoint == null)
        {
            int fallbackIndex = Random.Range(0, spawnPoints.Length);
            Debug.LogWarning("All spawn points used. Assigning random fallback.");
            chosenPoint = spawnPoints[fallbackIndex].transform;
        }

        StartCoroutine(SafeRagdollSpawn(chosenPoint.position, chosenPoint.rotation));
    }

    private IEnumerator SafeRagdollSpawn(Vector3 position, Quaternion rotation)
    {
        Rigidbody[] rigidbodies = GetComponentsInChildren<Rigidbody>();

        // Step 1: Pause physics safely
        foreach (var rb in rigidbodies)
        {
            if (rb == null) continue;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        // Wait for at least 1 physics frame to let the system settle
        yield return new WaitForFixedUpdate();

        // Step 2: Move using Rigidbody positions (not transform)
        Rigidbody root = GetComponent<Rigidbody>();
        if (root != null)
        {
            root.position = position;
            root.rotation = rotation;
        }
        else
        {
            transform.position = position;
            transform.rotation = rotation;
        }

        // Optional: wait again before reenabling physics
        yield return new WaitForFixedUpdate();

        // Step 3: Reactivate physics
        foreach (var rb in rigidbodies)
        {
            if (rb == null) continue;
            rb.isKinematic = false;
        }
    }


    private void Update()
    {
        if (!isLocalPlayer || !IsInGameScene) return;

        HandleMovementInput();
    }

    private void HandleMovementInput()
    {
        if (Application.isFocused && !ChatBehaviour.Instance.IsInputActive)
        {
            if (Input.GetKey(KeyCode.Space))
            {
                MovementState = Movement.Jumping;
                return;
            }

            if (Input.GetKeyDown(KeyCode.LeftControl))
            {
                IsCrouching = true;
            }
            else if (Input.GetKeyUp(KeyCode.LeftControl))
            {
                IsCrouching = false;
            }
        }

        bool hasInput = (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) ||
                         Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D)) &&
                         Application.isFocused && !ChatBehaviour.Instance.IsInputActive;

        if (hasInput)
        {
            MovementState = Input.GetKey(KeyCode.LeftShift)
                ? Movement.Running
                : Movement.Walking;
        }
        else if (IsGrounded)
        {
            MovementState = Movement.Idle;
        }
    }
}
