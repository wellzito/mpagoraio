using System;
using ReadyPlayerMe.Core;
using ReadyPlayerMe.Core.Analytics;
using ReadyPlayerMe.Samples.QuickStart;
using UnityEngine;
using UnityEngine.UI;
public class AvatarLoadPanel : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Text openPersonalAvatarPanelButtonText;
    [SerializeField] private Text linkText;
    [SerializeField] private InputField avatarUrlField;
    [SerializeField] private Button openPersonalAvatarPanelButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button linkButton;
    [SerializeField] private Button loadAvatarButton;
    [SerializeField] private GameObject personalAvatarPanel;

    private string defaultButtonText;

    private void OnEnable()
    {
        openPersonalAvatarPanelButton.onClick.AddListener(OnOpenPersonalAvatarPanel);
        closeButton.onClick.AddListener(OnCloseButton);
        linkButton.onClick.AddListener(OnLinkButton);
        loadAvatarButton.onClick.AddListener(OnLoadAvatarButton);
        avatarUrlField.onValueChanged.AddListener(OnAvatarUrlFieldValueChanged);
    }

    private void OnDisable()
    {
        openPersonalAvatarPanelButton.onClick.RemoveListener(OnOpenPersonalAvatarPanel);
        closeButton.onClick.RemoveListener(OnCloseButton);
        linkButton.onClick.RemoveListener(OnLinkButton);
        loadAvatarButton.onClick.RemoveListener(OnLoadAvatarButton);
        avatarUrlField.onValueChanged.RemoveListener(OnAvatarUrlFieldValueChanged);
    }

    private void OnOpenPersonalAvatarPanel()
    {
        linkText.text = $"https://{CoreSettingsHandler.CoreSettings.Subdomain}.readyplayer.me";
        personalAvatarPanel.SetActive(true);
        AnalyticsRuntimeLogger.EventLogger.LogLoadPersonalAvatarButton();
    }

    public void OnLoadAvatarButton()
    {
        PlayerInfo p = SpawnManager.instance.playerLocal.GetComponent<PlayerInfo>();
        // p.onInputExternal = true;
        p.onInputExternal = true;
        p.avatarLoad.OnLoadAvatarLink(avatarUrlField.text);
        //p.RPC_AvatarExternalSet(avatarUrlField.text);
        PlayerPrefs.SetString("myPlayer", avatarUrlField.text);
        OnCloseButton();
    }

    private void OnCloseButton()
    {
        personalAvatarPanel.SetActive(false);
    }

    private void OnLinkButton()
    {
        Application.OpenURL(linkText.text);
    }

    private void OnAvatarUrlFieldValueChanged(string url)
    {
        if (!string.IsNullOrEmpty(url) && Uri.TryCreate(url, UriKind.Absolute, out Uri _))
        {
            loadAvatarButton.interactable = true;
        }
        else
        {
            loadAvatarButton.interactable = false;
        }
    }
}
