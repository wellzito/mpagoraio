using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AgoraLocalCharacter : MonoBehaviour
{
    public uint UID;
    public bool IsVideoOn = false;
    public bool IsAudioOn = false;
    public Image audioIcon;
    public Sprite muteAudioSprite, onAudioSprite;
    public GameObject audioSpeakingOutline;
    public GameObject panelSound;

    public AgoraController agoraController;

    private List<AudioZone> audioZones;
    public List<AudioZone> AudioZones { get => audioZones; }

    public WebcamVideo webcamVideo;

    private void Awake()
    {
        audioSpeakingOutline.SetActive(false);
    }
    public void InitExternal()
    {
        StartCoroutine(Init());
    }
    IEnumerator Init()
    {
        audioZones = new List<AudioZone>();

      //  if (!photonView.IsMine)
        {
            //panelSound.SetActive(false);
        }

        yield return new WaitForSeconds(0.15f);
        AgoraCharacterData characterData = new AgoraCharacterData() { characterInstance = this };
        characterData.agoraVideoType = AgoraVideoType.Video3D;
        characterData.hasWebcamInitialized = true;

        if (AgoraController.instance)
        {
            AgoraController.instance.AddCharacter(characterData);
            Debug.Log($"Sucess Agora");
        }
        else
        {
            AgoraController agoraIO = agoraController;
            agoraIO.AddCharacter(characterData);
            Debug.Log($"Sucess Agora Find");
        }
        UID = agoraController.myUIDUint;//photonView.ViewID;
        agoraController.ShareByUID(characterData.characterInstance.webcamVideo.GetVideoSurface().transform, UID.ToString(), "webcam");
        yield return null;
    }

    private void Update()
    {
        if (!agoraController) panelSound.SetActive(false);

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

    private void OnDestroy()
    {
        if (agoraController)
            agoraController.CharacterLeaveChannel(UID);
        else if(AgoraController.instance) AgoraController.instance.CharacterLeaveChannel(UID);
    }

    public void AddToAudioZone(AudioZone zone)
    {
        if (!audioZones.Contains(zone))
            audioZones.Add(zone);
    }
    public void RemoveFromAudioZone(AudioZone zone)
    {
        audioZones.Remove(zone);
    }
}

[System.Serializable]
public class AgoraCharacterData
{
    public AgoraVideoType agoraVideoType;
    public AgoraLocalCharacter characterInstance;
    public bool hasWebcamInitialized;
}
