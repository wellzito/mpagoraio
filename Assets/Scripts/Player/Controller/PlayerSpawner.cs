using Fusion;
using Fusion.Sockets;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSpawner : SimulationBehaviour, INetworkRunnerCallbacks
{
    public static PlayerSpawner instance;
    public List<Transform> spawnPositionList = new List<Transform>();

    [SerializeField] private NetworkPrefabRef _playerPrefab;
    public Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = new Dictionary<PlayerRef, NetworkObject>();
    public GameObject _localPlayer;
    [SerializeField] private NetworkPrefabRef _gameManager;
    public GameObject gameManager;


    public int playersInScene = 0;

    private bool isMenu = false;
    private void Awake()
    {
        instance = this;
    }
    private void Update()
    {
        playersInScene = _spawnedCharacters.Count;
    }
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer)
        {
            // Create a unique position for the player
            Vector3 spawnPosition = new Vector3((player.RawEncoded % runner.Config.Simulation.PlayerCount) * 3, 5, 0);
            NetworkObject networkPlayerObject = runner.Spawn(_playerPrefab, spawnPosition, Quaternion.identity, player);
            // Keep track of the player avatars for easy access
            _spawnedCharacters.Add(player, networkPlayerObject);

            if (_spawnedCharacters.Count >= 100)
            {
                Runner.SessionInfo.IsOpen = false;
                Runner.SessionInfo.IsVisible = false;
            }


            if (gameManager == null)
            {
                Vector3 spawnPosition2 = new Vector3((player.RawEncoded % runner.Config.Simulation.PlayerCount) * 3, 20, 0);
                NetworkObject networkObject = runner.Spawn(_gameManager, spawnPosition2, Quaternion.identity, player);
                gameManager = networkObject.gameObject;
            }
        }

    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (_spawnedCharacters.TryGetValue(player, out NetworkObject networkObject))
        {
            runner.Despawn(networkObject);
            _spawnedCharacters.Remove(player);
        }

        if (_spawnedCharacters.Count < 2)
        {
            Runner.SessionInfo.IsOpen = true;
            Runner.SessionInfo.IsVisible = true;
        }
        Runner.Shutdown();
    }

    public void ExitToMainMenu()
    {
        isMenu = true;
        if (Runner != null)
            Runner.Shutdown();
        else SceneManager.LoadScene("MainMenu_Scene");
    }
    #region interfaces
    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        var data = new NetworkInputData();

        Vector3 direction = Vector3.zero;
        // Controle via Teclado (W, A, S, D)
        if (Input.GetKey(KeyCode.W))
        {
            direction += Vector3.forward;
            data.onMobile = false;
        }

        if (Input.GetKey(KeyCode.S))
        {
            direction += Vector3.back;
            data.onMobile = false;
        }

        if (Input.GetKey(KeyCode.A))
        {
            direction += Vector3.left;
            data.onMobile = false;
        }

        if (Input.GetKey(KeyCode.D))
        {
            direction += Vector3.right;
            data.onMobile = false;
        }

        // Controle via Joystick (Vertical e Horizontal)
        if (Input.GetAxis("Vertical") > 0)
        {
            direction += Vector3.forward;
            data.onMobile = false;
        }

        if (Input.GetAxis("Vertical") < 0)
        {
            direction += Vector3.back;
            data.onMobile = false;
        }

        if (Input.GetAxis("Horizontal") > 0)
        {
            direction += Vector3.right;
            data.onMobile = false;
        }

        if (Input.GetAxis("Horizontal") < 0)
        {
            direction += Vector3.left;
            data.onMobile = false;
        }

        // Transforma a direção do input de acordo com a rotação da câmera
        direction = Camera.main.transform.TransformDirection(direction);
        direction.y = 0;
        direction.Normalize();

        data.direction = direction;

        if (Input.GetKey(KeyCode.LeftShift) )
            data.isShift = true;
        else
            data.isShift = false;
        data.punch = Input.GetKey(KeyCode.Mouse0);

        input.Set(data);
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) {
        if(!isMenu)
        SceneManager.LoadScene("Gameplay");
        else
            SceneManager.LoadScene("Gameplay");
    }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    #endregion
}