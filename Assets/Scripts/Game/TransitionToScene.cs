using UnityEngine;
using Mirror;
using System.Collections;
using UnityEngine.SceneManagement;
using System.IO;

public class TransitionToScene : NetworkBehaviour
{

    private MyNetworkManager myNetworkManagerScript;
    private FadeInOutScreen fadeInOutScreenScript;


    [Scene]
    public string transitionToSceneName;
    public string scenePosToSpawnOn;




    private void Awake()
    {
        if (myNetworkManagerScript == null)
        {
            myNetworkManagerScript = FindObjectOfType<MyNetworkManager>();
            fadeInOutScreenScript = FindObjectOfType<FadeInOutScreen>();
        }
    }



    private void OnTriggerEnter(Collider collision)
    {
        if (collision.GetComponent<Movement>())
        {
            if (collision.TryGetComponent<Movement>(out Movement playerMoveScript))
            {
                playerMoveScript.enabled = false;
            }
            if (isServer)
            {
                StartCoroutine(SendPlayerToNewScene(collision.gameObject));
            }
        }
    }


    [ServerCallback]
    IEnumerator SendPlayerToNewScene(GameObject player)
    {
        if (player.TryGetComponent<NetworkIdentity>(out NetworkIdentity identity))
        {

            NetworkConnectionToClient conn = identity.connectionToClient;
            if (conn == null) yield break;


            conn.Send(new SceneMessage { sceneName = this.gameObject.scene.path, sceneOperation = SceneOperation.UnloadAdditive, customHandling = true });


            yield return new WaitForSeconds((fadeInOutScreenScript.speed * 0.1f));

            NetworkServer.RemovePlayerForConnection(conn, RemovePlayerOptions.Destroy);



            NetworkStartPosition[] allStartPos = FindObjectsOfType<NetworkStartPosition>();

            Transform start = myNetworkManagerScript.GetStartPosition();
            foreach (var item in allStartPos)
            {
                if (item.gameObject.scene.name == Path.GetFileNameWithoutExtension(transitionToSceneName) && item.name == scenePosToSpawnOn)
                {
                    start = item.transform;
                }
            }

            player.transform.position = start.position;



            SceneManager.MoveGameObjectToScene(player, SceneManager.GetSceneByPath(transitionToSceneName));

            conn.Send(new SceneMessage { sceneName = transitionToSceneName, sceneOperation = SceneOperation.LoadAdditive, customHandling = true });


            NetworkServer.AddPlayerForConnection(conn, player);


            if (NetworkClient.localPlayer != null && NetworkClient.localPlayer.TryGetComponent<Movement>(out Movement playerMoveScript))
            {
                playerMoveScript.enabled = true;
            }


        }
    }


}
