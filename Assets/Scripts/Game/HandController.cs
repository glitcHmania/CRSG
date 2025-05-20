using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HandController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private KeyCode controlkey;
    [SerializeField] private Rigidbody foreArm;
    [SerializeField] private PlayerState playerState;
    [SerializeField] private RagdollController ragdollController;
    [SerializeField] private PlayerAudioPlayer playerAudioPlayer;

    [Header("Settings")]
    [SerializeField] private bool isLeftHand = false; 
    [SerializeField] private LayerMask allowedLayers;

    [Header("Right hand reference for left hand only")]
    [SerializeField] private HandController rightHandReference;

    private FixedJoint fixedJoint;
    private GameObject collidedObject = null;
    private bool isColliding = false;
    private bool isCarrying = false;
    private bool isHolding = false;
    private Timer jumpTimer;
    private GameObject[] fingers;
    private Quaternion[] initialFingerRotations;

    // New state flag
    private bool collisionPaused = false;

    private void Start()
    {
        fingers = new GameObject[4];
        initialFingerRotations = new Quaternion[4];
        for (int i = 0; i < fingers.Length; i++)
        {
            fingers[i] = transform.GetChild(i).gameObject;
            initialFingerRotations[i] = fingers[i].transform.localRotation;
        }

        jumpTimer = new Timer(0.05f, () =>
        {
            collisionPaused = true;

            if (fixedJoint != null)
            {
                Destroy(fixedJoint);
                fixedJoint = null;
                isCarrying = false;
                isHolding = false;
                BendFingers(false);
            }
        }, true);
    }

    void Update()
    {
        if (!PlayerSpawner.IsInGameScene) return;

        jumpTimer.Update();

        if (Input.GetKeyDown(controlkey))
        {
            collisionPaused = false;
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            jumpTimer.Reset();
        }

        if (!isLeftHand && playerState.IsArmed)
        {
            if (isHolding || isCarrying)
            {
                if (fixedJoint != null)
                {
                    Destroy(fixedJoint);
                    fixedJoint = null;
                    isHolding = false;
                    isCarrying = false;
                }
                return;
            }
        }

        if (!collisionPaused && !isHolding && !isCarrying && Input.GetKey(controlkey))
        {
            if (isColliding && collidedObject != null)
            {
                if (collidedObject.GetComponent<Rigidbody>())
                {
                    fixedJoint = foreArm.gameObject.AddComponent<FixedJoint>();
                    fixedJoint.connectedBody = collidedObject.GetComponent<Rigidbody>();
                    isCarrying = true;
                }
                else
                {
                    fixedJoint = foreArm.gameObject.AddComponent<FixedJoint>();
                    fixedJoint.connectedAnchor = collidedObject.transform.position;
                    isHolding = true;
                    playerState.IsClimbing = true;
                    ragdollController.EnableBalance();

                }
                BendFingers(true);
            }
        }

        if (Input.GetKeyUp(controlkey))
        {
            if (fixedJoint != null)
            {
                Destroy(fixedJoint);
                fixedJoint = null;

                if (isCarrying)
                    isCarrying = false;
                else
                    isHolding = false;

                BendFingers(false);
            }
        }

        if (isLeftHand && !isHolding && !rightHandReference.isHolding)
        {
            playerState.IsClimbing = false;
        }
    }

    private void BendFingers(bool state)
    {
        for (int i = 0; i < fingers.Length; i++)
        {
            if (state)
            {
                fingers[i].transform.localRotation = Quaternion.Euler(initialFingerRotations[i].eulerAngles.x + 60, initialFingerRotations[i].eulerAngles.y, initialFingerRotations[i].eulerAngles.z);
                playerAudioPlayer.PlayHandSound();
            }
            else
            {
                fingers[i].transform.localRotation = initialFingerRotations[i];
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isLeftHand && playerState.IsArmed) return;
        if (collisionPaused) return;

        if (((1 << other.gameObject.layer) & allowedLayers) != 0)
        {
            isColliding = true;
            collidedObject = other.gameObject;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!isLeftHand && playerState.IsArmed) return;
        if (collisionPaused) return;

        if (((1 << other.gameObject.layer) & allowedLayers) != 0)
        {
            isColliding = true;
            collidedObject = other.gameObject;
        }
    }


    private void OnTriggerExit(Collider other)
    {
        if (!isLeftHand && playerState.IsArmed) return;

        if (((1 << other.gameObject.layer) & allowedLayers) != 0)
        {
            isColliding = false;
            collidedObject = null;
        }
    }
}
