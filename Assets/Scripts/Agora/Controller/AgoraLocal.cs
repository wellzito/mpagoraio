using agora_gaming_rtc;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AgoraLocal : MonoBehaviour
{
    public uint UID;
    public WebcamVideo webcamVideo;
    PlayerInfo playerInfo;
    public PlayerInfo GetPlayerInfo
    {
        get { return playerInfo; }
    }

    public bool IsVideoOn = false;
    public bool IsAudioOn = false;
    public Image audioIcon;
    public Sprite muteAudioSprite, onAudioSprite;
    public GameObject audioSpeakingOutline;
    public GameObject panelSound;

    private void Awake()
    {
        playerInfo = GetComponent<PlayerInfo>();
    }

    private void Start()
    {
        AgoraCharacterData characterData = new AgoraCharacterData() { characterInstance = this };
        AgoraUnityVideo.Instance.AddCharacter(characterData);
    }

    private void Update()
    {
        if (IsAudioOn)
        {
            audioIcon.sprite = onAudioSprite;
            panelSound.SetActive(true);
        }
        else
        {
            audioIcon.sprite = muteAudioSprite;
            panelSound.SetActive(false);
        }
    }

    public void ExternalInit(uint uid)
    {
        AgoraCharacterData characterData = new AgoraCharacterData() { characterInstance = this };
        characterData.agoraVideoType = AgoraVideoType.Video3D;
        characterData.hasWebcamInitialized = true;
        UID = uid;
        webcamVideo.GetVideoSurface().SetForUser(UID);
        webcamVideo.GetVideoSurface().SetEnable(true);
        webcamVideo.GetVideoSurface().SetVideoSurfaceType(AgoraVideoSurfaceType.RawImage);
    }

    [System.Serializable]
    public class AgoraCharacterData
    {
        public AgoraVideoType agoraVideoType;
        public AgoraLocal characterInstance;
        public bool hasWebcamInitialized;
    }
}

public enum AgoraVideoType
{
    None,
    Video2D,
    Video3D
}
