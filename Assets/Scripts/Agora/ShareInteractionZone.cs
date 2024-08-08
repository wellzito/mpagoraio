using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShareInteractionZone : MonoBehaviour
{
    public delegate void OnShareEventHandler(Transform videoHolder);
    public static event OnShareEventHandler OnClickedOnShare;
    public static event OnShareEventHandler OnStopShare;

    [SerializeField] private Transform videoHolder;

    private string myUid;
    private string currentSharingUID = "";
    private bool isMySharing = false;
    private bool isOnSharingArea = false;

    #region Variables

    public Transform GetVideoHolder() => videoHolder;

    #endregion

    #region Methods

    /// <summary>
    /// 
    /// </summary>
    private void Start()
    {
        AgoraController.OnLocalUserJoinedAgoraChannel += OnUserEnteredAgora;
        AgoraController.OnLocalUserStartedSharing += LocalUserStartedSharing;
        AgoraController.OnLocalUserStoppedSharing += LocalUserStoppedSharing;
        AgoraController.OnRemoteUserStartedSharing += RemoteUserStartedSharing;
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnDestroy()
    {
        AgoraController.OnLocalUserJoinedAgoraChannel -= OnUserEnteredAgora;
        AgoraController.OnLocalUserStartedSharing -= LocalUserStartedSharing;
        AgoraController.OnLocalUserStoppedSharing -= LocalUserStoppedSharing;
        AgoraController.OnRemoteUserStartedSharing -= RemoteUserStartedSharing;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="uid"></param>
    private void OnUserEnteredAgora(string uid)
    {
        this.myUid = uid;

        if (isOnSharingArea)
        {
           // RuntimeUIManager.Instance.ViewAction.View.Show();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="videoHolder"></param>
    /// <param name="uid"></param>
    private void LocalUserStoppedSharing(Transform videoHolder, string uid)
    {
        if (this.videoHolder != videoHolder)
            return;

        isMySharing = false;

        if (isOnSharingArea)
        {
            ShowScreenShareButton();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="videoHolder"></param>
    /// <param name="uid"></param>
    private void RemoteUserStartedSharing(Transform videoHolder, string uid)
    {
        if (this.videoHolder != videoHolder)
            return;

        currentSharingUID = uid;
        isMySharing = false;

        if (isOnSharingArea)
        {
            ShowScreenShareButton();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="videoHolder"></param>
    /// <param name="uid"></param>
    private void LocalUserStartedSharing(Transform videoHolder, string uid)
    {
        if (this.videoHolder != videoHolder)
            return;

        currentSharingUID = uid;
        isMySharing = true;

        if (isOnSharingArea)
        {
            ShowStopShareButton();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void ShowScreenShareButton()
    {
       /* RuntimeUIManager.Instance.ViewAction.AddListener(ClickedOnShare);
        RuntimeUIManager.Instance.ViewAction.UpdateText("action_share");
        RuntimeUIManager.Instance.ViewAction.View.Show();*/
    }

    /// <summary>
    /// 
    /// </summary>
    private void ShowStopShareButton()
    {
        /*RuntimeUIManager.Instance.ViewAction.AddListener(ClickedOnStopShare);
        RuntimeUIManager.Instance.ViewAction.UpdateText("action_stop_share");
        RuntimeUIManager.Instance.ViewAction.View.Show();*/
    }

    /// <summary>
    /// 
    /// </summary>
    private void HideButton()
    {
        //RuntimeUIManager.Instance.ViewAction.View.Hide();
    }

    /// <summary>
    /// 
    /// </summary>
    private void ClickedOnShare()
    {
        OnClickedOnShare?.Invoke(videoHolder);
        HideButton();
    }

    /// <summary>
    /// 
    /// </summary>
    private void ClickedOnStopShare()
    {
        isMySharing = false;

        OnStopShare?.Invoke(videoHolder);
        /*RuntimeUIManager.Instance.ViewAction.UpdateText("action_share");
        RuntimeUIManager.Instance.ViewAction.AddListener(ClickedOnShare);*/
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        if (string.IsNullOrEmpty(this.myUid))
            return;

        if (other.TryGetComponent(out PlayerInfo playerInput))
        {
            isOnSharingArea = true;

            if (isMySharing)
                ShowStopShareButton();
            else
                ShowScreenShareButton();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerExit(Collider other)
    {
        if (string.IsNullOrEmpty(this.myUid))
            return;

        if (other.TryGetComponent(out PlayerInfo playerInput))
        {
            isOnSharingArea = false;
            HideButton();

          /*  if (AgoraController.instance.m_popupAgoraShare != null)
                AgoraController.instance.m_popupAgoraShare.Hide();*/
        }
    }

    #endregion
}
