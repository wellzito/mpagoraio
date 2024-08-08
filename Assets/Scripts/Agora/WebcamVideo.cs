/*
 * 
 */

using agora_gaming_rtc;
using System;
using UnityEngine;
using UnityEngine.UI;

public class WebcamVideo : MonoBehaviour
{
    #region Variables

    [SerializeField] private GameObject videoSurfaceObj;
    [SerializeField] private GameObject videoSpeakingOutline;

    [SerializeField] private GameObject maskObj;
    [SerializeField] private GameObject noVideoObj;
    [SerializeField] private GameObject noAudioObj;
    [SerializeField] private GameObject audioSingleObj;
    [SerializeField] private Image iconAudio;
    [SerializeField] private Sprite spriteOnAudio;
    [SerializeField] private Sprite spriteNoAudio;
    [SerializeField] private GameObject audioSpeakingOutline;

    private Transform cameraMain;

    public GameObject VideoSurfaceObj
    {
        get => videoSurfaceObj;
        set => videoSurfaceObj = value;
    }

    public GameObject MaskObj
    {
        get => maskObj;
        set => maskObj = value;
    }

    public GameObject NoVideoObj
    {
        get => noVideoObj;
        set => noVideoObj = value;
    }

    public GameObject NoAudioObj
    {
        get => noAudioObj;
        set => noAudioObj = value;
    }

    public Transform TargetToLookAt { get; set; }
    public GameObject AudioSingleObj { get => audioSingleObj; }

    public GameObject VideoSpeakingOutline
    {
        get => videoSpeakingOutline;
        set => videoSpeakingOutline = value;
    }

    public GameObject AudioSpeakingOutline
    {
        get => audioSpeakingOutline;
        set => audioSpeakingOutline = value;
    }

    [Header("ONLY VIEW | DO NOT CHANGE")]
    public AgoraVideoType AgoraVideoType;

    #endregion

    #region Methods

    private void Start()
    {
        cameraMain = Camera.main.transform;
    }

    /// <summary>
    /// 
    /// </summary>
    private void Update()
    {
        if (TargetToLookAt && (AgoraVideoType == AgoraVideoType.Video3D))
        {
            PersistentLookAt(TargetToLookAt);
        }

        NoAudioIconLookTo(cameraMain.transform.position);
    }

    private void NoAudioIconLookTo(Vector3 target)
    {
        var targetPosition = new Vector3(target.x, target.y, target.z);
        if (audioSingleObj)
            audioSingleObj.transform.LookAt(targetPosition);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="target"></param>
    private void PersistentLookAt(Transform target)
    {
        var targetPosition = new Vector3(target.position.x, transform.position.y, target.position.z);
        transform.LookAt(targetPosition);
    }

    public VideoSurface GetVideoSurface()
    {
        return videoSurfaceObj.GetComponent<VideoSurface>();
    }

    public void SetChildrensActive(bool value)
    {
        foreach (Transform item in transform)
        {
            item.gameObject.SetActive(value);
        }
    }

    public void OnOffAudioIcon(bool value)
    {
        if (value)
        {
            iconAudio.sprite = spriteOnAudio;
        }
        else
        {
            iconAudio.sprite = spriteNoAudio;
        }
    }

    #endregion
}
