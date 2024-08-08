using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Unity.VisualScripting;
using System.Text.RegularExpressions;
namespace CCB.Controller
{
    public partial class GameController : NetworkBehaviour
    {
        [Header("Countdown State HUD")]
        public bool isChangingStateReset = false;

        [Networked] public NetworkBool isLoaded { get; set; } = false;

        public float timeToReset = 2;
        [Networked] public float _timeToReset { get; set; } = 0;

        void SetupResetHUD()
        {
            if (PlayerSpawner.instance == null) return;

            if (HasStateAuthority)
            {
                if(!isLoaded)
                {
                    _timeToReset += Time.deltaTime;
                    if (_timeToReset >= timeToReset)
                    {
                        _timeToReset = 0;
                        isLoaded = true;
                    }
                }
            }
            if (HasStateAuthority && !isChangingStateReset && isLoaded)
            StartCoroutine(ChangeToStartStateReset());

        }

        IEnumerator ChangeToStartStateReset()
        {
            isChangingStateReset = true;
            PlayerInfo playerinfo1 = player1GO.GetComponent<PlayerInfo>();
            playerinfo1.ChangeInitialPositionFromServer();
            playerinfo1.RPC_ResetAnimator();

            yield return new WaitForSeconds(.1f);

            _MatchStateInfo = (int)MatchState.COUNTDOWN;
            isChangingStateReset = false;
            canMove = true;

            isLoaded = false;

            //MatchController.instance.resetSceneGame.panelReset.SetActive(false);
            //MatchController.instance.resetSceneGame.loadingSlider.value = 0;
        }

    }
}
