using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using UnityEngine.UI;
using System.Linq;

namespace CCB.Controller
{
    public partial class GameController : NetworkBehaviour
    {
        public static GameController instance;

        [Header("Player Settings")]
        public List<PlayerInfo> _playerInfos = new List<PlayerInfo>();

        public GameObject player1GO;
        [Networked] public NetworkId player1ID { get; set; }
        [Networked] public NetworkId player2ID { get; set; }


        public MatchState _currentMatchState;
        [Networked] public int _MatchStateInfo { get; set; }

        [Networked] public NetworkBool canMove { get; set; }

        private void Awake()
        {
            instance = this;
        }

        public override void Spawned()
        {
            if (HasStateAuthority)
            {
                _currentMatchState = MatchState.START;
            }
        }

        public override void Render()
        {
            _currentMatchState = (MatchState)_MatchStateInfo;

            if (HasStateAuthority)
            {
                if (Input.GetKeyDown(KeyCode.P))
                {
                    ResetAllPlayersPos();
                }
                if (_playerInfos.Count != PlayerSpawner.instance._spawnedCharacters.Count) GetAllPlayers();
                GameObject __p1 = GetPlayer(1);
                if (__p1 != null)
                {
                    player1ID = __p1.GetComponent<PlayerInfo>().playerId;
                    player1GO = __p1;
                }
                GameObject __p2 = GetPlayer(2);
                if (__p2 != null)
                {
                    player2ID = __p2.GetComponent<PlayerInfo>().playerId;
                }
            }

            switch (_currentMatchState)
            {
                case MatchState.WAIT:
                    SetupWaitHUD();
                    break;
                case MatchState.START:
                    SetupStartHUD();
                    break;
            }
        }
        

        #region Helpers Functions
        GameObject GetPlayer(int value)
        {
            foreach (PlayerInfo player in _playerInfos)
            {
                if (value == 1)
                {
                    if (player.isLocalPlayer)
                    {
                        return player.gameObject;
                    }
                }
                if (value == 2)
                {
                    if (!player.isLocalPlayer)
                    {
                        return player.gameObject;
                    }
                }
            }
            return null;
        }
        void GetAllPlayers()
        {
            NetworkObject[] players = GameObject.FindObjectsOfType<NetworkObject>();
            _playerInfos.Clear();
            foreach (var player in players)
            {
                PlayerInfo playerInfo = player.GetComponent<PlayerInfo>();
                if(playerInfo != null) { _playerInfos.Add(playerInfo); }
            }
        }

        void ResetAllPlayersPos()
        {
            foreach (PlayerInfo player in _playerInfos)
            {
                player.ChangeInitialPositionFromServer();
            }
        }

        [System.Serializable]
        public enum MatchState
        {
            WAIT = 0,
            COUNTDOWN = 1,
            START = 2,
            GOAL = 3,
            VICTORY = 4,
            LOSS = 5,
            RESET = 6,
            FINISH = 7,
        }
        #endregion
    }
}
