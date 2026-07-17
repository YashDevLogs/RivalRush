using Game.Core;
using UnityEngine;

namespace Game.Progression
{
    public class ProgressionManager : MonoBehaviour
    {
        public static ProgressionManager Instance;

        [Header("Rank Database")]
        [SerializeField]
        private RankDatabase rankDatabase;

        private const int XP_PER_LEVEL = 1000;

        private void Awake()
        {
            if (Instance != null &&
                Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            DontDestroyOnLoad(gameObject);
        }

        #region Rewards

        public void AwardRaceRewards(int placement)
        {

              Debug.Log(
        $"[Rewards] Awarding rewards for place {placement}");

            RaceRewardData rewards =
                RaceRewardConfig.GetReward(placement);

    Debug.Log(
        $"[Rewards] Coins:{rewards.Coins} XP:{rewards.XP} Medals:{rewards.Medals}");

           PlayerDataManager data =
    PlayerDataManager.Instance;

Debug.Log(
    $"[Rewards] PlayerDataManager = {data}");

if (data == null)
{
    Debug.LogError(
        "[Rewards] PlayerDataManager NULL");

    return;
}

Debug.Log(
    "[Rewards] Calling AddCoins");

            data.AddCoins(rewards.Coins);
            

            data.AddXP(rewards.XP);

            data.AddMedals(rewards.Medals);

            HandleLevelUps();
        }

        #endregion

        #region Levels

        private void HandleLevelUps()
        {
            PlayerDataManager data =
                PlayerDataManager.Instance;

            if (data == null)
                return;

            while (data.XP >= data.Level * XP_PER_LEVEL)
            {
                data.SetLevel(
                    data.Level + 1);
            }
        }

        #endregion

        #region Rank

        public RankEntry GetRankData()
        {
            if (rankDatabase == null)
            {
                Debug.LogError(
                    "[ProgressionManager] RankDatabase not assigned.");

                return null;
            }

            int medals =
                PlayerDataManager.Instance.Medals;

            foreach (RankEntry rank in rankDatabase.Ranks)
            {
                if (medals >= rank.MinMedals &&
                    medals < rank.MaxMedals)
                {
                    return rank;
                }
            }

            return rankDatabase.Ranks[
                rankDatabase.Ranks.Length - 1];
        }

        #endregion
    }
}