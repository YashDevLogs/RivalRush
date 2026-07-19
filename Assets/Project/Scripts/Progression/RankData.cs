namespace Game.Progression
{
    public struct RankData
    {
        public PlayerRank Rank;
        public int MinMedals;
        public int MaxMedals;

        public RankData(
            PlayerRank rank,
            int minMedals,
            int maxMedals)
        {
            Rank = rank;
            MinMedals = minMedals;
            MaxMedals = maxMedals;
        }
    }
}