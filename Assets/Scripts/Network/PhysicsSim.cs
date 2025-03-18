using UnityEngine;
using Mirror;

public class PhysicsSim : MonoBehaviour
{

    PhysicsScene physicsScene;

    bool simulatePhysicsScene;


    private void Awake()
    {
        if (NetworkServer.active)
        {
            physicsScene = gameObject.scene.GetPhysicsScene();
            simulatePhysicsScene = physicsScene.IsValid() && physicsScene != Physics.defaultPhysicsScene;
        }
        else
        {
            enabled = false;
        }
    }


    private void FixedUpdate()
    {
        if (!NetworkServer.active) return;

        if (simulatePhysicsScene)
            physicsScene.Simulate(Time.fixedDeltaTime);
    }

}
