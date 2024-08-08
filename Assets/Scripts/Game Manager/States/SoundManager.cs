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
        public bool isChangingStateSound = false;

        /// <summary>
        /// 0 = countDown, 1 = whistle, 2 = goal, 3 = loss, 4 = fan
        /// </summary>
        [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
        void RPC_SetupSoundManager(int n)
        {
            if (PlayerSpawner.instance == null) return;

        }

    }
}
