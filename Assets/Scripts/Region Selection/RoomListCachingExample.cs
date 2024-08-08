using Fusion;
using Fusion.Photon.Realtime;
using Photon.Realtime;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class RoomListCachingExample :MonoBehaviour , ILobbyCallbacks, IConnectionCallbacks
{
    private TypedLobby customLobby = new TypedLobby("customLobby", LobbyType.Default);
    private LoadBalancingClient loadBalancingClient;

    private Dictionary<string, RoomInfo> cachedRoomList = new Dictionary<string, RoomInfo>();
    public NetworkRunner _runner;
    public GameObject runnerPrefab;
    public bool isConnected = false;
    public void JoinLobby()
    {
        loadBalancingClient.OpJoinLobby(customLobby);
    }
    public enum GameType : int
    {
        TwoXTwo,
        Team,
        Timed
    }

    public enum GameMap : int
    {
        Campnoll,
        Maracana,
        ParqueDosPrincipes
    }

    private void Start()
    {
        Game();
    }
    public async Task StartHost(NetworkRunner runner, GameMap gameMap, GameType gameType)
{
        var region = PlayerPrefs.GetString("server", "");
        var appSettings = BuildCustomAppSetting(region);
        var customProps = new Dictionary<string, SessionProperty>();

    customProps["map"] = (int)gameMap;
    customProps["type"] = (int)gameType;

    var result = await runner.StartGame(new StartGameArgs()
    {
        GameMode = GameMode.AutoHostOrClient,
        SessionProperties = customProps,
        CustomPhotonAppSettings = appSettings,

    });

    if (result.Ok)
    {
        // all good
    }
    else
    {
        Debug.LogError($"Failed to Start: {result.ShutdownReason}");
    }
}
    public void Game()
    {
        if(_runner == null)
        {
            _runner = Instantiate(runnerPrefab, new Vector3(0, 0, 0), Quaternion.identity).GetComponent<NetworkRunner>();
            
        }
        Game(_runner);
    }

    private FusionAppSettings BuildCustomAppSetting(string region, string customAppID = null, string appVersion = "1.0.0")
    {

        var appSettings = PhotonAppSettings.Global.AppSettings.GetCopy(); ;

        appSettings.UseNameServer = true;
        appSettings.AppVersion = appVersion;

        if (string.IsNullOrEmpty(customAppID) == false)
        {
            appSettings.AppIdFusion = customAppID;
        }

        if (string.IsNullOrEmpty(region) == false)
        {
            appSettings.FixedRegion = region.ToLower();
        }

        // If the Region is set to China (CN),
        // the Name Server will be automatically changed to the right one
        // appSettings.Server = "ns.photonengine.cn";

        return appSettings;
    }
    public void Game(NetworkRunner runner)
    {
        var region = PlayerPrefs.GetString("server", "");
        var appSettings = BuildCustomAppSetting(region);

        var customProps = new Dictionary<string, SessionProperty>();

        customProps["map"] = (int)GameMap.Maracana;
        customProps["type"] = (int)GameType.Team;

        _ = runner.StartGame(new StartGameArgs
        {
            SessionName = "",
            SessionProperties = customProps,
            //CustomLobbyName = "FlamengoZico",
            EnableClientSessionCreation = true,
            PlayerCount = 100,
            IsOpen = true,
            IsVisible = true,
            MatchmakingMode = (Fusion.Photon.Realtime.MatchmakingMode?)Fusion.Photon.Realtime.MatchmakingMode.FillRoom,
            GameMode = GameMode.AutoHostOrClient,
            CustomPhotonAppSettings = appSettings
        });
    }
    public void ChangeConnected(bool value)
    {
        isConnected = value;
    }
    private void Update()
    {

        if(Input.GetKeyDown(KeyCode.G)){
            _runner.SessionInfo.IsOpen = false;
            _runner.SessionInfo.IsVisible = false;
            return;
        }
        if (_runner == null  && isConnected)
        {
            if(_runner) Destroy(_runner.gameObject);
            Game();
            isConnected = false;
        }
        if(_runner != null ) isConnected = _runner.State == NetworkRunner.States.Running ? true : false;

        if (_runner == null && !isConnected) Game();
        return;
        if (_runner != null && _runner.IsServer)
        {
            int playerCount = _runner.SessionInfo.PlayerCount;
            if (playerCount >= 100)
            {
                _runner.SessionInfo.IsOpen = false;
                _runner.SessionInfo.IsVisible = false;
            }
        }
    }
    private void UpdateCachedRoomList(List<RoomInfo> roomList)
    {
        for (int i = 0; i < roomList.Count; i++)
        {
            RoomInfo info = roomList[i];
            if (info.RemovedFromList)
            {
                cachedRoomList.Remove(info.Name);
            }
            else
            {
                cachedRoomList[info.Name] = info;
            }
        }
    }

    // do not forget to register callbacks via loadBalancingClient.AddCallbackTarget
    // also deregister via loadBalancingClient.RemoveCallbackTarget
    #region ILobbyCallbacks

    void ILobbyCallbacks.OnJoinedLobby()
    {
        cachedRoomList.Clear();
    }

    void ILobbyCallbacks.OnLeftLobby()
    {
        cachedRoomList.Clear();
    }

    void ILobbyCallbacks.OnRoomListUpdate(List<RoomInfo> roomList)
    {
        // here you get the response, empty list if no rooms found
        UpdateCachedRoomList(roomList);
    }

    // [..] Other callbacks implementations are stripped out for brevity, they are empty in this case as not used.

    #endregion

    #region IConnectionCallbacks

    void IConnectionCallbacks.OnDisconnected(DisconnectCause cause)
    {
        cachedRoomList.Clear();
        Debug.Log($"Disconnected eeeeeeeeeeeee: {cause}");
        //Game();
        //isConnected = false;
    }

    public void OnLobbyStatisticsUpdate(List<TypedLobbyInfo> lobbyStatistics)
    {
        throw new System.NotImplementedException();
    }

    public void OnConnected()
    {
        // throw new System.NotImplementedException();
        //isConnected = true;
    }

    public void OnConnectedToMaster()
    {
        throw new System.NotImplementedException();
    }

    public void OnRegionListReceived(RegionHandler regionHandler)
    {
        throw new System.NotImplementedException();
    }

    public void OnCustomAuthenticationResponse(Dictionary<string, object> data)
    {
        throw new System.NotImplementedException();
    }

    public void OnCustomAuthenticationFailed(string debugMessage)
    {
        throw new System.NotImplementedException();
    }

    // [..] Other callbacks implementations are stripped out for brevity, they are empty in this case as not used.

    #endregion
}