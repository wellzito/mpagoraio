/*
 * AgoraController serves as a game controller object for this application.
 */

using agora_gaming_rtc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
public enum AgoraVideoType
{
    None,
    Video2D,
    Video3D
}

public class AgoraController : MonoBehaviour
{
    public static AgoraController instance;
    public static event Action<Transform, string> OnRemoteUserStartedSharing;   //videoHolder, uid
    public static event Action<string> OnLocalUserJoinedAgoraChannel;
    public static event Action<Transform, string> OnLocalUserStartedSharing;    //videoHolder, uid
    public static event Action<Transform, string> OnLocalUserStoppedSharing;    //videoHolder, uid
    public static event Action OnFailedToGetDeviceList;

    #region Variables

    private IRtcEngine mRtcEngine { get; set; }
    public string AppId { get; private set; }

    //private const string AppID = "5531ac0578164d6c84fad57e34efeeca";
    private string Token = "";

    private string ChannelName = "";

    private bool hasVideoOnJS = false;
    private bool hasAudioOnJS = false;


    [Header("LOCAL PLAYER | ONLY VIEW")]
    //public WebcamVideo LocalPlayerWebcamVideo;
    public AgoraLocalCharacter LocalCharacter;

    // [Header("2D WEBCAMS")]
    // [SerializeField] private GameObject m_WebcamsHolder;

    /* [Header("PREFAB")]
     [SerializeField] private WebcamVideo m_2DWebcamPrefab;
     [SerializeField] private WebcamVideo m_3DWebcamPrefab;*/

    [Header("ONLY VIEW | DO NOT CHANGE")]
    [SerializeField] private AgoraVideoType agoraVideoType;
    [SerializeField] private List<AgoraCharacterData> characters = new List<AgoraCharacterData>();

    private bool hasVideoDevice;

    public bool hasAudioRecording = false;
    public bool HasVideoDevice => (hasVideoDevice || hasVideoOnJS);
    public bool HasAudioRecording => (hasAudioRecording || hasAudioOnJS);

    private int recordingDeviceIndex = 0;
    private int playbackDeviceIndex = 0;
    private int videoDeviceIndex = 0;
    private GameObject screenShareObject;

    private AudioRecordingDeviceManager AudioRecordingDeviceManager;
    private AudioPlaybackDeviceManager AudioPlaybackDeviceManager;
    public VideoDeviceManager VideoDeviceManager;

    private Dictionary<int, string> audioRecordingDevices = new Dictionary<int, string>();
    private Dictionary<int, string> audioPlaybackDevices = new Dictionary<int, string>();
    private Dictionary<int, string> videoDeviceManager = new Dictionary<int, string>();
    private Dictionary<int, string> audioRecordingDeviceNames = new Dictionary<int, string>();
    private Dictionary<int, string> audioPlaybackDeviceNames = new Dictionary<int, string>();
    private Dictionary<int, string> videoDeviceManagerNames = new Dictionary<int, string>();

    private AudioVideoStates AudioVideoState = new AudioVideoStates();

    //private AgoraButtons AgoraButtons;
    public string myUID;
    public uint myUIDUint;
    private string currentSharingUID;
    private string currentSharingType;
    private Transform videoHolder;
    private List<AudioZone> myAudioZones;
    //private Dictionary<AudioZone, List<Character>> characterPerAudioZone;

    public TMP_Dropdown videoDropdown;
    private TMP_Dropdown recordingDropdown;
    private TMP_Dropdown playbackDropdown;

    //[HideInInspector] public UIPopup m_popupAgoraShare;

    #endregion

    #region Methods
    private void Awake()
    {
        instance = this;
    }

    private void FailedToGetDevicesList(string obj)
    {
        Debug.Log("Unable to get devices permission, reason " + obj);

        if (obj.Contains("NotAllowedError"))
        {
            //User denied permission
        }
        else if (obj.Contains("NotFoundError"))
        {
            //User does not have any device
        }
        OnFailedToGetDeviceList?.Invoke();
    }

    private void ReceivedDevicesList(string obj)
    {
        if (obj.Contains("videoinput"))
            hasVideoOnJS = true;
        if (obj.Contains("audioinput"))
            hasAudioOnJS = true;

        StartCoroutine(InitRtcEngine());
    }

    /// <summary>
    /// Start is called before the first frame update
    /// </summary>
    private IEnumerator Start()
    {
        //AgoraButtons = gameObject.GetComponent<AgoraButtons>();
        /*characterPerAudioZone = new Dictionary<AudioZone, List<Character>>();

        videoDropdown = ViewSettings.Instance.m_Panels.FirstOrDefault(x => x.UISettingsPanel == UISettingsPanel.Webcam).Dropdown;
        recordingDropdown = ViewSettings.Instance.m_Panels.FirstOrDefault(x => x.UISettingsPanel == UISettingsPanel.Microphone).Dropdown;
        playbackDropdown = ViewSettings.Instance.m_Panels.FirstOrDefault(x => x.UISettingsPanel == UISettingsPanel.AudioOutput).Dropdown;*/

        //Waits for player initialization to continue agora setup
        AgoraLocalCharacter playerInput = SpawnManager.instance.playerLocal.GetComponent<AgoraLocalCharacter>();
        LocalCharacter = playerInput;

        while (playerInput == null && LocalCharacter == null)
        {
            playerInput = SpawnManager.instance.playerLocal.GetComponent<AgoraLocalCharacter>();
            if (playerInput != null)
                LocalCharacter = playerInput;
            yield return new WaitForSeconds(0.1f);
        }
        //-----------------------------------------
        RoomData roomData = new RoomData();
        ChannelName = roomData.name;//"master_room";//GameManager.Instance.GameData.roomData.name;
        agoraVideoType = AgoraVideoType.Video2D;

        //AgoraButtons.DisableVideoButtons();

        /*AudioZone.OnUserEnteredZone += UserEnteredZone;
        AudioZone.OnUserExitedZone += UserExitedZone;*/

        /* ShareInteractionZone.OnClickedOnShare += ClickedOnShareButton;
         ShareInteractionZone.OnStopShare += StopShare;
         NodeManager.Instance.RoomController.onNetworkEntityAction += RemoteStartShareResponse;
         NodeManager.Instance.RoomController.onNetworkEntityAction += RemoteStopShareResponse;
         NodeManager.Instance.RoomController.onNetworkEntityAction += RemoteUpdateUserSharing;
         NodeManager.Instance.RoomController.onNetworkEntityAction += RemoteCameraStartupReponse;
         NodeManager.Instance.RoomController.onNetworkEntityAction += RemoteCameraRuntimeActivationResponse;
         NodeManager.Instance.RoomController.onNetworkEntityAction += ReceiveOtherPlayerAudioZones;*/

        StartCoroutine(InitRtcEngine());
    }

    public void UpdateCalls(CharacterAction c)
    {
        /*RemoteStartShareResponse(c);
        RemoteStopShareResponse(c);
        RemoteUpdateUserSharing(c);
        RemoteCameraStartupReponse(c);
        RemoteCameraRuntimeActivationResponse(c);*/
    }

    private void VolumeIndication(AudioVolumeInfo[] speakers, int speakerNumber, int totalVolume)
    {
        if (totalVolume > 0)
        {
            LocalCharacter.audioSpeakingOutline.SetActive(LocalCharacter.IsAudioOn);
            /* if (LocalPlayerWebcamVideo)
             {
                 if (LocalCharacter.IsVideoOn)
                 {
                     LocalPlayerWebcamVideo.VideoSpeakingOutline.SetActive(LocalCharacter.IsVideoOn);
                 }
                 else
                 {
                     LocalPlayerWebcamVideo.VideoSpeakingOutline.SetActive(LocalCharacter.IsAudioOn);
                 }
             }
             else
             {
                 LocalCharacter.WebcamVideo3D.AudioSpeakingOutline.SetActive(LocalCharacter.IsAudioOn);
             }*/
        }
        else
        {
            LocalCharacter.audioSpeakingOutline.SetActive(false);
            /* if (LocalPlayerWebcamVideo)
                 LocalPlayerWebcamVideo.VideoSpeakingOutline.SetActive(false);

             if (!LocalCharacter.IsVideoOn)
             {
                 LocalCharacter.WebcamVideo3D.AudioSpeakingOutline.SetActive(false);
             }*/
        }
    }

