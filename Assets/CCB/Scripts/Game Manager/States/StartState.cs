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
        [Header("Start State HUD")]
        public bool isChangingStateStart = false;

        [Networked] public float timer { get; set; } = 90;
        [Networked] public int player1Score { get; set; } = 0;
        [Networked] public int player2Score { get; set; } = 0;



        void SetupStartHUD()
        {
            if (PlayerSpawner.instance == null) return;

            if (HasStateAuthority)
            {
                if(!canMove) canMove = true;

                timer -= Time.deltaTime;
                if(timer <= 0)
                {
                    timer = 0;
                    _MatchStateInfo = (int)MatchState.FINISH;
                }
            }
        }

        public int goalPlayer = -1;

        public void Score(int value)
        {
            goalPlayer = value;

            if (value == 1)
            {
                player1Score++;
                
                //animacao e bagulho todo
            }
            else
            {
                player2Score++;
            }

            _MatchStateInfo = (int)MatchState.GOAL;
            //BallCollision.instance.RPC_ResetBallPosition();
        }

        IEnumerator ChangeToGoalState()
        {
            isChangingStateStart = true;
            BallCollision.instance.RPC_ResetBallPosition();

            yield return new WaitForSeconds(.5f);

            _MatchStateInfo = (int)MatchState.START;
            isChangingStateStart = false;
        }

    }
}
