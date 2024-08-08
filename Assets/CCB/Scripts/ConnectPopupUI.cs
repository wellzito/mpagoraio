using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ConnectPopupUI : MonoBehaviour
{
    public static ConnectPopupUI instance;
    public TextMeshProUGUI textMessage;
    public GameObject panelMessage;
    bool onStartMensage;
    bool onEndMensage;

    private void Awake()
    {
        instance = this;
    }
    public void EnableObj(bool v)
    {
        panelMessage.SetActive(v);
    }

    public void Message(string v)
    {
        textMessage.text = v;
    }

    private void Update()
    {
        if(PlayerSpawner.instance.playersInScene <= 0)
        {
            if (!onStartMensage)
            {
                onEndMensage = false;
                onStartMensage = true;
                EnableObj(true);
                StartCoroutine(FakeMessage(false));
            }
        }
        else
        {
            if (!onEndMensage)
            {
                onEndMensage = true;
                onStartMensage = false;         
            }
            EnableObj(false);
        }
    }

    IEnumerator FakeMessage(bool v)
    {
        if (!v)
        {
            Message("Start Connection");
            yield return new WaitForSeconds(0.2f);
            Message("Connecting to the server");
            yield return new WaitForSeconds(0.2f);
            Message("Receiving information");
            yield return new WaitForSeconds(0.2f);
            Message("Opening room");
            yield return new WaitForSeconds(0.2f);
            Message("Entering the Room");
            yield return new WaitForSeconds(0.2f);
            Message("Selecting the player");
            yield return new WaitForSeconds(0.2f);
            Message("You're all set");
        }
    }
}
