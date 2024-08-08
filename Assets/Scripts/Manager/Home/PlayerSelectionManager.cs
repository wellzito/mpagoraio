using ReadyPlayerMe.Samples.QuickStart;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class PlayerSelectionManager : MonoBehaviour
{
    public PersonalAvatarLoader personalAvatar;
    public static PlayerSelectionManager instance;
    public int playerSelectionNumber;
    public List<string> playersURL;
    List<Texture> textures = new List<Texture>();
    public RawImage rawImage;
    public GameObject panelSelectPlayer;
    public GameObject panelButtons;

    public Transform content;

    public bool onSelect;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        if (!PlayerPrefs.HasKey("myPlayer"))
            PlayerPrefs.SetString("myPlayer", playersURL[playerSelectionNumber]);
        //CreateCams();
    }

    private void Update()
    {
        if (onSelect)
        {
            onSelect = false;
            personalAvatar.OnLoadAvatarLink(playersURL[playerSelectionNumber]);
        }
    }

    public void NextPlayer()
    {
        playerSelectionNumber++;

        if (playerSelectionNumber > playersURL.Count - 1)
        {
            playerSelectionNumber = 0;
        }
    }

    public void PreviousPlayer()
    {
        playerSelectionNumber--;

        if (playerSelectionNumber < 0)
        {
            playerSelectionNumber = playersURL.Count - 1;
        }
    }

    public void ConnectPlayerCustom()
    {
        PlayerPrefs.SetInt("Skin", playerSelectionNumber);
        PlayerPrefs.SetString("myPlayer", playersURL[playerSelectionNumber]);
    }

    public void ConnectPlayerCustom(int val)
    {
        PlayerPrefs.SetInt("Skin", val);
        PlayerPrefs.SetString("myPlayer", playersURL[val]);
    }
}
