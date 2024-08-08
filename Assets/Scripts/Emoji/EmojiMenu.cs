using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmojiMenu : MonoBehaviour
{
    bool onPanel;
    public GameObject panel;

    private void Start()
    {
        panel.SetActive(onPanel);
    }

    public void PanelOpen()
    {
        onPanel = !onPanel;
        panel.SetActive(onPanel);
    }

    public void SetEmoji(string value)
    {
        SpawnManager.instance.playerLocal.GetComponent<ActionsPlayerController>().ShowCharacterReaction(value);
    }
}