    private void OnDestroy()
    {
        AudioZone.OnUserEnteredZone -= UserEnteredZone;
        AudioZone.OnUserExitedZone -= UserExitedZone;

       /* ShareInteractionZone.OnClickedOnShare -= ClickedOnShareButton;
        ShareInteractionZone.OnStopShare -= StopShare;

        if (NodeManager.Instance != null)
        {
            NodeManager.Instance.RoomController.onNetworkEntityAction -= RemoteStartShareResponse;
            NodeManager.Instance.RoomController.onNetworkEntityAction -= RemoteStopShareResponse;
            NodeManager.Instance.RoomController.onNetworkEntityAction -= RemoteUpdateUserSharing;
            NodeManager.Instance.RoomController.onNetworkEntityAction -= RemoteCameraStartupReponse;
            NodeManager.Instance.RoomController.onNetworkEntityAction -= RemoteCameraRuntimeActivationResponse;
            NodeManager.Instance.RoomController.onNetworkEntityAction -= ReceiveOtherPlayerAudioZones;
        }*/

#if UNITY_WEBGL && !UNITY_EDITOR
            AgoraWebGLEventHandler.OnReceivedDevicesList -= ReceivedDevicesList;
            AgoraWebGLEventHandler.OnFailedToGetDevices -= FailedToGetDevicesList;
#endif
    }


    /// <summary>
    /// Requesting microphone and camera permissions
    /// </summary>
    private void Update()
    {
        if (agoraVideoType != AgoraVideoType.None)
        {
            PermissionHelper.RequestMicrophontPermission();
            PermissionHelper.RequestCameraPermission();

        }
    }

    /// <summary>
    /// Initializes the RtcEngine
    /// </summary>
    private IEnumerator InitRtcEngine()
    {
        AppId = "f3b46a29c72c48d7896a42cb35525ba3";//"bcea3fb6dfe04f2b9d92a321a33f4b98";//"5531ac0578164d6c84fad57e34efeeca";

        yield return new WaitUntil(() => !string.IsNullOrEmpty(AppId));

        Debug.Log("AGORA: AppID Defined");

        //yield return TokenAuth();

        mRtcEngine = IRtcEngine.GetEngine(AppId);
        mRtcEngine.EnableAudio();
        mRtcEngine.EnableVideo();
        mRtcEngine.EnableVideoObserver();
        mRtcEngine.SetChannelProfile(CHANNEL_PROFILE.CHANNEL_PROFILE_COMMUNICATION/*CHANNEL_PROFILE.CHANNEL_PROFILE_LIVE_BROADCASTING*/);
        mRtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
        mRtcEngine.OnJoinChannelSuccess += OnLocalJoinChannelSuccessHandler;
        mRtcEngine.OnUserJoined += OnRemoteUserJoinedHandler;
        /*mRtcEngine.OnUserOffline += OnUserLeaveChannelHandler;
        mRtcEngine.OnLeaveChannel += OnLeaveChannelHandler;
        mRtcEngine.OnWarning += OnSDKWarningHandler;
        mRtcEngine.OnError += OnSDKErrorHandler;
        mRtcEngine.OnClientRoleChanged += OnClientRoleChangedHandler;
        mRtcEngine.OnConnectionLost += OnConnectionLostHandler;
        //mRtcEngine.OnCameraChanged += OnCameraChangedHandler;
        mRtcEngine.OnMicrophoneChanged += OnMicrophoneChangedHandler;
        mRtcEngine.OnPlaybackChanged += OnPlaybackChangedHandler;
        mRtcEngine.OnUserMutedAudio += OnUserMutedAudio;
        mRtcEngine.OnUserMuteVideo += OnUserMutedVideo;
        mRtcEngine.OnScreenShareStarted += OnScreenShareStarted;
        mRtcEngine.OnScreenShareStopped += OnScreenShareStopped;
        mRtcEngine.OnScreenShareCanceled += OnScreenShareCanceled;
        mRtcEngine.OnVolumeIndication += VolumeIndication;*/
        //mRtcEngine.OnRemoteAudioStateChanged += OnRemoteAudioStateChanged;
        //mRtcEngine.OnRemoteVideoStateChanged += OnRemoteVideoStateChanged;

        mRtcEngine.EnableAudioVolumeIndication(500, 8, report_vad: true);

        VideoEncoderConfiguration config = new VideoEncoderConfiguration
        {
            orientationMode = ORIENTATION_MODE.ORIENTATION_MODE_FIXED_LANDSCAPE,
            degradationPreference = DEGRADATION_PREFERENCE.MAINTAIN_FRAMERATE
        };

        mRtcEngine.SetVideoEncoderConfiguration(config);

        //Wait for SDK initialization to check user devices
        Invoke(nameof(JoinAgoraChannelWithDevices), 1f);
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="character"></param>
    public void AddCharacter(AgoraCharacterData characterData)
    {
        characters.Add(characterData);
    }

    //Check if I am sharing, if so needs to message other players about it
    private void NotifyMySharingToOtherUsers()
    {
        if (string.IsNullOrEmpty(currentSharingUID) || currentSharingUID != myUID)
            return;

        if (LocalCharacter)
        {
            /*var playerInput = LocalCharacter.GetComponent<PlayerInput>();
            if (playerInput)
                playerInput.OnRequestUpdateUserSharing(this.myUID);*/
        }
        Debug.Log("Notify new users that i am sharing");
    }


    private AgoraCharacterData GetCharacterDataByUID(string uid)
    {
        AgoraCharacterData character = null;
        var lcharacters = characters.Where(x => x.characterInstance.UID.ToString() == uid).ToList();
        if (lcharacters.Count > 0)
            character = lcharacters[0];
        return character;
    }

    /// <summary>
    /// Verify available devices and joins agora channel
    /// </summary>
    private void JoinAgoraChannelWithDevices()
    {
        Debug.Log("AGORA: tentando JoinAgoraChannelWithDevices");
        DevicesChecker();
        uint uid = LocalCharacter.UID;

        switch (HasVideoDevice)
        {
            case true when HasAudioRecording:
                {
                    Debug.Log("AGORA" + "JoinChannelByKey | Reason: hasAudioRecording && hasVideoDevice");
                    //mRtcEngine.JoinChannelByKey(Token, ChannelName, "", uid);
                    mRtcEngine.JoinChannel(ChannelName, "", uid);
                    break;
                }

            case true when !HasAudioRecording:
                {
                    Debug.Log("AGORA" + "JoinChannelByKey | Reason: hasAudioRecording = false");
                    mRtcEngine.EnableLocalAudio(false);
                    mRtcEngine.MuteLocalAudioStream(true);
                    //mRtcEngine.JoinChannelByKey(Token, ChannelName, "", uid);
                    mRtcEngine.JoinChannel(ChannelName, "", uid);
                    break;
                }

            case false when HasAudioRecording:
                {
                    Debug.Log("AGORA" + "JoinChannel | Reason: hasVideoDevice = false");
                    AudioVideoState.subAudio = true;
                    AudioVideoState.subVideo = true;
                    AudioVideoState.pubAudio = true;
                    AudioVideoState.pubVideo = false;

                    var channelMediaOptions = new ChannelMediaOptions(AudioVideoState.subAudio, AudioVideoState.subVideo,
                        AudioVideoState.pubAudio, AudioVideoState.pubVideo);

                    //mRtcEngine.JoinChannel(Token, ChannelName, info: "", uid, channelMediaOptions);
                    mRtcEngine.JoinChannel(null, ChannelName, info: "", uid, channelMediaOptions);
                    break;
                }

            case false when !HasAudioRecording:
                {
                    Debug.Log("AGORA" + "JoinChannelByKey | Reason: !hasAudioRecording && !hasVideoDevice");
                    mRtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_AUDIENCE);
                    //mRtcEngine.JoinChannelByKey(Token, ChannelName, "", uid);
                    mRtcEngine.JoinChannel(ChannelName, info: "", uid);
                    break;
                }
        }
    }

