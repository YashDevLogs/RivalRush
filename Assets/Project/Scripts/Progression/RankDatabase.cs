using UnityEngine;

namespace Game.Progression
{
    [CreateAssetMenu(
        menuName = "Rival Rush/Rank Database")]
    public class RankDatabase : ScriptableObject
    {
        public RankEntry[] Ranks;
    }

    [System.Serializable]
    public class RankEntry
{
    public PlayerRank Rank;

    public int MinMedals;

    public int MaxMedals;

    public Sprite BadgeSprite;
}
}