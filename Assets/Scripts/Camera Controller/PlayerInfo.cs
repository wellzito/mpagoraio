using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System.Linq;
using Fusion.Addons.KCC;
public class PlayerInfo : NetworkBehaviour
{
    public Camera renderCamera;
    public Camera renderCameraFace;

    [Networked] public NetworkString<_64> playerName { get; set; }
    [Networked] public NetworkId playerId { get; set; }

    public PlayerRef _playerRef;
    public RenderTexture _renderTexture;
    public RenderTexture _renderTextureFace;
    public bool isLocalPlayer;
    public Animator _anim;
    public PlayerMovement _playerMovement;
    //[Header("Skin Settings")]
    //public List<GameObject> skins = new List<GameObject>();
    //[Networked] public NetworkString<_64> _skinUrl { get; set; }
    [Networked] public int _skinIndex { get; set; }
    public int lastSkinIndex = -1;

    [Networked] public int _skinExternalIndex { get; set; }
    public int lastSkinExternalIndex = -1;
    [Networked] public NetworkString<_64> keySkin { get; set; }

    [Networked] public int _emojiIndex { get; set; }
    public int lastEmojiIndex = -1;

    public KCC _kcc;
    public AvatarLoadMP avatarLoad;
    public ReadyPlayerMe.Samples.QuickStart.CameraOrbit cameraOrbit;
    private ReadyPlayerMe.Samples.QuickStart.CameraFollow cameraFollow;
    public Transform targetCam;
    public bool onVideoShare;
    public bool onLoadAvatarExternal;
    public bool onInputExternal = false;
    public ActionsPlayerController actionsPlayer;
    public bool onActionEmoji = false;
    [Networked] public NetworkString<_64> keyEmoji { get; set; }
    private void Start()
    {
        _kcc = GetComponent<KCC>();
        _playerMovement = GetComponent<PlayerMovement>();
        //if (_anim == null) _anim = GetComponentInChildren<Animator>();
        lastSkinIndex = -1;
    }

    public override void Spawned()
    {
        Invoke(nameof(SetupPlayer), 1);
        StartCoroutine(DelayedSkinSetup());
        isLocalPlayer = HasInputAuthority;
    }

    void SetupPlayer()
    {
        if (HasInputAuthority)
        {
            cameraOrbit = FindObjectOfType<ReadyPlayerMe.Samples.QuickStart.CameraOrbit>();
            cameraFollow = cameraOrbit.GetComponent<ReadyPlayerMe.Samples.QuickStart.CameraFollow>();
            if (cameraFollow)
                cameraFollow.target = targetCam;

            RPC_NameChange(PlayerPrefs.GetString("PlayerName", string.Format("Player {0}", Object.Id)).ToString().Replace("[Id:", "").Replace("]", ""));
            _playerRef = PlayerSpawner.instance._spawnedCharacters.FirstOrDefault(x => x.Value.Equals(Object)).Key;
            RPC_ChangePlayerID(Object.Id);
            //playerId = Object.Id; //Need to be RPC
            Runner.SetPlayerObject(_playerRef, Object);
            PlayerSpawner.instance._localPlayer = this.gameObject;

            if (HasInputAuthority)
            {
                RPC_ResetInitialPosition();
            }

            SpawnManager.instance.SpawnPlayerAgora(PlayerSpawner.instance._localPlayer);
        }
    }

    public override void Render()
    {
        if (gameObject.name != playerName.Value) gameObject.name = playerName.Value;
        if (_skinIndex != lastSkinIndex)
        {
            lastSkinIndex = _skinIndex;
            ChangeSkin();
        }

        if(_skinExternalIndex != lastSkinExternalIndex)
        {
            _skinExternalIndex = lastSkinExternalIndex;
            ChangeSkinExternal((string)keySkin);
        }

        if(_emojiIndex != lastEmojiIndex)
        {
            lastEmojiIndex = _emojiIndex;
            actionsPlayer.ShowCharacterReaction((string)keyEmoji);
        }

        if (HasInputAuthority)
        {
            if (onActionEmoji) { onActionEmoji = false; RPC_Reactions(PlayerPrefs.GetString("keyEmoji")); }
            if (onInputExternal) { onInputExternal = false; RPC_AvatarExternal(PlayerPrefs.GetString("myPlayer")); }
            if (Input.GetKeyDown(KeyCode.F2)) { RPC_Skin(1); }
            if (Input.GetKeyDown(KeyCode.F1)) { RPC_Skin(-1); }
        }

    }

    public override void FixedUpdateNetwork()
    {
        base.FixedUpdateNetwork();
    }