    private IEnumerator TokenAuth()
    {
        var agoraPostToken = new AgoraPostToken()
        {
            channel = ChannelName,
            uid = LocalCharacter.UID.ToString(),
            expireTs = (3600 * 24 * 2).ToString()
        };

        yield return HttpRequest.Post<ReturnAgoraToken>(agoraPostToken, (data, code, messageError) =>
        {
            if (string.IsNullOrEmpty(messageError))
            {
                Token = data.rtcToken;
                Debug.Log("AGORA" + $"Received Agora Token: {Token}");
            }
            else
            {
                Debug.LogError($"Unable to get agora token with channelName={ChannelName}, uid={agoraPostToken.uid}, expireTS={agoraPostToken.expireTs}");
            }
        });

        /*string certificate = "83a020305efd403490c42814514773c3";
        Token = "83a020305efd403490c42814514773c3";//"0065531ac0578164d6c84fad57e34efeecaIAAHAr4wJ12V1Rtfgsa1dwgx+XnCY3MpipsDTyXrjW336JXCMsIUergdIgBWFwAA57yfZAQAAQBnDqFkAwBnDqFkAgBnDqFkBABnDqFk";
        */
        yield return null;
    }

    /// <summary>
    /// 
    /// </summary>
    private void DevicesChecker()
    {
        VideoDeviceManager = (VideoDeviceManager)mRtcEngine.GetVideoDeviceManager();
        VideoDeviceManager.CreateAVideoDeviceManager();

        AudioRecordingDeviceManager = (AudioRecordingDeviceManager)mRtcEngine.GetAudioRecordingDeviceManager();
        AudioRecordingDeviceManager.CreateAAudioRecordingDeviceManager();

        AudioPlaybackDeviceManager = (AudioPlaybackDeviceManager)mRtcEngine.GetAudioPlaybackDeviceManager();
        AudioPlaybackDeviceManager.CreateAAudioPlaybackDeviceManager();

        hasAudioRecording = AudioRecordingDeviceManager.GetAudioRecordingDeviceCount() > 0;
        hasVideoDevice = VideoDeviceManager.GetVideoDeviceCount() > 0;

      /*  recordingDropdown.gameObject.SetActive(HasAudioRecording);
        videoDropdown.gameObject.SetActive(HasVideoDevice);*/

       /* var button2D = ViewInteraction.Instance.Buttons.FirstOrDefault(x => x.UIElement.UIActionType == UIActionType.Cam2D)?.Button;
        var button3D = ViewInteraction.Instance.Buttons.FirstOrDefault(x => x.UIElement.UIActionType == UIActionType.Cam3D)?.Button;

        var buttonCamera = GameManager.Instance.GameData.sessionData.agoraio.camera_mode switch
        {
            "2d" => button2D,
            "3d" => button3D,
            _ => button3D
        };*/

       // if (buttonCamera) buttonCamera.gameObject.SetActive(HasVideoDevice);

        //AgoraButtons.EnableToggleAudio(hasAudioRecording);
        //AgoraButtons.EnableToggleCamera(HasVideoDevice);

        Debug.Log("AGORA" + $"hasAudioRecording: {HasAudioRecording}");
        Debug.Log("AGORA" + $"hasVideoDevice: {HasVideoDevice}");
    }

    #region Devices

    /// <summary>
    /// 
    /// </summary>
    public void GetDevices()
    {
        //GetAudioPlaybackDevice();
        GetVideoDeviceManager();
        //GetAudioRecordingDevice();
    }

    /// <summary>
    /// 
    /// </summary>
    private void GetAudioPlaybackDevice()
    {
        var audioPlaybackDeviceName = "";
        var audioPlaybackDeviceId = "";

        // AudioPlaybackDeviceManager = (AudioPlaybackDeviceManager) mRtcEngine.GetAudioPlaybackDeviceManager();
        // AudioPlaybackDeviceManager.CreateAAudioPlaybackDeviceManager();

        var count = AudioPlaybackDeviceManager.GetAudioPlaybackDeviceCount();
        Debug.Log("AGORA" + $"AudioPlaybackDeviceManager count: {count}");
        playbackDropdown.ClearOptions();
        audioPlaybackDevices.Clear();
        audioPlaybackDeviceNames.Clear();

        for (var i = 0; i < count; i++)
        {
            AudioPlaybackDeviceManager.GetAudioPlaybackDevice(i, ref audioPlaybackDeviceName, ref audioPlaybackDeviceId);

            if (!audioPlaybackDevices.ContainsKey(i))
            {
                audioPlaybackDevices.Add(i, audioPlaybackDeviceId);
                audioPlaybackDeviceNames.Add(i, audioPlaybackDeviceName);
            }
        }

        playbackDropdown.AddOptions(audioPlaybackDeviceNames.Values.ToList());
        playbackDropdown.value = AudioPlaybackDeviceManager.GetCurrentPlaybackDevice(ref audioPlaybackDeviceName);
    }

    /// <summary>
    /// 
    /// </summary>
    private void GetVideoDeviceManager()
    {
        var videoDeviceName = "";
        var videoDeviceId = "";

        // If you want to getVideoDeviceManager, you need to call startPreview() first;
        //mRtcEngine.StartPreview();
        //VideoDeviceManager = (VideoDeviceManager) mRtcEngine.GetVideoDeviceManager();
        //VideoDeviceManager.CreateAVideoDeviceManager();

        var count = VideoDeviceManager.GetVideoDeviceCount();
        Debug.Log("AGORA" + $"VideoDevice count: {count}");

        videoDropdown.ClearOptions();
        videoDeviceManager.Clear();
        videoDeviceManagerNames.Clear();

        for (var i = 0; i < count; i++)
        {
            VideoDeviceManager.GetVideoDevice(i, ref videoDeviceName, ref videoDeviceId);

            if (!videoDeviceManager.ContainsKey(i))
            {
                videoDeviceManager.Add(i, videoDeviceId);
                videoDeviceManagerNames.Add(i, videoDeviceName);
            }
        }

        videoDropdown.AddOptions(videoDeviceManagerNames.Values.ToList());
        videoDropdown.value = VideoDeviceManager.GetCurrentVideoDevice(ref videoDeviceName);
    }

    /// <summary>
    /// 
    /// </summary>
    private void GetAudioRecordingDevice()
    {
        var audioRecordingDeviceName = "";
        var audioRecordingDeviceId = "";

        //AudioRecordingDeviceManager = (AudioRecordingDeviceManager) mRtcEngine.GetAudioRecordingDeviceManager();
        //AudioRecordingDeviceManager.CreateAAudioRecordingDeviceManager();

        var count = AudioRecordingDeviceManager.GetAudioRecordingDeviceCount();
        Debug.Log("AGORA" + $"AudioRecordingDevice count: {count}");

        recordingDropdown.ClearOptions();
        audioRecordingDevices.Clear();
        audioRecordingDeviceNames.Clear();

        for (var i = 0; i < count; i++)
        {
            AudioRecordingDeviceManager.GetAudioRecordingDevice(i, ref audioRecordingDeviceName, ref audioRecordingDeviceId);

            if (!audioRecordingDevices.ContainsKey(i))
            {
                audioRecordingDevices.Add(i, audioRecordingDeviceId);
                audioRecordingDeviceNames.Add(i, audioRecordingDeviceName);
            }
        }

        recordingDropdown.AddOptions(audioRecordingDeviceNames.Values.ToList());
        recordingDropdown.value = AudioRecordingDeviceManager.GetCurrentRecordingDevice(ref audioRecordingDeviceName);
    }

