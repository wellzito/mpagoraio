using System;
using ReadyPlayerMe.Core;
using ReadyPlayerMe.Core.Analytics;
using UnityEngine;
using UnityEngine.UI;
using ReadyPlayerMe.Samples.QuickStart;

public class AvatarLoadMP : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject avatarLoading;
    [Header("Character Managers")]
    public PersonLoader thirdPersonLoader;

    public bool onSelect;
    public bool localCam;
    ReadyPlayerMe.Samples.QuickStart.CameraFollow cameraFollow;
    ReadyPlayerMe.Samples.QuickStart.CameraOrbit cameraOrbit;

    private void Start()
    {
        cameraOrbit = FindObjectOfType<CameraOrbit>();
        cameraFollow = cameraOrbit.cameraFollow;

        AnalyticsRuntimeLogger.EventLogger.LogRunQuickStartScene();
    }

    private void Update()
    {
        if (onSelect )
        {
            onSelect = false;
            string url = PlayerPrefs.GetString("myPlayer");
            OnLoadAvatarLink(url);
        }
    }

    public void OnLoadAvatarLink(string _url)
    {
        //_url = "https://models.readyplayer.me/" + _url + ".glb";
        if (!thirdPersonLoader) thirdPersonLoader = GetComponentInChildren<PersonLoader>();
        thirdPersonLoader.OnLoadComplete += OnLoadComplete;
        SetActiveLoading(true, "Loading...");
        Debug.Log($"AVATAR: {_url}");
        thirdPersonLoader.LoadAvatar(_url);
        AnalyticsRuntimeLogger.EventLogger.LogPersonalAvatarLoading(_url);
        PlayerPrefs.SetString("myPlayer", _url);
    }

    private void SetActiveLoading(bool enable, string text)
    {
        avatarLoading.SetActive(enable);
    }
    private void OnLoadComplete()
    {
        thirdPersonLoader.OnLoadComplete -= OnLoadComplete;
        SetActiveLoading(false, "");

        if (localCam)
        {
            cameraOrbit.playerInput = GetComponent<PlayerInput>();
        }
    }
}
