using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
namespace CCB.Controller
{
    public partial class GameController : NetworkBehaviour
    {
        [Header("Wait State HUD")]
        public bool isChangingStateWait = false;

        void SetupWaitHUD()
        {
            if (PlayerSpawner.instance == null) return;

            //if (isChangingStateWait) return;

            NetworkObject p1;
            NetworkObject p2;
            if (player1GO == null)
            {
                Runner.TryFindObject(player1ID, out p1);
                if (p1 != null) player1GO = p1.gameObject;
            }

           /* MatchController.instance.timerText.text = "";

            if (player1GO != null)
            {
                ConnectPopupUI.instance.EnableObj(false);
                //RPC_DisableLoadPanel();
                PlayerInfo playerInfo = player1GO.GetComponent<PlayerInfo>();
                MatchController.instance.rawPlayer1.texture = playerInfo._renderTexture;
                MatchController.instance.playerName1.text = playerInfo.playerName.ToString();
                MatchController.instance.player1Text.text = playerInfo.playerName.ToString();
                MatchController.instance.player1ScoreText.text = "0";
                MatchController.instance.rawPlayer1Face.texture = playerInfo._renderTextureFace;

            }
            if (player2GO != null)
            {
                PlayerInfo playerInfo = player2GO.GetComponent<PlayerInfo>();
                MatchController.instance.rawPlayer2.gameObject.SetActive(true);
                MatchController.instance.infosWait.SetActive(false);
                MatchController.instance.rawPlayer2.texture = playerInfo._renderTexture;
                MatchController.instance.rawPlayer2Face.texture = playerInfo._renderTextureFace;
                MatchController.instance.playerName2.text = playerInfo.playerName.ToString();

                MatchController.instance.player2Text.text = playerInfo.playerName.ToString();
                MatchController.instance.player2ScoreText.text = "0";

                if (HasStateAuthority && !isChangingStateWait)
                {
                    canMove = false;
                    StartCoroutine(nameof(ChangeToCountdownState));
                }
            }
            else
            {
                MatchController.instance.rawPlayer2.gameObject.SetActive(false);
                MatchController.instance.infosWait.SetActive(true);
                MatchController.instance.playerName1.text = "";

            }*/
        }

        IEnumerator ChangeToCountdownState()
        {
            isChangingStateWait = true;
            //RPC_DisableLoadPanel();
            yield return new WaitForSeconds(2);
          //  yield return new WaitUntil(() => MatchController.instance.rawPlayer2.texture != null && MatchController.instance.rawPlayer1.texture != null);
            _MatchStateInfo = (int)MatchState.COUNTDOWN;
            isChangingStateWait = false;
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
        public void RPC_DisableLoadPanel()
        {
            
        }

    }
}
