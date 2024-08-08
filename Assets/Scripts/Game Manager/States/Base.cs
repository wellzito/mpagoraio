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
        [Header("Base State HUD")]
        public bool isChangingStateBase = false;

        void SetupBaseHUD()
        {
            if (PlayerSpawner.instance == null) return;

            if (HasStateAuthority && !isChangingStateBase)
                _MatchStateInfo = (int)MatchState.START;

        }

        IEnumerator ChangeToBaseState()
        {
            isChangingStateBase = true;
            yield return new WaitForSeconds(.5f);

            _MatchStateInfo = (int)MatchState.START;
            isChangingStateBase = false;
        }

    }
}