    /// <summary>
    /// Set the current devices that are been used
    /// </summary>
    public void SetCurrentDevice()
    {
        /*playbackDeviceIndex = playbackDropdown.value;
        AudioPlaybackDeviceManager.SetAudioPlaybackDevice(audioPlaybackDevices[playbackDeviceIndex]);

        if (HasAudioRecording)
        {
            recordingDeviceIndex = recordingDropdown.value;
            AudioRecordingDeviceManager.SetAudioRecordingDevice(audioRecordingDevices[recordingDeviceIndex]);
        }*/

        if (HasVideoDevice)
        {
            videoDeviceIndex = videoDropdown.value;
            VideoDeviceManager.SetVideoDevice(videoDeviceManager[videoDeviceIndex]);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void ReleaseDeviceManager()
    {
        AudioPlaybackDeviceManager.ReleaseAAudioPlaybackDeviceManager();
        AudioRecordingDeviceManager.ReleaseAAudioRecordingDeviceManager();
        VideoDeviceManager.ReleaseAVideoDeviceManager();
    }

    /// <summary>
    /// 
    /// </summary>
    public void SetAndReleaseRecordingDevice()
    {
        AudioRecordingDeviceManager.SetAudioRecordingDevice(audioRecordingDevices[recordingDeviceIndex]);
        AudioRecordingDeviceManager.ReleaseAAudioRecordingDeviceManager();
    }

    /// <summary>
    /// 
    /// </summary>
    public void SetAndReleasePlaybackDevice()
    {
        AudioPlaybackDeviceManager.SetAudioPlaybackDevice(audioPlaybackDevices[playbackDeviceIndex]);
        AudioPlaybackDeviceManager.ReleaseAAudioPlaybackDeviceManager();
    }

    /// <summary>
    /// 
    /// </summary>
    public void SetAndReleaseVideoDevice()
    {
        VideoDeviceManager.SetVideoDevice(videoDeviceManager[videoDeviceIndex]);
        VideoDeviceManager.ReleaseAVideoDeviceManager();
    }

    /// <summary>
    /// 
    /// </summary>
    public void OnVideoTypeButtonClicked(AgoraVideoType agoraVideoType)
    {
        if (!HasVideoDevice)
        {
           /* UIPopup lPopup = UIPopup.GetPopup("Popup - Agora Message");
            PopupAgoraMessage lPopupAgoraMessage = lPopup.GetComponent<PopupAgoraMessage>();
            lPopup.Show();
            lPopupAgoraMessage.ShowMessage("agora_io_activate_webcam_message");*/
            return;
        }

        if (string.IsNullOrEmpty(this.myUID))
            return;

        // AgoraButtons.SetCameraButtonActive(true);
        // AgoraButtons.SetCameraTypeButtonActive(false);
       /* var toggleCamera = ViewInteraction.Instance.Toggles.FirstOrDefault(x => x.UIElement.UIActionType == UIActionType.Cam);
        if (toggleCamera) toggleCamera.gameObject.SetActive(true);
        this.agoraVideoType = agoraVideoType;

        if (GameManager.Instance.GameData.sessionData.agoraio.camera_mode == "2D")
        {
            this.agoraVideoType = AgoraVideoType.Video2D;
        }
        else if (GameManager.Instance.GameData.sessionData.agoraio.camera_mode == "3D")
        {
            this.agoraVideoType = AgoraVideoType.Video3D;
        }

        switch (this.agoraVideoType)
        {
            case AgoraVideoType.Video2D:
                //LocalCharacter.WebcamVideo = SetWebcamVideo2D();
                break;
            case AgoraVideoType.Video3D:
                LocalCharacter.WebcamVideo = LocalCharacter.WebcamVideo3D;
                break;
        }*/

        //LocalPlayerWebcamVideo = LocalCharacter.WebcamVideo;
        //LocalCharacter.IsAudioOn = hasAudioRecording;
        LocalCharacter.IsVideoOn = HasVideoDevice;


        AgoraCharacterData myCharacterData = GetCharacterDataByUID(this.myUID);
        myCharacterData.hasWebcamInitialized = true;
        myCharacterData.agoraVideoType = agoraVideoType;

        MakeVideoView(LocalCharacter.UID, LocalCharacter.webcamVideo, myCharacterData);
        /*var playerInput = LocalCharacter.GetComponent<PlayerInput>();
        if (playerInput)
        {
            playerInput.NotifyUserWebcamOnStartup(this.myUID, agoraVideoType);
            //playerInput.NotifyUserWebcamRuntimeActivation(this.myUID);
        }*/
    }

    #endregion

    #region mRtcEngine Callbacks

    /// <summary>
    /// Happens only when the local client enters the channel
    /// </summary>
    /// <param name="channelName"></param>
    /// <param name="uid"></param>
    /// <param name="elapsed"></param>
    private void OnLocalJoinChannelSuccessHandler(string channelName, uint uid, int elapsed)
    {
        Debug.Log("AGORA" + $"OnJoinChannelSuccess channelName: {channelName}, uid: {uid}, elapsed: {elapsed}");
        this.myUID = uid.ToString();
        myUIDUint = uid;
        //ShareByUID(LocalCharacter.webcamVideo.GetVideoSurface().transform, uid.ToString(), "webcam");
        OnLocalUserJoinedAgoraChannel?.Invoke(this.myUID);
    }

    /// <summary>
    /// Remote player enters an Agora channel
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="elapsed"></param>
    private void OnRemoteUserJoinedHandler(uint uid, int elapsed)
    {
        Debug.Log($"*** OnUserJoinedHandler channelId: {ChannelName} uid: ${uid} elapsed: ${elapsed}");

        StartCoroutine(RemoteUserJoinedCoroutine(uid, elapsed));
    }

    private IEnumerator RemoteUserJoinedCoroutine(uint uid, int elapsed)
    {
        //Notifies to new user if I have my camera active
        AgoraCharacterData characterData = null;

        while (characterData == null)
        {
            characterData = GetCharacterDataByUID(uid.ToString());
            yield return null;
        }

        NotifyMySharingToOtherUsers();
        //characterData.characterInstance.webcamVideo.GetVideoSurface().SetForUser(uid);
        //ShareByUID(characterData.characterInstance.webcamVideo.GetVideoSurface().transform, uid.ToString(), "webcam");

        //bool cameraInitialized = characterData.hasWebcamInitialized;
        if (LocalCharacter != null)
        {
            //LocalCharacter.webcamVideo.GetVideoSurface().SetForUser(uid);

            //If I have audio zones, must check if remote users is already in one, so I must mute him or not
            if (LocalCharacter.AudioZones.Count > 0)
             {
                 //If I dont have audio zones and the remote user has it, must mute him
                 if (characterData.characterInstance.AudioZones.Count == 0)
                 {
                     Debug.Log($"Muted recently joined user {uid}, it has no zones");
                     SetRemoteMuteCharacter(characterData.characterInstance, true);
                 }
                 else
                 {
                     //Muting character if it is not on local player zones
                     for (int i = 0; i < LocalCharacter.AudioZones.Count; i++)
                     {
                         //If it is on my zone must unmute it
                         if (LocalCharacter.AudioZones[i].GetCharactersInZone().Contains(characterData.characterInstance))
                         {
                             Debug.Log($"Unmuted recently joined user {uid}, it is on my zone");
                             SetRemoteMuteCharacter(characterData.characterInstance, false);
                             break;
                         }
                         //Verified until last zone index and character is not in my zone
                         else if (i == LocalCharacter.AudioZones.Count - 1)
                         {
                             Debug.Log($"Muted recently joined user {uid}, it not in my zone");
                             SetRemoteMuteCharacter(characterData.characterInstance, true);
                         }
                     }
                 }
             }
            yield return null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    //private WebcamVideo SetWebcamVideo2D()
    //{
    //    var webcam = Instantiate(m_2DWebcamPrefab, m_WebcamsHolder.transform);
    //    webcam.gameObject.SetActive(true);
    //    return webcam;
    //}

    /// <summary>
    /// 
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="webcamVideo"></param>
    /// <param name="character"></param>
    private void MakeVideoView(uint uid, WebcamVideo webcamVideo, AgoraCharacterData characterData)
    {
        var videoSurface = SetupVideoSurface(webcamVideo);
        var character = characterData.characterInstance;
        var agoraVideoType = characterData.agoraVideoType;
        if (!ReferenceEquals(videoSurface, null))
        {
            // configure videoSurface
            videoSurface.SetForUser(uid);
            videoSurface.SetEnable(true);
            videoSurface.SetVideoSurfaceType(AgoraVideoSurfaceType.RawImage);
        }

        webcamVideo.NoVideoObj.SetActive(!character.IsVideoOn);
        webcamVideo.NoAudioObj.SetActive(!character.IsAudioOn);
       /* if (webcamVideo.NoAudioSingleObj)
            webcamVideo.NoAudioSingleObj.gameObject.SetActive(false);*/

        //externalWebcams.Add(playerWebcamVideo);

        if (agoraVideoType == AgoraVideoType.Video3D)
            webcamVideo.TargetToLookAt = Camera.main.transform;

        //deactivates webcam if user is sharing
        if (currentSharingUID == myUID)
        {
            SetWebCamActive(uid, false);
        }

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="webcamVideo"></param>
    /// <returns></returns>
    private VideoSurface SetupVideoSurface(WebcamVideo webcamVideo)
    {
        var rawImage = webcamVideo.VideoSurfaceObj.AddComponent<RawImage>();
        rawImage.transform.localRotation = Quaternion.Euler(0, 180, 180);
        var videoSurface = webcamVideo.VideoSurfaceObj.AddComponent<VideoSurface>();
        webcamVideo.MaskObj.GetComponent<Image>().enabled = true;
        return videoSurface;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="state"></param>
    /// <param name="device"></param>
   /*private void OnCameraChangedHandler(string state, string device)
    {
        Debug.Log("AGORA" + $"OnCameraChanged state: {state} device: {device}");

        if (HasVideoDevice || hasVideoOnJS)
            GetVideoDeviceManager();
    }*/

    /// <summary>
    /// 
    /// </summary>
    /// <param name="state"></param>
    /// <param name="device"></param>
    private void OnMicrophoneChangedHandler(string state, string device)
    {
        Debug.Log("AGORA" + $"OnMicrophoneChanged state: {state} device: {device}");

        if (HasAudioRecording)
            GetAudioRecordingDevice();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="state"></param>
    /// <param name="device"></param>
    private void OnPlaybackChangedHandler(string state, string device)
    {
        Debug.Log("AGORA" + $"OnPlaybackChanged state: {state} device: {device}");
        GetAudioPlaybackDevice();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="muted"></param>
    private void OnUserMutedAudio(uint uid, bool muted)
    {
        Debug.Log("AGORA"+ $"OnUserMutedAudio: user {uid} muted audio:{muted}");

        foreach (var item in characters.Where(c => c.characterInstance.UID == uid))
        {
            item.characterInstance.IsAudioOn = !muted;

            if (item.characterInstance.webcamVideo != null)
            {
                Debug.Log("AGORA" + $"Found user (uid: {uid}), setting audio enabled to {!muted}");
                item.characterInstance.webcamVideo.NoAudioObj.SetActive(muted);
                break;
            }
            /*else
            {
                item.characterInstance.WebcamVideo3D.NoAudioSingleObj.SetActive(muted);
            }*/

            Debug.Log("AGORA" + $"Found user (uid: {uid}). WebcamVideo doesnt exists! Setting Character's IsAudioOn to {!muted}");
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="muted"></param>
    private void OnUserMutedVideo(uint uid, bool muted)
    {
        Debug.Log("AGORA" + $"OnUserMutedVideo: user {uid} muted video:{muted}");

        foreach (var item in characters.Where(c => c.characterInstance.UID == uid))
        {
            Debug.LogError("Enter Foreach OnUserMutedVideo");

            item.characterInstance.IsVideoOn = !muted;
            if (item.characterInstance.webcamVideo != null)
            {
                Debug.Log("AGORA" + $"Found user (uid: {uid}), setting camera enabled to {!muted}");
                //item.characterInstance.webcamVideo.NoVideoObj.SetActive(muted);
                item.characterInstance.webcamVideo.VideoSurfaceObj.GetComponent<RawImage>().enabled = !muted;
                break;
            }
            else
            {
                //item.characterInstance.WebcamVideo3D.NoAudioSingleObj.SetActive(muted);
            }

            Debug.Log("AGORA" + $"Found user (uid: {uid}). WebcamVideo doesnt exists! Setting Character's IsVideoOn to {!muted}");
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="stats"></param>
    private void OnLeaveChannelHandler(RtcStats stats)
    {
        Debug.Log("AGORA" + "OnLeaveChannelSuccess");

        /*foreach (var character in characters)
        {
            Destroy(character.characterInstance.WebcamVideo);
        }*/

        characters.Clear();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="uid"></param>
    public void CharacterLeaveChannel(uint uid)
    {
        Debug.Log("AGORA" + $"CharacterLeaveChannel for uid: {uid}");
        if (uid.ToString() == this.currentSharingUID)
        {
            Debug.Log("AGORA" + $"User sharing left the room {this.currentSharingUID}");

            var shareInteraction = FindObjectOfType<ShareInteractionZone>();
            if (shareInteraction)
            {
                Debug.Log("AGORA" + "Found a share interaction zone");
                //Destroys all childs from videoHolder
                foreach (Transform item in shareInteraction.GetVideoHolder())
                {
                    var videoSurface = item.GetComponent<VideoSurface>();
                    if (videoSurface)
                    {
                        Debug.Log("AGORA" + "Found video surface to destroy");
                        //videoSurface.RemoveUserInfo(uid);
                    }
                }
            }
            this.currentSharingUID = "";
            DestroyCurrentShareObject();
        }

        var characterData = GetCharacterDataByUID(uid.ToString());
        if (characterData != null)
        {
            characters.Remove(characterData);
            /*if (characterData.characterInstance && characterData.characterInstance.WebcamVideo)
                Destroy(characterData.characterInstance.WebcamVideo.gameObject);*/
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="reason"></param>
    private void OnUserLeaveChannelHandler(uint uid, USER_OFFLINE_REASON reason)
    {
        Debug.Log("AGORA" + $"OnUserLeaveChannelHandler for uid: {uid}, reason: {reason}");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="warn"></param>
    /// <param name="msg"></param>
    private void OnSDKWarningHandler(int warn, string msg)
    {
        Debug.LogWarningFormat("AGORA" + $"OnSDKWarning warn: {warn}, msg: {msg}", IRtcEngine.GetErrorDescription(warn));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="error"></param>
    /// <param name="msg"></param>
    private void OnSDKErrorHandler(int error, string msg)
    {
        Debug.LogError("AGORA" + $"OnSDKError error: {error}, msg: {msg}");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="oldRole"></param>
    /// <param name="newRole"></param>
    private void OnClientRoleChangedHandler(CLIENT_ROLE_TYPE oldRole, CLIENT_ROLE_TYPE newRole)
    {
        Debug.Log("AGORA" + "OnClientRoleChanged: " + oldRole + " -> " + newRole);
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnConnectionLostHandler()
    {
        Debug.Log("AGORA" + "OnConnectionLost");
    }

    #endregion

    /// <summary>
    /// 
    /// </summary>
    public void ToggleCamera()
    {
        Debug.Log("AGORA" + "Called ToggleCamera: " + !LocalCharacter.IsVideoOn);
        LocalCharacter.IsVideoOn = !LocalCharacter.IsVideoOn;
        LocalCharacter.webcamVideo.VideoSurfaceObj.GetComponent<RawImage>().enabled = LocalCharacter.IsVideoOn;
        mRtcEngine.MuteLocalVideoStream(!LocalCharacter.IsVideoOn);
    }

    #region Share on client side
    public void ClickedOnShareButton(Transform videoHolder)
    {
        this.videoHolder = videoHolder;
    }

    /// <summary>
    /// Activates an already existent webcam by its UID
    /// </summary>
    private void SetWebCamActive(uint uid, bool active)
    {
        uint targetUID = uid;
        if (uid == 0)
            targetUID = uint.Parse(this.myUID);
        var agoraVideoType = GetCharacterDataByUID(targetUID.ToString()).agoraVideoType;

        WebcamVideo webcam = null;
        if (agoraVideoType == AgoraVideoType.Video2D)
        {
            webcam = Get2DWebcamByUID(uid);
        }
        else if (agoraVideoType == AgoraVideoType.Video3D)
        {
            webcam = Get3DWebCamByUID(uid);
        }

        if (webcam)
        {
            webcam.gameObject.SetActive(active);
            VideoSurface videoSurface = webcam.GetVideoSurface();
            if (videoSurface)
            {
                videoSurface.SetEnable(active);
            }
        }
    }

    /// <summary>
    /// Called when interacting with UI button. Starts sharing and request to start sharing on other clients
    /// </summary>
    public void StartShare(string shareType, bool val)
    {
        videoHolder.gameObject.SetActive(val);
        ShareByUID(videoHolder, "0", shareType);
        Debug.Log("AGORA" + $"Local Start Share {this.myUID}");

        //If it is a "screen" shareType, remote share will be requested at OnScreenShareStarted(channelName, id, elapsed), which confirms that screen share really started
        if (shareType == "webcam")
        {
            //Activates webcam if there is already a sharing occuring
            if (!string.IsNullOrEmpty(this.currentSharingUID))
            {
                SetWebCamActive(uint.Parse(this.currentSharingUID), true);
            }
            SetWebCamActive(0, false);
            OnLocalUserStartedSharing?.Invoke(videoHolder, this.myUID);
            bool isOverridingSharing = !string.IsNullOrEmpty(this.currentSharingUID);
            RequestRemoteStartShare(this.myUID, shareType, isOverridingSharing);
            this.currentSharingType = shareType;
            this.currentSharingUID = this.myUID;
        }
    }

    /// <summary>
    /// Starts a sharing by its UID
    /// </summary>
    public void ShareByUID(Transform videoParent, string uid, string shareType)
    {
        screenShareObject = videoParent.gameObject;
        screenShareObject.transform.SetParent(videoParent);
        VideoSurface videoSurface = videoParent.gameObject.GetComponent<VideoSurface>();

        videoSurface.SetForUser(uint.Parse(uid));
        videoSurface.SetEnable(true);
        videoSurface.SetVideoSurfaceType(AgoraVideoSurfaceType.RawImage);

        if (shareType == "screen")
        {
            ScreenCaptureParameters screenCaptureParameters = new ScreenCaptureParameters
            {
                frameRate = 15,
                bitrate = 500,
                captureMouseCursor = true,
                windowFocus = true
            };

            // Specify the full screen area or desired area
            Rectangle captureRect = new Rectangle
            {
                x = 0,
                y = 0,
                width = Screen.width,
                height = Screen.height
            };

            mRtcEngine.StartScreenCaptureByDisplayId(0, captureRect, screenCaptureParameters);
            //keeps deactivated until user confirms screen share on browser
            //videoSurface.gameObject.SetActive(false);
            //Only starts sharing if it is my uid
            if (uid == "0")
                mRtcEngine.StartScreenCaptureForWeb(false);

        }
        else if (shareType == "webcam")
        {
            videoSurface.gameObject.SetActive(true);
          //  rect.localRotation = Quaternion.Euler(180, 180, 0);
            //screenShareObject.transform.localScale = new Vector3(1, -1, 1);
        }
       // RuntimeUIManager.Instance.rawFullScreenShare.texture = raw.texture;
    }

    private void AddOnClickFullScreenShareButton()
    {
        /*RuntimeUIManager.Instance.fullScreenShare.SetActive(true);
        if (RuntimeUIManager.Instance.m_videoSurface)
        {
            RuntimeUIManager.Instance.rawFullScreenShare.texture = RuntimeUIManager.Instance.m_videoSurface.GetComponent<RawImage>().texture;
            float padding = 0;

            var parent = RuntimeUIManager.Instance.fullScreenShare.transform.parent.GetComponent<RectTransform>();
            var imageTransform = RuntimeUIManager.Instance.rawFullScreenShare.GetComponent<RectTransform>();
            padding = 1 - padding;
            float w = 0, h = 0;
            float ratio = RuntimeUIManager.Instance.rawFullScreenShare.texture.width / (float)RuntimeUIManager.Instance.rawFullScreenShare.texture.height;
            var bounds = new Rect(0, 0, parent.rect.width, parent.rect.height);
            if (Mathf.RoundToInt(imageTransform.eulerAngles.z) % 180 == 90)
            {
                //Invert the bounds if the image is rotated
                bounds.size = new Vector2(bounds.height, bounds.width);
            }
            //Size by height first
            h = bounds.height * padding;
            w = h * ratio;
            if (w > bounds.width * padding)
            { //If it doesn't fit, fallback to width;
                w = bounds.width * padding;
                h = w / ratio;
            }
            imageTransform.sizeDelta = new Vector2(w, h);
        }*/
    }

    /// <summary>
    /// Destroys sharing object and requests stop sharing on other clients
    /// </summary>
    private void StopShare()
    {
        Debug.Log("AGORA" + $"Local Stop Share {myUID}");

        //If I am not sharing do nothing
        if (this.currentSharingUID != this.myUID)
            return;

        mRtcEngine.StopScreenCapture();
        DestroyCurrentShareObject();

        //Activates my webcam, if I have one
        SetWebCamActive(0, true);
        this.currentSharingUID = "";
        this.currentSharingType = "";

        RequestRemoteStopShare(this.myUID);
    }

    /// <summary>
    /// Response to colyseus message to start sharing an object
    /// </summary>
    private void RequestRemoteStartShare(string uid, string shareType, bool isOverridingSharing)
    {
        //Sends colyseus message to room to notify new screen share
        /*if (LocalCharacter && LocalCharacter.TryGetComponent(out PlayerInput playerInput))
        {
            Debug.Log("AGORA" + "Requested screen share");
            playerInput.OnRequestedRemoteShareScreen(uid, shareType, isOverridingSharing);
        }*/
    }

    /// <summary>
    /// Response to colyseus message to stop sharing an object
    /// </summary>
    private void RequestRemoteStopShare(string uid)
    {
        //Sends colyseus message to room to notify to stop share
       /* if (LocalCharacter && LocalCharacter.TryGetComponent(out PlayerInput playerInput))
        {
            Debug.Log("AGORA" + "Requested stop share");
            playerInput.OnRequestedStopShare(this.myUID.ToString());
        }*/
    }

    /*private WebcamVideo GetAnyWebCamByUID(uint uid)
    {
        WebcamVideo webcam = null;

        webcam = Get2DWebcamByUID(uid);
        if (webcam == null)
            webcam = Get3DWebCamByUID(uid);

        return webcam;
    }*/

    private WebcamVideo Get2DWebcamByUID(uint uid)
    {
        // List<WebcamVideo> webcams = m_WebcamsHolder.GetComponentsInChildren<WebcamVideo>(true).ToList();
        // WebcamVideo webcamByUID = null;
        //
        // foreach (WebcamVideo item in webcams)
        // {
        //     VideoSurface videoSurface = item.GetVideoSurface();
        //     if (videoSurface && videoSurface.MUid == uid)
        //         webcamByUID = item;
        // }
        //
        //return webcamByUID;
        return null;
    }

    private WebcamVideo Get3DWebCamByUID(uint uid)
    {
        WebcamVideo webcam = null;
        if (uid.ToString() == "0")
        {
            webcam = LocalCharacter.webcamVideo;
        }
        else
        {
            foreach (AgoraCharacterData item in characters)
            {
                if (item.characterInstance.UID == uid && item.characterInstance.webcamVideo/*.WebcamVideo3D*/ != null)
                    webcam = item.characterInstance.webcamVideo;
            }
        }
        return webcam;
    }

    private void DestroyCurrentShareObject()
    {
        var shareInteraction = FindObjectOfType<ShareInteractionZone>();
        Debug.Log("AGORA" + $"Trying to destroy sharing object, shareInteraction={shareInteraction}");
        if (shareInteraction == null)
            return;

        //Destroys all childs from videoHolder
        foreach (Transform item in shareInteraction.GetVideoHolder())
        {
            var videoSurface = item.GetComponent<VideoSurface>();
            if (videoSurface)
            {
                Debug.Log("AGORA" + "Destroyed current sharing object");
                GameObject.Destroy(videoSurface.gameObject);
            }
        }
    }
    #endregion

    #region Screen share client callbacks
    /// <summary>
    /// Triggered when client confirms screen share on browser
    /// </summary>
    private void OnScreenShareStarted(string channelName, uint id, int elapsed)
    {
        SetWebCamActive(0, false);
        screenShareObject.gameObject.SetActive(true);

        //Activates webcam if there is already a sharing occuring
        if (!string.IsNullOrEmpty(this.currentSharingUID))
        {
            SetWebCamActive(uint.Parse(this.currentSharingUID), true);
        }

        Debug.Log("AGORA" + "Local User started sharing");
        OnLocalUserStartedSharing?.Invoke(videoHolder, this.myUID);
        bool isOverridingSharing = false;

        if (!string.IsNullOrEmpty(this.currentSharingUID))
        {
            isOverridingSharing = id.ToString() != this.currentSharingUID;
        }

        RequestRemoteStartShare(id.ToString(), "screen", isOverridingSharing);
        this.currentSharingUID = id.ToString();
        this.currentSharingType = "screen";
    }
    /// <summary>
    /// Triggered when client stopped screen share on browser
    /// </summary>
    private void OnScreenShareStopped(string channelName, uint id, int elapsed)
    {
        Debug.Log("AGORA" + "Local User stopped sharing");
        SetWebCamActive(0, true);

        OnLocalUserStoppedSharing?.Invoke(this.videoHolder, this.myUID);

        //If I stopped my sharing using the browser "Stop Sharing" button
        if (this.currentSharingUID == id.ToString())
        {
            RequestRemoteStopShare(this.myUID);
            this.currentSharingType = "";
            DestroyCurrentShareObject();
        }
    }
    /// <summary>
    /// Triggered when client canceled screen share on browser, by clicking on "cancel" on pop up
    /// </summary>
    private void OnScreenShareCanceled(string channelName, uint id, int elapsed)
    {
        Debug.Log("AGORA" + "Local User canceled sharing");
        //If I canceled my sharing, destroy it
        if (this.currentSharingUID == id.ToString())
        {
            SetWebCamActive(0, true);
            this.currentSharingType = "";
            DestroyCurrentShareObject();
        }
    }
    #endregion

    #region Remote share response
    /// <summary>
    /// </summary>
    public void RemoteStartShareResponse(CharacterAction action)
    {
        if (string.IsNullOrEmpty(this.myUID)) return;
        if (action.action == null) return;
        if (!action.action.Contains("share_start_request")) return;

        var splitted = action.action.Split('%');
        var newSharingUID = splitted[1];
        var shareType = splitted[2];
        var isOverridingSharing = splitted[3];

        //If it is my id DO NOT share again
        if (this.myUID == newSharingUID)
        {
            return;
        }
        else if (this.myUID == currentSharingUID && bool.Parse(isOverridingSharing))
        {
            if (currentSharingType == "screen")
            {
                mRtcEngine.StopScreenCapture();
            }
            //activates my camera again if someone shared while I was sharing
            SetWebCamActive(0, true);
        }

        //Updates current uid sharing
        this.currentSharingUID = newSharingUID;
        this.currentSharingType = shareType;

        Debug.Log("AGORA" + $"Remote user started sharing, uid: {newSharingUID}");
        var videoParent = FindObjectOfType<ShareInteractionZone>().GetVideoHolder();

        OnRemoteUserStartedSharing?.Invoke(videoHolder, this.currentSharingUID);
        //Deactivates webcam and enables share on new object
        SetWebCamActive(uint.Parse(currentSharingUID), false);
        ShareByUID(videoParent, currentSharingUID, shareType);
        screenShareObject.gameObject.SetActive(true);
    }
    /// <summary>
    /// Response from colyseus message to stop sharing a remote object
    /// </summary>
    private void RemoteStopShareResponse(CharacterAction action)
    {
        if (string.IsNullOrEmpty(this.myUID)) return;
        if (action.action == null) return;
        if (!action.action.Contains("share_stop_request")) return;

        var splitted = action.action.Split('%');
        var remoteUID = splitted[1];

        //If it is my id DO NOT stop share again
        if (this.myUID == remoteUID)
            return;

        //Updates current uid sharing
        Debug.Log("AGORA" + $"Remote user stopped sharing, uid: {remoteUID}");
        SetWebCamActive(uint.Parse(currentSharingUID), true);
        DestroyCurrentShareObject();
        this.currentSharingUID = "";
    }
    private void RemoteUpdateUserSharing(CharacterAction action)
    {
        if (string.IsNullOrEmpty(this.myUID)) return;
        if (action.action == null) return;
        if (!action.action.Contains("update_user_sharing")) return;

        var splitted = action.action.Split('%');
        var remoteUID = splitted[1];

        if (this.myUID == remoteUID)
            return;

        this.currentSharingUID = remoteUID;

        //Creates sharing instance and deactivates its user camera
        SetWebCamActive(uint.Parse(this.currentSharingUID), false);
        var shareInteraction = FindObjectOfType<ShareInteractionZone>();
        ShareByUID(shareInteraction.GetVideoHolder(), this.currentSharingUID, "webcam");
    }
    /// <summary>
    /// Response for new users that there is a user with a camera initialized
    /// </summary>
    private void RemoteCameraStartupReponse(CharacterAction action)
    {
        if (string.IsNullOrEmpty(this.myUID)) return;

        if (action.action == null)
            return;
        if (!action.action.Contains("notify_camera_on_startup")) return;
        var splitted = action.action.Split('%');
        string messengerUID = splitted[1];
        var remoteAgoraVideoType = Enum.Parse(typeof(AgoraVideoType), splitted[2]);
        string targetUID = splitted[3];

        //Do not notify myself about my initialization
        if (this.myUID == messengerUID)
            return;

        //I am not the target
        if (!string.IsNullOrEmpty(targetUID) && targetUID != this.myUID)
            return;

        //Updates user that initialized its camera
        AgoraCharacterData characterDataMessage = GetCharacterDataByUID(messengerUID);
        characterDataMessage.hasWebcamInitialized = true;
        characterDataMessage.agoraVideoType = (AgoraVideoType)remoteAgoraVideoType;

        var characterInstance = characterDataMessage.characterInstance;

       /* switch (remoteAgoraVideoType)
        {
            case AgoraVideoType.Video2D:
                //characterInstance.WebcamVideo = SetWebcamVideo2D();
                break;
            case AgoraVideoType.Video3D:
                characterInstance.webcamVideo = characterInstance.webcamVideo3D;
                break;
        }*/
        MakeVideoView(uint.Parse(messengerUID), characterInstance.webcamVideo, characterDataMessage);
        if (this.currentSharingUID == messengerUID)
            SetWebCamActive(uint.Parse(messengerUID), false);
    }
    private void RemoteCameraRuntimeActivationResponse(CharacterAction action)
    {
        if (string.IsNullOrEmpty(this.myUID)) return;

        if (action.action == null)
            return;

        if (!action.action.Contains("notify_camera_runtime")) return;
        var splitted = action.action.Split('%');
        string messengerUID = splitted[1];

        if (messengerUID == this.myUID)
            return;

        //Updates user that initialized its camera
        AgoraCharacterData characterData = GetCharacterDataByUID(messengerUID);
        characterData.agoraVideoType = agoraVideoType;
        characterData.hasWebcamInitialized = true;

        var characterInstance = characterData.characterInstance;

        switch (agoraVideoType)
        {
            case AgoraVideoType.Video2D:
                characterInstance.webcamVideo = characterData.characterInstance.webcamVideo;
                break;
            case AgoraVideoType.Video3D:
                characterInstance.webcamVideo = characterData.characterInstance.webcamVideo;
                break;
        }
        MakeVideoView(uint.Parse(messengerUID), characterInstance.webcamVideo, characterData);
        //Deactivates camera if user is sharing
        if (messengerUID == currentSharingUID)
            SetWebCamActive(uint.Parse(messengerUID), false);
    }
    #endregion

    #region Audio methods
    /*private void ReceiveOtherPlayerAudioZones(CharacterAction action)
    {
        if (string.IsNullOrEmpty(this.myUID)) return;
        if (action.action == null) return;
        if (!action.action.Contains("notify_audio_zone_startup")) return;

        var splitted = action.action.Split('%');
        var playerUID = splitted[1];
        var targetUID = splitted[2];

        if (targetUID != myUID)
            return;

        var ids = splitted[3].Split('|');
        List<AudioZone> zones = new List<AudioZone>();
        for (int i = 0; i < ids.Length; i++)
        {
            zones.Add(AudioZone.ZonesMap[ids[i]]);
        }

        //Character targetCharacter = null;
        //for (int j = 0; j < characters.Count; j++)
        //{
        //    if(characters[j].characterInstance.UID == )
        //    targetCharacter = characters[j].characterInstance;
        //}
    }*/

    private void UserEnteredZone(AudioZone enteredZone, object characterObject)
    {
        AgoraLocalCharacter character = (AgoraLocalCharacter)characterObject;
        character.AddToAudioZone(enteredZone);
        //AddToZone(enteredZone, character);

        if (character.UID.ToString() == this.myUID)
        {
            //Check all user in my zone
            foreach (AgoraCharacterData item in characters)
            {
                if (item.characterInstance)
                {
                    bool hasMutedCharacter = false;
                    foreach (AudioZone itemZone in character.AudioZones)
                    {
                        //User is in one of my zones
                        if (itemZone.GetCharactersInZone().Contains(item.characterInstance))
                        {
                            SetRemoteMuteCharacter(item.characterInstance, false);
                            break;  //Must exit loop if he is in my zone
                        }
                        else
                        {
                            //Avoiding muting the same character more than once
                            hasMutedCharacter = true;
                            if (!hasMutedCharacter)
                                SetRemoteMuteCharacter(item.characterInstance, true);
                        }
                    }
                }

            }
        }
        else
        {
            //New user has entered my zone, must set mute to false
            if (LocalCharacter.AudioZones.Contains(enteredZone))
            {
                SetRemoteMuteCharacter(character, false);
            }
            //New user entered a zone which I am not in
            else
            {
                SetRemoteMuteCharacter(character, true);
            }
        }
    }
    
    private void UserExitedZone(AudioZone exitedZone, object characterObject)
    {
        AgoraLocalCharacter character = (AgoraLocalCharacter)characterObject;
        character.RemoveFromAudioZone(exitedZone);
        //RemoveFromZone(zone, character);

        //I exited a zone
        if (character.UID.ToString() == this.myUID)
        {
            //If I am not in any audio zone, must unmute everyone in the same situation
            if (character.AudioZones.Count == 0)
            {
                foreach (AgoraCharacterData item in characters)
                {
                    //If user is in a audio zone, must mute it
                    if (item.characterInstance && item.characterInstance.AudioZones.Count > 0)
                        SetRemoteMuteCharacter(item.characterInstance, true);
                    //User does not have an audio zone, exactly like me
                    else if (item.characterInstance && item.characterInstance.AudioZones.Count == 0)
                        SetRemoteMuteCharacter(item.characterInstance, false);
                }
            }
            ///Must mute everyone that was on my previous zone
            else
            {
                foreach (AgoraLocalCharacter item in exitedZone.GetCharactersInZone())
                {
                    SetRemoteMuteCharacter(item, true);
                }
            }
        }

        //Other user exited a zone
        else
        {
            //Checking if user that exited a zone, still inside on another zone of mine
            bool userIsOnMyZone = false;
            if (LocalCharacter.AudioZones.Count > 0)
            {
                foreach (AudioZone item in character.AudioZones)
                {
                    //Checking my zones
                    if (item.GetCharactersInZone().Contains(character))
                    {
                        userIsOnMyZone = true;
                        break;
                    }
                }
                //If not not on any zone of mine, must set mute to true
                if (!userIsOnMyZone)
                    SetRemoteMuteCharacter(character, true);
            }
            //If I dont have any zones and the user dont have it too
            else if (character.AudioZones.Count == 0)
            {
                SetRemoteMuteCharacter(character, false);
            }

        }
    }
    
    /// <summary>
    /// This method do not work for user local audio, user mRtcEngine.MuteLocalAudioStream() instead
    /// </summary>
    private void SetRemoteMuteCharacter(AgoraLocalCharacter character, bool muted) => mRtcEngine.MuteRemoteAudioStream(character.UID, muted);
    #endregion

    public bool GetIsAudio
    {
        get { return LocalCharacter.IsAudioOn; }
    }

    /// <summary>
    /// 
    /// </summary>
    public void ToggleAudio()
    {
        Debug.Log("AGORA" + $"Called ToggleAudio: {!LocalCharacter.IsAudioOn}");
        LocalCharacter.IsAudioOn = !LocalCharacter.IsAudioOn;
        //if(!SPACIAL_AUDIO_ENABLED)
        mRtcEngine.MuteLocalAudioStream(!LocalCharacter.IsAudioOn);

        /*if (LocalPlayerWebcamVideo)
        {
            LocalPlayerWebcamVideo.NoAudioObj.SetActive(!LocalCharacter.IsAudioOn);
        }
        else
        {
            if (LocalCharacter.IsVideoOn)
            {
                if (LocalCharacter.WebcamVideo3D.NoAudioSingleObj && GameManager.Instance.GameData.sessionData.agoraio.spacial_audio.enable)
                    LocalCharacter.WebcamVideo3D.NoAudioSingleObj.SetActive(!LocalCharacter.IsAudioOn);
            }
            else
            {
                if (LocalCharacter.WebcamVideo3D.NoAudioSingleObj && GameManager.Instance.GameData.sessionData.agoraio.spacial_audio.enable)
                    LocalCharacter.WebcamVideo3D.NoAudioSingleObj.SetActive(true);

                LocalCharacter.WebcamVideo3D.OnOffAudioIcon(LocalCharacter.IsAudioOn);
            }
        }*/
    }

    /// <summary>
    /// 
    /// </summary>
    public void LeaveChannel()
    {
        mRtcEngine.LeaveChannel();
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnApplicationQuit()
    {
        Debug.Log("AGORA" + "OnApplicationQuit: IRtcEngine destroyed!");

        if (mRtcEngine != null)
        {
            LeaveChannel();
            IRtcEngine.Destroy();
        }
    }

    #endregion
}