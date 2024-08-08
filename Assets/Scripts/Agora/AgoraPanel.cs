using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AgoraPanel : MonoBehaviour
{
    public static AgoraPanel instance;

    [Header("Agora Microphone")]
    public Image iconMic;
    public Sprite micOn;
    public Sprite micOff;

    [Header("List Webcams")]
    public TMP_Dropdown dropdownCams;
    public Toggle flipV, flipH;
    bool onInitCall;
    public Transform panelShare;
    public bool onScreenShare;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        iconMic.sprite = micOff;
    }

    void Update()
    {
        if (AgoraController.instance)
        {
            if (AgoraController.instance.GetIsAudio)
            {
                iconMic.sprite = micOn;
            }
            else
            {
                iconMic.sprite = micOff;
            }

            if (!onInitCall)
            {
                if (AgoraController.instance.VideoDeviceManager != null)
                {
                    onInitCall = true;
                    AgoraController.instance.videoDropdown = dropdownCams;
                    AgoraController.instance.GetDevices();
                }
            }
        }
    }

    public void ToggleAudio()
    {
        AgoraController.instance.ToggleAudio();
    }

    public void ToggleCamera()
    {
        AgoraController.instance.ToggleCamera();
    }

    public void SetCurrentDevice(int n)
    {
        AgoraController.instance.SetCurrentDevice();
    }

    public void FlipCamH(bool o)
    {
        AgoraController.instance.LocalCharacter.webcamVideo.GetVideoSurface().EnableFlipTextureApplyX(flipH);
    }
    public void FlipCamV(bool o)
    {
        AgoraController.instance.LocalCharacter.webcamVideo.GetVideoSurface().EnableFlipTextureApplyY(flipV);
    }

    public void StartShare()
    {
        onScreenShare = !onScreenShare;
        AgoraController.instance.LocalCharacter.GetComponent<PlayerInfo>();
        AgoraController.instance.ClickedOnShareButton(panelShare);
        AgoraController.instance.StartShare("screen", onScreenShare);
    }
}
