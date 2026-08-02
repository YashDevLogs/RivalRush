using System.Collections.Generic;
using UnityEngine;
using Game.Core;

namespace Game.Systems
{
    public readonly struct RaceProgressData
    {
        public readonly IPlayerEntity Entity;
        public readonly float Progress;

        public RaceProgressData(IPlayerEntity entity, float progress)
        {
            Entity = entity;
            Progress = progress;
        }
    }

    public sealed class RaceProgressManager : MonoBehaviour
    {
        [SerializeField] private RaceManager raceManager;

        private readonly List<RaceProgressData> progressData = new();

        public IReadOnlyList<RaceProgressData> ProgressData => progressData;

        private float raceStartX;
        private float raceFinishX;
        private float raceDistance;

        private readonly Dictionary<IPlayerEntity, float> startPositions = new();

        private readonly HashSet<IPlayerEntity> finishedRacers = new();

        #region Unity

        private void Awake()
        {
            if (raceManager == null)
                raceManager = RaceManager.Instance;

            raceStartX = raceManager.RaceStart.position.x;
            raceFinishX = raceManager.RaceFinish.position.x;
            raceDistance = raceFinishX - raceStartX;
        }

        private void Update()
        {
            UpdateProgress();
        }

        #endregion

        #region Progress

        private void UpdateProgress()
        {
            progressData.Clear();

            IReadOnlyList<IPlayerEntity> racers = raceManager.GetPlayerEntities();

            foreach (IPlayerEntity racer in racers)
            {
                if (!startPositions.ContainsKey(racer))
                {
                    startPositions.Add(
                        racer,
                        racer.Transform.position.x);
                }

                float progress = CalculateProgress(racer);

                progressData.Add(
                    new RaceProgressData(
                        racer,
                        progress));
            }

            progressData.Sort((a, b) => b.Progress.CompareTo(a.Progress));
        }

        private float CalculateProgress(IPlayerEntity racer)
        {
            if (racer == null)
                return 0f;

            if (finishedRacers.Contains(racer))
                return 1f;

            float startX = startPositions[racer];

            float distanceTravelled =
                racer.Transform.position.x - startX;

            float normalized =
                distanceTravelled / raceDistance;

            return Mathf.Clamp01(normalized);
        }
        public void MarkFinished(IPlayerController controller)
        {
            if (controller is not IPlayerEntity entity)
            {
                Debug.LogError("MarkFinished: controller is NOT an IPlayerEntity.");
                return;
            }

            Debug.Log($"MarkFinished -> {entity.Transform.name}");

            finishedRacers.Add(entity);

            Debug.Log($"Finished Count = {finishedRacers.Count}");
        }
        #endregion
    }
}