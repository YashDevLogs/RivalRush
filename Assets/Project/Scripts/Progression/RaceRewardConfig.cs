using UnityEngine;

namespace Game.Progression
{
    public static class RaceRewardConfig
    {
        public static RaceRewardData GetReward(int placement)
        {
            switch (placement)
            {
                case 1:
                    return new RaceRewardData(
                        500,
                        150,
                        30);

                case 2:
                    return new RaceRewardData(
                        300,
                        100,
                        20);

                case 3:
                    return new RaceRewardData(
                        200,
                        75,
                        10);

                default:
                    return new RaceRewardData(
                        100,
                        50,
                        5);
            }
        }
    }
}