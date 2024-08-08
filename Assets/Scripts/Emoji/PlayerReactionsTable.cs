using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = nameof(PlayerReactionsTable), menuName = "Scriptables/PlayerReactions/PlayerReactionsTable")]
public class PlayerReactionsTable : ScriptableObject
{
    [SerializeField] private EmojiData[] m_emojisData;

    public EmojiData[] EmojisData { get => m_emojisData; }

    [System.Serializable]
    public class EmojiData
    {
        [SerializeField] private string m_emojiName;
        [SerializeField] private Sprite m_emojiSprite;

        public string EmojiName { get => m_emojiName; }
        public Sprite EmojiSprite { get => m_emojiSprite; }
    }

    public Sprite GetSpriteByName(string reactionName)
    {
        Sprite sprite = null;

        foreach (EmojiData emojiData in m_emojisData)
        {
            if (emojiData.EmojiName == reactionName)
                sprite = emojiData.EmojiSprite;
        }

        return sprite;
    }
}
