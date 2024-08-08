using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class CamAvatar : NetworkBehaviour
{
    public Camera cam;
    public int numAvatar;
    RenderTexture renderTexture;
    NetworkObject networkObject;

    private void Start()
    {
        networkObject = GetComponent<NetworkObject>();

        foreach (var item in PlayerSpawner.instance._spawnedCharacters)
        {
            if (item.Key.IsMasterClient)
            {
                numAvatar = 0;
            }
            else
            {
                numAvatar = 1;
            }
        }

        CreateTexture();
    }

    // Update is called once per frame
    void Update()
    {
       /* if (MatchController.instance)
        {
            foreach (var item in PlayerSpawner.instance._spawnedCharacters)
            {
                if (item.Key.IsMasterClient)
                {
                    MatchController.instance.rawPlayer1.texture = cam.targetTexture;
                    MatchController.instance.playerName1.text = gameObject.name;
                    MatchController.instance.player2Text.text = networkObject.Name;
                }
                else
                {
                    MatchController.instance.rawPlayer2.texture = cam.targetTexture;
                    MatchController.instance.playerName2.text = gameObject.name;
                    MatchController.instance.player1Text.text = networkObject.Name;
                }
            }
        }*/
    }

    void CreateTexture()
    {
        int textureWidth = 768;
        int textureHeight = 1024;
        renderTexture = new RenderTexture(textureWidth, textureHeight, 24);
        renderTexture.Create();

        cam.targetTexture = renderTexture;
    }
}
