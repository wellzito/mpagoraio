using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionsPlayerController : MonoBehaviour
{
    [SerializeField] private ParticleSystem emojiParticles;
    [SerializeField] private PlayerReactionsTable playerReactionsTable;
    [SerializeField] PlayerInfo playerInfo;

    #region Emoji
    public void ShowCharacterReaction(string reactionName)
    {
        emojiParticles.Stop();
        Sprite sprite = playerReactionsTable.GetSpriteByName(reactionName);
        emojiParticles.textureSheetAnimation.SetSprite(0, sprite);
        emojiParticles.Play();
        playerInfo.keyEmoji = reactionName;
        playerInfo.onActionEmoji = true;
        playerInfo.RPC_Reactions(reactionName);
    }

    #endregion
}
