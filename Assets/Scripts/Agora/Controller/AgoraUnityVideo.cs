using agora_gaming_rtc;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AgoraUnityVideo : MonoBehaviour
{
    public static AgoraUnityVideo Instance;
    private IRtcEngine mRtcEngine;

    public IRtcEngine GetRtcEngine
    {
        get { return mRtcEngine; }
    }

    private string token;

    private int lastError;

    private uint localUserId;

    public uint LocalUserId
    {
        get => localUserId;
    }

    private VideoDeviceManager VideoDeviceManager;
    private Dictionary<int, string> videoDeviceManager = new Dictionary<int, string>();
    private Dictionary<int, string> videoDeviceManagerNames = new Dictionary<int, string>();
    [SerializeField] private List<AgoraLocal.AgoraCharacterData> characters = new List<AgoraLocal.AgoraCharacterData>();
    private bool hasVideoDevice;
    public bool HasVideoDevice => (hasVideoDevice);
    private int videoDeviceIndex = 0;
    public TMP_Dropdown dropdownCams;
    public AgoraLocal LocalCharacter;
    public VideoSurface localScreenShare;
    public bool onScreenShare;
    private void Awake()
    {
        Instance = this;
    }

    public void LoadEngine(string appId, string token = null)
    {
        Debug.Log("Loading Engine initialization");

        this.token = token;

        if (mRtcEngine != null)
        {
            Debug.Log("Engine exists. Please unload it first!");
            return;
        }

        mRtcEngine = IRtcEngine.GetEngine(appId);
        mRtcEngine.SetLogFilter(LOG_FILTER.DEBUG | LOG_FILTER.INFO | LOG_FILTER.WARNING | LOG_FILTER.ERROR | LOG_FILTER.CRITICAL);
    }

    public void Join(string channel)
    {
        Debug.Log($"Calling join(channel = {channel})");

        if (mRtcEngine == null) return;

        // set callbacks (optional)
        mRtcEngine.OnJoinChannelSuccess = OnJoinChannelSuccess;
        mRtcEngine.OnUserJoined = OnUserJoined;
        mRtcEngine.OnUserOffline = OnUserOffline;
        mRtcEngine.OnWarning = (int warn, string msg) =>
        {
            Debug.LogWarning($"Warning code:{warn} msg:{IRtcEngine.GetErrorDescription(warn)}");
        };

        mRtcEngine.OnError = HandleError;
        mRtcEngine.EnableVideo();

        // allow camera output callback
        mRtcEngine.EnableVideoObserver();
        mRtcEngine.OnVolumeIndication += VolumeIndication;
        mRtcEngine.EnableAudioVolumeIndication(500, 8, report_vad: true);
        streamId = mRtcEngine.CreateDataStream(true, true);
        mRtcEngine.OnStreamMessage = OnStreamMessage;
        // join channel
        /*  This API Assumes the use of a test-mode AppID
             mRtcEngine.JoinChannel(channel, null, 0);
        */

        /*  This API Accepts AppID with token; by default omiting info and use 0 as the local user id */
        mRtcEngine.JoinChannelByKey(channelKey: token, channelName: channel);

        AgoraLocal playerInput = SpawnManager.instance.playerLocal.GetComponent<AgoraLocal>();
        LocalCharacter = playerInput;
    }

    public void Leave()
    {
        Debug.Log("Leaving channel");
        if (mRtcEngine == null) return;

        mRtcEngine.LeaveChannel();

        // deregister video frame observers in native-c code
        mRtcEngine.DisableVideoObserver();

        GameObject go = GameObject.Find($"{localUserId}");
        if (go != null) Destroy(go);
    }

    // unload agora engine
    public void UnloadEngine()
    {
        Debug.Log("Calling unloadEngine");
        if (mRtcEngine != null)
        {
            IRtcEngine.Destroy();  // Place this call in ApplicationQuit
            mRtcEngine = null;
        }
    }

    public void EnableVideo(bool pauseVideo)
    {
        if (mRtcEngine != null)
        {
            if (!pauseVideo)
                mRtcEngine.EnableVideo();
            else
                mRtcEngine.DisableVideo();
        }
    }

    // Implement engine callbacks
    private void OnJoinChannelSuccess(string channelName, uint uid, int elapsed)
    {
        localUserId = uid;
        Debug.Log($"OnJoinChannelSuccess: uid = {uid}");
        Debug.Log($"SDK Version : {IRtcEngine.GetSdkVersion()}");

        GameObject childVideo = GetChildVideoLocation(uid);
        VideoSurface videoSurface = MakeImageVideoSurface(childVideo);
        DevicesChecker();
    }

    // When a remote user joined, this delegate will be called. Typically
    // create a GameObject to render video on it
    private void OnUserJoined(uint uid, int elapsed)
    {
        Debug.Log($"onUserJoined: uid = {uid} elapsed = {elapsed}");

        GameObject childVideo = GetChildVideoLocation(uid);

        // create a GameObject and assign to this new user
        VideoSurface videoSurface = MakeImageVideoSurface(childVideo);

        if (videoSurface != null)
        {
            // configure videoSurface
            videoSurface.SetForUser(uid);
            videoSurface.SetEnable(true);
            videoSurface.SetVideoSurfaceType(AgoraVideoSurfaceType.RawImage);
        }
    }

    private void DevicesChecker()
    {
        VideoDeviceManager = (VideoDeviceManager)AgoraUnityVideo.Instance.GetRtcEngine.GetVideoDeviceManager();
        VideoDeviceManager.CreateAVideoDeviceManager();
        hasVideoDevice = VideoDeviceManager.GetVideoDeviceCount() > 0;

        GetVideoDeviceManager();
    }

    private void GetVideoDeviceManager()
    {
        var videoDeviceName = "";
        var videoDeviceId = "";

        var count = VideoDeviceManager.GetVideoDeviceCount();
        Debug.Log("AGORA" + $"VideoDevice count: {count}");

        dropdownCams.ClearOptions();
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

        dropdownCams.AddOptions(videoDeviceManagerNames.Values.ToList());
        dropdownCams.value = VideoDeviceManager.GetCurrentVideoDevice(ref videoDeviceName);
    }

    public void SetCurrentDevice(int n)
    {
        if (HasVideoDevice)
        {
            videoDeviceIndex = dropdownCams.value;
            VideoDeviceManager.SetVideoDevice(videoDeviceManager[videoDeviceIndex]);
        }
    }

    private static GameObject GetChildVideoLocation(uint uid)
    {
        // find a game object to render video stream from 'uid'
        GameObject go = GameObject.Find("Videos");
        GameObject childVideo = go.transform.Find($"{uid}")?.gameObject;

        if (childVideo == null)
        {
            childVideo = new GameObject($"{uid}");
            childVideo.transform.parent = go.transform;
        }

        return childVideo;
    }

    public VideoSurface MakeImageVideoSurface(GameObject go)
    {
        go.AddComponent<RawImage>();
        go.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        var rectTransform = go.GetComponent<RectTransform>();
        
        rectTransform.sizeDelta = new Vector2(208.5305f, 117.2985f);
        rectTransform.localPosition = new Vector3(rectTransform.position.x,
            rectTransform.position.y, 0);

        rectTransform.localRotation = new Quaternion(0, rectTransform.localRotation.y,
            -180.0f, rectTransform.localRotation.w);

        return go.AddComponent<VideoSurface>();
    }

    // When remote user is offline, this delegate will be called. Typically
    // delete the GameObject for this user
    private void OnUserOffline(uint uid, USER_OFFLINE_REASON reason)
    {
        Debug.Log($"OnUserOffline: uid = {uid} reason = {reason}");
        GameObject go = GameObject.Find(uid.ToString());
        if (go != null) Destroy(go);
    }

    private void HandleError(int error, string msg)
    {
        if (error == lastError) return;

        if (string.IsNullOrEmpty(msg))
        {
            msg = string.Format($"Error code:{error} msg:{IRtcEngine.GetErrorDescription(error)}");
        }

        switch (error)
        {
            case 101:
                msg += "\nPlease make sure your AppId is valid and it does not require a certificate for this demo.";
                break;
        }

        Debug.LogError(msg);
        lastError = error;
    }


    public void AddCharacter(AgoraLocal.AgoraCharacterData characterData)
    {
        characters.Add(characterData);
    }

    //Volume
    private void VolumeIndication(AudioVolumeInfo[] speakers, int speakerNumber, int totalVolume)
    {
        if (totalVolume > 0)
        {
            LocalCharacter.audioSpeakingOutline.SetActive(LocalCharacter.IsAudioOn);
        }
        else
        {
            LocalCharacter.audioSpeakingOutline.SetActive(false);
        }
    }

    //Audio
    public void ToggleAudio()
    {
        Debug.Log("AGORA" + $"Called ToggleAudio: {!LocalCharacter.IsAudioOn}");
        LocalCharacter.IsAudioOn = !LocalCharacter.IsAudioOn;
        //if(!SPACIAL_AUDIO_ENABLED)
        mRtcEngine.MuteLocalAudioStream(!LocalCharacter.IsAudioOn);
    }
    private int streamId;
    //ScreenShare
    public void StartShare()
    {
        onScreenShare = !onScreenShare;

        if (onScreenShare)
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

            if (localScreenShare != null)
            {
                localScreenShare.GetComponent<RawImage>().enabled = true;
                localScreenShare.SetForUser(0);
                localScreenShare.SetEnable(true);
                localScreenShare.SetVideoSurfaceType(AgoraVideoSurfaceType.RawImage);
            }
            mRtcEngine.StartScreenCaptureByDisplayId(0, captureRect, screenCaptureParameters);
            LocalCharacter.GetPlayerInfo.onVideoShare = true;

            byte[] message = System.Text.Encoding.UTF8.GetBytes("START_SCREEN_SHARE");
            mRtcEngine.SendStreamMessage(streamId, message);
        }
        else
        {
            localScreenShare.GetComponent<RawImage>().enabled = false;
            mRtcEngine.StopScreenCapture();

            byte[] message = System.Text.Encoding.UTF8.GetBytes("STOP_SCREEN_SHARE");
            mRtcEngine.SendStreamMessage(streamId, message);
        }
    }

    private void OnStreamMessage(uint userId, int streamId, byte[] data, int length)
    {
        string message = System.Text.Encoding.UTF8.GetString(data);

        if (message == "START_SCREEN_SHARE")
        {
            Debug.Log("START_SCREEN_SHARE");
            if (localScreenShare != null)
            {
                localScreenShare.GetComponent<RawImage>().enabled = true;
                localScreenShare.SetForUser(userId);
                localScreenShare.SetEnable(true);
                localScreenShare.SetVideoSurfaceType(AgoraVideoSurfaceType.RawImage);
            }
        }
        else if (message == "STOP_SCREEN_SHARE")
        {
            Debug.Log("STOP_SCREEN_SHARE");
            localScreenShare.GetComponent<RawImage>().enabled = false;
            mRtcEngine.StopScreenCapture();
        }
    }
}