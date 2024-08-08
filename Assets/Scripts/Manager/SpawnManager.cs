using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager instance;
    public Transform[] positions;

    public GameObject playerLocal;
    public GameObject panelLoadAvatar;

    private void Awake()
    {
        instance = this;
        panelLoadAvatar.SetActive(false);
    }
    public void SpawnPlayerAgora(GameObject playerGameObject)
    {
        playerLocal = playerGameObject;
        /* AgoraController agora = Instantiate(agoraControllerPrefab);
         AgoraLocalCharacter agoraLocal = playerGameObject.GetComponent<AgoraLocalCharacter>();
         agoraLocal.agoraController = agora;
         agoraLocal.InitExternal();*/
        panelLoadAvatar.SetActive(true);
    }
}