    public void ResetPos2()
    {
        if (HasInputAuthority)
        {
            RPC_ResetInitialPosition();
        }
    }
    private void Update()
    {

        if (HasInputAuthority)
        {
            if (Input.GetKeyDown(KeyCode.K))
            {
                RPC_ResetInitialPosition();
                Debug.Log("Teleported");
            }
        }
    }
    //[Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority)]
    public void ChangeInitialPositionFromServer()
    {
        if (!HasStateAuthority) { Debug.LogError("Krl menó"); return; }
        if (HasInputAuthority)
        {
            Debug.Log("ChangeInitialPositionFromServer p1");
            _kcc.RPC_TeleportRPC2(SpawnManager.instance.positions[Random.Range(0, SpawnManager.instance.positions.Length)].position, 0, 0);
        }
        else
        {
            Debug.Log("ChangeInitialPositionFromServer p2");
            _kcc.RPC_TeleportRPC2(SpawnManager.instance.positions[Random.Range(0, SpawnManager.instance.positions.Length)].position, 0, 0);
        }
    }
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_ResetInitialPosition()
    {
        if (HasStateAuthority)
        {
            ChangeInitialPositionFromServer();
        }

    }
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_NameChange(string value)
    {

        if (!HasStateAuthority) return;
        if (!HasInputAuthority)
        {
            string stateName = PlayerPrefs.GetString("PlayerName", string.Format("Player {0}", Object.Id)).ToString().Replace("[Id:", "").Replace("]", "");
            playerName = value == stateName ? string.Format("{0} {1}", value, Object.Id) : value;
            return;
        }
        playerName = value;
    }
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_ChangePlayerID(NetworkId value)
    {
        if (!HasStateAuthority) return;

        playerId = value;
    }
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_ResetAnimator()
    {
        //Animator animator = GetComponent<PlayerMovement>()._anim;
        _anim.Play("Blend Tree");
        _anim.SetFloat("Speed", 0);
    }

    public void Setup(GameObject target, RuntimeAnimatorController runtimeAnimatorController, NetworkMecanimAnimator networkAnim)
    {
        _anim = target.GetComponent<Animator>();
        _anim.runtimeAnimatorController = runtimeAnimatorController;
        _anim.applyRootMotion = false;
        var mov = GetComponent<PlayerMovement>();
        mov._anim = _anim;
        networkAnim.Animator = _anim;
    }

    #region Skin

    IEnumerator DelayedSkinSetup()
    {
        if (HasInputAuthority)
        {
            RPC_SkinChange(PlayerPrefs.GetString("myPlayer"));
            yield return new WaitForSeconds(.5f);
            //_anim = avatarLoad.thirdPersonLoader.AvatarCurrent.GetComponent<Animator>();
            //_playerMovement._anim = _anim;
        }
    }

    public void ChangeSkin()
    {
        if (!onLoadAvatarExternal)
            PlayerSelectionManager.instance.ConnectPlayerCustom(_skinIndex);
        avatarLoad.OnLoadAvatarLink(PlayerPrefs.GetString("myPlayer"));
    }

    public void ChangeSkinExternal(string value)
    {
        avatarLoad.OnLoadAvatarLink(value);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_SkinChange(string value)
    {
        if (!HasStateAuthority) return;
        if (!avatarLoad) avatarLoad = GetComponent<AvatarLoadMP>();
        avatarLoad.localCam = true;
        avatarLoad.OnLoadAvatarLink(value);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_Skin(int value)
    {
        onLoadAvatarExternal = false;
        if (value == -1)
        {
            _skinIndex = (_skinIndex - 1 + PlayerSelectionManager.instance.playersURL.Count) % PlayerSelectionManager.instance.playersURL.Count;

        }
        else
        {
            _skinIndex = (_skinIndex + 1) % PlayerSelectionManager.instance.playersURL.Count;

        }

        PlayerPrefs.SetInt("myPlayerNum", _skinIndex);
    }
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_ScreenShare()
    {
        AgoraUnityVideo.Instance.StartShare();
    }
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_AvatarExternal(string value)
    {
        keySkin = value;
        List<int> possibleValues = Enumerable.Range(0, 250).ToList();
        possibleValues.Remove(_skinExternalIndex);

        int newSkinIndex = possibleValues[Random.Range(0, possibleValues.Count)];
        _skinExternalIndex = newSkinIndex;
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_Reactions(string value)
    {
        keyEmoji = value;
       // _emojiIndex = Random.Range(_emojiIndex + 1, 50);
        List<int> possibleValues = Enumerable.Range(0, 250).ToList();
        possibleValues.Remove(_emojiIndex);

        int newEmojiIndex = possibleValues[Random.Range(0, possibleValues.Count)];
        _emojiIndex = newEmojiIndex;
        //actionsPlayer.ShowCharacterReaction(value);
        Debug.Log($"{gameObject.name} Emoji Reaction");
    }
    #endregion
}
