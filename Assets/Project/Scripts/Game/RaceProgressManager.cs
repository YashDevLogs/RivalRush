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
                float progress = CalculateProgress(racer);

                progressData.Add(new RaceProgressData(racer, progress));
            }

            progressData.Sort((a, b) => b.Progress.CompareTo(a.Progress));
        }

        private float CalculateProgress(IPlayerEntity racer)
        {
            if (racer == null)
                return 0f;

            float currentX = racer.Transform.position.x;

            float normalized =
                Mathf.InverseLerp(
                    raceStartX,
                    raceFinishX,
                    currentX);

            return Mathf.Clamp01(normalized);
        }

        #endregion
    }
}