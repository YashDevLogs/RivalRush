namespace Game.Progression
{
    public struct RaceRewardData
    {
        public int Coins;
        public int XP;
        public int Medals;

        public RaceRewardData(
            int coins,
            int xp,
            int medals)
        {
            Coins = coins;
            XP = xp;
            Medals = medals;
        }
    }
}